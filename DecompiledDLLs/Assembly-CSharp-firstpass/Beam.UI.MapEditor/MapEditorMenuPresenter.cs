using Beam.Language;
using Beam.Terrain;
using LE_LevelEditor;
using LE_LevelEditor.Core;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam.UI.MapEditor;

public class MapEditorMenuPresenter : MonoBehaviour
{
	public static Map MAP_TO_EDIT;

	[SerializeField]
	private MapEditorSaveMenuPresenter _saveMenuPresenter;

	private IMapEditorMenuViewAdapter _view;

	private Map _currentMap;

	private bool _initialized;

	private bool _childPanelOpen;

	private void Awake()
	{
		Time.timeScale = 1f;
	}

	private void Start()
	{
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: true, 11);
	}

	public void Initialize(IMapEditorMenuViewAdapter view)
	{
		Game.State = (GameState)4;
		_view = view;
		_view.HideView += _saveMenuPresenter.Hide;
		_saveMenuPresenter.Saved += SaveMenuPresenter_Save;
		_saveMenuPresenter.ShowView += SaveMenuPresenter_ShowView;
		_saveMenuPresenter.HideView += SaveMenuPresenter_HideView;
		CursorUtils.LockState = (CursorLockMode)2;
		CursorUtils.Visible = true;
		InitializeMap();
		SetControlEvents();
		_initialized = true;
	}

	private void InitializeMap()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (MAP_TO_EDIT == null)
		{
			_currentMap = new Map();
			Singleton<LE_ZoneTileGenerator>.Instance.GenerateRandomIsland();
		}
		else
		{
			_currentMap = MAP_TO_EDIT;
			Singleton<LE_ZoneTileGenerator>.Instance.LoadMap(_currentMap);
			new MapLoader().LoadMap(_currentMap.ObjectData);
		}
		if (string.IsNullOrWhiteSpace(_currentMap.EditorData.Name))
		{
			_view.MapTitleLabel.Text = Localization.GetLanguageHandler().Localize("MAP_EDITOR_NEW_MAP_TITLE");
		}
		else
		{
			_view.MapTitleLabel.Text = _currentMap.EditorData.Name;
		}
	}

	private void OnDestroy()
	{
		MAP_TO_EDIT = null;
		if (_initialized)
		{
			_view.HideView -= _saveMenuPresenter.Hide;
			_saveMenuPresenter.Saved -= SaveMenuPresenter_Save;
			_saveMenuPresenter.ShowView -= SaveMenuPresenter_ShowView;
			_saveMenuPresenter.HideView -= SaveMenuPresenter_HideView;
		}
	}

	private void SetControlEvents()
	{
		_view.ResumeButton.Click += ResumeButton_Click;
		_view.SaveButton.Click += SaveButton_Click;
		_view.QuitButton.Click += QuitButton_Click;
	}

	private void SaveMenuPresenter_Save()
	{
		_view.MapTitleLabel.Text = _currentMap.EditorData.Name;
	}

	private void SaveMenuPresenter_ShowView()
	{
		_childPanelOpen = true;
	}

	private void SaveMenuPresenter_HideView()
	{
		_childPanelOpen = false;
	}

	private void Update()
	{
		if (_initialized && ReInput.players.GetPlayer(0).GetButtonDown(39) && !_childPanelOpen)
		{
			if (_view.Visible)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}
	}

	public void Show()
	{
		_view.Show();
	}

	public void Hide()
	{
		_view.Hide();
	}

	private void ResumeButton_Click(object sender)
	{
		Hide();
	}

	public void SaveButton_Click(object sender)
	{
		if (Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer.childCount == 0 || ((Component)Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer).GetComponentsInChildren<LE_Object>(true).Length == 0)
		{
			DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_SAVE_MENU_DB_SORRY_TITLE", "MAP_EDITOR_MENU_DB_SAVE_EMPTY_MAP_DESC", DialogueButton.Ok, DialogueIcon.Warning);
			return;
		}
		_saveMenuPresenter.CurrentMap = _currentMap;
		_saveMenuPresenter.Show();
	}

	private void QuitButton_Click(object sender)
	{
		DialogueBox.Show(ReInput.players.GetPlayer(0), "MAP_EDITOR_MENU_DB_QUIT_TITLE", "MAP_EDITOR_MENU_DB_QUIT_DESC", DialogueButton.YesNo, DialogueIcon.Warning).Accept += () =>
		{
			_view.CanvasGroup.Interactable = false;
			Quit();
		};
	}

	private void Quit()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: false, 11);
		Singleton<Fader>.Instance.FadeOut(3f, FadeMode.Solid, () =>
		{
			SceneManager.LoadScene(1);
		});
	}
}
