using System.Collections.Generic;
using System.Linq;
using Beam.Crafting;
using Beam.Events;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Beam.UI;

public class QuickCrafterMenuPresenter : MonoBehaviour, INeedPlayerComponent
{
	[SerializeField]
	private QuickCrafterRadialMenuPresenter _radialPresenter;

	private bool _open;

	private IPlayer _player;

	private Crafter _crafter;

	private bool _time;

	private float _timer;

	private IList<IList<CraftingData>> _craftingData;

	private CraftingCombination _currentCombination;

	[SerializeField]
	private UnityEvent _declined;

	private bool _suspend;

	public bool Open => _open;

	public bool CanOpen { get; set; } = true;

	IPlayer INeedPlayerComponent.Player => _player;

	public event EmptyEventHandler CrafterKeyFellThrough;

	protected virtual void OnCrafterKeyFellThrough()
	{
		CrafterKeyFellThrough?.Invoke();
	}

	private void Start()
	{
		_radialPresenter.Title = "QUICK_CRAFT_MENU_TITLE";
		_radialPresenter.SelectionChanged += RadialPresenter_SelectionChanged;
		_radialPresenter.ElementClick += RadialPresenter_ElementClick;
		_radialPresenter.Hide();
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.GameInput.Interacter.InteractingUpdated += Interacter_InteractingUpdated;
		_player.GameInput.Interacter.InteractingFinished += Interacter_InteractingFellThrough;
		_crafter = _player.Crafter;
	}

	private void Interacter_InteractingUpdated(float value)
	{
		_suspend = true;
	}

	private void Interacter_InteractingFellThrough()
	{
		_suspend = false;
	}

	private void OnDestroy()
	{
		_radialPresenter.SelectionChanged -= RadialPresenter_SelectionChanged;
		_radialPresenter.ElementClick -= RadialPresenter_ElementClick;
		if (!BeamTeam.IsNullOrDestroyed(_player) && Object.op_Implicit((Object)(object)_player.GameInput) && Object.op_Implicit((Object)(object)_player.GameInput.Interacter))
		{
			_player.GameInput.Interacter.InteractingUpdated -= Interacter_InteractingUpdated;
			_player.GameInput.Interacter.InteractingFinished -= Interacter_InteractingFellThrough;
		}
	}

	private void Update()
	{
		if (_open && _player.Statistics.IsKnockedOut)
		{
			Hide();
			return;
		}
		if (_suspend)
		{
			_timer = 0f;
			_time = false;
			return;
		}
		if (_time)
		{
			_timer += Time.deltaTime;
			if (!(_timer > 0.25f) && _player.Input.GetButton(13))
			{
				return;
			}
			_time = false;
			if (_timer < 0.25f)
			{
				OnCrafterKeyFellThrough();
			}
			else
			{
				if (!CanOpen)
				{
					return;
				}
				_craftingData = GetCraftingData();
				if (_craftingData != null && _craftingData.Count > 0)
				{
					Show();
					return;
				}
				UnityEvent declined = _declined;
				if (declined != null)
				{
					declined.Invoke();
				}
			}
			return;
		}
		if (_player.Input.GetButtonDown(13))
		{
			_timer = 0f;
			_time = true;
		}
		if (!_open)
		{
			return;
		}
		_radialPresenter.UpdateSelector();
		if (!_player.Input.GetButton(13))
		{
			_radialPresenter.SelectCurrentElement();
			Hide();
			if (_currentCombination != null)
			{
				_crafter.ReplicatedCraft(_currentCombination);
				_radialPresenter.PreviousComboName = _currentCombination.Name;
				_currentCombination = null;
			}
			else
			{
				_player.PlayerUI.Refresh(RefreshUIMode.Refresh);
			}
		}
	}

	private void OnQuickCrafterMenuOpen()
	{
		EventManager.RaiseEvent(new QuickCrafterMenuOpenedEvent());
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		Hide();
	}

	public void Show()
	{
		if (!_player.Statistics.IsKnockedOut && !_player.Movement.IsInCutscene)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 6);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 0);
			RewiredUtilities.SetCursorEnabled(_player.Input, enabled: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Clear);
			_radialPresenter.Initialize(_craftingData);
			_radialPresenter.Show();
			_open = true;
			OnQuickCrafterMenuOpen();
		}
	}

	public void Hide()
	{
		if (_open)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 6);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 0);
			RewiredUtilities.SetCursorEnabled(_player.Input, enabled: false);
			_radialPresenter.Hide();
			_open = false;
		}
	}

	private IList<IList<CraftingData>> GetCraftingData()
	{
		IList<CraftingCombination> list = _crafter.ReplicatedGetCraftingCombinations();
		if (list != null && list.Count > 0)
		{
			List<IList<CraftingData>> list2 = new List<IList<CraftingData>>();
			{
				foreach (CraftingCombination item in from combo in list
					where !combo.IsLimited && (!combo.Lockable || (combo.Lockable && !combo.IsLocked))
					orderby combo.Name == _radialPresenter.PreviousComboName descending
					select combo)
				{
					CraftingData craftingData = new CraftingData();
					craftingData.Value = item;
					list2.Add(new List<CraftingData> { craftingData });
				}
				return list2;
			}
		}
		return null;
	}

	private void RadialPresenter_SelectionChanged(IRadialMenuElement<CraftingData> element)
	{
		_currentCombination = element.SelectedValue.Value;
	}

	private void RadialPresenter_ElementClick(IRadialMenuElement<CraftingData> element)
	{
		Hide();
		CraftingCombination value = element.SelectedValue.Value;
		_crafter.ReplicatedCraft(value);
		_radialPresenter.PreviousComboName = value.Name;
	}
}
