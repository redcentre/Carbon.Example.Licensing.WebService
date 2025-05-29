namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class CreateStorageAccountRequest
	{
		public CreateStorageAccountRequest(string name, string resourceGroupName, string location, bool allowPublicBlobAccess)
		{
			Name = name;
			ResourceGroupName = resourceGroupName;
			Location = location;
			AllowPublicBlobAccess = allowPublicBlobAccess;
		}

		/// <summary>
		/// Storage Account Name.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Resource Group Name that will contain the account.
		/// </summary>
		public string ResourceGroupName { get; set; }
		/// <summary>
		/// The location (aka region) can be the short name like "westus" or the full name like "West US".
		/// </summary>
		public string Location { get; set; }
		/// <summary>
		/// True to allow public access to blobs in containers in the account.
		/// </summary>
		public bool AllowPublicBlobAccess { get; set; } = true;
	}
}
