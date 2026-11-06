using System;
using LE_LevelEditor.Core;
using UnityEngine;

namespace LE_LevelEditor.UI;

public class LE_GUIInterface : MonoBehaviour
{
	public class Delegates
	{
		public enum ETerrainUIMode
		{
			CREATE,
			EDIT
		}

		public enum EDraggedObjectState
		{
			NONE,
			IN_3D_PREVIEW,
			NOT_PLACEABLE
		}

		public delegate bool GetBoolFunction();

		public delegate float GetFloatFunction();

		public GetBoolFunction IsCursorOverUI;

		public GetFloatFunction GetCameraPerspectiveGizmoRightPixelOffset;

		public GetFloatFunction GetObliqueCameraPerspectiveRightPixelOffset;

		public Action<Texture2D[], int> SetTerrainHeightmapTextures;

		public Action<Texture2D[], int> SetTerrainBrushes;

		public Action<float> SetTerrainEditBrushSize;

		public Action<float> SetTerrainEditBrushAmount;

		public Action<float> SetTerrainEditBrushTargetValue;

		public Action<bool> SetTerrainIsReadingSetHeight;

		public Action<ETerrainUIMode> SetTerrainUIMode;

		public Action<LE_ObjectMap> SetObjects;

		public GetBoolFunction IsObjectDragged;

		public Action<string> SetDraggableObjectMessage;

		public Action<EDraggedObjectState> SetDraggableObjectState;

		public Action<bool> SetIsSelectedObjectFocusBtnInteractable;

		public Action<bool> SetIsSelectedObjectDuplicateBtnInteractable;

		public Action<bool> SetIsSelectedObjectDeleteBtnInteractable;

		public Action<Action<bool>> ShowPopupConfirmDeleteObject;

		public void OnDestroy()
		{
			IsCursorOverUI = null;
			GetCameraPerspectiveGizmoRightPixelOffset = null;
			GetObliqueCameraPerspectiveRightPixelOffset = null;
			SetTerrainHeightmapTextures = null;
			SetTerrainBrushes = null;
			SetTerrainEditBrushSize = null;
			SetTerrainEditBrushAmount = null;
			SetTerrainEditBrushTargetValue = null;
			SetTerrainIsReadingSetHeight = null;
			SetTerrainUIMode = null;
			IsObjectDragged = null;
			SetObjects = null;
			SetDraggableObjectMessage = null;
			SetDraggableObjectState = null;
			SetIsSelectedObjectFocusBtnInteractable = null;
			SetIsSelectedObjectDuplicateBtnInteractable = null;
			SetIsSelectedObjectDeleteBtnInteractable = null;
			ShowPopupConfirmDeleteObject = null;
		}
	}

	public class EventHandlers
	{
		public enum ETerrainChangeHeightMode
		{
			RAISE,
			LOWER
		}

		public class BoolEventArgs : EventArgs
		{
			public readonly bool Value;

			public BoolEventArgs(bool p_value)
			{
				Value = p_value;
			}
		}

		public class IntEventArgs : EventArgs
		{
			public readonly int Value;

			public IntEventArgs(int p_value)
			{
				Value = p_value;
			}
		}

		public class FloatEventArgs : EventArgs
		{
			public readonly float Value;

			public FloatEventArgs(float p_value)
			{
				Value = p_value;
			}
		}

		public class StringEventArgs : EventArgs
		{
			public readonly string Value;

			public StringEventArgs(string p_value)
			{
				Value = p_value;
			}
		}

		public class ColorEventArgs : EventArgs
		{
			public readonly Color Value;

			public ColorEventArgs(Color p_value)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				Value = p_value;
			}
		}

		public class TextureEventArgs : EventArgs
		{
			public readonly Texture2D Value;

			public TextureEventArgs(Texture2D p_value)
			{
				Value = p_value;
			}
		}

		public class EditModeEventArgs : EventArgs
		{
			public readonly LE_EEditMode EditMode;

			public EditModeEventArgs(LE_EEditMode p_editMode)
			{
				EditMode = p_editMode;
			}
		}

		public class TerrainEditModeEventArgs : EventArgs
		{
			public readonly LE_ETerrainEditMode EditMode;

			public TerrainEditModeEventArgs(LE_ETerrainEditMode p_editMode)
			{
				EditMode = p_editMode;
			}
		}

		public class TerrainChangeHeightModeEventArgs : EventArgs
		{
			public readonly ETerrainChangeHeightMode ChangeHeightMode;

			public TerrainChangeHeightModeEventArgs(ETerrainChangeHeightMode p_changeHeightMode)
			{
				ChangeHeightMode = p_changeHeightMode;
			}
		}

		public class ObjectEditModeEventArgs : EventArgs
		{
			public readonly LE_EObjectEditMode EditMode;

			public ObjectEditModeEventArgs(LE_EObjectEditMode p_editMode)
			{
				EditMode = p_editMode;
			}
		}

		public class ObjectSelectDraggableEventArgs : EventArgs
		{
			public readonly LE_Object ObjPrefab;

			public ObjectSelectDraggableEventArgs(LE_Object p_objPrefab)
			{
				ObjPrefab = p_objPrefab;
			}
		}

		public EventHandler<EditModeEventArgs> OnEditModeBtn;

		public EventHandler<IntEventArgs> OnTerrainHeightmapTextureChanged;

		public EventHandler<IntEventArgs> OnTerrainBrushChanged;

		public EventHandler<FloatEventArgs> OnTerrainEditBrushSizeChanged;

		public EventHandler<FloatEventArgs> OnTerrainEditBrushAmountChanged;

		public EventHandler<FloatEventArgs> OnTerrainEditBrushTargetValueChanged;

		public EventHandler<FloatEventArgs> OnTerrainEditDirectionChanged;

		public EventHandler<TerrainChangeHeightModeEventArgs> OnTerrainChangeHeightModeChanged;

		public EventHandler<BoolEventArgs> OnTerrainIsDirectedSmoothChanged;

		public EventHandler<TerrainEditModeEventArgs> OnTerrainEditModeBtn;

		public EventHandler OnTerrainReadSetHeightBtn;

		public EventHandler<ObjectEditModeEventArgs> OnObjectEditModeBtn;

		public EventHandler<ObjectSelectDraggableEventArgs> OnObjectSelectDraggable;

		public EventHandler OnSelectedObjectFocusBtn;

		public EventHandler OnSelectedObjectDuplicateBtn;

		public EventHandler OnSelectedObjectDeleteBtn;

		public EventHandler OnTerrainGenerateShoremap;

		public EventHandler OnTerrainGenerateCustomHeightmap;

		public EventHandler OnTerrainGeneratePerlin;

		public EventHandler OnTerrainGenerateNormalise;

		public EventHandler OnTerrainGenerateIsland;

		public EventHandler OnTerrainSmooth;

		public void OnDestroy()
		{
			OnEditModeBtn = null;
			OnTerrainHeightmapTextureChanged = null;
			OnTerrainBrushChanged = null;
			OnTerrainEditBrushSizeChanged = null;
			OnTerrainEditBrushAmountChanged = null;
			OnTerrainEditBrushTargetValueChanged = null;
			OnTerrainEditDirectionChanged = null;
			OnTerrainChangeHeightModeChanged = null;
			OnTerrainIsDirectedSmoothChanged = null;
			OnTerrainEditModeBtn = null;
			OnTerrainReadSetHeightBtn = null;
			OnObjectEditModeBtn = null;
			OnObjectSelectDraggable = null;
			OnSelectedObjectFocusBtn = null;
			OnSelectedObjectDuplicateBtn = null;
			OnSelectedObjectDeleteBtn = null;
			OnTerrainGenerateShoremap = null;
			OnTerrainGenerateCustomHeightmap = null;
			OnTerrainGeneratePerlin = null;
			OnTerrainGenerateNormalise = null;
			OnTerrainGenerateIsland = null;
			OnTerrainSmooth = null;
		}
	}

	private static LE_GUIInterface s_instance;

	public readonly Delegates delegates = new Delegates();

	public readonly EventHandlers events = new EventHandlers();

	public static LE_GUIInterface Instance => s_instance;

	private void Awake()
	{
		if ((Object)(object)s_instance != (Object)null)
		{
			Debug.LogError((object)"LE_GUIInterface: there is already an instance of LE_GUIInterface in this scene!");
		}
		s_instance = this;
	}

	private void OnDestroy()
	{
		if ((Object)(object)s_instance == (Object)(object)this)
		{
			s_instance = null;
		}
		events.OnDestroy();
		delegates.OnDestroy();
	}

	public void OnEditModeBtn(int p_editMode)
	{
		if (events.OnEditModeBtn != null)
		{
			events.OnEditModeBtn(this, new EventHandlers.EditModeEventArgs((LE_EEditMode)p_editMode));
		}
	}

	public void OnTerrainHeightmapTextureChanged(int p_heightmapTextureIndex)
	{
		if (events.OnTerrainHeightmapTextureChanged != null)
		{
			events.OnTerrainHeightmapTextureChanged(this, new EventHandlers.IntEventArgs(p_heightmapTextureIndex));
		}
	}

	public void OnTerrainBrushChanged(int p_brushIndex)
	{
		if (events.OnTerrainBrushChanged != null)
		{
			events.OnTerrainBrushChanged(this, new EventHandlers.IntEventArgs(p_brushIndex));
		}
	}

	public void OnTerrainEditBrushSizeChanged(float p_brushSize)
	{
		if (events.OnTerrainEditBrushSizeChanged != null)
		{
			events.OnTerrainEditBrushSizeChanged(this, new EventHandlers.FloatEventArgs(p_brushSize));
		}
	}

	public void OnTerrainEditBrushAmountChanged(float p_brushAmount)
	{
		if (events.OnTerrainEditBrushAmountChanged != null)
		{
			events.OnTerrainEditBrushAmountChanged(this, new EventHandlers.FloatEventArgs(p_brushAmount));
		}
	}

	public void OnTerrainEditBrushTargetValueChanged(float p_brushTargetValue)
	{
		if (events.OnTerrainEditBrushTargetValueChanged != null)
		{
			events.OnTerrainEditBrushTargetValueChanged(this, new EventHandlers.FloatEventArgs(p_brushTargetValue));
		}
	}

	public void OnTerrainEditDirectionChanged(float p_direction)
	{
		if (events.OnTerrainEditDirectionChanged != null)
		{
			events.OnTerrainEditDirectionChanged(this, new EventHandlers.FloatEventArgs(p_direction));
		}
	}

	public void OnTerrainChangeHeightModeChanged(bool p_isRaiseHeight)
	{
		if (events.OnTerrainChangeHeightModeChanged != null)
		{
			events.OnTerrainChangeHeightModeChanged(this, new EventHandlers.TerrainChangeHeightModeEventArgs((!p_isRaiseHeight) ? EventHandlers.ETerrainChangeHeightMode.LOWER : EventHandlers.ETerrainChangeHeightMode.RAISE));
		}
	}

	public void OnTerrainIsDirectedSmoothChanged(bool p_isDirectedSmooth)
	{
		if (events.OnTerrainIsDirectedSmoothChanged != null)
		{
			events.OnTerrainIsDirectedSmoothChanged(this, new EventHandlers.BoolEventArgs(p_isDirectedSmooth));
		}
	}

	public void OnTerrainEditModeBtn(int p_editMode)
	{
		if (events.OnTerrainEditModeBtn != null)
		{
			events.OnTerrainEditModeBtn(this, new EventHandlers.TerrainEditModeEventArgs((LE_ETerrainEditMode)p_editMode));
		}
	}

	public void OnTerrainReadSetHeightBtn()
	{
		if (events.OnTerrainReadSetHeightBtn != null)
		{
			events.OnTerrainReadSetHeightBtn(this, EventArgs.Empty);
		}
	}

	public void OnObjectEditModeBtn(int p_editMode)
	{
		if (events.OnObjectEditModeBtn != null)
		{
			events.OnObjectEditModeBtn(this, new EventHandlers.ObjectEditModeEventArgs((LE_EObjectEditMode)p_editMode));
		}
	}

	public void OnObjectSelectDraggable(LE_Object p_objPrefab)
	{
		if (events.OnObjectSelectDraggable != null)
		{
			events.OnObjectSelectDraggable(this, new EventHandlers.ObjectSelectDraggableEventArgs(p_objPrefab));
		}
	}

	public void OnSelectedObjectFocusBtn()
	{
		if (events.OnSelectedObjectFocusBtn != null)
		{
			events.OnSelectedObjectFocusBtn(this, EventArgs.Empty);
		}
	}

	public void OnSelectedObjectDuplicateBtn()
	{
		if (events.OnSelectedObjectDuplicateBtn != null)
		{
			events.OnSelectedObjectDuplicateBtn(this, EventArgs.Empty);
		}
	}

	public void OnSelectedObjectDeleteBtn()
	{
		if (events.OnSelectedObjectDeleteBtn != null)
		{
			events.OnSelectedObjectDeleteBtn(this, EventArgs.Empty);
		}
	}

	public void OnTerrainGenerateShoremap()
	{
		if (events.OnTerrainGenerateShoremap != null)
		{
			events.OnTerrainGenerateShoremap(this, EventArgs.Empty);
		}
	}

	public void OnTerrainGenerateCustomHeightmap()
	{
		if (events.OnTerrainGenerateCustomHeightmap != null)
		{
			events.OnTerrainGenerateCustomHeightmap(this, EventArgs.Empty);
		}
	}

	public void OnTerrainGeneratePerlin()
	{
		if (events.OnTerrainGeneratePerlin != null)
		{
			events.OnTerrainGeneratePerlin(this, EventArgs.Empty);
		}
	}

	public void OnTerrainGenerateNormalise()
	{
		if (events.OnTerrainGenerateNormalise != null)
		{
			events.OnTerrainGenerateNormalise(this, EventArgs.Empty);
		}
	}

	public void OnTerrainGenerateIsland()
	{
		if (events.OnTerrainGenerateIsland != null)
		{
			events.OnTerrainGenerateIsland(this, EventArgs.Empty);
		}
	}

	public void OnTerrainSmooth()
	{
		if (events.OnTerrainSmooth != null)
		{
			events.OnTerrainSmooth(this, EventArgs.Empty);
		}
	}
}
