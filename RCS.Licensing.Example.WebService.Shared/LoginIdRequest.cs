namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class LoginIdRequest
	{
		public LoginIdRequest()
		{
		}

		public LoginIdRequest(string userId, string password, bool skipCache = false)
		{
			UserId = userId;
			Password = password;
			SkipCache = skipCache;
		}

		public string UserId { get; set; }
		public string Password { get; set; }
		public bool SkipCache { get; set; }
	}
}
