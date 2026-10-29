using LE_LevelEditor.Logic;
using UnityEngine;

namespace LE_LevelEditor;

public class LE_ConfigTerrain : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Scene object with a 'Projector' component that will be used to project the terrain edit brush. Can be 'null' if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is 'false'.")]
	private Projector m_brushProjector;

	[SerializeField]
	[Tooltip("Array with selectable brush textures.")]
	private Texture2D[] m_brushes = new Texture2D[0];

	[SerializeField]
	[Tooltip("Array with selectable heightmap textures.")]
	private LE_LogicTerrain.Heightmap[] _heightmaps = new LE_LogicTerrain.Heightmap[0];

	[SerializeField]
	[Tooltip("Set a predefined Unity terrain scene object.")]
	private Terrain m_customDefaultTerrain;

	[SerializeField]
	[Tooltip("Created/loaded terrains will be moved to this layer. Raycast layer for terrain editing and terrain snapped object placement.")]
	private int m_terrainLayer = 14;

	public Projector BrushProjector => m_brushProjector;

	public Texture2D[] Brushes => m_brushes;

	public LE_LogicTerrain.Heightmap[] Heightmaps
	{
		get
		{
			return _heightmaps;
		}
		set
		{
			_heightmaps = value;
		}
	}

	public Terrain CustomDefaultTerrain => m_customDefaultTerrain;

	public int TerrainLayer => m_terrainLayer;
}
