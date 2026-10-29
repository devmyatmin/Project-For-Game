using System.IO;
using UnityEngine;

namespace LE_LevelEditor;

public static class LE_SaveLoad
{
	private static string filePath = "/CustomMaps/Original/";

	private static void SaveMapHeightMapFile(float[,] heightmap, string mapName)
	{
		FileStream fileStream = new FileStream(Application.dataPath + filePath + mapName + "_heightmap.map", FileMode.Create, FileAccess.ReadWrite);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		for (int i = 0; i < 257; i++)
		{
			for (int j = 0; j < 257; j++)
			{
				if (heightmap[i, j] > 1f)
				{
					heightmap[i, j] = 1f;
				}
				if (heightmap[i, j] < 0f)
				{
					heightmap[i, j] = 0f;
				}
				ushort value = (ushort)(heightmap[i, j] * 65535f);
				binaryWriter.Write(value);
			}
		}
		fileStream.Close();
		binaryWriter.Close();
	}

	public static float[,] LoadMapHeightMapFile(string mapName)
	{
		float[,] array = new float[257, 257];
		FileStream fileStream = new FileStream(Application.dataPath + filePath + mapName + "_heightmap.map", FileMode.Open, FileAccess.ReadWrite);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		for (int i = 0; i < 257; i++)
		{
			for (int j = 0; j < 257; j++)
			{
				array[i, j] = (float)(int)binaryReader.ReadUInt16() / 65535f;
			}
		}
		fileStream.Close();
		binaryReader.Close();
		return array;
	}
}
