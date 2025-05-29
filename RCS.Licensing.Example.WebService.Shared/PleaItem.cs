using System;
using System.ComponentModel.DataAnnotations;

namespace RCS.Licensing.Example.WebService.Shared;

/// <summary>
/// Contains the data for a 'plea' for authenticated work.
/// </summary>
public sealed class PleaItem
{
	public PleaItem()
	{
	}

	public PleaItem(string id, DateTime created, string type, string data)
	{
		Id = id;
		Created = created;
		Type = type;
		Data = data;
	}

	/// <summary>
	/// The Id of the plea. The value is strongly random to prevent guessing attacks.
	/// </summary>
	[Required]
	public string Id { get; set; }
	/// <summary>
	/// The UTC time the plea was created.
	/// </summary>
	[Required]
	public DateTime Created { get; set; }
	/// <summary>
	/// The plea type is expected to be a known string that defines the work requested in the plea.
	/// The type is part of the contract between the application making the plea and the one that
	/// will process it.
	/// </summary>
	[Required]
	public string Type { get; set; }
	/// <summary>
	/// Arbitrary data asociated with the plea. The meaning of the data will be specific to different plea types.
	/// </summary>
	public string Data { get; set; }
}
