using System.Net.Mime;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Route("realm")]
[Tags("Realm")]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class RealmController : LicensingControllerBase
{
	public RealmController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<Realm?>> InnerReadRealm(string realmId)
	{
		var realm = await Licprov.ReadRealm(realmId);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		return new ResponseWrap<Realm?>(realm!);
	}

	async Task<ResponseWrap<Realm[]>> InnerListRealms()
	{
		var realms = await Licprov.ListRealms();
		return new ResponseWrap<Realm[]>(realms!);
	}

	async Task<ResponseWrap<UpsertResult<Realm>>> InnerUpsertRealm(Realm realm)
	{
		var result = await Licprov.UpsertRealm(realm);
		return new ResponseWrap<UpsertResult<Realm>>(result);
	}

	async Task<ResponseWrap<string[]?>> InnerValidateRealm(string realmId)
	{
		var messages = await Licprov.ValidateRealm(realmId);
		return new ResponseWrap<string[]?>(messages);
	}

	async Task<ResponseWrap<int>> InnerDeleteRealm(string realmId)
	{
		int count = await Licprov.DeleteRealm(realmId);
		return new ResponseWrap<int>(count);
	}

	async Task<ResponseWrap<Realm?>> InnerDisconnectRealmChildCustomer(string realmId, string customerId)
	{
		var realm = await Licprov.DisconnectRealmChildCustomer(realmId, customerId);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		Info($"DisconnectCustomerRealms {realmId} from {customerId} -> {realm}");
		return new ResponseWrap<Realm?>(realm!);
	}

	async Task<ResponseWrap<Realm?>> InnerConnectRealmChildCustomers(JoinsRequest request)
	{
		var realm = await Licprov.ConnectRealmChildCustomers(request.ParentId, request.ChildIds);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		Info($"ReplaceRealmCustomerJoins {request.ParentId} to [{rjoin}]) -> {realm}");
		return new ResponseWrap<Realm?>(realm!);
	}

	async Task<ResponseWrap<Realm?>> InnerReplaceRealmChildCustomers(JoinsRequest request)
	{
		var realm = await Licprov.ReplaceRealmChildCustomers(request.ParentId, request.ChildIds);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		Info($"ReplaceRealmChildCustomers {request.ParentId} to [{rjoin}]) -> {realm}");
		return new ResponseWrap<Realm?>(realm!);
	}

	async Task<ResponseWrap<Realm?>> InnerDisconnectRealmChildUser(string realmId, string userId)
	{
		var realm = await Licprov.DisconnectRealmChildUser(realmId, userId);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		Info($"DisconnectUserRealms {realmId} from {userId} -> {realm}");
		return new ResponseWrap<Realm?>(realm!);
	}

	async Task<ResponseWrap<Realm?>> InnerConnectRealmChildUsers(JoinsRequest request)
	{
		var realm = await Licprov.ConnectRealmChildUsers(request.ParentId, request.ChildIds);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		Info($"ReplaceRealmUserJoins {request.ParentId} to [{rjoin}]) -> {realm}");
		return new ResponseWrap<Realm?>(realm!);
	}

	async Task<ResponseWrap<Realm?>> InnerReplaceRealmChildUsers(JoinsRequest request)
	{
		var realm = await Licprov.ReplaceRealmChildUsers(request.ParentId, request.ChildIds);
		if (realm == null) return new ResponseWrap<Realm?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		Info($"ReplaceRealmChildUsers {request.ParentId} to [{rjoin}]) -> {realm}");
		return new ResponseWrap<Realm?>(realm!);
	}
}
