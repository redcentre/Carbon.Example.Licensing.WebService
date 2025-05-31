using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Azure.StorageAccount;
using RCS.Licensing.Example.WebService.Shared;
using RCS.Licensing.Provider.Shared;

namespace RCS.Licensing.Example.WebService.Controllers;

public class LicensingControllerBase : ControllerBase
{
	public LicensingControllerBase(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
	{
		Logger = loggerFactory.CreateLogger("CON");
		Config = configuration;
		Licprov = licprov;
	}

	protected ILogger Logger { get; private set; }
	
	protected IConfiguration Config { get; private set; }

	protected ILicensingProvider Licprov { get; private set; }

	protected Guid? SessionId => HttpContext.Items.TryGetValue(ExampleLicensingServiceClient.SessionIdHeaderName, out object? value) ? (Guid?)value : null;

	protected string? ApiKey => HttpContext.Items.TryGetValue(ExampleLicensingServiceClient.ApiKeyHeaderName, out object? value) ? value?.ToString() : null;

	readonly JsonSerializerOptions Jopts1 = new() { WriteIndented = true };

	protected void DumpObj(object? value, string? title = null)
	{
		Trace.WriteIf(title != null, $"=========== {title} ===========");
		if (value == null)
		{
			Trace.WriteLine("NULL");
			return;
		}
		string json = JsonSerializer.Serialize(value, Jopts1);
		Trace.WriteLine(json);
	}

	SubscriptionUtility? _subutil;
	protected SubscriptionUtility SubscriptionUtil => LazyInitializer.EnsureInitialized(ref _subutil, () => new SubscriptionUtility(
		Config["LicensingService:SubscriptionId"]!,
		Config["LicensingService:TenantId"]!,
		Config["LicensingService:ApplicationId"]!,
		Config["LicensingService:ClientSecret"]!
	));

	PleaProcessor? _pleaproc;
	bool pleaFailed;

	/// <summary>
	/// Gets a lazy singleton of the plea processor if the required config values are defined,
	/// otherwise it always returns null.
	/// </summary>
	protected async Task<PleaProcessor?> GetPleaProcAsync()
	{
		if (_pleaproc == null || !pleaFailed)
		{
			string? connect = Config["LicensingService:AzureConnect"];
			string? conname = Config["LicensingService:AzureContainer"];
			if (string.IsNullOrEmpty(connect) || string.IsNullOrEmpty(conname))
			{
				pleaFailed = true;
				return null;
			}
			_pleaproc = await PleaProcessor.CreateAsync(connect, conname, "plea.xml");
		}
		return _pleaproc;
	}
}
