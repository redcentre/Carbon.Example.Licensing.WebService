namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class AuthenticateNameRequest
	{
		public AuthenticateNameRequest()
		{
		}

		public AuthenticateNameRequest(string userName, string password, bool skipCache = false)
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
