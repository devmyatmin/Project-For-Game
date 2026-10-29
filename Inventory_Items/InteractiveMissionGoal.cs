using UnityEngine;

namespace Beam;

public class InteractiveMissionGoal : MissionGoalBase
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject_GOAL _interactiveObject;

	public override string Name => _interactiveObject.DisplayName;

	public override Vector3 Location => base.transform.position;

	public override float GetCompletionPercent()
	{
		return _interactiveObject.GetHealthUnits();
	}

	public override void Reset()
	{
		_interactiveObject.Reset();
	}

	public void Goal_Damaged(IBase sender, IBaseActionEventData data)
	{
		Zone zone = StrandedWorld.GetZone(base.transform.position);
		Zone zone2 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if (PlayerRegistry.LocalPlayer.CurrentMissionGoal == this || zone2 == zone)
		{
			OnUpdateCompletion(data.Value);
		}
	}
}
