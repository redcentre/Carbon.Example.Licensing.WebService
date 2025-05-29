using System;

namespace RCS.Licensing.Example.WebService.Shared
{
	/// <summary>
	/// A standard error reponse for application errors.
	/// </summary>
	[Obsolete("Legacy class for the old Authentication controller")]
	public sealed class ErrorResponse
	{
		/// <ignore/>
		public ErrorResponse()
		{
		}

		public ErrorResponse(int code, string message, string? description = null)
		{
			Code = code;
			Message = message;
			Description = description;
		}

		/// <summary>
		/// An unique code for the error.
		/// </summary>
		public int Code { get; set; }
		/// <summary>
		/// The error message.
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// An optional description of the error.
		/// </summary>
		public string? Description { get; set; }
		/// <summary>
		/// An optional error type, usually an Exception class type.
		/// </summary>
		public string? Type { get; set; }
	}
}
