using System;
using System.Linq;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RCS.Licensing.Example.WebService;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SessionCheckerAttribute : Attribute, IAsyncAuthorizationFilter
{
	static ILogger? _logger;

	/// <summary>
	/// An endpoint with this attribute must contain a request header that carries a session
	/// Id (a Guid) that was created by a successful call to one of the login methods.
	/// </summary>
	public SessionCheckerAttribute(ILoggerFactory logfac)
	{
		if (_logger == null)
		{
			_logger = logfac.CreateLogger("SESS");
		}
	}

	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		await Task.CompletedTask;
		var req = context.HttpContext.Request;
		// ┌──────────────────────────────────────────────────┐
		// │  If the controller's calling method has the      │
		// │  [AllowAnonymous] attribute then session         │
		// │  checking is completely skipped.                 │
		// └──────────────────────────────────────────────────┘
		bool allowAnon = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
		if (allowAnon) return;
		var sessattr = context.ActionDescriptor.EndpointMetadata.OfType<SessionCheckerAttribute>().FirstOrDefault();
		// ┌────────────────────────────────────────────────────────────────────────┐
		// │  This service endpoint requires a session Id created by a successful   │
		// │  successful authentication request to be in the x-session-id header.   │
		// │  In this case the API Key is not required and ignored.                 │
		// └────────────────────────────────────────────────────────────────────────┘
		string? rawId = null;
		if (context.HttpContext.Request.Headers.TryGetValue(ExampleLicensingServiceClient.SessionIdHeaderName, out var values))
		{
			rawId = values.FirstOrDefault();
		}
		if (rawId == null)
		{
			_logger!.LogWarning("NullSession {Method} {Path}", req.Method, req.Path);
			var wrap = new ResponseWrap<MockResponse>(403, $"Request header key '{ExampleLicensingServiceClient.SessionIdHeaderName}' is required.");
			context.Result = new ObjectResult(wrap) { StatusCode = StatusCodes.Status200OK };
			return;
		}
		if (!Guid.TryParse(rawId, out var sessionId))
		{
			_logger!.LogWarning("BadSession {Method} {Path}", req.Method, req.Path);
			var wrap = new ResponseWrap<MockResponse>(403, $"Request header key '{ExampleLicensingServiceClient.SessionIdHeaderName}' value is incorrectlty formatted.");
			context.Result = new ObjectResult(wrap) { StatusCode = StatusCodes.Status200OK };
			return;
		}
		// Any valid session Id is placed in the context item collection
		// so it can be easily referenced further down request processing.
		context.HttpContext.Items.Add(ExampleLicensingServiceClient.SessionIdHeaderName, sessionId);
		_logger!.LogInformation("Session Auth {Method} {Path}", req.Method, req.Path);
	}
}
