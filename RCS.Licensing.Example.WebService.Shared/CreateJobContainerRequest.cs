namespace RCS.Licensing.Example.WebService.Shared
{
	/// <summary>
	/// This enum exactly matches the Azure library enum PublicAccessType.
	/// 0=None 1=BlobContainer 2=Blob
	/// </summary>
	public enum JobContainerAccessType
	{
		/// <summary>
		/// No public access.
		/// </summary>
		None,
		/// <summary>
		/// Access to blobs and the container.
		/// </summary>
		BlobContainer,
		/// <summary>
		/// Access to blobs only.
		/// </summary>
		Blob
	}

	public sealed class CreateJobContainerRequest
	{
		public CreateJobContainerRequest(string jobId, JobContainerAccessType accessType)
		{
			JobId = jobId;
			AccessType = accessType;
		}

		/// <summary>
		/// Job Container Name.
		/// </summary>
		public string JobId { get; set; }
		/// <summary>
		/// Resource Group Name that will contain the account.
		/// </summary>
		public JobContainerAccessType AccessType { get; set; }
	}
}
