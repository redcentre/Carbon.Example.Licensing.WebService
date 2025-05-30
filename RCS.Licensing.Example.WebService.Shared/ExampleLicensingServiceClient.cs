using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RCS.Licensing.Provider.Shared;

namespace RCS.Licensing.Example.WebService.Shared
{
	/// <summary>
	/// A strongly-typed .NET client class for the example licensing web service.
	/// </summary>
	/// <remarks>
	/// A business processing error in the service call is converted into a throw of an
	/// <see cref="ApplicationException"/>. Any other type of Exception indicates some kind
	/// of unexpected error outside of the service processing logic.
	/// </remarks>
	public partial class ExampleLicensingServiceClient : IExampleLicensingServiceClient, IDisposable
	{
		public const string ResponseTypeHeaderName = "x-error-response-type";
		public const string ApiKeyHeaderName = "x-api-key";
		public const string SignatureHeaderName = "x-signature";
		public const string SessionIdHeaderName = "x-session-id";
		static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
		readonly bool throwOnError;

		/// <summary>
		/// <para>
		///   Constructs a service client with a custom <see cref="HttpClient"/> that will internally
		///   be used to make calls to the web service.
		/// </para>
		/// <para>
		///   The caller is expected to set the <see cref="HttpClient.BaseAddress"/> to point to the service.
		/// </para>
		/// <para>
		///   If the client will be making any service calls that access the licensing database then a request
		///   header with name <c>x-api-key</c> must be added to the client to authorise access.
		/// </para>
		/// </summary>
		/// <param name="client">A custom <see cref="HttpClient"/> configured to call the licensing web service.></param>
		/// <include file='DocHelp.xml' path='doc/members[@name="ThrowOnError"]/*'/>
		public ExampleLicensingServiceClient(HttpClient client, bool throwOnError = false)
		{
			Client = client;
			this.throwOnError = throwOnError;
		}

		/// <summary>
		///   Constructs a service client for a licensing web service published at a specified base Url,
		///   with an optional API Key to authorise access to the licensing database.
		/// </summary>
		/// <param name="serviceBaseUri">The Uri of the base address of the licensing web service.</param>
		/// <param name="apiKey">An optional API Key which will be placed in request headers with the
		///   name <c>x-api-key</c> to allow access to the licensing database.</param>
		///   <param name="signature"></param>
		/// <include file='DocHelp.xml' path='doc/members[@name="ThrowOnError"]/*'/>
		public ExampleLicensingServiceClient(Uri serviceBaseUri, string? apiKey = null, string? signature = null, bool throwOnError = false)
		{
			Client = new HttpClient
			{
				BaseAddress = serviceBaseUri
			};
			if (apiKey != null)
			{
				Client.DefaultRequestHeaders.Add(ApiKeyHeaderName, apiKey);
			}
			if (signature != null)
			{
				Client.DefaultRequestHeaders.Add(SignatureHeaderName, signature);
			}
			this.throwOnError = throwOnError;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Client?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Contains the licensing information returned by the last successful authentication call.
		/// The property is set automatically by the authentication call so there is no need for the
		/// client to manually set this property.
		/// </summary>
		public LicenceFull? Licence { get; set; }

		/// <summary>
		/// Only the three authentication methods need some special handling to get the LicenceFull
		/// coming back from the licensing provider into a smaller LicenceInfo class
		/// </summary>
		/// <param name="licence"></param>
		void InspectLicence(LicenceFull licence)
		{
			Licence = licence;
		}

		#region Helpers

		void AddSessionHeader()
		{
			if (Client.DefaultRequestHeaders.Contains(SessionIdHeaderName))
			{
				Client.DefaultRequestHeaders.Remove(SessionIdHeaderName);
			}
			if (Licence != null)
			{
				Client.DefaultRequestHeaders.Add(SessionIdHeaderName, Licence.SessionId.ToString());
			}
		}

		void RemoveSessionHeader()
		{
			if (Client.DefaultRequestHeaders.Contains(SessionIdHeaderName))
			{
				Client.DefaultRequestHeaders.Remove(SessionIdHeaderName);
			}
		}

		static StringContent MakeContent<T>(T data)
		{
			string json = JsonSerializer.Serialize(data);
			return new StringContent(json, Encoding.UTF8, "application/json");
		}

		static async Task<ResponseWrap<T>> UnwrapAndCheckResult<T>(HttpResponseMessage hrm, bool throwOnError)
		{
			string body = await hrm.Content.ReadAsStringAsync();
			if (hrm.StatusCode != HttpStatusCode.OK)
			{
				// This service ONLY returns status 200. Anything else indicates a serious processing problem.
				var ex = new ApplicationException($"Unexpected status code {hrm.StatusCode} from {hrm.RequestMessage?.Method} {hrm.RequestMessage?.RequestUri}");
				// Give the caller the body so at least they have a change of
				// debugging and discovering a clue to the unexpected failure.
				ex.Data.Add("body", body);
				throw ex;
			}
			var wrap = JsonSerializer.Deserialize<ResponseWrap<T>>(body, JsonOpts)!;
			if (wrap.HasError && throwOnError)
			{
				// There has been some sort of business logic processing problem. Error information
				// is extracted from the standard response properties and thrown back to the caller.
				// The Exception's Data collection will containing any additional information.
				string msg = wrap.Message ?? $"Error code {wrap.Code} (no error message is provided)";
				var ex = new ApplicationException(msg);
				ex.Data.Add("Code", wrap.Code);
				if (wrap.Detail != null)
				{
					ex.Data.Add("Detail", wrap.Detail);
				}
				throw ex;
			}
			return wrap;
		}

		HttpClient Client { get; set; }

		#endregion
	}
}