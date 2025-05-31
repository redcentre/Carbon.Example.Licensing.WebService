using System;
using System.Linq;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RCS.Licensing.Example.WebService;

/// <summary>
/// An endpoint with this attribute must contain a request header that carries one of the
/// registered API keys that are given to client app developers.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuthCheckerAttribute : Attribute, IAuthorizationFilter
{
	static string[]? validApiKeys;
	static ILogger? _logger;

	public AuthCheckerAttribute(ILoggerFactory logfac)
	{
		_logger ??= logfac.CreateLogger("AUTH");
	}

	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var req = context.HttpContext.Request;
		// ┌──────────────────────────────────────────────────┐
		// │  If the controller's calling method has the      │
		// │  [AllowAnonymous] attribute then authorization   │
		// │  checking is completely skipped.                 │
		// └──────────────────────────────────────────────────┘
		bool allowAnon = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
		if (allowAnon) return;
		// ┌─────────────────────────────────────────────────────────┐
		// │  This service endpoint requires either the x-api-key    │
		// │  or x-signature header values. If the API key is        │
		// │  present then they have traditional full access to      │
		// │  everything. The signature can be used in the future    │
		// │  to implement roles based authorisation, but it's only  │
		// │  experimental at the moment and is ignored.             │
		// └─────────────────────────────────────────────────────────┘
		string? key = null;
		string? signature;
		if (context.HttpContext.Request.Headers.TryGetValue(ExampleLicensingServiceClient.ApiKeyHeaderName, out var values))
		{
			key = values.FirstOrDefault();
		}
		if (context.HttpContext.Request.Headers.TryGetValue(ExampleLicensingServiceClient.SignatureHeaderName, out values))
		{
			// Ignored at the moment.
			signature = values.FirstOrDefault();
		}
		if (key == null)
		{
			_logger!.LogWarning("{Method} {Path} missing authorisation key", req.Method, req.Path);
			var wrap = new ResponseWrap<MockResponse>(403, $"Request header key '{ExampleLicensingServiceClient.ApiKeyHeaderName}' is required.");
			context.Result = new ObjectResult(wrap) { StatusCode = StatusCodes.Status200OK };
			return;
		}
		// ┌──────────────────────────────────────────────────┐
		// │  Lazy first-time load the registered API Keys    │
		// │  out of the app config file. The value from the  │
		// │  request header must be one of the keys.         │
		// └──────────────────────────────────────────────────┘
		if (validApiKeys == null)
		{
			var configsvc = (IConfiguration)context.HttpContext.RequestServices.GetService(typeof(IConfiguration))!;
			string apiKeyList = configsvc["LicensingService:ApiKeyList"]!;
			validApiKeys = apiKeyList?.Split(',') ?? [];
		}
		if (!validApiKeys.Contains(key))
		{
			_logger!.LogWarning("{Method} {Path} unregistered authorisation key", req.Method, req.Path);
			// Any valid API Key is placed in the context item collection
			// so it can be easily referenced further down request processing.
			var wrap = new ResponseWrap<MockResponse>(403, $"Request header key '{ExampleLicensingServiceClient.ApiKeyHeaderName}' value is not registered.");
			context.Result = new ObjectResult(wrap) { StatusCode = StatusCodes.Status200OK };
			return;
		}
		context.HttpContext.Items.Add(ExampleLicensingServiceClient.ApiKeyHeaderName, key);
		//_logger!.LogInformation("Key Auth {Method} {Path}", req.Method, req.Path);	#### DEBUGGING
	}
}
