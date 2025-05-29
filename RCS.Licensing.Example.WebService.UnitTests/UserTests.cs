using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RCS.Licensing.Example.WebService.UnitTests;

[TestClass]
public class UserTests : TestBase
{
	[TestMethod]
	public async Task U100_List_User_Picks()
	{
		await WrapClientLoginName(async (client) =>
		{
			var picks = await client.ListUserPicksForRealms();
			DumpObject(picks);
		});
	}
}