using UnityEngine;

namespace Beam.Terrain;

public static class ZoneObjectGenerator
{
	private const int INDEX_SIZE = 256;

	public static GenerationPoints[,] GenerateGridData(Zone zone, int seed)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		float num = 125f / 128f;
		GenerationPoints[,] array = new GenerationPoints[256, 256];
		Terrain terrain = zone.Terrain;
		TerrainData terrainData = zone.Terrain.terrainData;
		float[,] heights = terrainData.GetHeights(0, 0, 257, 257);
		float[,] array2 = WorldTools.GENERATE_ISLAND_HEIGHTMAP(StrandedWorld.WORLD_SEED - seed, zone.Biome, soilMapGeneration: true);
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				float num2 = 150f * heights[i, j] + -100f;
				Vector3 val = new Vector3(((Component)terrain).transform.position.x + num * (float)j, num2, ((Component)terrain).transform.position.z + num * (float)i);
				float num3 = (val.x - ((Component)terrain).transform.position.x) / 250f;
				float num4 = (val.z - ((Component)terrain).transform.position.z) / 250f;
				Quaternion rot = Quaternion.LookRotation(Vector3.forward, terrainData.GetInterpolatedNormal(num3, num4));
				float steepness = terrainData.GetSteepness(num3, num4);
				array[i, j] = new GenerationPoints(val, rot, occupied: false, steepness, array2[i, j]);
			}
		}
		array2 = null;
		heights = null;
		return array;
	}

	private static void DebugSoilMap(Zone zone, float[,] soilMap)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameObject.CreatePrimitive((PrimitiveType)4);
		((Object)val.gameObject).name = "Debug_SoilMap";
		val.transform.parent = ((Component)zone).transform;
		val.transform.position = ((Component)zone).transform.position + Vector3.up * 50f;
		val.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
		val.transform.localScale = new Vector3(-25f, 1f, 25f);
		Texture2D val2 = IslandDebugTexture.RenderSoilMapImageIcon(soilMap, 257);
		((Texture)val2).wrapMode = (TextureWrapMode)1;
		val2.Apply();
		val.GetComponent<Renderer>().material.mainTexture = (Texture)(object)val2;
	}
}
