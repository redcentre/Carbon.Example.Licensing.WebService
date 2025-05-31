using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Example.Provider;
using RCS.Licensing.Example.WebService.Controllers;
using RCS.Licensing.Example.WebService.Shared;
using RCS.Licensing.Provider.Shared;

namespace RCS.Licensing.Example.WebService;

/// <summary>
/// The global error handler converts unhandled Exceptions into the expected status 500
/// response, but it always puts an ErrorResponse in the body.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : LicensingControllerBase
{
	public ErrorController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	/// <ignore/>
	[Route("error")]
	public IActionResult Error()
	{
		IExceptionHandlerFeature? handler = HttpContext.Features.Get<IExceptionHandlerFeature>();
		if (handler == null)
		{
			// Guard against a crazy condition.
			return StatusCode(StatusCodes.Status500InternalServerError, new ResponseWrap<string?>(901, "The error handler is not available"));
		}
		double? secs = null;
		if (HttpContext.Items.TryGetValue(StandardActionFilterAttribute.StartTimeKey, out object? start))
		{
			DateTime startTime = (DateTime)start!;
			secs = DateTime.Now.Subtract(startTime).TotalSeconds;
		}
		Logger.LogError(handler.Error, "{StatusCode} {Method} {Path} {ErrorType} {ErrorMessage} [{Secs:F1}]", HttpContext.Response.StatusCode, HttpContext.Request.Method, HttpContext.Request.Path, handler.Error.GetType().Name, handler.Error.Message, secs);
		if (handler.Error is ExampleLicensingException elex)
		{
			// This is a known application error and returns an OK status but the response body contains a known code and message.
			var resp = new ResponseWrap<string?>((int)elex.ErrorType, handler.Error.GetBaseException().Message);
			return StatusCode(StatusCodes.Status200OK, resp);
		}
		return StatusCode(StatusCodes.Status500InternalServerError, new ResponseWrap<string?>(902, $"{handler.Error.GetType().Name} : {handler.Error.GetBaseException().Message}"));
	}
}