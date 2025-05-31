using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RCS.Licensing.Example.WebService;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class StandardActionFilterAttribute : ActionFilterAttribute
{
	public const string StartTimeKey = "StartTime";
	static ILogger? _logger;

	public StandardActionFilterAttribute(ILoggerFactory logfac)
	{
		if (_logger == null)
		{
			_logger = logfac.CreateLogger("STD");
		}
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		context.HttpContext.Items[StartTimeKey] = DateTime.Now;
		var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger));
		var req = context.HttpContext.Request;
		//_logger!.LogInformation("{Method} {Path}", req.Method, req.Path);  // #### DEBUGGING
	}

	public override void OnResultExecuted(ResultExecutedContext context)
	{
		double? secs = null;
		// In cases like 404s the OnActionExecuting method above is skipped so the Items
		// collection won't contain our custom values. So we have to be careful here to
		// check if they exist and use fallback values.
		if (context.HttpContext.Items.TryGetValue(StartTimeKey, out object? start))
		{
			DateTime startTime = (DateTime)start!;
			secs = DateTime.Now.Subtract(startTime).TotalSeconds;
		}
		var req = context.HttpContext.Request;
		var resp = context.HttpContext.Response;
		_logger!.LogInformation("{StatusCode} {Method} {Path} [{Secs:F1}]", resp.StatusCode, req.Method, req.Path, secs);
		base.OnResultExecuted(context);
	}

}
