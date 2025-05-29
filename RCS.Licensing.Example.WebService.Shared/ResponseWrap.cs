namespace RCS.Licensing.Example.WebService.Shared
{
	/// <summary>
	/// All responses from the licensing service are an instance of this generic class.
	/// </summary>
	/// <typeparam name="T">The type of data in the response.</typeparam>
	public class ResponseWrap<T>
	{
		public ResponseWrap()
		{
		}

		public ResponseWrap(T data)
		{
			Data = data;
		}

		public ResponseWrap(int code, string? message, string? detail = null)
		{
			Code = code;
			Message = message;
			Detail = detail;
		}

		/// <summary>
		/// An error code which is required when <c>HasError</c> is True. The value should be unique
		/// for different types of error conditions.
		/// </summary>
		public int? Code { get; set; }
		/// <summary>
		/// A summary error message which is required when <c>HasError</c> is True.
		/// </summary>
		public string? Message { get; set; }
		/// <summary>
		/// A optional error details message which may be present when <c>HasError</c> is True.
		/// </summary>
		public string? Detail { get; set; }
		/// <summary>
		/// A flag indicating if the response is an error or not. If the value is True then values are
		/// expected in <c>Code</c> and <c>Message</c> to describe the error condition.
		/// </summary>
		public bool HasError => Code != null;
		/// <summary>
		/// Arbitrary data expected to be returned when <c>HasError</c> is False. The value may be a scalar
		/// value such as a number or string, or it may be a class which will generate JSON in different
		/// shapes according to the class type.
		/// </summary>
		public T Data { get; set; }
	}
}
