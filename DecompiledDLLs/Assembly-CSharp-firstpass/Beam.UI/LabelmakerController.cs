using System;
using Beam.Events;
using Beam.Language;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.UI;

public class LabelmakerController : ControllerBase, INeedPlayerComponent
{
	public class ReplicateRename : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab BaseObject;

		[Replicate]
		public string Renamed;

		public override void OnPeer()
		{
			BaseObject baseObject = (BaseObject)BaseObject;
			if ((Object)(object)baseObject != (Object)null)
			{
				RenameAction(baseObject, Renamed);
			}
		}
	}

	private IPlayer _player;

	private DialogueBoxViewAdapter _dialogue;

	private BaseObject _currentObject;

	private string _renamed;

	private static bool _labelMakerIsUsedCoop;

	public IPlayer Player => _player;

	public bool IsVisible => (Object)(object)_currentObject != (Object)null;

	private void Start()
	{
		EventManager.AddListener<GameOverEvent>(EventManager_GameOverEvent);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<GameOverEvent>(EventManager_GameOverEvent);
	}

	private void EventManager_GameOverEvent(GameOverEvent e)
	{
		if (!BeamTeam.IsNullOrDestroyed(_dialogue) && IsVisible)
		{
			Hide();
		}
	}

	public override void SetView(ViewAdapterBase view)
	{
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	public void Show(IBase obj)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		if (!_labelMakerIsUsedCoop)
		{
			_labelMakerIsUsedCoop = (int)Game.Mode == 1;
			_currentObject = obj as BaseObject;
			if (!BeamTeam.IsNullOrDestroyed(_currentObject) && !MainMenuPresenter.Instance.IsGamePaused)
			{
				RewiredUtilities.DisableMaps(RewiredUtilities.GetOtherPlayers(_player.Input));
				RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 16);
				RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 0);
				RewiredUtilities.SetCursorEnabled(_player.Input, enabled: true);
				_dialogue = DialogueBox.Show(_player.Input, "LABELMAKER_DB_TITLE", string.Empty, DialogueButton.OkCancel, DialogueIcon.None, DialogueType.Input);
				_dialogue.InputField.CharacterLimit = 20;
				_dialogue.InputField.Text = Localization.GetLanguageHandler().LocalizeMany(_currentObject.DisplayName, ' ').ToUpper();
				_dialogue.InputField.ValueChanged += DialogueInputField_ValueChanged;
				_dialogue.InputField.KeyDown += DialogueInputField_KeyDown;
				_dialogue.Accept += ReplicatedRename;
				_dialogue.Decline += Hide;
				_renamed = _dialogue.InputField.Text;
			}
		}
	}

	public void Hide()
	{
		if ((Object)(object)_dialogue != (Object)null)
		{
			_dialogue.InputField.ValueChanged -= DialogueInputField_ValueChanged;
			_dialogue.InputField.KeyDown -= DialogueInputField_KeyDown;
			_dialogue.Destroy();
		}
		RewiredUtilities.RestoreMapsEnabled(RewiredUtilities.GetOtherPlayers(_player.Input));
		RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 16);
		RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 0);
		RewiredUtilities.SetCursorEnabled(_player.Input, enabled: false);
		_player.PlayerUI.Refresh(RefreshUIMode.Refresh);
		_labelMakerIsUsedCoop = false;
	}

	private void DialogueInputField_ValueChanged(string value)
	{
		_dialogue.InputField.Text = value.ToUpper();
		_renamed = _dialogue.InputField.Text;
	}

	private void DialogueInputField_KeyDown(string value)
	{
		if (Input.GetKey((KeyCode)13))
		{
			ReplicatedRename();
		}
	}

	private void Rename()
	{
		RenameAction(_currentObject, _renamed);
		Hide();
	}

	public static void RenameAction(BaseObject currentObject, string renamed)
	{
		if (!((Object)(object)currentObject == (Object)null))
		{
			renamed = renamed.Trim();
			if (!string.IsNullOrEmpty(renamed) && !Localization.GetLanguageHandler().LocalizeMany(currentObject.DisplayName, ' ').ToUpper()
				.Equals(renamed, StringComparison.InvariantCultureIgnoreCase))
			{
				currentObject.DisplayName = renamed;
			}
		}
	}

	public void ReplicatedRename()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateRename replicateRename = new ReplicateRename();
			replicateRename.BaseObject = _currentObject;
			replicateRename.Renamed = _renamed;
			replicateRename.Post();
		}
		Rename();
	}
}
