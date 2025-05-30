using System;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RCS.Licensing.Example.WebService.UnitTests;

[TestClass]
public class ServiceTests : TestBase
{
	[TestMethod]
	public async Task S100_GetInfo()
	{
		using var client = new ExampleLicensingServiceClient(new Uri(SeviceUri), null, null, true);
		var info = await client.GetServiceInfo();
		DumpObject(info);
	}
}