using System;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Provider.Shared;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Route("service")]
[Tags("Service")]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class ServiceController : LicensingControllerBase
{
	public ServiceController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<ServiceInfo>> InnerGetServiceInfo()
	{
		var asm = typeof(Program).Assembly;
		var an = asm.GetName();
		var build = asm.GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == "BuildTime").Value!;
		var info = new ServiceInfo()
		{
			Machine = Environment.MachineName,
			User = Environment.UserName,
			ServiceVersion = an.Version!.ToString(),
			ServiceBuild = build,
			LicensingName = Licprov.Name,
			LicensingDescription = Licprov.Description
		};
		var resp = new ResponseWrap<ServiceInfo>(info);
		return await Task.FromResult(resp);
	}

	async Task<ResponseWrap<MockResponse>> InnerThrowError(int number)
	{
		if (DateTime.Now.Ticks > 0)
		{
			throw new Exception($"This is a deliberate error for argument number {number}");
		}
		var resp = new ResponseWrap<MockResponse>(new MockResponse($"The argument number is {number}"));
		return await Task.FromResult(resp);
	}
}
