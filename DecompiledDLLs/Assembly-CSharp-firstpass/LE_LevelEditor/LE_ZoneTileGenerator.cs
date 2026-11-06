using Beam;
using Beam.Terrain;
using Ceto;
using LE_LevelEditor.UI;
using UnityEngine;

namespace LE_LevelEditor;

public class LE_ZoneTileGenerator : Singleton<LE_ZoneTileGenerator>
{
	public Terrain terrain;

	private AddWaveOverlay _waveOverlay;

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("GenerateShoreMap", 0f, 10f);
	}

	public void GenerateHeightmapFromTexture(Texture2D heightmap)
	{
		terrain.terrainData.SetHeights(0, 0, MapTools.GetHeightsFromTexture(heightmap));
	}

	public void GenerateShoreMap()
	{
		if (Singleton<LE_LevelEditorMain>.Instance.EditMode == LE_EEditMode.TERRAIN)
		{
			Texture2D height = WorldTools.CreateHeightMapTexture(terrain.terrainData);
			Texture2D shore = WorldTools.CreateShoreMaskTexture(terrain.terrainData);
			AddWaveOverlay(((Component)terrain).gameObject, height, shore);
		}
	}

	public void GenerateRandomIsland()
	{
		Singleton<LE_LevelEditorMain>.Instance.ClearLevel();
		int randowWorldSeed = StrandedWorld.GetRandowWorldSeed();
		terrain.terrainData.SetHeights(0, 0, WorldTools.GenerateZoneHeightmap(Zone.BiomeType.ISLAND, randowWorldSeed));
	}

	public void GenerateAdvancedPerlin(int freq, float amplitude, int octaves, float blend)
	{
		Singleton<LE_LevelEditorMain>.Instance.ClearLevel();
		terrain.terrainData.SetHeights(0, 0, WorldTools.PerlinGenerator(terrain.terrainData.GetHeights(0, 0, 257, 257), Random.Range(1, 99999999), freq, amplitude, octaves, blend));
	}

	public void GenerateAdvancedNormalise(float min, float max, float blend)
	{
		Singleton<LE_LevelEditorMain>.Instance.ClearLevel();
		terrain.terrainData.SetHeights(0, 0, WorldTools.NormalizeHeightmap(terrain.terrainData.GetHeights(0, 0, 257, 257), min, max, blend));
	}

	public void GenerateSmooth()
	{
		terrain.terrainData.SetHeights(0, 0, WorldTools.SmoothTerrain(terrain.terrainData.GetHeights(0, 0, 257, 257), 2, 0.25f, Zone.BiomeType.ISLAND));
	}

	private Texture2D CreateHeightMapTexture(TerrainData terrain)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		int heightmapResolution = terrain.heightmapResolution;
		int heightmapResolution2 = terrain.heightmapResolution;
		Texture2D val = new Texture2D(heightmapResolution, heightmapResolution2, (TextureFormat)3, false, false);
		for (int i = 0; i < heightmapResolution; i++)
		{
			for (int j = 0; j < heightmapResolution2; j++)
			{
				float num = terrain.GetHeight(i, j) + -100f;
				Color val2 = Color.Lerp(Color.black, Color.white, Mathf.Clamp01((num + 10f) / 10f));
				Color val3 = ((num > 0f) ? Color.white : val2);
				val.SetPixel(i, j, val3);
			}
		}
		val.Apply();
		return val;
	}

	private Texture2D CreateShoreMaskTexture(TerrainData terrain)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		int heightmapResolution = terrain.heightmapResolution;
		int heightmapResolution2 = terrain.heightmapResolution;
		Texture2D val = new Texture2D(heightmapResolution, heightmapResolution2, (TextureFormat)3, false, false);
		int num = -100;
		for (int i = 0; i < heightmapResolution; i++)
		{
			for (int j = 0; j < heightmapResolution2; j++)
			{
				Color val2 = ((terrain.GetHeight(i, j) + (float)num >= 0f) ? Color.white : Color.black);
				val.SetPixel(i, j, val2);
			}
		}
		val.Apply();
		return val;
	}

	private void AddWaveOverlay(GameObject zone, Texture2D height, Texture2D shore)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_waveOverlay))
		{
			Object.Destroy((Object)(object)_waveOverlay.heightTexture.tex);
			Object.Destroy((Object)(object)_waveOverlay.heightTexture.mask);
			Object.Destroy((Object)(object)_waveOverlay.normalTexture.tex);
			Object.Destroy((Object)(object)_waveOverlay.normalTexture.mask);
			Object.Destroy((Object)(object)_waveOverlay.foamTexture.tex);
			Object.Destroy((Object)(object)_waveOverlay.foamTexture.mask);
			Object.Destroy((Object)(object)((Component)_waveOverlay).gameObject);
		}
		GameObject val = new GameObject("WaveOverlay");
		val.transform.parent = zone.transform;
		val.transform.position = new Vector3(0f, 0f, 0f);
		_waveOverlay = val.AddComponent<AddWaveOverlay>();
		_waveOverlay.width = 250f;
		_waveOverlay.height = 250f;
		_waveOverlay.heightTexture = new OverlayHeightTexture();
		_waveOverlay.heightTexture.tex = (Texture)(object)shore;
		_waveOverlay.heightTexture.mask = (Texture)(object)height;
		_waveOverlay.heightTexture.alpha = 0f;
		_waveOverlay.heightTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		_waveOverlay.normalTexture = new OverlayNormalTexture();
		_waveOverlay.normalTexture.tex = (Texture)(object)shore;
		_waveOverlay.normalTexture.mask = (Texture)(object)height;
		_waveOverlay.normalTexture.alpha = 0f;
		_waveOverlay.normalTexture.maskAlpha = 0.4f;
		_waveOverlay.normalTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		_waveOverlay.foamTexture = new OverlayFoamTexture();
		_waveOverlay.foamTexture.tex = (Texture)(object)shore;
		_waveOverlay.foamTexture.mask = (Texture)(object)height;
		_waveOverlay.foamTexture.alpha = 0.4f;
		_waveOverlay.foamTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
	}

	public void LoadMap(Map map)
	{
		Singleton<LE_LevelEditorMain>.Instance.ClearLevel();
		terrain.terrainData.SetHeights(0, 0, map.HeightmapData);
	}
}
