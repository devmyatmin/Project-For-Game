using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class Selecter : MonoBehaviour, IGameInputHandler
{
	[SerializeField]
	private Color _selectedColor;

	private IGameInputController _controller;

	private ISelectable _selectable;

	private IPlayer _player;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalColor("_HighlightRimColor", _selectedColor);
		Shader.SetGlobalFloat("_HighlightRimPower", 1f);
	}

	public void Initialize(IGameInputController controller)
	{
		_controller = controller;
		_controller.Raycaster.EnterObject += Raycaster_EnterObject;
		_controller.Raycaster.LeaveObject += Raycaster_LeaveObject;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void OnDestroy()
	{
		if (!BeamTeam.IsNullOrDestroyed(_controller))
		{
			_controller.Raycaster.EnterObject -= Raycaster_EnterObject;
			_controller.Raycaster.LeaveObject -= Raycaster_LeaveObject;
		}
	}

	public void CheckInput()
	{
		if (_player.IsOwner && !BeamTeam.IsNullOrDestroyed(_selectable))
		{
			_selectable.Select(_player);
		}
	}

	private void Raycaster_EnterObject(IBase obj)
	{
		if (_player.IsOwner)
		{
			if (!BeamTeam.IsNullOrDestroyed(_selectable))
			{
				_selectable.Deselect();
			}
			_selectable = obj as ISelectable;
			if (!BeamTeam.IsNullOrDestroyed(_selectable))
			{
				_selectable.Select(_player);
			}
		}
	}

	private void Raycaster_LeaveObject(IBase obj)
	{
		if (_player.IsOwner)
		{
			if (!BeamTeam.IsNullOrDestroyed(_selectable))
			{
				_selectable.Deselect();
			}
			_selectable = null;
		}
	}
}
