using System.Net.Mime;
using System.Threading.Tasks;
using RCS.Licensing.Example.WebService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RCS.Licensing.Provider.Shared;
using RCS.Licensing.Provider.Shared.Entities;

namespace RCS.Licensing.Example.WebService.Controllers;

[ApiController]
[Route("job")]
[Tags("Job")]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Text.Plain)]
public partial class JobController : LicensingControllerBase
{
	public JobController(ILoggerFactory loggerFactory, IConfiguration configuration, ILicensingProvider licprov)
		: base(loggerFactory, configuration, licprov)
	{
	}

	async Task<ResponseWrap<Job?>> InnerReadJob(string id)
	{
		var job = await Licprov.ReadJob(id);
		Info($"Read job {id} -> {job?.Name}");
		if (job == null)
		{
			return new ResponseWrap<Job?>(1, "Not found");
		}
		return new ResponseWrap<Job?>(job);
	}

	async Task<ResponseWrap<Job[]>> InnerListJobs()
	{
		var list = await Licprov.ListJobs();
		return new ResponseWrap<Job[]>(list);
	}

	async Task<ResponseWrap<UpsertResult<Job>>> InnerUpsertJob(Job job)
	{
		var result = await Licprov.UpsertJob(job);
		return new ResponseWrap<UpsertResult<Job>>(result);
	}

	async Task<ResponseWrap<string[]>> InnerValidateJob(string id)
	{
		var errors = await Licprov.ValidateJob(id);
		return new ResponseWrap<string[]>(errors);
	}

	async Task<ResponseWrap<int>> InnerDeleteJob(string id)
	{
		int count = await Licprov.DeleteJob(id);
		Info($"Delete job {id} -> {count}");
		return new ResponseWrap<int>(count);
	}

	async Task<ResponseWrap<Job?>> InnerDisconnectJobChildUser(string jobid, string userId)
	{
		var job = await Licprov.DisconnectJobChildUser(jobid, userId);
		if (job == null) return new ResponseWrap<Job?>(1, "Not found");
		Info($"DisconnectJobChildUser {jobid} from {userId} -> {job}");
		return new ResponseWrap<Job?>(job);
	}

	async Task<ResponseWrap<Job?>> InnerConnectJobChildUsers([FromBody] JoinsRequest request)
	{
		var job = await Licprov.ConnectJobChildUsers(request.ParentId, request.ChildIds);
		if (job == null) return new ResponseWrap<Job?>(1, "Not found");
		string ujoin = string.Join(",", request.ChildIds);
		Info($"ConnectJobUsers {request.ParentId} to [{ujoin}]) -> {job}");
		return new ResponseWrap<Job?>(job);
	}

	async Task<ResponseWrap<Job?>> InnerReplaceJobChildUsers(JoinsRequest request)
	{
		var job = await Licprov.ReplaceJobChildUsers(request.ParentId, request.ChildIds);
		if (job == null) return new ResponseWrap<Job?>(1, "Not found");
		string rjoin = string.Join(",", request.ChildIds);
		Info($"ReplaceJobUserJoins {request.ParentId} to [{rjoin}]) -> {job}");
		return new ResponseWrap<Job?>(job);
	}

	async Task<ResponseWrap<string>> InnerCompareJobsAndContainers()
	{
		var element = await Licprov.CompareJobsAndContainers();
		var xml = element.ToString();
		return new ResponseWrap<string>(xml);
	}

	async Task<ResponseWrap<bool>> InnerCreateJobContainer(CreateJobContainerRequest request)
	{
		var job = await Licprov.ReadJob(request.JobId);
		if (job == null) return new ResponseWrap<bool>(1, "Job not found");
		if (job.CustomerId == null) return new ResponseWrap<bool>(2, $"Job Id {request.JobId} does not have a parent customer.");
		var cust = await Licprov.ReadCustomer(job.CustomerId);
		if (cust == null) return new ResponseWrap<bool>(3, $"Job id {request.JobId} parent customer Id {job.CustomerId} not found");
		if (cust.StorageKey == null) return new ResponseWrap<bool>(4, $"Job id {request.JobId} parent customer Id {job.CustomerId} does not have a Storage Account key value");
		var man = new RCS.Azure.StorageAccount.StorageAccountUtility(cust.StorageKey);
		bool created = await man.CreateContainer(job.Name);
		if (!created) return new ResponseWrap<bool>(5, $"Failed to create container {job.Name}. This generic error most commonly occurs when the container already exists.");
		await man.UpdateContainerMetadata(job.Name, (int)request.AccessType, null);
		return new ResponseWrap<bool>(true);
	}
}
