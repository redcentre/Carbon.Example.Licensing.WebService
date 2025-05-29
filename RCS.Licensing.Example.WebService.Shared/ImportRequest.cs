namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class ImportRequest
	{
		public ImportRequest(string customerName, string jobName, string userName, string description, string uri, bool overwrite, bool zeroBased, bool keepSource, bool tryBlend, int maxCases)
		{
			CustomerName = customerName;
			JobName = jobName;
			UserName = userName;
			Description = description;
			Uri = uri;
			Overwrite = overwrite;
			ZeroBased = zeroBased;
			KeepSource = keepSource;
			TryBlend = tryBlend;
			MaxCases = maxCases;
		}
		public string CustomerName { get; set; }
		public string JobName { get; set; }
		public string UserName { get; set; }
		public string Description { get; set; }
		public string Uri { get; set; }
		// The following are for the SAV import parameters.
		public bool Overwrite { get; set; } = true;
		public bool ZeroBased { get; set; }
		public bool KeepSource { get; set; } = true;
		public bool TryBlend { get; set; }
		public int MaxCases { get; set; } = -1;

	}
}
