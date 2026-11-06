using System;
using System.Linq;
using Beam.AccountServices;
using Beam.Language;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Terrain;
using Funlabs;
using LE_LevelEditor;
using LE_LevelEditor.Core;
using Rewired;
using UnityEngine;

namespace Beam.UI.MapEditor;

public class MapEditorSaveMenuPresenter : MonoBehaviour
{
	private IMapEditorSaveMenuViewAdapter _view;

	private bool _initialized;

	public Map CurrentMap { get; set; }

	public event EmptyEventHandler ShowView;

	public event EmptyEventHandler HideView;

	public event EmptyEventHandler Saved;

	private void View_ShowView()
	{
		ShowView?.Invoke();
	}

	private void View_HideView()
	{
		HideView?.Invoke();
	}

	protected virtual void OnSaved()
	{
		Saved?.Invoke();
	}

	public void Initialize(IMapEditorSaveMenuViewAdapter view)
	{
		_view = view;
		_view.ShowView += View_ShowView;
		_view.HideView += View_HideView;
		SetControlEvents();
		Hide();
		_initialized = true;
	}

	private void OnDestroy()
	{
		if (_initialized)
		{
			_view.ShowView -= View_ShowView;
			_view.HideView -= View_HideView;
			Saved = null;
			ShowView = null;
			HideView = null;
			ClearControlEvents();
		}
	}

	private void SetControlEvents()
	{
		_view.SaveButton.Click += SaveButton_Click;
		_view.BackButton.Click += BackButton_Click;
		_view.NameInputField.ValueChanged += NameInputField_Changed;
	}

	private void ClearControlEvents()
	{
		_view.SaveButton.Click -= SaveButton_Click;
		_view.BackButton.Click -= BackButton_Click;
		_view.NameInputField.ValueChanged -= NameInputField_Changed;
	}

	public void Show()
	{
		Player player = ReInput.players.GetPlayer(0);
		RewiredUtilities.SetAndCacheMapsEnabled(player, state: false, 11);
		RewiredUtilities.SetAndCacheMapsEnabled(player, state: true, 8);
		if (!string.IsNullOrWhiteSpace(CurrentMap.EditorData.Name))
		{
			_view.NameInputField.Text = CurrentMap.EditorData.Name;
		}
		if (!string.IsNullOrWhiteSpace(CurrentMap.EditorData.Description))
		{
			_view.DescriptionInputField.Text = CurrentMap.EditorData.Description;
		}
		UpdatePreviewImage();
		_view.Show();
	}

	public void Hide()
	{
		Player player = ReInput.players.GetPlayer(0);
		RewiredUtilities.SetAndCacheMapsEnabled(player, state: true, 11);
		RewiredUtilities.SetAndCacheMapsEnabled(player, state: false, 8);
		_view.Hide();
	}

	private void UpdatePreviewImage()
	{
	}

	private void SaveButton_Click(object sender)
	{
		_view.CanvasGroup.Interactable = false;
		if (string.IsNullOrWhiteSpace(_view.NameInputField.Text))
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_SORRY_TITLE", "MAP_EDITOR_SAVE_MENU_DB_SAVE_NAME_DESC", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				_view.CanvasGroup.Interactable = true;
			};
		}
		else if (string.IsNullOrWhiteSpace(_view.DescriptionInputField.Text))
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_SORRY_TITLE", "MAP_EDITOR_SAVE_MENU_DB_SAVE_DESCRIPTION_DESC", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				_view.CanvasGroup.Interactable = true;
			};
		}
		else if (Maps.DefaultMapList.FirstOrDefault((Map map) => map.EditorData.Id == CurrentMap.EditorData.Id) != null)
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_SAVE_TITLE", "MAP_EDITOR_SAVE_MENU_DB_SAVE_RESERVED_DESC", DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
			{
				_view.CanvasGroup.Interactable = true;
			};
		}
		else
		{
			OnValidateTextsSucceeded();
		}
	}

	private void OnValidateTextsSucceeded()
	{
		if (Maps.UserMapList.FirstOrDefault((Map map) => map.EditorData.Id == CurrentMap.EditorData.Id) != null)
		{
			DialogueBoxViewAdapter dialogueBoxViewAdapter = DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_SAVE_TITLE", "MAP_EDITOR_SAVE_MENU_DB_SAVE_EXISTING_DESC", DialogueButton.YesNo);
			dialogueBoxViewAdapter.Accept += () =>
			{
				SaveMap();
				_view.CanvasGroup.Interactable = true;
			};
			dialogueBoxViewAdapter.Decline += () =>
			{
				_view.CanvasGroup.Interactable = true;
			};
		}
		else
		{
			SaveMap();
			_view.CanvasGroup.Interactable = true;
		}
	}

	private void OnValidateTextsFailed(string message)
	{
		DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning).Accept += () =>
		{
			_view.CanvasGroup.Interactable = true;
		};
	}

	private void BackButton_Click(object sender)
	{
		Hide();
	}

	private void NameInputField_Changed(string value)
	{
	}

	private void SaveMap()
	{
		CurrentMap.HeightmapData = Singleton<LE_ZoneTileGenerator>.Instance.terrain.terrainData.GetHeights(0, 0, 257, 257);
		JObject jObject = new JObject();
		LE_Object[] componentsInChildren = ((Component)Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer).GetComponentsInChildren<LE_Object>(true);
		foreach (LE_Object lE_Object in componentsInChildren)
		{
			JObject jObject2 = new JObject();
			jObject2.AddField("prefab", Prefabs.ConvertPrefabIdToLegacy(lE_Object.Id));
			jObject2.AddField("reference", MiniGuid.New().ToString());
			JObject data = JSerializer.Serialize((Component)(object)((Component)lE_Object).transform);
			jObject2.AddField("Transform", data);
			jObject.Add(jObject2);
		}
		CurrentMap.ObjectData = jObject;
		CurrentMap.EditorData.Author = GAccountService.Get().LocalUser.UserInfo.DisplayName;
		CurrentMap.EditorData.Name = _view.NameInputField.Text.Trim();
		CurrentMap.EditorData.Description = _view.DescriptionInputField.Text.Trim();
		CurrentMap.EditorData.VersionNumber++;
		bool flag = false;
		string text = "";
		try
		{
			Maps.SaveMap(CurrentMap, FilePath.LOCAL_MAPS_FOLDER);
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
			text = ex.Message;
		}
		if (flag)
		{
			OnSaved();
		}
		else
		{
			Maps.DeleteMap(CurrentMap);
			string message = Localization.GetLanguageHandler().Localize("MAP_EDITOR_SAVE_MENU_DB_ERROR_SAVING_MAP_DESC", text);
			DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_ERROR_TITLE", message, DialogueButton.Ok, DialogueIcon.Warning);
		}
		Hide();
	}
}
