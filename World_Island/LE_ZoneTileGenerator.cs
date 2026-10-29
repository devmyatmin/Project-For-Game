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
		InvokeRepeating("GenerateShoreMap", 0f, 10f);
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
			AddWaveOverlay(terrain.gameObject, height, shore);
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
		int heightmapResolution = terrain.heightmapResolution;
		int heightmapResolution2 = terrain.heightmapResolution;
		Texture2D texture2D = new Texture2D(heightmapResolution, heightmapResolution2, TextureFormat.RGB24, mipChain: false, linear: false);
		for (int i = 0; i < heightmapResolution; i++)
		{
			for (int j = 0; j < heightmapResolution2; j++)
			{
				float num = terrain.GetHeight(i, j) + -100f;
				Color color = Color.Lerp(Color.black, Color.white, Mathf.Clamp01((num + 10f) / 10f));
				Color color2 = ((num > 0f) ? Color.white : color);
				texture2D.SetPixel(i, j, color2);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D CreateShoreMaskTexture(TerrainData terrain)
	{
		int heightmapResolution = terrain.heightmapResolution;
		int heightmapResolution2 = terrain.heightmapResolution;
		Texture2D texture2D = new Texture2D(heightmapResolution, heightmapResolution2, TextureFormat.RGB24, mipChain: false, linear: false);
		int num = -100;
		for (int i = 0; i < heightmapResolution; i++)
		{
			for (int j = 0; j < heightmapResolution2; j++)
			{
				Color color = ((terrain.GetHeight(i, j) + (float)num >= 0f) ? Color.white : Color.black);
				texture2D.SetPixel(i, j, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private void AddWaveOverlay(GameObject zone, Texture2D height, Texture2D shore)
	{
		if ((bool)_waveOverlay)
		{
			Object.Destroy(_waveOverlay.heightTexture.tex);
			Object.Destroy(_waveOverlay.heightTexture.mask);
			Object.Destroy(_waveOverlay.normalTexture.tex);
			Object.Destroy(_waveOverlay.normalTexture.mask);
			Object.Destroy(_waveOverlay.foamTexture.tex);
			Object.Destroy(_waveOverlay.foamTexture.mask);
			Object.Destroy(_waveOverlay.gameObject);
		}
		GameObject gameObject = new GameObject("WaveOverlay");
		gameObject.transform.parent = zone.transform;
		gameObject.transform.position = new Vector3(0f, 0f, 0f);
		_waveOverlay = gameObject.AddComponent<AddWaveOverlay>();
		_waveOverlay.width = 250f;
		_waveOverlay.height = 250f;
		_waveOverlay.heightTexture = new OverlayHeightTexture();
		_waveOverlay.heightTexture.tex = shore;
		_waveOverlay.heightTexture.mask = height;
		_waveOverlay.heightTexture.alpha = 0f;
		_waveOverlay.heightTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		_waveOverlay.normalTexture = new OverlayNormalTexture();
		_waveOverlay.normalTexture.tex = shore;
		_waveOverlay.normalTexture.mask = height;
		_waveOverlay.normalTexture.alpha = 0f;
		_waveOverlay.normalTexture.maskAlpha = 0.4f;
		_waveOverlay.normalTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
		_waveOverlay.foamTexture = new OverlayFoamTexture();
		_waveOverlay.foamTexture.tex = shore;
		_waveOverlay.foamTexture.mask = height;
		_waveOverlay.foamTexture.alpha = 0.4f;
		_waveOverlay.foamTexture.maskMode = OVERLAY_MASK_MODE.WAVES;
	}

	public void LoadMap(Map map)
	{
		Singleton<LE_LevelEditorMain>.Instance.ClearLevel();
		terrain.terrainData.SetHeights(0, 0, map.HeightmapData);
	}
}
