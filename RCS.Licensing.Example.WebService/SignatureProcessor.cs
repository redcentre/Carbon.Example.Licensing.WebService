using System;
using System.IO.Hashing;
using System.Text.RegularExpressions;
using System.Text;

namespace RCS.Licensing.Example.WebService;

/// <summary>
/// Enumeration describing the result of the <see cref="SignatureProcessor.Validate(string, Func{string, bool})"/> method.
/// </summary>
public enum SignatureStatus
{
	/// <summary>
	/// The signature is valid.
	/// </summary>
	Success,
	/// <summary>
	/// The signature string is not correctly formatted.
	/// </summary>
	BadFormat,
	/// <summary>
	/// The signature has expired.
	/// </summary>
	Expired,
	/// <summary>
	/// The Id is not valid.
	/// </summary>
	BadId,
	/// <summary>
	/// The signature hash is incorrect.
	/// </summary>
	BadHash
}

/// <summary>
/// <para>
/// This experimental class creates and validates a moderately secure signed string
/// of data values. The full signed signature string can be used by this service to
/// implemenent roles based authorisation.</para>
/// <para>Forgery resistance is provided by the 8-byte hash and 2-bytes of salted entropy.
/// It is hoped that this non-cryptographic system provides 80-bits of flat search space that
/// would make it quite laborious to produce a forgery. Using a cryptographic signature would
/// produce a very long signature string.
/// </para>
/// </summary>
/// <remarks>
/// The signature string has the general format <c>T-V-HHHH-II-SS-HHHHHHHHHHHHHHHH</c> where the parts are:
/// Type, Version, Hours, Id, Salt, Hash. However, the signature is an opaque value to clients who simply
/// roundtrip it without knowing what it means.
/// </remarks>
public static class SignatureProcessor
{
	const char ProviderId = 'E';
	const int SignatureVersion = 1;
	static readonly DateTime SignatureBaseTime = new(2025, 5, 1);

	/// <summary>
	/// Creates a signature string for a specified Id that is valid for a specifeid number of hours.
	/// </summary>
	/// <param name="id">The arbitrary Id value to include in the signature. It will typically represent the Id
	/// of a licensing user account.</param>
	/// <param name="expiryHours">The number of hours the signature is valid.</param>
	/// <returns>A string containing some data values and a signature of the data.</returns>
	public static string Create(string id, int expiryHours)
	{
		double expireHour = DateTime.Now.Subtract(SignatureBaseTime).TotalHours + expiryHours;
		int expireNum = Convert.ToInt32(expireHour);
		string salt = Guid.NewGuid().ToString()[..2].ToUpperInvariant();
		string datajoin = $"{ProviderId}-{SignatureVersion}-{expireNum}-{id}-{salt}";
		return AppendHash(datajoin);
	}

	/// <summary>
	/// Validates a signature created by the <see cref="Create(string, int)"/> method.
	/// </summary>
	/// <param name="signature">The full signature string to verify.</param>
	/// <param name="callback">A callback delegate which the caller implements to verify
	/// if the Id in the signature is valid. The signature cannot determine if an Id is
	/// valid and must delegate that work to the calling application.</param>
	/// <returns>An enumeration of the validation result.</returns>
	public static SignatureStatus Validate(string signature, Func<string, bool> callback)
	{
		var m = Regex.Match(signature ?? "", @"^([A-Z])-(\d{1,3})-(\d{1,7})-(.+?)-([0-9A-F]{2})-([0-9A-F]{16})$");
		if (!m.Success) return SignatureStatus.BadFormat;
		char providerId = m.Groups[1].Value[0];
		int version = int.Parse(m.Groups[2].Value);
		int expireNum = int.Parse(m.Groups[3].Value);
		string id = m.Groups[4].Value;
		string salt = m.Groups[5].Value;
		string hash = m.Groups[6].Value;
		double nowHours = DateTime.Now.Subtract(SignatureBaseTime).TotalHours;
		int nowNum = Convert.ToInt32(nowHours);
		if (nowNum >= expireNum) return SignatureStatus.Expired;
		if (!callback(id)) return SignatureStatus.BadId;
		string datajoin = $"{providerId}-{version}-{expireNum}-{id}-{salt}";
		string sigcheck = AppendHash(datajoin);
		return string.Compare(sigcheck, sigcheck, StringComparison.Ordinal) == 0 ? SignatureStatus.Success : SignatureStatus.BadHash;
	}

	static string AppendHash(string dataJoin)
	{
		byte[] databuff = Encoding.UTF8.GetBytes(dataJoin);
		byte[] hashbuff = XxHash64.Hash(databuff, 6281787409036960971);
		ulong hash = BitConverter.ToUInt64(hashbuff, 0);
		return $"{dataJoin}-{hash:X16}";
	}
}