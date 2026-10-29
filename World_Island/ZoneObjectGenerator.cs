using UnityEngine;

namespace Beam.Terrain;

public static class ZoneObjectGenerator
{
	private const int INDEX_SIZE = 256;

	public static GenerationPoints[,] GenerateGridData(Zone zone, int seed)
	{
		float num = 125f / 128f;
		GenerationPoints[,] array = new GenerationPoints[256, 256];
		UnityEngine.Terrain terrain = zone.Terrain;
		TerrainData terrainData = zone.Terrain.terrainData;
		float[,] heights = terrainData.GetHeights(0, 0, 257, 257);
		float[,] array2 = WorldTools.GENERATE_ISLAND_HEIGHTMAP(StrandedWorld.WORLD_SEED - seed, zone.Biome, soilMapGeneration: true);
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				float y = 150f * heights[i, j] + -100f;
				Vector3 pos = new Vector3(terrain.transform.position.x + num * (float)j, y, terrain.transform.position.z + num * (float)i);
				float x = (pos.x - terrain.transform.position.x) / 250f;
				float y2 = (pos.z - terrain.transform.position.z) / 250f;
				Quaternion rot = Quaternion.LookRotation(Vector3.forward, terrainData.GetInterpolatedNormal(x, y2));
				float steepness = terrainData.GetSteepness(x, y2);
				array[i, j] = new GenerationPoints(pos, rot, occupied: false, steepness, array2[i, j]);
			}
		}
		array2 = null;
		heights = null;
		return array;
	}

	private static void DebugSoilMap(Zone zone, float[,] soilMap)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
		gameObject.gameObject.name = "Debug_SoilMap";
		gameObject.transform.parent = zone.transform;
		gameObject.transform.position = zone.transform.position + Vector3.up * 50f;
		gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
		gameObject.transform.localScale = new Vector3(-25f, 1f, 25f);
		Texture2D texture2D = IslandDebugTexture.RenderSoilMapImageIcon(soilMap, 257);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.Apply();
		gameObject.GetComponent<Renderer>().material.mainTexture = texture2D;
	}
}
