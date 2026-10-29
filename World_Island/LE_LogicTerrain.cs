using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beam;
using Beam.Terrain;
using LE_LevelEditor.Events;
using LE_LevelEditor.UI;
using UnityEngine;

namespace LE_LevelEditor.Logic;

public class LE_LogicTerrain : LE_LogicBase
{
	[Serializable]
	public class Heightmap
	{
		public Texture2D Preview;

		public float[,] Heights;
	}

	private LE_ConfigTerrain m_confT;

	private LE_GUI3dTerrain m_GUI3dTerrain;

	private bool m_doRebuildTerrainTab;

	private int m_selectedBrushIndex;

	private int m_selectedHeightmapIndex;

	private bool m_isHeightRaise = true;

	private void LoadRawHeightmaps(LE_ConfigTerrain confTerrain)
	{
		string hEIGHTMAPS_FOLDER = FilePath.HEIGHTMAPS_FOLDER;
		if (!Directory.Exists(hEIGHTMAPS_FOLDER))
		{
			Directory.CreateDirectory(hEIGHTMAPS_FOLDER);
		}
		List<Heightmap> list = new List<Heightmap>();
		string[] files = Directory.GetFiles(hEIGHTMAPS_FOLDER, "*.raw");
		foreach (string path in files)
		{
			Heightmap heightmap = new Heightmap();
			float[,] array = World.ImportHeightmapFromRawFile(path);
			Texture2D preview = MapTools.CreateHeightmapTexture(array);
			heightmap.Heights = array;
			heightmap.Preview = preview;
			list.Add(heightmap);
		}
		confTerrain.Heightmaps = list.ToArray();
	}

	public LE_LogicTerrain(LE_ConfigTerrain p_confTerrain, LE_GUI3dTerrain p_GUI3dTerrain)
	{
		m_confT = p_confTerrain;
		m_GUI3dTerrain = p_GUI3dTerrain;
		m_GUI3dTerrain.TERRAIN_LAYER = p_confTerrain.TerrainLayer;
		if (p_confTerrain.BrushProjector != null)
		{
			m_GUI3dTerrain.BrushProjector = p_confTerrain.BrushProjector;
		}
		else
		{
			Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain.BrushProjector was not initialized! You need to set it in the inspector.");
		}
		if (p_confTerrain.Brushes.Length != 0)
		{
			m_GUI3dTerrain.BrushAlphaTexture = p_confTerrain.Brushes[0];
		}
		else
		{
			Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain.Brushes was not initialized! You need to set it in the inspector.");
		}
		LoadRawHeightmaps(p_confTerrain);
		if (p_confTerrain.Heightmaps.Length != 0)
		{
			m_GUI3dTerrain.Heightmap = p_confTerrain.Heightmaps[0];
		}
		else
		{
			Debug.Log("LE_GUILogicTerrain: LE_ConfigTerrain.Heightmap was not initialized! You need to set it in the inspector.");
		}
		CheckParameters();
		InitTerrainValues();
		LE_EventInterface.OnLoadedLevelInEditor = (EventHandler)Delegate.Combine(LE_EventInterface.OnLoadedLevelInEditor, new EventHandler(OnLoadedLevelInEditor));
		LE_GUIInterface.EventHandlers events = LE_GUIInterface.Instance.events;
		events.OnTerrainBrushChanged = (EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>)Delegate.Combine(events.OnTerrainBrushChanged, new EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>(OnTerrainBrushChanged));
		LE_GUIInterface.EventHandlers events2 = LE_GUIInterface.Instance.events;
		events2.OnTerrainHeightmapTextureChanged = (EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>)Delegate.Combine(events2.OnTerrainHeightmapTextureChanged, new EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>(OnTerrainHeightmapTextureChanged));
		LE_GUIInterface.EventHandlers events3 = LE_GUIInterface.Instance.events;
		events3.OnTerrainEditBrushSizeChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Combine(events3.OnTerrainEditBrushSizeChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditBrushSizeChanged));
		LE_GUIInterface.EventHandlers events4 = LE_GUIInterface.Instance.events;
		events4.OnTerrainEditBrushAmountChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Combine(events4.OnTerrainEditBrushAmountChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditBrushAmountChanged));
		LE_GUIInterface.EventHandlers events5 = LE_GUIInterface.Instance.events;
		events5.OnTerrainEditBrushTargetValueChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Combine(events5.OnTerrainEditBrushTargetValueChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditBrushTargetValueChanged));
		LE_GUIInterface.EventHandlers events6 = LE_GUIInterface.Instance.events;
		events6.OnTerrainEditDirectionChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Combine(events6.OnTerrainEditDirectionChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditDirectionChanged));
		LE_GUIInterface.EventHandlers events7 = LE_GUIInterface.Instance.events;
		events7.OnTerrainChangeHeightModeChanged = (EventHandler<LE_GUIInterface.EventHandlers.TerrainChangeHeightModeEventArgs>)Delegate.Combine(events7.OnTerrainChangeHeightModeChanged, new EventHandler<LE_GUIInterface.EventHandlers.TerrainChangeHeightModeEventArgs>(OnTerrainChangeHeightModeChanged));
		LE_GUIInterface.EventHandlers events8 = LE_GUIInterface.Instance.events;
		events8.OnTerrainIsDirectedSmoothChanged = (EventHandler<LE_GUIInterface.EventHandlers.BoolEventArgs>)Delegate.Combine(events8.OnTerrainIsDirectedSmoothChanged, new EventHandler<LE_GUIInterface.EventHandlers.BoolEventArgs>(OnTerrainIsDirectedSmoothChanged));
		LE_GUIInterface.EventHandlers events9 = LE_GUIInterface.Instance.events;
		events9.OnTerrainEditModeBtn = (EventHandler<LE_GUIInterface.EventHandlers.TerrainEditModeEventArgs>)Delegate.Combine(events9.OnTerrainEditModeBtn, new EventHandler<LE_GUIInterface.EventHandlers.TerrainEditModeEventArgs>(OnTerrainEditModeBtn));
		LE_GUIInterface.EventHandlers events10 = LE_GUIInterface.Instance.events;
		events10.OnTerrainReadSetHeightBtn = (EventHandler)Delegate.Combine(events10.OnTerrainReadSetHeightBtn, new EventHandler(OnTerrainReadSetHeightBtn));
	}

	public override void Destroy()
	{
		LE_EventInterface.OnLoadedLevelInEditor = (EventHandler)Delegate.Remove(LE_EventInterface.OnLoadedLevelInEditor, new EventHandler(OnLoadedLevelInEditor));
		if (LE_GUIInterface.Instance != null)
		{
			LE_GUIInterface.EventHandlers events = LE_GUIInterface.Instance.events;
			events.OnTerrainBrushChanged = (EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>)Delegate.Remove(events.OnTerrainBrushChanged, new EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>(OnTerrainBrushChanged));
			LE_GUIInterface.EventHandlers events2 = LE_GUIInterface.Instance.events;
			events2.OnTerrainHeightmapTextureChanged = (EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>)Delegate.Remove(events2.OnTerrainHeightmapTextureChanged, new EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs>(OnTerrainHeightmapTextureChanged));
			LE_GUIInterface.EventHandlers events3 = LE_GUIInterface.Instance.events;
			events3.OnTerrainEditBrushSizeChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Remove(events3.OnTerrainEditBrushSizeChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditBrushSizeChanged));
			LE_GUIInterface.EventHandlers events4 = LE_GUIInterface.Instance.events;
			events4.OnTerrainEditBrushAmountChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Remove(events4.OnTerrainEditBrushAmountChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditBrushAmountChanged));
			LE_GUIInterface.EventHandlers events5 = LE_GUIInterface.Instance.events;
			events5.OnTerrainEditBrushTargetValueChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Remove(events5.OnTerrainEditBrushTargetValueChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditBrushTargetValueChanged));
			LE_GUIInterface.EventHandlers events6 = LE_GUIInterface.Instance.events;
			events6.OnTerrainEditDirectionChanged = (EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>)Delegate.Remove(events6.OnTerrainEditDirectionChanged, new EventHandler<LE_GUIInterface.EventHandlers.FloatEventArgs>(OnTerrainEditDirectionChanged));
			LE_GUIInterface.EventHandlers events7 = LE_GUIInterface.Instance.events;
			events7.OnTerrainChangeHeightModeChanged = (EventHandler<LE_GUIInterface.EventHandlers.TerrainChangeHeightModeEventArgs>)Delegate.Remove(events7.OnTerrainChangeHeightModeChanged, new EventHandler<LE_GUIInterface.EventHandlers.TerrainChangeHeightModeEventArgs>(OnTerrainChangeHeightModeChanged));
			LE_GUIInterface.EventHandlers events8 = LE_GUIInterface.Instance.events;
			events8.OnTerrainIsDirectedSmoothChanged = (EventHandler<LE_GUIInterface.EventHandlers.BoolEventArgs>)Delegate.Remove(events8.OnTerrainIsDirectedSmoothChanged, new EventHandler<LE_GUIInterface.EventHandlers.BoolEventArgs>(OnTerrainIsDirectedSmoothChanged));
			LE_GUIInterface.EventHandlers events9 = LE_GUIInterface.Instance.events;
			events9.OnTerrainEditModeBtn = (EventHandler<LE_GUIInterface.EventHandlers.TerrainEditModeEventArgs>)Delegate.Remove(events9.OnTerrainEditModeBtn, new EventHandler<LE_GUIInterface.EventHandlers.TerrainEditModeEventArgs>(OnTerrainEditModeBtn));
			LE_GUIInterface.EventHandlers events10 = LE_GUIInterface.Instance.events;
			events10.OnTerrainReadSetHeightBtn = (EventHandler)Delegate.Remove(events10.OnTerrainReadSetHeightBtn, new EventHandler(OnTerrainReadSetHeightBtn));
		}
	}

	public override void Update()
	{
		if (m_doRebuildTerrainTab)
		{
			m_doRebuildTerrainTab = false;
			InitTerrainValues();
		}
	}

	private void OnLoadedLevelInEditor(object p_obj, EventArgs p_args)
	{
		LE_GUIInterface.Delegates.ETerrainUIMode terrainUIMode = GetTerrainUIMode();
		if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainUIMode(terrainUIMode);
		}
		else
		{
			Debug.LogWarning("LE_LogicTerrain: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. Set it to show the create UI if the loaded level has no terrain and the edit UI if the loaded level has a terrain!");
		}
		Debug.Log("Setup complete");
		m_doRebuildTerrainTab = true;
	}

	private void OnTerrainBrushChanged(object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)
	{
		m_selectedBrushIndex = p_args.Value;
		m_GUI3dTerrain.BrushAlphaTexture = m_confT.Brushes[m_selectedBrushIndex];
		m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
	}

	private void OnTerrainHeightmapTextureChanged(object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)
	{
		m_selectedHeightmapIndex = p_args.Value;
		m_GUI3dTerrain.Heightmap = m_confT.Heightmaps[m_selectedHeightmapIndex];
		m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
	}

	private void OnTerrainEditBrushSizeChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
	{
		float value = p_args.Value;
		value *= value;
		value = Mathf.Max(0.002f, value);
		m_GUI3dTerrain.Size = value;
		m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
	}

	private void OnTerrainEditBrushAmountChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
	{
		float value = p_args.Value;
		value = Mathf.Max(0.002f, value);
		m_GUI3dTerrain.Amount = (m_isHeightRaise ? value : (0f - value));
	}

	private void OnTerrainEditBrushTargetValueChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
	{
		m_GUI3dTerrain.TargetRelativeValue = p_args.Value;
	}

	private void OnTerrainEditDirectionChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
	{
		m_GUI3dTerrain.DirectedSmoothAngle = Mathf.Round(p_args.Value * 360f / 22.5f) * 22.5f;
		m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
	}

	private void OnTerrainChangeHeightModeChanged(object p_obj, LE_GUIInterface.EventHandlers.TerrainChangeHeightModeEventArgs p_args)
	{
		if (p_args.ChangeHeightMode == LE_GUIInterface.EventHandlers.ETerrainChangeHeightMode.RAISE)
		{
			m_isHeightRaise = true;
			m_GUI3dTerrain.Amount = Mathf.Abs(m_GUI3dTerrain.Amount);
		}
		else
		{
			m_isHeightRaise = false;
			m_GUI3dTerrain.Amount = 0f - Mathf.Abs(m_GUI3dTerrain.Amount);
		}
	}

	private void OnTerrainIsDirectedSmoothChanged(object p_obj, LE_GUIInterface.EventHandlers.BoolEventArgs p_args)
	{
		m_GUI3dTerrain.IsDirectedSmooth = p_args.Value;
		m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
	}

	private void OnTerrainEditModeBtn(object p_obj, LE_GUIInterface.EventHandlers.TerrainEditModeEventArgs p_args)
	{
		m_GUI3dTerrain.EditMode = p_args.EditMode;
	}

	private void OnTerrainReadSetHeightBtn(object sender, EventArgs e)
	{
		if (LE_GUIInterface.Instance.delegates.SetTerrainIsReadingSetHeight != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainIsReadingSetHeight(obj: true);
		}
		else
		{
			Debug.LogWarning("LE_LogicTerrain: OnTerrainReadSetHeightBtn: you have not set the LE_GUIInterface.delegates.SetTerrainIsReadingSetHeight delegate. Set it if you want to give visual feedback to the user showing if the terrain set height is currently being read!");
		}
		m_GUI3dTerrain.IsReadingTerrainSetHeight = true;
		m_GUI3dTerrain.StartCoroutine(ReadSetHeightRoutine());
	}

	private void CheckParameters()
	{
		if (string.IsNullOrEmpty(LayerMask.LayerToName(m_confT.TerrainLayer)))
		{
			Debug.LogWarning("LE_GUILogicTerrain: Inspector property 'LE_ConfigTerrain.TerrainLayer' is set to '" + m_confT.TerrainLayer + "', but this layer has no name in 'Tags & Layers' set. Please set the name of this layer to 'Terrain' or change the value of the 'LE_ConfigTerrain.TerrainLayer' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'.");
		}
		else if (LayerMask.LayerToName(m_confT.TerrainLayer) != "Terrain")
		{
			Debug.LogWarning("LE_GUILogicTerrain: Inspector property 'LE_ConfigTerrain.TerrainLayer' is set to '" + m_confT.TerrainLayer + "', but the name of this layer is '" + LayerMask.LayerToName(m_confT.TerrainLayer) + "'. Is this intended? If not please set the name of this layer to 'Terrain' or change the value of the 'LE_ConfigTerrain.TerrainLayer' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'.");
		}
	}

	private void InitTerrainValues()
	{
		m_selectedBrushIndex = 0;
		if (m_confT.Brushes.Length != 0)
		{
			m_GUI3dTerrain.BrushAlphaTexture = m_confT.Brushes[0];
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainBrushes != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainBrushes(m_confT.Brushes, m_selectedBrushIndex);
		}
		else
		{
			Debug.LogError("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainBrushes delegate. You must set this delegate to show and update the brush texture selection in your UI!");
		}
		m_selectedHeightmapIndex = 0;
		if (m_confT.Heightmaps.Length != 0)
		{
			m_GUI3dTerrain.Heightmap = m_confT.Heightmaps[0];
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainHeightmapTextures != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainHeightmapTextures(m_confT.Heightmaps.Select((Heightmap h) => h.Preview).ToArray(), m_selectedHeightmapIndex);
		}
		else
		{
			Debug.LogError("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainHeightmapTextures delegate. You must set this delegate to show and update the brush texture selection in your UI!");
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushSize != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainEditBrushSize(Mathf.Sqrt(m_GUI3dTerrain.Size));
		}
		else
		{
			Debug.LogWarning("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainEditHeightmap delegate.");
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushAmount != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainEditBrushAmount(Mathf.Abs(m_GUI3dTerrain.Amount));
		}
		else
		{
			Debug.LogWarning("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushAmount delegate. If you have a brush amount slider, then its value might be different from the value used by the level editor.");
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue(m_GUI3dTerrain.TargetRelativeValue);
		}
		else
		{
			Debug.LogWarning("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushTargetValue delegate. If you have a brush target value slider, then its value might be different from the value used by the level editor.");
		}
	}

	private LE_GUIInterface.Delegates.ETerrainUIMode GetTerrainUIMode()
	{
		if (m_GUI3dTerrain.TerrainManager != null)
		{
			return LE_GUIInterface.Delegates.ETerrainUIMode.EDIT;
		}
		return LE_GUIInterface.Delegates.ETerrainUIMode.CREATE;
	}

	private IEnumerator ReadSetHeightRoutine()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		while (m_GUI3dTerrain.IsReadingTerrainSetHeight)
		{
			if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue(m_GUI3dTerrain.TargetRelativeValue);
			}
			yield return wait;
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainIsReadingSetHeight != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainIsReadingSetHeight(obj: false);
		}
		else
		{
			Debug.LogWarning("LE_LogicTerrain: OnTerrainReadSetHeightBtn: you have not set the LE_GUIInterface.delegates.SetTerrainIsReadingSetHeight delegate. Set it if you want to give visual feedback to the user showing if the terrain set height is currently being read!");
		}
		if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue != null)
		{
			LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue(m_GUI3dTerrain.TargetRelativeValue);
		}
		else
		{
			Debug.LogWarning("LE_GUILogicTerrain: ReadSetHeightRoutine: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushTargetValue delegate. If you have a brush target value slider, then its value is now different from the value used by the level editor, because the terrain set height was read.");
		}
	}
}
