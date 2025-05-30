namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class PasswordChangeRequest
	{
		public PasswordChangeRequest()
		{
		}

		public PasswordChangeRequest(string userId, string oldPassword, string newPassword)
		{
			UserId = userId;
			OldPassword = oldPassword;
			NewPassword = newPassword;
		}

		public string UserId { get; set; }
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
	}
}
