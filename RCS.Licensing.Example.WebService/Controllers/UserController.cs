using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Example.Provider;
using RCS.Licensing.Example.WebService.Shared;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Tags("User")]
[Route("user")]
[TypeFilter(typeof(StandardActionFilterAttribute))]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class UserController : LicensingControllerBase
{
	public UserController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<int>> InnerPasswordChange(PasswordChangeRequest request, bool canThrow)
	{
		try
		{
			int count = await Licprov.ChangePassword(request.UserId, request.OldPassword, request.NewPassword);
			return new ResponseWrap<int>(count);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<int>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<string?>> InnerPasswordChangePlea(string userId)
	{
		var user = await Licprov.ReadUser(userId);
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// Verify the user and email value exist.
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		if (user == null)
		{
			return new ResponseWrap<string?>(1, $"User Id {userId} found");
		}
		string email = user.Email ?? user.Name;
		if (!AppUtility.IsValidEmail(email))
		{
			return new ResponseWrap<string?>(11, $"User Id {userId} does not define an email address in the Name or Email values.");
		}
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// Verify required config values are defined.
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		string? twilkey = Config["LicensingService:TwilioKey"];
		string? twilfrom = Config["LicensingService:TwilioFromEmail"];
		string? twilfromname = Config["LicensingService:TwilioFromName"];
		string? templateUri = Config["LicensingService:ChangePasswordEmailTemplateUri"];
		if (string.IsNullOrEmpty(twilkey) || string.IsNullOrEmpty(twilfromname) || string.IsNullOrEmpty(twilfromname) || string.IsNullOrEmpty(templateUri))
		{
			return new ResponseWrap<string?>(12, $"Configuration does not define email key, from address, from name and body template Uri.");
		}
		var pproc = await GetPleaProcAsync();
		if (pproc == null)
		{
			return new ResponseWrap<string?>(13, $"Configuration does not define storage values for 2FA processing.");
		}
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// Attempt to add a 2FA plea, fill the email body and send it.
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		string pleaId = await pproc.AddItem("PasswordChange", userId);
		var client = new SendGridClient(twilkey);
		var msg = new SendGridMessage()
		{
			From = new EmailAddress(twilfrom, twilfromname),
			Subject = $"Change password request"
		};
		using (var http = new HttpClient())
		{
			string body = await http.GetStringAsync(templateUri);
			msg.HtmlContent = body.Replace("$(USERNAME)", user.Name).Replace("$(REQUESTID)", pleaId);
		}
		msg.AddTo(email);
		//msg.AddBcc(from);
		var response = await client.SendEmailAsync(msg);
		if (response?.StatusCode != HttpStatusCode.Accepted)
		{
			return new ResponseWrap<string?>(14, $"User Id {userId} email send status {response?.StatusCode}");
		}
		string? msgid = response.Headers.FirstOrDefault(h => h.Key == "X-Message-Id").Value.FirstOrDefault();
		return await Task.FromResult(new ResponseWrap<string?>(pleaId));
	}

	async Task<ResponseWrap<PleaItem?>> InnerGetPlea(string id)
	{
		var pproc = await GetPleaProcAsync();
		if (pproc == null)
		{
			return new ResponseWrap<PleaItem?>(1, $"Plea Id {id} cannot be retrieved due to service configuration errors");
		}
		var item = await pproc.GetItem(id);
		if (item == null)
		{
			return new ResponseWrap<PleaItem?>(1, $"Plea Id {id} not found or expired");
		}
		return new ResponseWrap<PleaItem?>(item);
	}

	async Task<ResponseWrap<int>> InnerUpdateAccount(UpdateAccountRequest request, bool canThrow)
	{
		try
		{
			int count = await Licprov.UpdateAccount(request.UserId, request.UserName, request.Comment, request.Email);
			return new ResponseWrap<int>(count);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<int>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<UserPick[]>> InnerListUserPicksForRealms(IdFilterRequest request)
	{
		var picks = await Licprov.ListUserPicksForRealms(request.Ids);
		string rjoin = request.Ids == null ? "NULL" : string.Join(",", request.Ids);
		return new ResponseWrap<UserPick[]>(picks);
	}

	async Task<ResponseWrap<UpsertResult<User>>> InnerCreateUser(User request)
	{
		request.Id = null;
		var result = (await Licprov.UpsertUser(request))!;
		var user = result.Entity;
		return new ResponseWrap<UpsertResult<User>>(result);
	}

	async Task<ResponseWrap<User?>> InnerReadUser(string id)
	{
		var user = await Licprov.ReadUser(id);
		if (user == null)
		{
			return new ResponseWrap<User?>(1, $"User Id {id} found");
		}
		return new ResponseWrap<User?>(user);
	}

	async Task<ResponseWrap<User[]>> InnerListUsers()
	{
		var list = await Licprov.ListUsers();
		return new ResponseWrap<User[]>(list);
	}

	async Task<ResponseWrap<User[]>> InnerListUsers(IdFilterRequest request)
	{
		var list = await Licprov.ListUsers(request.Ids);
		string ids = request.Ids == null ? "NULL" : "[" + string.Join(",", request.Ids) + "]";
		return new ResponseWrap<User[]>(list);
	}

	async Task<ResponseWrap<UpsertResult<User>>> InnerUpsertUser(User user)
	{
		var result = await Licprov.UpsertUser(user);
		var upuser = result.Entity;
		if (upuser == null)
		{
			string msg = $"User Id {user.Id} not found";
			return new ResponseWrap<UpsertResult<User>>(1,msg);
		}
		return new ResponseWrap<UpsertResult<User>>(result);
	}

	async Task<ResponseWrap<int>> InnerDeleteUser(string id)
	{
		int count = await Licprov.DeleteUser(id);
		return new ResponseWrap<int>(count);
	}

	async Task<ResponseWrap<User?>> InnerDisconnectUserChildCustomer(string userId, string customerId)
	{
		var user = await Licprov.DisconnectUserChildCustomer(userId, customerId);
		return new ResponseWrap<User?>(user!);
	}

	async Task<ResponseWrap<User?>> InnerConnectUserChildCustomers(JoinsRequest request)
	{
		var user = await Licprov.ConnectUserChildCustomers(request.ParentId, request.ChildIds);
		if (user == null) return new ResponseWrap<User?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		return new ResponseWrap<User?>(user);
	}

	async Task<ResponseWrap<User?>> InnerReplaceUserChildCustomers(JoinsRequest request)
	{
		var user = await Licprov.ReplaceUserChildCustomers(request.ParentId, request.ChildIds);
		if (user == null) return new ResponseWrap<User?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		return new ResponseWrap<User?>(user);
	}

	async Task<ResponseWrap<User?>> InnerDisconnectUserChildJob(string userId, string jobId)
	{
		var user = await Licprov.DisconnectUserChildJob(userId, jobId);
		return new ResponseWrap<User?>(user!);
	}

	async Task<ResponseWrap<User?>> InnerConnectUserChildJobs(JoinsRequest request)
	{
		var user = await Licprov.ConnectUserChildJobs(request.ParentId, request.ChildIds);
		if (user == null) return new ResponseWrap<User?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		return new ResponseWrap<User?>(user);
	}

	async Task<ResponseWrap<User?>> InnerReplaceUserChildJobs(JoinsRequest request)
	{
		var user = await Licprov.ReplaceUserChildJobs(request.ParentId, request.ChildIds);
		if (user == null) return new ResponseWrap<User?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		return new ResponseWrap<User?>(user);
	}

	async Task<ResponseWrap<User?>> InnerDisconnectUserChildRealm(string userId, string realmId)
	{
		var user = await Licprov.DisconnectUserChildRealm(userId, realmId);
		return new ResponseWrap<User?>(user!);
	}

	async Task<ResponseWrap<User?>> InnerConnectUserChildRealms(JoinsRequest request)
	{
		var user = await Licprov.ConnectUserChildRealms(request.ParentId, request.ChildIds);
		if (user == null) return new ResponseWrap<User?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		return new ResponseWrap<User?>(user);
	}

	async Task<ResponseWrap<User?>> InnerReplaceUserChildRealms(JoinsRequest request)
	{
		DumpObj(request, "InnerReplaceUserChildRealms request");
		var user = await Licprov.ReplaceUserChildRealms(request.ParentId, request.ChildIds);
		DumpObj(request, "ReplaceUserChildRealms return");
		if (user == null) return new ResponseWrap<User?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		return new ResponseWrap<User?>(user);
	}
}
