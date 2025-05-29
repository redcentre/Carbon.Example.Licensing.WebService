namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class LoginNameRequest
	{
		public LoginNameRequest()
		{
		}

		public LoginNameRequest(string userName, string password, bool skipCache = false)
		{
			UserName = userName;
			Password = password;
			SkipCache = skipCache;
		}

		public string UserName { get; set; }
		public string Password { get; set; }
		public bool SkipCache { get; set; }
	}
}
