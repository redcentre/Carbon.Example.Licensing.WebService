using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RCS.Licensing.Provider.Shared;
using Microsoft.Extensions.Configuration;

namespace RCS.Licensing.Example.WebService.UnitTests;

public class TestBase
{
	public TestContext TestContext { get; set; }

	// ADJUST THE FOLLOWING FOR YOUR ENVIRONMENT
	// ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
	protected const string SeviceUri = "https://localhost:7255/";
	//protected const string SeviceUri = "https://bayesprice.azurewebsites.net/licensing/";
	protected string UserId;
	protected string UserName;
	protected string UserPass;
	protected string ApiKey;
	protected string ProductKey;
	protected IConfiguration Config;

	readonly JsonSerializerOptions JOpts = new JsonSerializerOptions() { WriteIndented = true };
	readonly JsonSerializerOptions JDumpOpts = new JsonSerializerOptions() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	protected TestBase()
	{
		Config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddUserSecrets("e630debe-54df-4ae4-bdaf-9d46eb4caab0").Build();
		UserId = Config["UnitTests:UserId"]!;
		UserName = Config["UnitTests:UserName"]!;
		UserName = Config["UnitTests:UserPass"]!;
		ApiKey = Config["UnitTests:ApiKey"]!;
		ProductKey = Config["UnitTests:ProductKey"]!;
	}

	protected async Task WrapClientLoginName(Func<ExampleLicensingServiceClient, Task> callback, string? uri = null, string? apiKey = null, string? userName = null, string? password = null)
	{
		using var client = new ExampleLicensingServiceClient(new Uri(uri ?? SeviceUri), apiKey ?? ApiKey, null, true);
		var wrapin = await client.AuthenticateName(userName ?? UserName, password ?? UserPass);
		await callback(client);
	}

	protected async Task WrapClientLoginId(Func<ExampleLicensingServiceClient, Task> callback, string? uri = null, string? apiKey = null, string? userId = null, string? password = null)
	{
		using var client = new ExampleLicensingServiceClient(new Uri(uri ?? SeviceUri), apiKey ?? ApiKey, null, true);
		var wrap1 = await client.AuthenticateId(userId ?? UserId, password ?? UserPass);
		await callback(client);
	}

	protected void Sep1(string? title = null)
	{
		if (title != null)
		{
			int len = title.Length + 4;
			Trace("┌" + new string('─', len) + "┐");
			Trace("│  " + title + "  │");
			Trace("└" + new string('─', len) + "┘");
		}
	}

	protected void DumpLic(LicenceFull? lic)
	{
		if (lic == null)
		{
			Trace("NULL");
			return;
		}
		Trace($"Id ............ {lic.Id}");
		Trace($"Name .......... {lic.Name}");
		Trace($"Email ......... {lic.Email}");
		Trace($"Roles ......... {Join(lic.Roles)}");
		Trace($"Realms ........ {Join(lic.Realms)}");
		foreach (var cust in lic.Customers)
		{
			Trace($"|  {cust.Id} {cust.Name} {cust.DisplayName}");
			foreach (var job in cust.Jobs)
			{
				string vtjoin = Join(job.VartreeNames);
				Trace($"|  |  {job.Id} {job.Name} {job.DisplayName} {vtjoin}");
			}
		}
	}

	protected void Trace(string message)
	{
		//TestContext.WriteLine(message);
		System.Diagnostics.Trace.WriteLine(message);
	}

	protected static string Join(IEnumerable<object> list) => list == null ? "NULL" : "[" + string.Join(",", list) + "]";

	protected void DumpObject(object? value)
	{
		if (value == null)
		{
			Trace("NULL");
		}
		else
		{
			Trace($"═══════════════ {value.GetType().Name} ═══════════════");
			string json = JsonSerializer.Serialize(value, JDumpOpts);
			Trace(json);
		}
	}
}
