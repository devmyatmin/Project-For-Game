using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using SharpNeatLib.Maths;
using UnityEngine;

namespace Beam.Terrain;

public static class World
{
	public struct Bundle
	{
		public Map[] Maps;

		public Vector2[] GenerationZonePositons;

		public Vector2[] GeneratedZonePoints;

		public Bundle Clear()
		{
			Maps = new Map[49];
			GenerationZonePositons = (GeneratedZonePoints = null);
			return this;
		}

		public static Bundle New()
		{
			return default(Bundle).Clear();
		}

		public void Save(Stream stream, byte[] header = null)
		{
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
			if (header != null && header.Length != 0)
			{
				gZipStream.Write(header, 0, header.Length);
			}
			using BinaryWriter binaryWriter = new BinaryWriter(gZipStream);
			for (int i = 0; i < 49; i++)
			{
				Map map = Maps[i];
				using MemoryStream memoryStream = new MemoryStream();
				using MemoryStream memoryStream2 = new MemoryStream();
				using MemoryStream memoryStream3 = new MemoryStream();
				map.Save(memoryStream, memoryStream2, memoryStream3);
				byte[] array = memoryStream.ToArray();
				byte[] array2 = memoryStream2.ToArray();
				byte[] array3 = memoryStream3.ToArray();
				binaryWriter.Write(array.Length);
				binaryWriter.Write(array2.Length);
				binaryWriter.Write(array3.Length);
				binaryWriter.Write(array);
				binaryWriter.Write(array2);
				binaryWriter.Write(array3);
			}
			for (int j = 0; j < 49; j++)
			{
				MPExtensions.Write(binaryWriter, GenerationZonePositons[j]);
			}
			binaryWriter.Write((ushort)GeneratedZonePoints.Length);
			for (int k = 0; k < GeneratedZonePoints.Length; k++)
			{
				MPExtensions.Write(binaryWriter, GeneratedZonePoints[k]);
			}
		}

		public void Load(Stream stream, bool createTextures, byte[] header = null)
		{
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
			if (header != null && header.Length != 0)
			{
				gZipStream.Read(header, 0, header.Length);
			}
			using BinaryReader binaryReader = new BinaryReader(gZipStream);
			Maps = new Map[49];
			for (int i = 0; i < 49; i++)
			{
				Map map = new Map();
				int num = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				byte[] buffer = new byte[num];
				byte[] buffer2 = new byte[num2];
				byte[] buffer3 = new byte[num3];
				binaryReader.Read(buffer, 0, num);
				binaryReader.Read(buffer2, 0, num2);
				binaryReader.Read(buffer3, 0, num3);
				using (MemoryStream editorStream = new MemoryStream(buffer, 0, num))
				{
					using MemoryStream objectStream = new MemoryStream(buffer2, 0, num2);
					using MemoryStream heightStream = new MemoryStream(buffer3, 0, num3);
					map.Load(editorStream, objectStream, heightStream, checkVersion: false, createTextures);
				}
				Maps[i] = map;
			}
			GenerationZonePositons = new Vector2[49];
			for (int j = 0; j < 49; j++)
			{
				GenerationZonePositons[j] = MPExtensions.ReadVector2(binaryReader);
			}
			ushort num4 = binaryReader.ReadUInt16();
			GeneratedZonePoints = new Vector2[num4];
			for (int k = 0; k < num4; k++)
			{
				GeneratedZonePoints[k] = MPExtensions.ReadVector2(binaryReader);
			}
		}

		public override string ToString()
		{
			string text = base.ToString();
			if (BeamTeam.IsValid(Maps) && Maps.Length != 0)
			{
				int num = 0;
				Map[] maps = Maps;
				foreach (Map map in maps)
				{
					num += map.HeightmapData.GetLength(0) * map.HeightmapData.GetLength(1);
				}
				num /= Maps.Length;
				int num2 = (int)Mathf.Sqrt((float)num);
				text += $"\nMaps[{Maps.Length}] {num2}x{num2}";
			}
			if (BeamTeam.IsValid(GenerationZonePositons))
			{
				text += $"\nGenerationZonePositons[{GenerationZonePositons.Length}]";
			}
			if (BeamTeam.IsValid(GeneratedZonePoints))
			{
				text += $"\nGeneratedZonePoints[{GeneratedZonePoints.Length}]";
			}
			return text;
		}
	}

	public delegate void WorldSavedCallback(LoadingResult result);

	public const string WORLD_SEED_FILENAME = "Seed.sdd";

	private const int NUM_MISSIONS = 4;

	private static Bundle[] _worlds = new Bundle[2]
	{
		Bundle.New(),
		Bundle.New()
	};

	private static int _currentWorld = 0;

	public static Bundle CurrentWorld => _worlds[_currentWorld];

	private static Map[] _mapList
	{
		get
		{
			return _worlds[_currentWorld].Maps;
		}
		set
		{
			_worlds[_currentWorld].Maps = value;
		}
	}

	public static Map[] MapList => _mapList;

	private static Vector2[] _generationZonePositons
	{
		get
		{
			return _worlds[_currentWorld].GenerationZonePositons;
		}
		set
		{
			_worlds[_currentWorld].GenerationZonePositons = value;
		}
	}

	public static Vector2[] GenerationZonePositons => _generationZonePositons;

	private static Vector2[] _generatedZonePoints
	{
		get
		{
			return _worlds[_currentWorld].GeneratedZonePoints;
		}
		set
		{
			_worlds[_currentWorld].GeneratedZonePoints = value;
		}
	}

	public static Vector2[] GeneratedZonePoints => _generatedZonePoints;

	public static void PushWorld(Bundle world)
	{
		if (_currentWorld == 1)
		{
			Debug.LogError((object)"World is already back-up. This is client map flow issue.");
		}
		_worlds[++_currentWorld] = world;
	}

	public static void PopWorld()
	{
		if (_currentWorld != 0)
		{
			_worlds[_currentWorld].Clear();
			_currentWorld--;
		}
	}

	private static LoadingResult CreateWorldFolders()
	{
		try
		{
			if (!Directory.Exists(FilePath.WORLD_FOLDER))
			{
				Directory.CreateDirectory(FilePath.WORLD_FOLDER);
			}
		}
		catch (Exception ex)
		{
			return new LoadingResult
			{
				Message = "WORLD_MSG_WORLD_DIR_NOT_FOUND " + ex.Message,
				Succeeded = false
			};
		}
		return new LoadingResult
		{
			Succeeded = true
		};
	}

	public static void UpgradeLegacyWorld(out LoadingResult result)
	{
		Map[] array = new Map[25];
		result = LoadWorld_(legacy: true);
		if (result.Succeeded)
		{
			CopyLegacyMapsRefs(array);
			CreateWorld(legacy: true);
			MergeLegacyMaps(array);
			SaveWorld(null);
		}
	}

	private static void CopyLegacyMapsRefs(Map[] maps)
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				int num = j * 5 + i;
				maps[num] = _mapList[num];
			}
		}
	}

	private static void MergeLegacyMaps(Map[] legacyMaps)
	{
		int num = 1;
		int num2 = 1;
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				Map map = legacyMaps[j * 5 + i];
				int num3 = i + num;
				int num4 = j + num2;
				_mapList[num4 * 7 + num3] = map;
			}
		}
	}

	private static LoadingResult ValidateWorldFolders(bool legacy = false)
	{
		if (!File.Exists(PathTools.Combine(new string[2]
		{
			FilePath.WORLD_FOLDER,
			"Seed.sdd"
		})))
		{
			return new LoadingResult
			{
				Message = "WORLD_MSG_SEED_FILE_MISSING",
				Succeeded = false
			};
		}
		try
		{
			if (Directory.GetDirectories(FilePath.WORLD_FOLDER).Length == 0)
			{
				return new LoadingResult
				{
					Message = "WORLD_MSG_EMPTY_WORLD_FOLDER",
					Succeeded = false
				};
			}
		}
		catch (Exception ex)
		{
			return new LoadingResult
			{
				Message = ex.Message,
				Succeeded = false
			};
		}
		try
		{
			int num = (legacy ? 5 : 7);
			for (int i = 0; i < num * num; i++)
			{
				string worldMapFolderName = GetWorldMapFolderName(i, legacy);
				string text = PathTools.Combine(new string[2]
				{
					FilePath.WORLD_FOLDER,
					worldMapFolderName
				});
				if (Directory.Exists(text))
				{
					string[] directories = Directory.GetDirectories(text);
					for (int j = 0; j < directories.Length; j++)
					{
						string fileName = Path.GetFileName(directories[j]);
						if (!Maps.ValidateMapFolder(PathTools.Combine(new string[2] { text, fileName }), fileName))
						{
							return new LoadingResult
							{
								Message = "WORLD_MSG_MISSING_MAP_FOLDER " + fileName,
								Succeeded = false
							};
						}
					}
					continue;
				}
				return new LoadingResult
				{
					Message = "WORLD_MSG_MISSING_WORLD_FOLDER",
					Succeeded = false
				};
			}
		}
		catch (Exception ex2)
		{
			return new LoadingResult
			{
				Message = ex2.Message,
				Succeeded = false
			};
		}
		return new LoadingResult
		{
			Succeeded = true
		};
	}

	private static Map CreateProceduralMap(int index, int seed)
	{
		Map map = new Map();
		Zone.BiomeType biomeType = WorldTools.GenerateZoneBiome(Zone.BiomeType.RANDOM, StrandedWorld.WORLD_SEED - seed);
		if (index == 0)
		{
			biomeType = Zone.BiomeType.ISLAND_SMALL;
		}
		float[,] heightmapData = WorldTools.GenerateZoneHeightmap(biomeType, StrandedWorld.WORLD_SEED - seed);
		map.EditorData = new MapEditorData
		{
			Name = MapNameGenerator.GenerateRandomMapName(seed),
			Id = string.Format("{0}_{1}", "INTERNAL_PROCEDURAL", index),
			VersionNumber = 0,
			Biome = biomeType,
			Author = "Beam Team",
			Description = "MAP_DESC_PROCEDURAL"
		};
		map.ObjectData = new JObject();
		map.HeightmapData = heightmapData;
		map.EditorData.SmallPreviewImage = MapTools.CreateHeightmapTexture(map.HeightmapData);
		return map;
	}

	public static bool ReplaceMapWithProceduralMap(Map replaceMap, out Map proceduralMap)
	{
		proceduralMap = null;
		int num = 0;
		for (int i = 0; i < 49; i++)
		{
			if (_mapList[i] == replaceMap)
			{
				Map map = CreateProceduralMap(i, num);
				_mapList[i] = map;
				proceduralMap = map;
				return true;
			}
			num++;
		}
		return false;
	}

	public static void ReplaceMap(Map mapToReplace, Map replacingMap)
	{
		for (int i = 0; i < 49; i++)
		{
			if (_mapList[i] == mapToReplace)
			{
				_mapList[i] = replacingMap;
				break;
			}
		}
	}

	private static int[] CreateMissionTiles()
	{
		FastRandom fastRandom = new FastRandom(StrandedWorld.WORLD_SEED);
		int upperBound = 48;
		List<int> list = new List<int>();
		int[] array = new int[4];
		for (int i = 0; i < 4; i++)
		{
			int num = fastRandom.Next(1, upperBound);
			while (list.Contains(num))
			{
				num = fastRandom.Next(1, upperBound);
			}
			list.Add(num);
			array[i] = num;
		}
		return array;
	}

	public static void CreateWorld(bool legacy = false)
	{
		CreateWorldZonePoints(StrandedWorld.WORLD_SEED);
		_mapList = new Map[49];
		for (int i = 0; i < _generationZonePositons.Length; i++)
		{
			int seed = i;
			Map map = CreateProceduralMap(i, seed);
			_mapList[i] = map;
		}
		if (legacy)
		{
			return;
		}
		int[] array = CreateMissionTiles();
		int num = 0;
		if (Maps.DefaultMapList.Count == 0)
		{
			Maps.LoadDefaultMaps(out var result);
			_ = result.Succeeded;
		}
		for (int j = 0; j < 49; j++)
		{
			bool flag = false;
			for (int k = 0; k < 4; k++)
			{
				if (j == array[k])
				{
					flag = true;
					break;
				}
			}
			if (flag && num < Maps.DefaultMapList.Count)
			{
				Map map2 = Maps.DefaultMapList[num];
				_mapList[j] = map2;
				if (map2.EditorData.Biome == Zone.BiomeType.CARRIER)
				{
					MapEditorData editorData = map2.EditorData;
					Object val = Resources.Load("Icons/WorldMap_Icon_Carrier");
					editorData.SmallPreviewImage = (Texture2D)(object)((val is Texture2D) ? val : null);
				}
				else if (map2.EditorData.Biome == Zone.BiomeType.MISSION)
				{
					MapEditorData editorData2 = map2.EditorData;
					Object val2 = Resources.Load("Icons/WorldMap_Icon_Mission");
					editorData2.SmallPreviewImage = (Texture2D)(object)((val2 is Texture2D) ? val2 : null);
				}
				num++;
			}
			else
			{
				int seed2 = j;
				Map map3 = CreateProceduralMap(j, seed2);
				_mapList[j] = map3;
			}
		}
	}

	public static void CreateWorldZonePoints(int seed)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		int num = 49;
		float num2 = 500f;
		float num3 = num2 * 1.25f * 7f;
		int numSamplesBeforeRejection = 30;
		_generatedZonePoints = ZonePositionGenerator.GeneratePoints(seed, num2, new Vector2(num3, num3), numSamplesBeforeRejection);
		_generationZonePositons = new Vector2[num];
		FastRandom fastRandom = new FastRandom(seed);
		List<int> list = new List<int>();
		int upperBound = _generatedZonePoints.Length;
		for (int i = 0; i < num; i++)
		{
			int num4 = fastRandom.Next(0, upperBound);
			while (list.Contains(num4))
			{
				num4 = fastRandom.Next(0, upperBound);
			}
			if (i == 0)
			{
				num4 = 0;
			}
			list.Add(num4);
			_generationZonePositons[i] = _generatedZonePoints[num4];
		}
		if (_generationZonePositons.Length < num - 1)
		{
			Debug.LogError((object)"Error: Not enough island positions");
		}
	}

	public static void SaveWorld(WorldSavedCallback callback)
	{
		LoadingResult loadingResult = CreateWorldFolders();
		if (loadingResult.Succeeded)
		{
			loadingResult = SaveWorldSeed();
		}
		if (loadingResult.Succeeded)
		{
			loadingResult = SaveWorldMaps();
		}
		if (!loadingResult.Succeeded)
		{
			DeleteWorld(FilePath.WORLD_FOLDER);
			callback?.Invoke(loadingResult);
		}
		else
		{
			callback?.Invoke(loadingResult);
		}
	}

	private static void DeleteWorld(string folderPath)
	{
		if (string.IsNullOrWhiteSpace(folderPath))
		{
			return;
		}
		try
		{
			if (!Directory.Exists(folderPath))
			{
				return;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			foreach (FileInfo item in directoryInfo.EnumerateFiles())
			{
				item.Delete();
			}
			foreach (DirectoryInfo item2 in directoryInfo.EnumerateDirectories())
			{
				item2.Delete(recursive: true);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	private static LoadingResult SaveWorldSeed()
	{
		try
		{
			using (FileStream serializationStream = new FileStream(PathTools.Combine(new string[2]
			{
				FilePath.WORLD_FOLDER,
				"Seed.sdd"
			}), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				new BinaryFormatter().Serialize(serializationStream, StrandedWorld.LOCAL_WORLD_SEED);
			}
			return new LoadingResult
			{
				Message = "WORLD_MSG_SUCCESS",
				Succeeded = true
			};
		}
		catch (Exception ex)
		{
			LoadingResult result = new LoadingResult
			{
				Message = "WORLD_MSG_ERROR_SAVING_WORLD_SEED " + ex.Message,
				Succeeded = false
			};
			Debug.LogError((object)("World::SaveWorldSeed:: " + ex.Message));
			return result;
		}
	}

	private static LoadingResult SaveWorldMaps()
	{
		try
		{
			for (int i = 0; i < 49; i++)
			{
				string text = PathTools.Combine(new string[2]
				{
					FilePath.WORLD_FOLDER,
					i.ToString()
				});
				if (Directory.Exists(text))
				{
					Directory.Delete(text, recursive: true);
				}
				Directory.CreateDirectory(text);
				Maps.DeleteMapFolder(Directory.GetDirectories(text));
				Maps.SaveMap(_mapList[i], text);
			}
			return new LoadingResult
			{
				Message = "WORLD_MSG_SUCCESS",
				Succeeded = true
			};
		}
		catch (Exception ex)
		{
			LoadingResult result = new LoadingResult
			{
				Message = "WORLD_MSG_ERROR_SAVING_WORLD " + ex.Message,
				Succeeded = false
			};
			Debug.LogError((object)("World::SaveWorldMaps:: " + ex.Message));
			return result;
		}
	}

	public static LoadingResult LoadWorld()
	{
		return LoadWorld_();
	}

	private static LoadingResult LoadWorld_(bool legacy = false)
	{
		LoadingResult loadingResult = ValidateWorldFolders(legacy);
		if (loadingResult.Succeeded)
		{
			LoadingResult loadingResult2 = LoadWorldSeed();
			if (!loadingResult2.Succeeded)
			{
				return loadingResult2;
			}
			if (!legacy)
			{
				CreateWorldZonePoints(StrandedWorld.WORLD_SEED);
			}
			loadingResult2 = LoadWorldMaps(legacy);
			if (!loadingResult2.Succeeded)
			{
				return loadingResult2;
			}
			return new LoadingResult
			{
				Succeeded = true
			};
		}
		LoadingResult result = new LoadingResult
		{
			Message = "WORLD_MSG_ERROR_VALIDATING_WORLD_STRUCTURE " + loadingResult.Message,
			Succeeded = false
		};
		Debug.LogError((object)("World::LoadWorld:: " + loadingResult.Message));
		return result;
	}

	public static bool CheckWorldSeedFileExists()
	{
		return File.Exists(PathTools.Combine(new string[2]
		{
			FilePath.WORLD_FOLDER,
			"Seed.sdd"
		}));
	}

	private static LoadingResult LoadWorldSeed()
	{
		try
		{
			using (FileStream serializationStream = new FileStream(PathTools.Combine(new string[2]
			{
				FilePath.WORLD_FOLDER,
				"Seed.sdd"
			}), FileMode.Open, FileAccess.Read))
			{
				StrandedWorld.SetWorldSeed(((int)new BinaryFormatter().Deserialize(serializationStream)).ToString());
			}
			LoadingResult result = new LoadingResult
			{
				Message = "WORLD_MSG_SUCCESS",
				Succeeded = true
			};
			Debug.Log((object)"World::LoadWorldSeed:: Success.");
			return result;
		}
		catch (Exception ex)
		{
			LoadingResult result2 = new LoadingResult
			{
				Message = "WORLD_MSG_ERROR_LOADING_WORLD_SEED " + ex.Message,
				Succeeded = false
			};
			Debug.LogError((object)("World::LoadWorldSeed:: " + ex.Message));
			return result2;
		}
	}

	private static LoadingResult LoadWorldMaps(bool legacy = false)
	{
		LoadingResult loadingResult = new LoadingResult
		{
			Succeeded = true
		};
		try
		{
			int num = (legacy ? 5 : 7);
			_mapList = new Map[num * num];
			for (int i = 0; i < num * num; i++)
			{
				string worldMapFolderName = GetWorldMapFolderName(i, legacy);
				string text = PathTools.Combine(new string[2]
				{
					FilePath.WORLD_FOLDER,
					worldMapFolderName
				});
				string[] directories = Directory.GetDirectories(text);
				for (int j = 0; j < directories.Length; j++)
				{
					string fileName = Path.GetFileName(directories[j]);
					string mapPath = PathTools.Combine(new string[2] { text, fileName });
					try
					{
						Map map = Maps.LoadMap(mapPath, fileName, checkVersion: true);
						_mapList[i] = map;
					}
					catch (VersionException ex)
					{
						loadingResult.Message = "WORLD_MSG_MAP_VERSION_EXCEPTION";
						loadingResult.Succeeded = false;
						Debug.LogError((object)("World::LoadWorldMaps:: " + ex.Message));
						goto end_IL_0138;
					}
					catch (Exception ex2)
					{
						LoadingResult loadingResult2 = loadingResult;
						loadingResult2.Message = loadingResult2.Message + "WORLD_MSG_ERROR_LOADING_MAP " + fileName + "\n\n" + ex2.Message;
						loadingResult.Succeeded = false;
						Debug.LogError((object)("World::LoadWorldMaps:: " + ex2.Message));
					}
				}
				continue;
				end_IL_0138:
				break;
			}
			if (loadingResult.Succeeded)
			{
				loadingResult.Message = "WORLD_MSG_SUCCESS";
				Debug.Log((object)"World::LoadWorldMaps:: Success.");
			}
			return loadingResult;
		}
		catch (Exception ex3)
		{
			loadingResult.Message = "WORLD_MSG_ERROR_LOADING_WORLD_MAPS " + ex3.Message;
			loadingResult.Succeeded = false;
			Debug.LogError((object)("World::LoadWorldMaps:: " + ex3.Message));
			return loadingResult;
		}
	}

	private static string GetWorldMapFolderName(int index, bool legacy = false)
	{
		if (legacy)
		{
			int num = index % 5;
			int num2 = (index - num) / 5;
			return $"{num}_{num2}";
		}
		return index.ToString();
	}

	public static float[,] ImportHeightmapFromRawFile(string path)
	{
		float[,] array = new float[257, 257];
		int num = 66049;
		num *= 2;
		byte[] array2 = new byte[num];
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			try
			{
				fileStream.Read(array2, 0, num);
			}
			catch (ArgumentException)
			{
				throw new Exception("Error raw heightmap import.");
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("World::ImportHeightmapFromRawFile::\n\n" + ex2.Message));
			return array;
		}
		int num2 = 0;
		for (int i = 0; i < 257; i++)
		{
			for (int j = 0; j < 257; j++)
			{
				float num3 = (float)(int)array2[num2++] + (float)(int)array2[num2++] * 256f;
				array[257 - i - 1, j] = num3 / 65535f;
			}
		}
		return array;
	}
}
