using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace RCS.LicensingV2.WebApi;

internal class StandardActionFilterAttribute : ActionFilterAttribute
{
	public const string RequestCountKey = "ReqCount";
	public const string StartTimeKey = "StartTime";
	static ILogger? _logger;
	static int reqCount = 1000;

	public StandardActionFilterAttribute(ILoggerFactory logfac)
	{
		if (_logger == null)
		{
			_logger = logfac.CreateLogger("STD");
		}
	}

	public override void OnActionExecuting(ActionExecutingContext context)
	{
		Interlocked.Increment(ref reqCount);
		context.HttpContext.Items.Add(RequestCountKey, reqCount);
		context.HttpContext.Items.Add(StartTimeKey, DateTime.Now);
		var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger));
		var req = context.HttpContext.Request;
		_logger!.LogInformation("{Count} Executing {Method} {Path}", reqCount, req.Method, req.Path);
	}

	public override void OnResultExecuted(ResultExecutedContext context)
	{
		int currCount = reqCount;
		double secs = -1;
		// In cases like 404s the OnActionExecuting method above is skipped so the Items
		// collection won't contain our custom values. So we have to be careful here to
		// check if they exist and use fallback values.
		if (context.HttpContext.Items.TryGetValue(RequestCountKey, out object? value))
		{
			currCount = (int)value!;
		}
		if (context.HttpContext.Items.TryGetValue(StartTimeKey, out object? start))
		{
			DateTime startTime = (DateTime)start!;
			secs = DateTime.Now.Subtract(startTime).TotalSeconds;
		}
		var req = context.HttpContext.Request;
		var resp = context.HttpContext.Response;
		_logger!.LogInformation("{Count} Executed {StatusCode} {Method} {Path} [{Secs:F1}]", currCount, resp.StatusCode, req.Method, req.Path, secs);
	}

}
