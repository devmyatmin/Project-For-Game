using System;
using System.Collections.Generic;
using Beam.Crafting;
using Beam.Events;
using Beam.Language;
using Beam.Utilities;
using Rewired;
using UnityEngine;

namespace Beam.UI;

public class HudPresenter : ControllerBase, INeedPlayerComponent, IRefresh
{
	private const float DEFAULT_CROSSHAIR_SCALE = 0.3f;

	[SerializeField]
	private HudViewAdapterBase _view;

	private IPlayer _player;

	private Raycaster _raycaster;

	private Interacter _interacter;

	private Crafter _crafter;

	private List<IGameActionProvider> _gameActionProviders = new List<IGameActionProvider>();

	private Stack<ActionHudElementViewAdapterBase> _actionElementStack = new Stack<ActionHudElementViewAdapterBase>();

	private List<ActionHudElementViewAdapterBase> _activeActionElements = new List<ActionHudElementViewAdapterBase>();

	private bool _showCursor;

	private bool _showAction;

	private bool _suspended;

	IPlayer INeedPlayerComponent.Player => _player;

	public void Start()
	{
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"HudPresenter:: No view component assigned!");
			_view = NullHudViewAdapter.Instance;
		}
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		EventManager.AddListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
		LoadOptions();
		Reset();
	}

	public override void SetView(ViewAdapterBase view)
	{
		_view = view as HudViewAdapterBase;
		if ((Object)(object)_view == (Object)null)
		{
			Debug.LogError((object)"HudPresenter:: No view component assigned!");
			_view = NullHudViewAdapter.Instance;
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Movement.ClimbChanged += PlayerMovement_ClimbChanged;
		_raycaster = _player.GameInput.Raycaster;
		_raycaster.EnterObject += Raycaster_EnterObject;
		_raycaster.LeaveObject += Raycaster_LeaveObject;
		_interacter = _player.GameInput.Interacter;
		_interacter.InteractingEnabled += Interacter_InteractingEnabled;
		_interacter.InteractingUpdated += Interacter_InteractingUpdated;
		_interacter.InteractingFinished += Interacter_InteractingFinished;
		_crafter = _player.Crafter;
		_crafter.StartedPlacing += Crafter_StartedPlacing;
		_gameActionProviders.Add(_player.Holder);
		_gameActionProviders.Add(_interacter);
		_gameActionProviders.Add(((Component)_player.PlayerUI.Canvas).GetComponentInChildren<StorageMenuPresenter>());
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_player) && (Object)(object)_player.Movement != (Object)null)
		{
			_player.Movement.ClimbChanged -= PlayerMovement_ClimbChanged;
		}
		if ((Object)(object)_interacter != (Object)null)
		{
			_interacter.InteractingEnabled -= Interacter_InteractingEnabled;
			_interacter.InteractingUpdated -= Interacter_InteractingUpdated;
			_interacter.InteractingFinished -= Interacter_InteractingFinished;
		}
		if ((Object)(object)_raycaster != (Object)null)
		{
			_raycaster.EnterObject -= Raycaster_EnterObject;
			_raycaster.LeaveObject -= Raycaster_LeaveObject;
		}
		if ((Object)(object)_crafter != (Object)null)
		{
			_crafter.StartedPlacing -= Crafter_StartedPlacing;
		}
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
		EventManager.RemoveListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
	}

	public void LoadOptions()
	{
		_showAction = Options.GeneralSettings.HudInformation == Quality_HUDInformation.ITEM_NAMES;
		_showCursor = Options.GeneralSettings.ShowCrosshair == Quality_OFF_ON.ON;
		Refresh(RefreshUIMode.Refresh);
		Debug.Log((object)"HudPresenter::LoadOptions:: Loaded options.");
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void EventManager_EndGameSequenceEvent(EndGameSequenceEvent e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Game.Mode == 1 && _player.Id == 1)
		{
			Suspend(suspend: true);
		}
	}

	private ActionHudElementViewAdapterBase PopActionElement()
	{
		ActionHudElementViewAdapterBase actionHudElementViewAdapterBase = ((_actionElementStack.Count <= 0) ? Object.Instantiate<ActionHudElementViewAdapterBase>(_view.ActionHudElementPrefab) : _actionElementStack.Pop());
		((Component)actionHudElementViewAdapterBase).transform.SetParent(_view.ActionHudElementsContainer.Transform, false);
		_activeActionElements.Add(actionHudElementViewAdapterBase);
		return actionHudElementViewAdapterBase;
	}

	private void PushActionElement(ActionHudElementViewAdapterBase element)
	{
		((Component)element).transform.SetParent(_view.PooledActionHudElementsContainer.Transform, false);
		_actionElementStack.Push(element);
	}

	private void PoolActionElements()
	{
		for (int i = 0; i < _activeActionElements.Count; i++)
		{
			ActionHudElementViewAdapterBase element = _activeActionElements[i];
			PushActionElement(element);
		}
		_activeActionElements.Clear();
	}

	private void Interacter_InteractingEnabled(bool enabled)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		_view.DefaultCursor.Color = Color.white;
		_view.DefaultCursor.Enabled = enabled || _showCursor;
		_view.InteractingCursor.Color = Color.white;
		_view.InteractingCursor.Enabled = enabled;
	}

	private void Interacter_InteractingUpdated(float time)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(0.3f + 0.6f * (4f * time), 0f, 0.9f);
		_view.DefaultCursor.transform.localScale = Vector3.one * num;
	}

	private void Interacter_InteractingFinished()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_view.DefaultCursor.transform.localScale = Vector3.one * 0.3f;
		Refresh(RefreshUIMode.Refresh);
	}

	private void Raycaster_EnterObject(IBase obj)
	{
		Select(obj);
		((Component)_view.ClimbUI).gameObject.SetActive(_player.Movement.ClimbAvailable && CurrentObjectIsActiveLadder());
	}

	private void Raycaster_LeaveObject(IBase obj)
	{
		FadeOutAndReset();
	}

	private void Crafter_StartedPlacing()
	{
		FadeOutAndReset();
	}

	private void Select(IBase obj)
	{
		if (_suspended || _player.Statistics.IsKnockedOut)
		{
			return;
		}
		ISelectable selectable = obj as ISelectable;
		if (BeamTeam.IsNullOrDestroyed(selectable))
		{
			FadeOutAndReset();
			return;
		}
		LeanTween.cancel(((Component)this).gameObject);
		_view.DefaultCursor.Enabled = _showCursor;
		_view.ItemNameLabel.Text = obj.DisplayName;
		_view.ItemActionGroup.Alpha = 1f;
		((Component)_view.ItemActionGroup.transform).gameObject.SetActive(_showAction);
		PoolActionElements();
		for (int i = 0; i < _gameActionProviders.Count; i++)
		{
			IEnumerable<IGameAction> gameActions = _gameActionProviders[i].GetGameActions();
			if (gameActions == null)
			{
				continue;
			}
			IPickupable pickupable = selectable as IPickupable;
			bool flag = false;
			if (!BeamTeam.IsNullOrDestroyed(pickupable))
			{
				flag = pickupable.CanPickUp && (!_player.Holder.CanPickup(pickupable, notification: false) || _player.Movement.Vehicle != null);
			}
			foreach (IGameAction item in gameActions)
			{
				Player input = _player.Input;
				if (RewiredUtilities.IsActionEnabled((input != null) ? input.id : 0, item.ElementRequiredId) && (!flag || item.ElementRequiredId != 9))
				{
					ActionHudElementViewAdapterBase element = PopActionElement();
					FormatActionElement(element, item.PlayerActionRequiredDescription, item.ElementRequiredId, item.ResultingActionDescription);
				}
			}
		}
		((Component)_view.ActionHudElementsContainer.Transform).gameObject.SetActive(_activeActionElements.Count > 0);
	}

	private void FormatActionElement(ActionHudElementViewAdapterBase element, string playerActionTerm, int elementRequired, string resultActionTerm)
	{
		string text = Localization.GetLanguageHandler().Localize("PLAYER_ACTION_SENTENCE", playerActionTerm, "PLAYER_ACTION_PREPOSITION", resultActionTerm);
		text = PlatformLocalization.GetHandler().SubstituteControllerGlyphs(text, _player.Id, elementRequired);
		element.PressLabel.Text = text;
	}

	private void FadeOutAndReset()
	{
		if (_view.ItemActionGroup.Alpha == 0f)
		{
			Reset();
		}
		else
		{
			LeanTween.value(((Component)this).gameObject, (Action<float>)FadeCallback, 1f, 0f, 0.2f).setOnComplete((Action)Reset);
		}
	}

	private void Reset()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		LeanTween.cancel(((Component)this).gameObject);
		Color white = Color.white;
		white.a = 0.5f;
		_view.DefaultCursor.Color = white;
		_view.DefaultCursor.Enabled = !_suspended && _showCursor;
		_view.InteractingCursor.Enabled = false;
		_view.ItemNameLabel.Text = "";
		_view.ItemActionGroup.Alpha = 1f;
		((Component)_view.ItemActionGroup.transform).gameObject.SetActive(false);
	}

	public void Suspend(bool suspend)
	{
		_suspended = suspend;
		Refresh((!_suspended) ? RefreshUIMode.Refresh : RefreshUIMode.Clear);
	}

	public void Refresh(RefreshUIMode mode)
	{
		switch (mode)
		{
		case RefreshUIMode.Clear:
			Reset();
			break;
		case RefreshUIMode.Refresh:
			Select(_raycaster.CurrentObject);
			break;
		}
	}

	private void PlayerMovement_ClimbChanged(bool value)
	{
		if (CurrentObjectIsActiveLadder())
		{
			((Component)_view.ClimbUI).gameObject.SetActive(value);
		}
		if (BeamTeam.IsNullOrDestroyed(_player.Movement.activeLadder))
		{
			((Component)_view.ClimbUI).gameObject.SetActive(false);
		}
		if (((Component)_view.ClimbUI).gameObject.activeSelf)
		{
			FormatActionElement(_view.ClimbUI, "PLAYER_ACTION_HOLD", 4, "PLAYER_ACTION_CLIMB");
		}
	}

	private void FadeCallback(float value)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		_view.ItemActionGroup.Alpha = value;
		Color white = Color.white;
		white.a = value;
		_view.InteractingCursor.Color = white;
		white.a = Mathf.Clamp(white.a, 0.5f, 1f);
		_view.DefaultCursor.Color = white;
	}

	private bool CurrentObjectIsActiveLadder()
	{
		if (BeamTeam.IsNullOrDestroyed(_player.Movement.activeLadder))
		{
			return false;
		}
		if (BeamTeam.IsNullOrDestroyed(_raycaster.CurrentObject))
		{
			return false;
		}
		return ((Object)((Component)((Component)_player.Movement.activeLadder).transform.parent).gameObject).GetInstanceID() == ((Object)_raycaster.CurrentObject.gameObject).GetInstanceID();
	}
}
