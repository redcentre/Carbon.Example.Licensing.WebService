using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RCS.Licensing.Example.WebService.Shared
{
	public enum WorkerType
	{
		Import
	}

	public enum WorkerStatus
	{
		Created,
		Started,
		Cancel,
		Fail,
		Complete,
		Lost
	}

	/// <summary>
	/// Contains the current state of a worker and is saved as a collection of worker history.
	/// </summary>
	public sealed class WorkerInfo
	{
		public WorkerInfo()
		{
		}

		public WorkerInfo(WorkerType type, int id, string customerName, string jobName, string userName, string description)
		{
			Type = type;
			Id = id;
			CustomerName = customerName;
			JobName = jobName;
			UserName = userName;
			Description = description;
			CreatedTime = DateTime.UtcNow;
			Properties = new Dictionary<string, string>();
		}
		public WorkerType Type { get; set; }
		public int Id { get; set; }
		public string CustomerName { get; set; }
		public string JobName { get; set; }
		public string UserName { get; set; }
		public string Description { get; set; }
		public WorkerStatus Status { get; set; }
		public DateTime CreatedTime { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public string? FailMessage { get; set; }
		public string? FailStack { get; set; }
		public double? Percent { get; set; }
		public string? Feedback { get; set; }
		public Dictionary<string, string> Properties { get; set; }
		[JsonIgnore]
		public TimeSpan Elapsed => StartTime != null && EndTime != null ? EndTime.Value.Subtract(StartTime.Value) : DateTime.UtcNow.Subtract(StartTime ?? CreatedTime);
	}
}