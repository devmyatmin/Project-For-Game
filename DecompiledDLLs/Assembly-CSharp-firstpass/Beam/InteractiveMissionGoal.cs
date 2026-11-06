using UnityEngine;

namespace Beam;

public class InteractiveMissionGoal : MissionGoalBase
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject_GOAL _interactiveObject;

	public override string Name => _interactiveObject.DisplayName;

	public override Vector3 Location
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
	}

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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(((Component)this).transform.position);
		Zone zone2 = StrandedWorld.GetZone(PlayerRegistry.LocalPlayer.transform.position);
		if (PlayerRegistry.LocalPlayer.CurrentMissionGoal == this || (Object)(object)zone2 == (Object)(object)zone)
		{
			OnUpdateCompletion(data.Value);
		}
	}
}
