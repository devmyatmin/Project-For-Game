using UnityEngine;

namespace Beam.PostEffects;

public class NullMenuEffectsController : NullObject<NullMenuEffectsController>, IMenuEffectsController
{
	public void SetMaskTexture(Texture2D texture)
	{
		Debug.LogError((object)"NullMenuEffectsController::SetTexture:: Not implemented!");
	}

	public void Show()
	{
		Debug.LogError((object)"NullMenuEffectsController::Show:: Not implemented!");
	}

	public void Hide()
	{
		Debug.LogError((object)"NullMenuEffectsController::Hide:: Not implemented!");
	}
}
