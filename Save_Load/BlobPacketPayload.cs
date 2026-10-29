using System;

namespace Beam.IO;

[Serializable]
public struct BlobPacketPayload
{
	public string Name;

	public byte[] Data;

	public BlobPacketPayload(string name, byte[] data)
	{
		Name = name;
		Data = data;
	}
}
