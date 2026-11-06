using System;
using Beam.Serialization.Json;

namespace Beam.Terrain;

[Serializable]
public class Map
{
	public MapEditorData EditorData;

	public JObject ObjectData;

	public float[,] HeightmapData;

	public Map()
	{
		EditorData = new MapEditorData();
	}

	public bool IsProceduralMap()
	{
		if (EditorData == null)
		{
			return false;
		}
		return EditorData.Id.Contains("INTERNAL_PROCEDURAL");
	}

	public bool IsCustomMap()
	{
		if (EditorData == null)
		{
			return false;
		}
		return !IsProceduralMap();
	}

	private bool IsInternalMap()
	{
		if (EditorData == null)
		{
			return false;
		}
		return EditorData.Id.Contains("INTERNAL");
	}

	public bool IsUserMap()
	{
		if (EditorData == null)
		{
			return false;
		}
		return !IsInternalMap();
	}
}
