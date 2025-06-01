using System;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RCS.Licensing.Example.WebService.UnitTests;

[TestClass]
public class AuthenticateTests : TestBase
{
	[TestMethod]
	public async Task L100_Login_Bad_Uri()
	{
		using var client = new ExampleLicensingServiceClient(new Uri(SeviceUri + "foo/"));
		var ex = await Assert.ThrowsExceptionAsync<ApplicationException>(() => client.AuthenticateId(UserId, UserPass));
		Trace(ex.ToString());
		Assert.IsTrue(ex.Message.StartsWith("Unexpected status code NotFound from POST"));
	}

	[TestMethod]
	public async Task L110_Login_Bad_Id_Psw()
	{
		using var client = new ExampleLicensingServiceClient(new Uri(SeviceUri), null, null, true);
		var ex = await Assert.ThrowsExceptionAsync<ApplicationException>(() => client.AuthenticateId(UserId, "wibble"));
		Trace(ex.Message);
		Assert.AreEqual($"User Id '{UserId}' incorrect password", ex.Message);
	}

	[TestMethod]
	public async Task L120_Login_Id()
	{
		await WrapClientLoginId(async (client) =>
		{
			DumpObject(client.Licence);
			await Task.CompletedTask;
		});
	}

	[TestMethod]
	public async Task L170_Login_Bad_Name_Psw()
	{
		using var client = new ExampleLicensingServiceClient(new Uri(SeviceUri), null, null, true);
		var ex = await Assert.ThrowsExceptionAsync<ApplicationException>(() => client.AuthenticateName(UserName, "wobble"));
		Trace(ex.Message);
		Assert.AreEqual($"User Name '{UserName}' incorrect password", ex.Message);
	}

	[TestMethod]
	public async Task L180_Login_Name()
	{
		await WrapClientLoginName(async (client) =>
		{
			DumpObject(client.Licence);
			await Task.CompletedTask;
		});
	}
}