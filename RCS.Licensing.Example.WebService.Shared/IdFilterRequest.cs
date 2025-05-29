using System.Collections.Generic;
using System.Linq;

namespace RCS.Licensing.Example.WebService.Shared
{
	public sealed class IdFilterRequest
	{
		public IdFilterRequest()
		{
		}

		public IdFilterRequest(IEnumerable<string>? ids)
		{
			Ids = ids?.ToArray();
		}

		public string[]? Ids { get; set; }
	}
}
