namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class UpdateAccountRequest
	{
		public UpdateAccountRequest()
		{
		}

		public UpdateAccountRequest(string userId, string userName, string? email, string? comment)
		{
			UserId = userId;
			UserName = userName;
			Email = email;
			Comment = comment;
		}

		public string UserId { get; set; }
		public string UserName { get; set; }
		public string? Email { get; set; }
		public string? Comment { get; set; }
	}
}
