using UnityEngine;

namespace Beam;

public class InteractiveMissionTrigger : MissionTrigger
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Interactive_MISSION_TRIGGER _interactiveObject;

	private void Start()
	{
		_interactiveObject.Interacted += OnTriggered;
	}

	protected override void OnDestroy()
	{
		_interactiveObject.Interacted -= OnTriggered;
		base.OnDestroy();
	}

	public override void Enable()
	{
		base.Enable();
		_interactiveObject.CanInteract = Enabled;
	}

	public override void Disable()
	{
		base.Disable();
		_interactiveObject.CanInteract = Enabled;
	}
}
