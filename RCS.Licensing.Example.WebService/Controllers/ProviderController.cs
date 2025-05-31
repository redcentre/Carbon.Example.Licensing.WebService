using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Example.WebService.Shared;
using RCS.Licensing.Provider.Shared;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Route("provider")]
[Tags("Provider")]
[TypeFilter(typeof(StandardActionFilterAttribute))]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class ProviderController : LicensingControllerBase
{
	public ProviderController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<string>> InnerGetName()
	{
		var wrap = new ResponseWrap<string>(Licprov.Name);
		return await Task.FromResult(wrap);
	}

	async Task<ResponseWrap<string>> InnerGetDescription()
	{
		var wrap = new ResponseWrap<string>(Licprov.Description);
		return await Task.FromResult(wrap);
	}

	async Task<ResponseWrap<string>> InnerGetConfigSummary()
	{
		var wrap = new ResponseWrap<string>(Licprov.ConfigSummary);
		return await Task.FromResult(wrap);
	}

	async Task<ResponseWrap<bool>> InnerGetSupportsRealms()
	{
		var wrap = new ResponseWrap<bool>(Licprov.SupportsRealms);
		return await Task.FromResult(wrap);
	}

	async Task<ResponseWrap<NavData>> InnerGetNavigationData()
	{
		var navdata = await Licprov.GetNavigationData();
		return new ResponseWrap<NavData>(navdata);
	}

	async Task<ResponseWrap<ReportItem[]>> InnerGetDatabaseReport()
	{
		var items = await Licprov.GetDatabaseReport();
		return new ResponseWrap<ReportItem[]>(items);
	}
}
