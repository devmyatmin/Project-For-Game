using UnityEngine;

namespace Beam.PostEffects;

public interface IMenuEffectsController
{
	void SetMaskTexture(Texture2D texture);

	void Show();

	void Hide();
}
