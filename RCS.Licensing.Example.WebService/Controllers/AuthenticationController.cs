using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Example.Provider;
using RCS.Licensing.Example.WebService.Shared;
using RCS.Licensing.Provider.Shared;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Route("authentication")]
[Tags("Authentication")]
[TypeFilter(typeof(StandardActionFilterAttribute))]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class AuthenticationController : LicensingControllerBase
{
	public AuthenticationController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<LicenceFull>> InnerAuthenticateName(AuthenticateNameRequest request, bool canThrow)
	{
		try
		{
			var licfull = await Licprov.AuthenticateName(request.UserName, request.Password, request.SkipCache);
			EnrichLicence(licfull);
			return new ResponseWrap<LicenceFull>(licfull);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<LicenceFull>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<LicenceFull>> InnerAuthenticateId(AuthenticateIdRequest request, bool canThrow)
	{
		try
		{
			var licfull = await Licprov.AuthenticateId(request.UserId, request.Password, request.SkipCache);
			EnrichLicence(licfull);
			return new ResponseWrap<LicenceFull>(licfull);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<LicenceFull>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<LicenceFull>> InnerGetFreeLicence(FreeLicenceRequest request, bool canThrow)
	{
		try
		{
			var licfull = await Licprov.GetFreeLicence(request.ClientIdentifier, request.SkipCache);
			EnrichLicence(licfull);
			return new ResponseWrap<LicenceFull>(licfull);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<LicenceFull>((int)ex.ErrorType, ex.Message);
		}
	}

	/// <summary>
	/// Add extra environment and licensing information to the response that is only meaningful to this
	/// web service and not the underlying database.
	/// </summary>
	void EnrichLicence(LicenceFull licence)
	{

		// ┌───────────────────────────────────────────────────────────────┐
		// │  The following product key is will be used by clients who     │
		// │  will use the Carbon engine. This service can't know what     │
		// │  clients will do, so it passes one back just in case.         │
		// └───────────────────────────────────────────────────────────────┘
		licence.ProductKey = Config["LicensingService:ProductKey"];
		// ┌───────────────────────────────────────────────────────────────┐
		// │  The signature returned in the authentication response is     │
		// │  experimental. It contains proof of the Id and time that a    │
		// │  user account authenticated against the service. It could be  │
		// │  treated like a mini JWT if placed in request headers and     │
		// │  used for endpoint authorisation checking. By default, the    │
		// │  example service has a flat authorisation model where a valid │
		// │  API in the x-api-key header provides full access. This is a  │
		// │  purely exprimental property which may develop into something │
		// │  useful later. Developers adapting this project could replace │
		// │  the signature with a real JWT or similar.                    │
		// └───────────────────────────────────────────────────────────────┘
		int hours = Config.GetValue<int>("LicensingService:SignatureHours");
		licence.LicenceSignature = SignatureProcessor.Create(licence.Id, hours);
	}
}
