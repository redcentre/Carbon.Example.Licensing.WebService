namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class FreeLicenceRequest
	{
		public FreeLicenceRequest()
		{
		}

		public FreeLicenceRequest(string clientIdentifier, bool skipCache = false)
		{
			ClientIdentifier = clientIdentifier;
			SkipCache = skipCache;
		}

		public string ClientIdentifier { get; set; }
		public bool SkipCache { get; set; }
	}
}
