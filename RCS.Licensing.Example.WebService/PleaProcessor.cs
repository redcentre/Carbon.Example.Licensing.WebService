using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Azure.Storage.Blobs;
using RCS.Licensing.Example.WebService.Shared;

namespace RCS.Licensing.Example.WebService;

/// <summary>
/// Encapsulates processing of 'pleas'. A plea is a request to perform some licensing action that requires two-factor
/// authentication. Processing proceeds like this:
/// <list type="number">
/// <item><description>
/// A request is made in this service to do something that requires 2FA with the user (change password for example).
/// The service cannot processes the request immedaitely because external user confirmation is required.
/// </description></item>
/// <item><description>
/// The 'plea' to do something is saved in an XML Blob with an Id and an email is sent to the user. The body of the email is taken
/// from a template file in a uri specified in configuration.
/// </description></item>
/// <item><description>
/// The user receives the email and clicks a link in the body that points to some external web app that can process the plea.
/// </description></item>
/// <item><description>
/// The external web app makes a callback to this service to get the plea details, verify their contents and perform the actual
/// plea work originally requested.
/// </description></item>
/// </list>
/// </summary>
public sealed class PleaProcessor
{
	PleaProcessor(string storageConnect, string containerName, string blobname)
	{
		_connect = storageConnect;
		_container = containerName;
		_blobname = blobname;
	}

	readonly string _connect;
	readonly string _container;
	readonly string _blobname;
	const int ExpireMinutes = 15;

	public static async Task<PleaProcessor> CreateAsync(string storageConnect, string containerName, string blobname)
	{
		var proc = new PleaProcessor(storageConnect, containerName, blobname);
		await proc.PrepareAsync();
		return proc;
	}

	async Task PrepareAsync()
	{
		await CClient.CreateIfNotExistsAsync();
	}

	public async Task<string> AddItem(string type, string data)
	{
		XDocument doc = await GetDoc();
		string id = Guid.NewGuid().ToString("N");
		doc.Root!.Add(
			new XElement("item",
				new XElement("type", type),
				new XElement("created", DateTime.UtcNow),
				new XElement("id", id),
				new XElement("data", data)
			)
		);
		var olditems = doc.Root.Elements().Where(e => DateTime.UtcNow.Subtract((DateTime)e.Element("created")!).TotalMinutes > ExpireMinutes).ToArray();
		foreach (XElement item in olditems)
		{
			item.Remove();
		}
		await PutDoc(doc);
		return id;
	}

	public async Task<PleaItem?> GetItem(string id)
	{
		XDocument doc = await GetDoc();
		XElement? elem = doc.Root!.Elements().FirstOrDefault(e => (string?)e.Element("id") == id);
		if (elem == null) return null;
		DateTime created = (DateTime)elem.Element("created")!;
		if (DateTime.UtcNow.Subtract(created).TotalMinutes > ExpireMinutes) return null;
		return new PleaItem(
			(string)elem.Element("id")!,
			(DateTime)elem.Element("created")!,
			(string)elem.Element("type")!,
			(string)elem.Element("data")!
		);
	}

	async Task<XDocument> GetDoc()
	{
		XDocument doc;
		if (await BClient.ExistsAsync())
		{
			using var stream = await BClient.OpenReadAsync();
			doc = XDocument.Load(stream);
		}
		else
		{
			doc = new XDocument(new XElement("pleas"));
		}
		return doc;
	}

	async Task PutDoc(XDocument doc)
	{
		using var stream = await BClient.OpenWriteAsync(true);
		using var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });
		doc.WriteTo(writer);
	}

	BlobServiceClient? _sclient;
	BlobServiceClient SClient => LazyInitializer.EnsureInitialized(ref _sclient, () => new BlobServiceClient(_connect));

	BlobContainerClient? _cclient;
	BlobContainerClient CClient => LazyInitializer.EnsureInitialized(ref _cclient, () => SClient.GetBlobContainerClient(_container));

	BlobClient? _bclient;
	BlobClient BClient => LazyInitializer.EnsureInitialized(ref _bclient, () => CClient.GetBlobClient(_blobname));
}
