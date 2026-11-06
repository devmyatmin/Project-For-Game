using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Beam.AccountServices;
using Beam.Language;
using Beam.Terrain;
using Beam.UI.MapEditor;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Newtonsoft.Json.Linq;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Beam.UI;

public class CartographerMenuPresenter : MonoBehaviour
{
	[SerializeField]
	private MainMenuPresenter _mainMenuPresenter;

	[SerializeField]
	private CartographerWorldMapTooltipPresenter _tooltipPresenter;

	[SerializeField]
	private CartographerMenuViewAdapterBase _view;

	private IViewAdapter _parent;

	private bool _validSave;

	private bool _dirty;

	private bool _suspend;

	private IMapListElement _currentDraggingMapListElement;

	private IWorldMapSlot _currentMapSlot;

	private List<IMapListElement> _currentMapElements = new List<IMapListElement>();

	private List<string> _reservedZoneIds = new List<string>();

	private int _mapsInserted;

	private IEnumerable<string> _remoteMapPaths;

	private bool _retrievedUGC;

	private bool _loadedUGC;

	private LoadingResult _loadingUGCResult;

	public event Action<CartographerMenuPresenter, CartographerEventArgs> WorldApplied;

	public event EmptyEventHandler ShowView;

	public event EmptyEventHandler HideView;

	public event EmptyEventHandler CreatedNewWorld;

	private Player GetPlayerInput()
	{
		return ReInput.players.GetPlayer(0);
	}

	protected virtual void OnWorldApplied()
	{
		WorldApplied?.Invoke(this, new CartographerEventArgs
		{
			MapsInserted = _mapsInserted
		});
	}

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	protected virtual void OnCreatedNewWorld()
	{
		CreatedNewWorld?.Invoke();
	}

	private void Awake()
	{
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		_view.LocalMapsButton.Click += LocalMapsButton_Click;
		_view.RemoteMapsButton.Click += RemoteMapsButton_Click;
		SetControlEvents();
	}

	public void Initialize(IViewAdapter parent)
	{
		_parent = parent;
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView += Hide;
		}
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
		if (!BeamTeam.IsNullOrDestroyed(_parent))
		{
			_parent.HideView -= Hide;
		}
		_view.ShowView -= View_ShowView;
		_view.HideView -= View_HideView;
		_view.LocalMapsButton.Click -= LocalMapsButton_Click;
		_view.RemoteMapsButton.Click -= RemoteMapsButton_Click;
		ShowView = null;
		HideView = null;
		WorldApplied = null;
		ClearControlEvents();
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex != 1)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	public void Show()
	{
		ControllerFocus.Instance?.Subscribe(_view);
		GUserContentProvider.Get().LoadedUserContent += UserContentProvider_LoadedUserContent;
		GUserContentProvider.Get().RequestUserContent();
		_view.Show();
		_view.ApplyButton.Enabled = false;
		_mapsInserted = 0;
		_currentMapSlot = null;
		_currentDraggingMapListElement = null;
		DialogueBoxViewAdapter dialogue = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_LOADING_TITLE", "CARTOGRAPHER_MENU_DB_LOADING_WORLD_DESC", DialogueButton.None);
		InvokeDelayed(0.3f, () =>
		{
			Load(dialogue.Destroy);
		});
	}

	public void Hide()
	{
		ControllerFocus.Instance?.Unsubscribe(_view);
		GUserContentProvider.Get().LoadedUserContent -= UserContentProvider_LoadedUserContent;
		_tooltipPresenter.Hide();
		_view.Hide();
	}

	private void SetControlEvents()
	{
		_view.SearchInputField.ValueChanged += SearchInputField_ValueChanged;
		_view.RandomizeButton.Click += RandomizeButton_Click;
		_view.ApplyButton.Click += ApplyButton_Click;
		_view.CreateIslandButton.Click += CreateIslandButton_Click;
		_view.CancelButton.Click += CancelButton_Click;
	}

	private void ClearControlEvents()
	{
		_view.SearchInputField.ValueChanged -= SearchInputField_ValueChanged;
		_view.RandomizeButton.Click -= RandomizeButton_Click;
		_view.ApplyButton.Click -= ApplyButton_Click;
		_view.CreateIslandButton.Click -= CreateIslandButton_Click;
		_view.CancelButton.Click -= CancelButton_Click;
	}

	private void SearchInputField_ValueChanged(string input)
	{
		string text = _view.SearchInputField.Text.ToUpper();
		if (_view.SearchInputField.Text != text)
		{
			_view.SearchInputField.Text = text;
		}
		List<IMapListElement> currentMapElements = _currentMapElements;
		if (currentMapElements != null && currentMapElements.Count == 0)
		{
			return;
		}
		List<IMapListElement> list = (from element in _currentMapElements
			where element.NameLabel.Text.StartsWith(input, ignoreCase: true, CultureInfo.InvariantCulture)
			orderby element.NameLabel.Text.Length
			select element).ToList();
		foreach (IMapListElement currentMapElement in _currentMapElements)
		{
			bool active = list.Contains(currentMapElement);
			currentMapElement.gameObject.SetActive(active);
		}
	}

	private void RandomizeButton_Click(object sender)
	{
		ShowRandomizeWorldDialogue();
	}

	private void CreateIslandButton_Click(object sender)
	{
		if (_dirty)
		{
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_WARNING_TITLE", "CARTOGRAPHER_MENU_DB_WARNING_DISCARD_CHANGES_DESC", DialogueButton.YesNo, DialogueIcon.Warning).Accept += CreateIsland;
		}
		else
		{
			CreateIsland();
		}
	}

	private void ApplyButton_Click(object sender)
	{
		DialogueBoxViewAdapter dialogue = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_SAVING_TITLE", "CARTOGRAPHER_MENU_DB_SAVING_WORLD_DESC", DialogueButton.None);
		InvokeDelayed(0.3f, () =>
		{
			SaveWorld(OnWorldApplied);
			dialogue.Destroy();
		});
	}

	private void CancelButton_Click(object sender)
	{
		if (_dirty)
		{
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_WARNING_TITLE", "CARTOGRAPHER_MENU_DB_WARNING_DISCARD_CHANGES_DESC", DialogueButton.YesNo, DialogueIcon.Warning).Accept += Hide;
		}
		else
		{
			Hide();
		}
		Game.State = (GameState)0;
	}

	private void Update()
	{
		if (!_suspend && !DialogueBox.Notifying)
		{
			Player playerInput = GetPlayerInput();
			if (playerInput.GetButtonDown(31) && _view.Visible)
			{
				CancelButton_Click(null);
			}
			if (_currentMapSlot != null && playerInput.GetButtonDown(41))
			{
				RemoveWorldMap(_currentMapSlot);
			}
		}
	}

	private void UpdateWorldMap(IWorldMapSlot slot)
	{
		Map editorMap = Maps.AllMapList.FirstOrDefault((Map map) => map.EditorData.Id == slot.Map.EditorData.Id && map.EditorData.Source == slot.Map.EditorData.Source);
		if (editorMap != null)
		{
			string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_UPDATE_ZONE_DESC", slot.Map.EditorData.VersionNumber.ToString("F1"), editorMap.EditorData.VersionNumber.ToString("F1"));
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_UPDATE_TITLE", message, DialogueButton.YesNo).Accept += () =>
			{
				World.ReplaceMap(slot.Map, editorMap);
				UpdateWorldMapSlot(slot, editorMap);
				SetDirty();
			};
		}
	}

	private void ReplaceWorldMap(IWorldMapSlot slot, Map map)
	{
		string text = Reflection.GetAttribute<DescriptionAttribute>(slot.Map.EditorData.Source).Text;
		string text2 = Reflection.GetAttribute<DescriptionAttribute>(map.EditorData.Source).Text;
		string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_REPLACE_MAP_DESC", slot.Map.EditorData.Name, text, map.EditorData.Name, text2);
		DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_REPLACE_TITLE", message, DialogueButton.YesNo).Accept += () =>
		{
			World.ReplaceMap(slot.Map, map);
			UpdateWorldMapSlot(slot, map);
			SetDirty();
		};
	}

	private void RemoveWorldMap(IWorldMapSlot slot)
	{
		if (slot.Map.EditorData.Id.Contains("INTERNAL_PROCEDURAL") || !slot.Map.EditorData.Valid)
		{
			return;
		}
		string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_REMOVE_DESC", slot.Map.EditorData.Name);
		DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_REMOVE_TITLE", message, DialogueButton.YesNo, DialogueIcon.Warning).Accept += () =>
		{
			DialogueBoxViewAdapter d = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_REMOVING_TITLE", "CARTOGRAPHER_MENU_DB_REMOVING_DESC", DialogueButton.None);
			InvokeDelayed(0.3f, () =>
			{
				if (World.ReplaceMapWithProceduralMap(slot.Map, out var proceduralMap))
				{
					UpdateWorldMapSlot(slot, proceduralMap);
					SetDirty();
				}
				d.Destroy();
			});
		};
	}

	private void CreateWorldMapSlots()
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		BeamTeam.DestroyAllChildren(_view.MapSlotsContainer.transform);
		ZoomComponent componentInParent = _view.MapSlotsContainer.GetComponentInParent<ZoomComponent>();
		componentInParent.Reset();
		for (int i = 0; i < 49; i++)
		{
			Map map = World.MapList[i];
			UWorldMapSlot uWorldMapSlot = Object.Instantiate<UWorldMapSlot>(_view.MapSlotPrefab);
			((Component)uWorldMapSlot).transform.SetParent(_view.MapSlotsContainer.transform, false);
			uWorldMapSlot.SetMap(map);
			uWorldMapSlot.Click += WorldMapSlot_Click;
			uWorldMapSlot.Enter += WorldMapSlot_Enter;
			uWorldMapSlot.Leave += WorldMapSlot_Leave;
			uWorldMapSlot.Drop += WorldMapSlot_Drop;
			if ((_reservedZoneIds.Count == 0 && i == 0) || _reservedZoneIds.Contains(map.EditorData.Id) || map.EditorData.Id.Contains("INTERNAL_MISSION"))
			{
				uWorldMapSlot.Map.EditorData.Valid = false;
				uWorldMapSlot.Button.Enabled = false;
			}
			else
			{
				uWorldMapSlot.Map.EditorData.Valid = true;
				uWorldMapSlot.Button.Enabled = true;
			}
			Vector2 val = World.GenerationZonePositons[i];
			RectTransform val2 = (RectTransform)((Component)uWorldMapSlot).transform;
			((Transform)val2).localPosition = new Vector3(val.x, val.y, 0f);
			val2.sizeDelta = new Vector2(257f, 257f);
		}
		componentInParent.ScaleToFitContent();
		componentInParent.SetScale(0.5f);
		_currentMapSlot = null;
	}

	private void RefreshWorldMapSlots()
	{
		UWorldMapSlot[] componentsInChildren = _view.MapSlotsContainer.GetComponentsInChildren<UWorldMapSlot>();
		foreach (UWorldMapSlot obj in componentsInChildren)
		{
			obj.SetState(obj.Map.EditorData.State);
		}
	}

	public void UpdateWorldMapSlot(IWorldMapSlot slot, Map map)
	{
		slot.SetMap(map);
		WorldMapSlot_Click(slot, null);
		slot.Button.Select();
		_mapsInserted++;
	}

	private void CreateMapListElements(IEnumerable<Map> maps, Transform container)
	{
		_currentMapElements.Clear();
		BeamTeam.DestroyAllChildren(container);
		foreach (Map map in maps)
		{
			IMapListElement mapListElement = _view.CreateMapListElement(container);
			mapListElement.BeginDrag += MapListElement_BeginDrag;
			mapListElement.Drag += MapListElement_Drag;
			mapListElement.EndDrag += MapListElement_EndDrag;
			mapListElement.EditClick += MapListElement_EditClick;
			mapListElement.DeleteClick += MapListElement_DeleteClick;
			((Component)mapListElement.EditButton.transform).gameObject.SetActive(_view.LocalMapListContainer.activeSelf);
			((Component)mapListElement.DeleteButton.transform).gameObject.SetActive(_view.LocalMapListContainer.activeSelf);
			PopulateMapListElement(mapListElement, map);
			_currentMapElements.Add(mapListElement);
		}
	}

	private void PopulateMapListElement(IMapListElement element, Map map)
	{
		element.Map = map;
		element.NameLabel.Text = element.Map.EditorData.Name;
		element.AuthorLabel.Text = element.Map.EditorData.Author;
		element.VersionLabel.Text = element.Map.EditorData.VersionNumber.ToString("F1");
		element.DescriptionLabel.Text = element.Map.EditorData.Description;
		element.PreviewImage.Texture = (Texture)(object)element.Map.EditorData.SmallPreviewImage;
	}

	private void MapListElement_EditClick(IMapListElement sender, PointerEventData eventData)
	{
		Map map = sender.Map;
		if (_dirty)
		{
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_WARNING_TITLE", "CARTOGRAPHER_MENU_DB_WARNING_DISCARD_CHANGES_DESC", DialogueButton.YesNo, DialogueIcon.Warning).Accept += () =>
			{
				EditIsland(map);
			};
		}
		else
		{
			EditIsland(map);
		}
	}

	private void MapListElement_DeleteClick(IMapListElement sender, PointerEventData eventData)
	{
		Map map = sender.Map;
		string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_WARNING_DELETE_MAP_DESC", map.EditorData.Name);
		DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_WARNING_TITLE", message, DialogueButton.YesNo, DialogueIcon.Warning).Accept += () =>
		{
			MapListElement_OnDelete(sender);
		};
	}

	private void MapListElement_OnDelete(IMapListElement mapListElement)
	{
		Maps.DeleteUserMap(mapListElement.Map);
		_currentMapElements.Remove(mapListElement);
		BeamTeam.Destroy<GameObject>(mapListElement.gameObject);
		UpdateMapsStatus();
		RefreshWorldMapSlots();
	}

	private void MapListElement_BeginDrag(IMapListElement sender, PointerEventData eventData)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		_currentDraggingMapListElement = sender;
		_view.MapHandle.HandleSprite.Texture = _currentDraggingMapListElement.PreviewImage.Texture;
		_view.MapHandle.SetSize(Vector2.one * 257f);
		_view.MapHandle.SetScale(Vector2.op_Implicit(_view.MapSlotsContainer.transform.localScale));
		_view.MapHandle.Show();
	}

	private void MapListElement_Drag(IMapListElement sender, PointerEventData eventData)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_view.MapHandle.SetPosition(eventData.pointerCurrentRaycast.screenPosition);
	}

	private void MapListElement_EndDrag(IMapListElement sender, PointerEventData eventData)
	{
		_currentDraggingMapListElement = null;
		_view.MapHandle.Hide();
	}

	private void WorldMapSlot_Click(IWorldMapSlot sender, PointerEventData eventData)
	{
		_currentMapSlot = sender;
	}

	private void WorldMapSlot_Enter(IWorldMapSlot sender, PointerEventData eventData)
	{
		_view.InfoLabel.Text = "CARTOGRAPHER_MENU_LABEL_SELECT_AND_PRESS_TO_REMOVE_DESC";
		_tooltipPresenter.Show(sender, eventData);
	}

	private void WorldMapSlot_Leave(IWorldMapSlot sender, PointerEventData eventData)
	{
		_view.InfoLabel.Text = "CARTOGRAPHER_MENU_LABEL_DRAG_AND_DROP_DESC";
		_tooltipPresenter.Hide();
	}

	private void WorldMapSlot_Drop(IWorldMapSlot sender, PointerEventData eventData)
	{
		if (!sender.Map.EditorData.Valid || BeamTeam.IsNullOrDestroyed(_currentDraggingMapListElement))
		{
			return;
		}
		Map map = _currentDraggingMapListElement.Map;
		if (sender.Map.EditorData.Id != map.EditorData.Id && !sender.Map.EditorData.Id.Contains("INTERNAL_PROCEDURAL"))
		{
			if (!ValidateDuplicateMap(map))
			{
				string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_SORRY_MAP_EXISTS_DESC", map.EditorData.Name);
				DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_SORRY_TITLE", message, DialogueButton.Ok);
			}
			else
			{
				ReplaceWorldMap(sender, map);
			}
		}
		else if (sender.Map.EditorData.VersionNumber != map.EditorData.VersionNumber && !sender.Map.EditorData.Id.Contains("INTERNAL_PROCEDURAL"))
		{
			if (sender.Map.EditorData.Source == map.EditorData.Source)
			{
				_currentMapSlot = sender;
				UpdateWorldMap(_currentMapSlot);
			}
			else
			{
				ReplaceWorldMap(sender, map);
			}
		}
		else if (!ValidateDuplicateMap(map))
		{
			string message2 = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_SORRY_MAP_EXISTS_DESC", map.EditorData.Name);
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_SORRY_TITLE", message2, DialogueButton.Ok);
		}
		else
		{
			World.ReplaceMap(sender.Map, map);
			UpdateWorldMapSlot(sender, map);
			SetDirty();
		}
		_tooltipPresenter.Hide();
	}

	private void Load(Action onComplete)
	{
		_validSave = SaveManager.ValidateSaveFile((GameMode)0) || SaveManager.ValidateSaveFile((GameMode)1) || SaveManager.ValidateSaveFile((GameMode)4);
		LoadingResult loadingResult = LoadMaps();
		if (!loadingResult.Succeeded)
		{
			string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_ERROR_LOADING_MAPS_DESC", StringTools.Truncate(loadingResult.Message));
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				InvokeDelayed(0.3f, () =>
				{
					LoadWorld();
					onComplete?.Invoke();
				});
			};
		}
		else
		{
			LoadWorld();
			onComplete?.Invoke();
		}
	}

	private LoadingResult LoadMaps()
	{
		LoadingResult loadingResult = new LoadingResult
		{
			Succeeded = true
		};
		Maps.LoadDefaultMaps(out var result);
		if (!result.Succeeded)
		{
			loadingResult.Succeeded = false;
			loadingResult.Message = result.Message;
		}
		Maps.LoadLocalMaps(out var result2);
		if (!result2.Succeeded)
		{
			loadingResult.Succeeded = false;
			loadingResult.Message += result2.Message;
		}
		LoadRemoteMaps();
		return loadingResult;
	}

	private void LoadWorld()
	{
		if (!World.CheckWorldSeedFileExists())
		{
			ShowCreateNewWorldDialogue();
			return;
		}
		LoadingResult loadingResult = World.LoadWorld();
		if (!loadingResult.Succeeded)
		{
			string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_ERROR_LOADING_WORLD_DESC", StringTools.Truncate(loadingResult.Message));
			DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning).Accept += ShowCreateNewWorldDialogue;
		}
		else
		{
			ResetWorldView();
		}
	}

	private void CreateWorld(string seedValue)
	{
		StrandedWorld.SetWorldSeed(seedValue);
		World.CreateWorld();
		SaveWorld();
		OnCreatedNewWorld();
	}

	public void CreateRandomWorld()
	{
		CreateWorld(string.Empty);
	}

	private void SaveWorld(Action onSuccess = null)
	{
		World.SaveWorld((LoadingResult result) =>
		{
			if (result.Succeeded)
			{
				UpdateMapsStatus();
				onSuccess?.Invoke();
				_mapsInserted = 0;
				ResetDirty();
			}
			else
			{
				string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_ERROR_SAVING_WORLD_DESC", StringTools.Truncate(result.Message));
				DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning);
			}
		});
	}

	private void ResetWorldView()
	{
		_view.InfoLabel.Text = "CARTOGRAPHER_MENU_LABEL_DRAG_AND_DROP_DESC";
		_view.WorldSeedLabel.Text = "#" + StrandedWorld.WORLD_SEED;
		LoadPlayerMaps();
		UpdateMapsStatus();
		CreateWorldMapSlots();
		_view.LocalMapsButton.PerformClick();
		_view.SelectFirstButton();
	}

	private void LocalMapsButton_Click(object sender)
	{
		_view.LocalMapListContainer.gameObject.SetActive(true);
		_view.RemoteMapListContainer.gameObject.SetActive(false);
		((Component)_view.RetrievingLabel.transform).gameObject.SetActive(false);
		((Component)_view.NoMapsLabel.transform).gameObject.SetActive(false);
		CreateMapListElements(Maps.UserMapList, _view.LocalMapListContainer.transform);
	}

	private void RemoteMapsButton_Click(object sender)
	{
		_view.LocalMapListContainer.gameObject.SetActive(false);
		_view.RemoteMapListContainer.gameObject.SetActive(true);
		((Component)_view.RetrievingLabel.transform).gameObject.SetActive(true);
		ShowRemoteMaps();
	}

	private void UserContentProvider_LoadedUserContent(LoadedContentArgs args)
	{
		if (_retrievedUGC)
		{
			return;
		}
		_remoteMapPaths = args.LoadedContent.Where((LoadedContent c) => Beam.Utilities.ExtensionMethods.Any_NonAlloc(c.Tags, (string tag) => tag.Equals(UgcTags.MAPS))).SelectMany((LoadedContent c) => Directory.GetDirectories(c.Path));
		_retrievedUGC = true;
	}

	private void LoadRemoteMaps()
	{
		if (_retrievedUGC && !_loadedUGC)
		{
			Maps.LoadRemoteMaps(_remoteMapPaths, out _loadingUGCResult);
			_loadedUGC = _loadingUGCResult.Succeeded || (Maps.RemoteMaps != null && Maps.RemoteMaps.Count() > 0);
		}
	}

	private void ShowRemoteMaps()
	{
		LoadRemoteMaps();
		if (_loadedUGC)
		{
			if (!_loadingUGCResult.Succeeded)
			{
				string message = Localization.GetLanguageHandler().Localize("CARTOGRAPHER_MENU_DB_ERROR_LOADING_MAPS_DESC", StringTools.Truncate(_loadingUGCResult.Message));
				DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning);
			}
			CreateMapListElements(Maps.RemoteMaps, _view.RemoteMapListContainer.transform);
			UpdateMapsStatus();
			((Component)_view.RetrievingLabel.transform).gameObject.SetActive(false);
			((Component)_view.NoMapsLabel.transform).gameObject.SetActive(_view.RemoteMapListContainer.transform.childCount == 0);
		}
	}

	private void SetDirty()
	{
		_dirty = true;
		_view.ApplyButton.Enabled = true;
	}

	private void ResetDirty()
	{
		_dirty = false;
		_view.ApplyButton.Enabled = false;
	}

	private void ShowCreateNewWorldDialogue()
	{
		DialogueBoxViewAdapter dialogue = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_CREATE_NEW_WORLD_TITLE", "CARTOGRAPHER_MENU_DB_CREATE_NEW_WORLD_DESC", DialogueButton.OkCancel, DialogueIcon.Information, DialogueType.Input);
		dialogue.InputField.CharacterLimit = 8;
		dialogue.InputField.Text = StrandedWorld.GetRandowWorldSeed().ToString();
		dialogue.Decline += Hide;
		dialogue.Accept += () =>
		{
			string seed = dialogue.InputField.Text;
			DialogueBoxViewAdapter d = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_CREATING_TITLE", "CARTOGRAPHER_MENU_DB_CREATING_DESC", DialogueButton.None);
			InvokeDelayed(0.3f, () =>
			{
				CreateWorld(seed);
				ResetWorldView();
				d.Destroy();
			});
		};
	}

	private void ShowRandomizeWorldDialogue()
	{
		DialogueBoxViewAdapter dialogue = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_WARNING_TITLE", "CARTOGRAPHER_MENU_DB_WARNING_RANDOMIZE_SEED_DESC", DialogueButton.OkCancel, DialogueIcon.Warning, DialogueType.Input);
		dialogue.InputField.CharacterLimit = 8;
		dialogue.InputField.Text = StrandedWorld.GetRandowWorldSeed().ToString();
		dialogue.Accept += () =>
		{
			string seed = dialogue.InputField.Text;
			DialogueBoxViewAdapter d = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_CREATING_TITLE", "CARTOGRAPHER_MENU_DB_CREATING_DESC", DialogueButton.None);
			InvokeDelayed(0.3f, () =>
			{
				DeleteSave();
				CreateWorld(seed);
				ResetWorldView();
				d.Destroy();
			});
		};
	}

	public void ShowRandomizeWorldDialogue_NoInput(Action success, Action cancel)
	{
		DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_WARNING_TITLE", "CARTOGRAPHER_MENU_DB_WARNING_RANDOMIZE_DESC", DialogueButton.OkCancel, DialogueIcon.Warning);
		dialogueBoxViewAdapter.Accept += () =>
		{
			DialogueBoxViewAdapter d = DialogueBox.Show(GetPlayerInput(), "CARTOGRAPHER_MENU_DB_CREATING_TITLE", "CARTOGRAPHER_MENU_DB_CREATING_DESC", DialogueButton.None);
			InvokeDelayed(0.3f, () =>
			{
				DeleteSave();
				CreateRandomWorld();
				d.Destroy();
				success?.Invoke();
			});
		};
		dialogueBoxViewAdapter.Cancel += () =>
		{
			cancel();
		};
		dialogueBoxViewAdapter.Decline += () =>
		{
			cancel();
		};
	}

	private void CreateIsland()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		MapEditorMenuPresenter.MAP_TO_EDIT = null;
		_view.CanvasGroup.Interactable = false;
		_suspend = true;
		Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, () =>
		{
			_mainMenuPresenter.LoadLevel(4, (LoadSceneMode)0);
		});
	}

	private void EditIsland(Map map)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		MapEditorMenuPresenter.MAP_TO_EDIT = map;
		_view.CanvasGroup.Interactable = false;
		_suspend = true;
		Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, () =>
		{
			_mainMenuPresenter.LoadLevel(4, (LoadSceneMode)0);
		});
	}

	private void LoadPlayerMaps()
	{
		_reservedZoneIds.Clear();
		if (_validSave)
		{
			LoadPlayerMap((GameMode)0);
			LoadPlayerMap((GameMode)1);
			LoadPlayerMap((GameMode)4);
		}
	}

	private void LoadPlayerMap(GameMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!SaveManager.TryGetSaveFilePath(mode, out var saveFilePath))
		{
			return;
		}
		JToken val = SaveManager.LoadSaveFileFast(saveFilePath)[(object)"Persistent"][(object)"StrandedWorld"];
		JToken val2 = val[(object)"PlayersData"];
		if (val2 != null && val2.HasValues)
		{
			IJEnumerable<JToken> val3 = (IJEnumerable<JToken>)(object)val2.Children();
			if ((int)val2.Type == 1)
			{
				val3 = Extensions.Children<JToken>((IEnumerable<JToken>)(object)val2.Children());
			}
			{
				foreach (JToken item2 in (IEnumerable<JToken>)val3)
				{
					string item = item2[(object)"ZoneId"].ToObject<string>();
					_reservedZoneIds.Add(item);
				}
				return;
			}
		}
		if (LegacyUtilities.LoadStrandedWorldPlayerZoneId_PreCoop(val, out var zoneId))
		{
			_reservedZoneIds.Add(zoneId);
		}
		else
		{
			Debug.LogError((object)"LoadPlayerMap:: Could not load player data.");
		}
	}

	private void UpdateMapsStatus()
	{
		Map[] mapList = World.MapList;
		foreach (Map map in mapList)
		{
			UpdateMapStatus(map);
		}
	}

	private void UpdateMapStatus(Map map)
	{
		if (map == null || map.EditorData.Id.Contains("INTERNAL_PROCEDURAL"))
		{
			return;
		}
		Map map2 = Maps.AllMapList.FirstOrDefault((Map m) => m.EditorData.Id == map.EditorData.Id && m.EditorData.Source == map.EditorData.Source);
		if (map2 != null)
		{
			if (map.EditorData.VersionNumber < map2.EditorData.VersionNumber)
			{
				map.EditorData.State = MapEditorData.EMapState.Older;
			}
			else
			{
				map.EditorData.State = MapEditorData.EMapState.Current;
			}
		}
		else
		{
			map.EditorData.State = MapEditorData.EMapState.Unavailable;
		}
	}

	private bool ValidateDuplicateMap(Map newMap, bool checkSource = true)
	{
		for (int i = 0; i < 49; i++)
		{
			Map map = World.MapList[i];
			if (map.EditorData.Id == newMap.EditorData.Id && (!checkSource || map.EditorData.Source == newMap.EditorData.Source))
			{
				return false;
			}
		}
		return true;
	}

	private void DeleteSave()
	{
		SaveManager.BackupSaveFiles();
		SaveManager.DeleteSaveFiles();
		_validSave = false;
	}

	private void InvokeDelayed(float delay, Action callback)
	{
		if (callback != null)
		{
			((MonoBehaviour)this).StartCoroutine(InvokeDelayed_Sequence(delay, callback));
		}
	}

	private IEnumerator InvokeDelayed_Sequence(float delay, Action callback)
	{
		yield return (object)new WaitForEndOfFrame();
		float timer = 0f;
		while (timer < delay)
		{
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		callback?.Invoke();
	}
}
