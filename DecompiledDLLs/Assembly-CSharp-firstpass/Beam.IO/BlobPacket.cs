using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Beam.IO;

[Serializable]
public class BlobPacket
{
	private readonly string _name;

	private readonly HashSet<BlobPacketPayload> _payloadEntries = new HashSet<BlobPacketPayload>();

	public string Name => _name;

	public IEnumerable<BlobPacketPayload> PayloadEntries => _payloadEntries;

	public BlobPacket(string name)
	{
		_name = name;
	}

	public bool AddPayloadEntry(string name, byte[] data)
	{
		string entryName = Path.Sanitize(name);
		if (_payloadEntries.Any((BlobPacketPayload file) => file.Name == entryName))
		{
			return false;
		}
		return _payloadEntries.Add(new BlobPacketPayload(entryName, data));
	}

	public byte[] Serialize()
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, this);
		memoryStream.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		byte[] buffer = memoryStream.GetBuffer();
		using MemoryStream memoryStream2 = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream2, CompressionMode.Compress))
		{
			gZipStream.Write(buffer, 0, buffer.Length);
		}
		return memoryStream2.ToArray();
	}

	public static BlobPacket Deserialize(byte[] bytes)
	{
		using MemoryStream memoryStream = new MemoryStream(bytes);
		byte[] buffer;
		using (MemoryStream stream = new MemoryStream(memoryStream.ToArray()))
		{
			using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
			using MemoryStream memoryStream2 = new MemoryStream();
			gZipStream.CopyTo(memoryStream2);
			buffer = memoryStream2.ToArray();
		}
		using MemoryStream serializationStream = new MemoryStream(buffer);
		return (BlobPacket)new BinaryFormatter().Deserialize(serializationStream);
	}
}
