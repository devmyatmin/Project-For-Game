using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Beam.Utilities;
using UnityEngine;

namespace Beam.IO;

public class BlobStorageUtility
{
	private string _rootPath;

	public void SetRootPath(string absolutePath)
	{
		if (absolutePath == null)
		{
			throw new ArgumentNullException("absolutePath");
		}
		_rootPath = Path.Sanitize(absolutePath);
		if (!_rootPath.EndsWith('/'.ToString()))
		{
			_rootPath += "/";
		}
	}

	public string GetBlobName(string filePath)
	{
		return GetRelativePath(_rootPath, filePath);
	}

	public BlobPacket CreateBlobPacket(string filePath)
	{
		filePath = Path.Sanitize(filePath);
		if (!filePath.Contains(_rootPath))
		{
			throw new ArgumentException("Full path '" + filePath + "' does not contain root path '" + _rootPath + "'", "filePath");
		}
		return CreateBlobPacket(filePath, _rootPath);
	}

	private BlobPacket CreateBlobPacket(string filePath, string rootPath)
	{
		string relativePath = GetRelativePath(rootPath, filePath);
		BlobPacket blobPacket = new BlobPacket(relativePath);
		FileAttributes attributes = File.GetAttributes(filePath);
		if (attributes.HasFlag(FileAttributes.Directory))
		{
			foreach (string item in from file in Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories)
				where !file.EndsWith("meta")
				select file)
			{
				blobPacket.AddPayloadEntry(GetRelativePath(rootPath, item), File.ReadAllBytes(item));
			}
		}
		else
		{
			blobPacket.AddPayloadEntry(relativePath, File.ReadAllBytes(filePath));
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (BlobPacketPayload payloadEntry in blobPacket.PayloadEntries)
		{
			stringBuilder.AppendFormat("{0} >> {1} KB\n", payloadEntry.Name, payloadEntry.Data.Length.ToSize(ESizeUnits.KB));
		}
		Debug.Log($"Created blob packet '{blobPacket.Name}' with {blobPacket.PayloadEntries.Count()} payload entry(s):\n" + stringBuilder.ToString());
		return blobPacket;
	}

	public void IngestBlobs(IDictionary<string, byte[]> blobsData)
	{
		foreach (KeyValuePair<string, byte[]> blobsDatum in blobsData)
		{
			BlobPacket blobPacket = BlobPacket.Deserialize(blobsDatum.Value);
			IngestBlobPacket(blobPacket);
		}
	}

	public void IngestBlobPacket(BlobPacket blobPacket)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (BlobPacketPayload payloadEntry in blobPacket.PayloadEntries)
		{
			stringBuilder.AppendFormat("{0} >> {1} KB\n", payloadEntry.Name, payloadEntry.Data.Length.ToSize(ESizeUnits.KB));
		}
		Debug.Log($"Received blob packet '{blobPacket.Name}' with {blobPacket.PayloadEntries.Count()} payload entry(s):\n" + stringBuilder.ToString());
		foreach (BlobPacketPayload payloadEntry2 in blobPacket.PayloadEntries)
		{
			string name = payloadEntry2.Name;
			byte[] data = payloadEntry2.Data;
			string path = Path.Combine(_rootPath, name);
			string fullName = Directory.GetParent(path).FullName;
			if (!Directory.Exists(fullName))
			{
				Directory.CreateDirectory(fullName);
			}
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.WriteAllBytes(path, data);
		}
	}

	private static string GetRelativePath(string relativeTo, string path)
	{
		if (string.IsNullOrEmpty(relativeTo))
		{
			throw new ArgumentNullException("relativeTo");
		}
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentNullException("path");
		}
		relativeTo = Path.Sanitize(relativeTo);
		if (!relativeTo.EndsWith('/'.ToString()))
		{
			throw new ArgumentException($"'{relativeTo}' path must end with a directory separator '{'/'}'", "relativeTo");
		}
		path = Path.Sanitize(path);
		return Path.Sanitize(path.Replace(relativeTo, ""));
	}
}
