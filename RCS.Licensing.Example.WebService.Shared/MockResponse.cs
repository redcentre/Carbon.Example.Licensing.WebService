namespace RCS.Licensing.Example.WebService.Shared
{
	/// <summary>
	/// This class is only used for testing and sending back generic error responses.
	/// </summary>
	public sealed class MockResponse
	{
		public MockResponse()
		{
		}

		public MockResponse(string? message)
		{
			Message = message;
		}

		public string? Message { get; set; }
	}
}
