using System.Text.RegularExpressions;

namespace RCS.Licensing.Example.WebService;

static class AppUtility
{
	public static bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[\w-]+(\.[\w-]+)*@([a-z0-9-]+(\.[a-z0-9-]+)*?\.[a-z]{2,6}|(\d{1,3}\.){3}\d{1,3})(:\d{4})?$");
}

