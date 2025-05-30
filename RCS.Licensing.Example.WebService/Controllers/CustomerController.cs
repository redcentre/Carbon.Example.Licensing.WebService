using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Azure.StorageAccount.Shared;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;
using RCS.Licensing.Example.Provider;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Route("customer")]
[Tags("Customer")]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class CustomerController : LicensingControllerBase
{
	public CustomerController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<Customer[]>> InnerListCustomers()
	{
		var list = await Licprov.ListCustomers();
		return new ResponseWrap<Customer[]>(list);
	}

	async Task<ResponseWrap<Customer[]>> InnerListCustomers(IdFilterRequest request)
	{
		var list = await Licprov.ListCustomers(request.Ids ?? []);
		return new ResponseWrap<Customer[]>(list);
	}

	async Task<ResponseWrap<UpsertResult<Customer>>> InnerUpsertCustomer(Customer customer)
	{
		var result = await Licprov.UpsertCustomer(customer);
		return new ResponseWrap<UpsertResult<Customer>>(result);
	}

	async Task<ResponseWrap<string[]>> InnerValidateCustomer(string id)
	{
		var errors = await Licprov.ValidateCustomer(id);
		return new ResponseWrap<string[]>(errors);
	}

	async Task<ResponseWrap<CustomerPick[]>> InnerListCustomerPicksForRealms(IdFilterRequest request)
	{
		var picks = await Licprov.ListCustomerPicksForRealms(request.Ids);
		string rjoin = request.Ids == null ? "NULL" : string.Join(",", request.Ids);
		Info($"{RequestCount} ListCustomerPicksForRealms([{rjoin}]) -> {picks.Length}");
		return new ResponseWrap<CustomerPick[]>(picks);
	}

	async Task<ResponseWrap<int>> InnerDeleteCustomer(string id)
	{
		int count = await Licprov.DeleteCustomer(id);
		Info($"Delete customer {id} -> {count}");
		return new ResponseWrap<int>(count);
	}

	async Task<ResponseWrap<Customer?>> InnerReadCustomer(string id)
	{
		var customer = await Licprov.ReadCustomer(id);
		Info($"Read customer {id} -> {customer?.Name}");
		if (customer == null)
		{
			return new ResponseWrap<Customer?>(1, "Not found");
		}
		return new ResponseWrap<Customer?>(customer);
	}

	async Task<ResponseWrap<Customer[]>> InnerReadCustomersByName(string name)
	{
		var customers = await Licprov.ReadCustomersByName(name);
		Info($"Read customer name {name} -> {customers.Length}");
		return new ResponseWrap<Customer[]>(customers);
	}

	async Task<ResponseWrap<Customer?>> InnerDisconnectCustomerChildJob(string jobId, string customerId, bool canThrow)
	{
		try
		{
			var customer = await Licprov.DisconnectCustomerChildJob(jobId, customerId);
			Info($"DisconnectJobCustomers {jobId} from {customerId} -> {customer}");
			return new ResponseWrap<Customer?>(customer!);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<Customer?>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<Customer?>> InnerConnectCustomerChildJobs(JoinsRequest request, bool canThrow)
	{
		try
		{
			var cust = await Licprov.ConnectCustomerChildJobs(request.ParentId, request.ChildIds);
			if (cust == null) return new ResponseWrap<Customer?>(1, "Not found");
			string ujoin = string.Join(",", request.ChildIds);
			Info($"ConnectCustomerChildJobs {request.ParentId} to [{ujoin}]) -> {cust}");
			return new ResponseWrap<Customer?>(cust);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<Customer?>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<Customer?>> InnerReplaceCustomerChildJobs(JoinsRequest request, bool canThrow)
	{
		try
		{
			var cust = await Licprov.ReplaceCustomerChildJobs(request.ParentId, request.ChildIds);
			if (cust == null) return new ResponseWrap<Customer?>(1, "Not found");
			string rjoin = string.Join(",", request.ChildIds);
			Info($"SetCustomerJobJoins {request.ParentId} to [{rjoin}]) -> {cust}");
			return new ResponseWrap<Customer?>(cust!);
		}
		catch (ExampleLicensingException ex)
		{
			return new ResponseWrap<Customer?>((int)ex.ErrorType, ex.Message);
		}
	}

	async Task<ResponseWrap<Customer?>> InnerDisconnectCustomerChildUser(string customerId, string userId)
	{
		var cust = await Licprov.DisconnectCustomerChildUser(customerId, userId);
		if (cust == null) return new ResponseWrap<Customer?>(1, "Not found");
		Info($"DisconnectCustomerChildUser {customerId} from {userId} -> {cust}");
		return new ResponseWrap<Customer?>(cust);
	}

	async Task<ResponseWrap<Customer?>> InnerConnectCustomerChildUsers(JoinsRequest request)
	{
		var cust = await Licprov.ConnectCustomerChildUsers(request.ParentId, request.ChildIds);
		if (cust == null) return new ResponseWrap<Customer?>(1, "Not found");
		string ujoin = string.Join(",", request.ChildIds);
		Info($"ConnectCustomerUsers {request.ParentId} to [{ujoin}]) -> {cust}");
		return new ResponseWrap<Customer?>(cust);
	}

	async Task<ResponseWrap<Customer?>> InnerReplaceCustomerChildUsers(JoinsRequest request)
	{
		var cust = await Licprov.ReplaceCustomerChildUsers(request.ParentId, request.ChildIds);
		if (cust == null) return new ResponseWrap<Customer?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		Info($"ReplaceCustomerChildUsers {request.ParentId} to [{rjoin}]) -> {cust}");
		return new ResponseWrap<Customer?>(cust);
	}

	async Task<ResponseWrap<SubscriptionAccount[]>> InnerListStorageAccounts()
	{
		var accounts = await SubscriptionUtil.ListAccounts().ToArrayAsync();
		Info($"ListStorageAccounts -> {accounts.Length}");
		return new ResponseWrap<SubscriptionAccount[]>(accounts);
	}

	async Task<ResponseWrap<bool?>> InnerIsStorageAccountNameAvailable(string name)
	{
		bool? result = await SubscriptionUtil.IsStorageAccountNameAvailable(name);
		Info($"IsStorageAccountNameAvailable({name}) -> {result}");
		return new ResponseWrap<bool?>(result);
	}

	async Task<ResponseWrap<SubscriptionAccount?>> InnerCreateStorageAccount([FromBody] CreateStorageAccountRequest request)
	{
		SubscriptionAccount? result = await SubscriptionUtil.CreateStorageAccount(request.Name, request.ResourceGroupName, request.Location, request.AllowPublicBlobAccess);
		Info($"CreateStorageAccount({request.Name},{request.ResourceGroupName},{request.Location},{request.AllowPublicBlobAccess}) -> {result?.ConnectionString}");
		return new ResponseWrap<SubscriptionAccount?>(result);
	}
}
