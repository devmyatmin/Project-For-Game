using System;

namespace Beam.IO;

[Serializable]
public struct BlobPacketPayload(string name, byte[] data)
{
	public string Name = name;

	public byte[] Data = data;
}
