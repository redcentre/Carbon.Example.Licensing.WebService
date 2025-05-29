namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class JoinsRequest
	{
		public JoinsRequest()
		{
		}

		public JoinsRequest(string parentId, params string[] childIds)
		{
			ParentId = parentId;
			ChildIds = childIds;
		}

		public string ParentId { get; set; }
		public string[] ChildIds { get; set; }
	}
}
