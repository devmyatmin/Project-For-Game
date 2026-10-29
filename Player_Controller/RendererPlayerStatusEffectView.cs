using UnityEngine;

namespace Beam;

public class RendererPlayerStatusEffectView : PlayerStatusEffectViewBase
{
	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private string _shaderVariableName;

	public override void DoEffect(float value)
	{
		_renderer.sharedMaterial.SetFloat(_shaderVariableName, value);
	}
}
