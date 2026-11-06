using UnityEngine;

public interface IAimAssist
{
	LayerMask AimAssistLayerMask { get; }

	void DoAimAssist();

	void ResetAimAssist();
}
