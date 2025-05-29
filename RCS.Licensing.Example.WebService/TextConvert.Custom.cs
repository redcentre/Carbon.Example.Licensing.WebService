using System.Linq;
using RCS.Licensing.Provider.Shared;
using Lines = System.Collections.Generic.List<string>;

namespace RCS.Licensing.Example.WebService;

partial class TextConvert
{
	/// <summary>
	/// Custom text serialization of a licence authentication response which contains nested class properties.
	/// </summary>
	static void SerializeData(LicenceFull value, Lines lines)
	{
		lines.Add($"{nameof(LicenceFull.Name)}={value.Name}");
		lines.Add($"{nameof(LicenceFull.Id)}={value.Id}");
		lines.Add($"{nameof(LicenceFull.Email)}={value.Email}");
		lines.Add($"{nameof(LicenceFull.Comment)}={value.Comment}");
		lines.Add($"{nameof(LicenceFull.Filter)}={value.Filter}");
		lines.Add($"{nameof(LicenceFull.LoginCount)}={value.LoginCount}");
		lines.Add($"{nameof(LicenceFull.LoginMax)}={value.LoginMax}");
		lines.Add($"{nameof(LicenceFull.LoginMacs)}={value.LoginMacs}");
		lines.Add($"{nameof(LicenceFull.LastLogin)}={value.LastLogin:s}");
		lines.Add($"{nameof(LicenceFull.Sequence)}={value.Sequence}");
		lines.Add($"{nameof(LicenceFull.Created)}={value.Created:s}");
		lines.Add($"{nameof(LicenceFull.Sunset)}={value.Sunset:s}");
		lines.Add($"{nameof(LicenceFull.DaysRemaining)}={value.DaysRemaining}");
		lines.Add($"{nameof(LicenceFull.Version)}={value.Version}");
		lines.Add($"{nameof(LicenceFull.MinVersion)}={value.MinVersion}");
		lines.Add($"{nameof(LicenceFull.DataLocation)}={value.DataLocation}");
		lines.Add($"{nameof(LicenceFull.Recovered)}={value.Recovered}");
		lines.Add($"{nameof(LicenceFull.SessionId)}={value.SessionId}");
		lines.Add($"{nameof(LicenceFull.EntityId)}={value.EntityId}");
		lines.Add($"{nameof(LicenceFull.EntityName)}={value.EntityName}");
		lines.Add($"{nameof(LicenceFull.EntityType)}={value.EntityType}");
		lines.Add($"{nameof(LicenceFull.EntityLogo)}={value.EntityLogo}");
		string? join = value.GuestJobs == null ? null : string.Join(',', value.GuestJobs.Select(x => x.JobName));
		lines.Add($"{nameof(LicenceFull.GuestJobs)}={join}");
		join = value.Roles == null ? null : string.Join(',', value.Roles);
		lines.Add($"{nameof(LicenceFull.Roles)}={join}");
		join = value.Realms == null ? null : string.Join(',', value.Realms.Select(r => r.Name));
		lines.Add($"{nameof(LicenceFull.Realms)}={join}");
		join = value.CloudCustomerNames == null ? null : string.Join(',', value.CloudCustomerNames);
		lines.Add($"{nameof(LicenceFull.CloudCustomerNames)}={join}");
		join = value.CloudJobNames == null ? null : string.Join(',', value.CloudJobNames);
		lines.Add($"{nameof(LicenceFull.CloudJobNames)}={join}");
		join = value.VartreeNames == null ? null : string.Join(',', value.VartreeNames);
		lines.Add($"{nameof(LicenceFull.VartreeNames)}={join}");
		join = value.DashboardNames == null ? null : string.Join(',', value.DashboardNames);
		lines.Add($"{nameof(LicenceFull.DashboardNames)}={join}");
		lines.Add("# Customers, Jobs and Vartrees are normally a 3 level tree.");
		lines.Add("# For the text response they are flattened into their logical node walking sequence.");
		lines.Add("# ║ Customer[cix]=Id,Name,DisplayName");
		lines.Add("# ║ Job[jix]=Id,Name,DisplayName,Description");
		lines.Add("# ║ VartreeNames=V1,V2,V3,etc");
		lines.Add("# ║ RealCloudVartreeNames=V1,V2,V3,etc");
		foreach (var ctup in value.Customers.Select((c, ci) => new { c, ci }))
		{
			lines.Add($"Customer[{ctup.ci}]={ctup.c.Id},{ctup.c.Name},{ctup.c.DisplayName},{ctup.c.StorageKey}");
			foreach (var jtup in ctup.c.Jobs.Select((j, ji) => new { j, ji }))
			{
				lines.Add($"Job[{jtup.ji}]={jtup.j.Id},{jtup.j.Name},{jtup.j.DisplayName},{jtup.j.Description}");
				join = jtup.j.VartreeNames == null ? null : string.Join(',', jtup.j.VartreeNames);
				lines.Add($"VartreeNames={join}");
				join = jtup.j.RealCloudVartreeNames == null ? null : string.Join(',', jtup.j.RealCloudVartreeNames);
				lines.Add($"RealCloudVartreeNames={join}");
			}
		}
	}

	//static void SerializeToLines(AuthenticateResponse response, Lines lines)
	//{
	//	BasicObjectToLines(response, lines);
	//	if (response.Customers?.Length > 0)
	//	{
	//		lines.Add("# | Cloud Customers, Jobs and Vartrees that the user is permitted to access.");
	//		lines.Add("# | Customer line format:");
	//		lines.Add("# | |  Name,Id,DisplayName,AgencyId,Comment");
	//		lines.Add("# | Child job line format:");
	//		lines.Add("# | | Name,Id,DisplayName,Vartrees (Vartree names are joined with +)");
	//		foreach (AuthenticateCustomer cust in response.Customers)
	//		{
	//			WriteValues(lines, "Customer", cust.Name, cust.Id, cust.DisplayName, cust.AgencyId, cust.Comment);
	//			foreach (AuthenticateJob job in cust.Jobs)
	//			{
	//				string vtjoin = string.Join("+", job.VartreeNames);
	//				WriteValues(lines, "Job", job.Name, job.Id, job.DisplayName, vtjoin);
	//			}
	//		}
	//		string guestjoin = string.Join(",", response.GuestJobs.Select(x => $"{x.JobName}.{x.Variable}"));
	//		lines.Add($"GuestJobs={guestjoin}");
	//	}
	//}

	//static void SerializeToLines(ContainerListResponse response, Lines lines)
	//{
	//	var dashroot = response.ListElement.Element("dashboards");
	//	foreach (var elem in dashroot!.Elements("dashboard"))
	//	{
	//		string name = (string)elem.Attribute("name")!;
	//		long? bytes = (long?)elem.Element("bytes");
	//		string? modified = (string?)elem.Element("modified");
	//		lines.Add($"Dashboard,{Path.GetFileNameWithoutExtension(name)},{bytes},{modified}");
	//	}
	//	var vtroot = response.ListElement.Element("vartrees");
	//	if (vtroot != null)
	//	{
	//		foreach (var elem in vtroot.Elements("vartree"))
	//		{
	//			string name = (string)elem.Attribute("name")!;
	//			long? bytes = (long?)elem.Element("bytes");
	//			string? modified = (string?)elem.Element("modified");
	//			lines.Add($"Vartree,{Path.GetFileNameWithoutExtension(name)},{bytes},{modified}");
	//		}
	//	}
	//	var reproot = response.ListElement.Element("reports");
	//	if (reproot != null)
	//	{
	//		foreach (var elem in reproot.DescendantsAndSelf())
	//		{
	//			string name = (string)elem.Attribute("name")!;
	//			if (elem.Name == "vdir")
	//			{
	//				lines.Add($"Report,D,{name}");
	//			}
	//			else
	//			{
	//				var dirname = (string)elem.Parent!.Attribute("name")!;
	//				long? bytes = (long?)elem.Attribute("bytes");
	//				string? modified = (string?)elem.Attribute("modified");
	//				lines.Add($"Report,F,{dirname}{name},{bytes},{modified}");
	//			}
	//		}
	//	}
	//}

	//static void SerializeToLines(OrganisationNode[] orgroots, Lines lines)
	//{
	//	InnerWriteOrgNodes(0, orgroots, lines);
	//}

	//static void InnerWriteOrgNodes(int depth, IEnumerable<OrganisationNode> nodes, Lines lines)
	//{
	//	foreach (var node in nodes)
	//	{
	//		WriteValues(lines, "Type", node.DocType);
	//		if (node.Name != null)
	//		{
	//			lines.Add($"Name={node.Name}");
	//		}
	//		WriteValues(lines, "Depth", depth);
	//		if (node.Id != null)
	//		{
	//			lines.Add($"Id={node.Id}");
	//		}
	//		++depth;
	//		InnerWriteOrgNodes(depth, node.Children, lines);
	//		--depth;
	//	}
	//}

	//[GeneratedRegex(@"Metadata\.([a-z_]+?)=(.+)")]
	//private static partial Regex RegMetadata();
}
