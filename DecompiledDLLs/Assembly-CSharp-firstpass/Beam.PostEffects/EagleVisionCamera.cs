using Beam.Utilities;
using UnityEngine;

namespace Beam.PostEffects;

public class EagleVisionCamera : MonoBehaviour
{
	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private RenderTexture _rt;

	private void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		_rt = new RenderTexture(Screen.width / 2, Screen.height / 2, 0, (RenderTextureFormat)7);
		Camera component = ((Component)this).GetComponent<Camera>();
		component.CopyFrom(Camera.main);
		component.cullingMask = 1 << Layers.INTERACTIVE_OBJECTS;
		component.depthTextureMode = (DepthTextureMode)0;
		component.renderingPath = (RenderingPath)0;
		component.SetReplacementShader(_shader, "");
		component.targetTexture = _rt;
		Shader.SetGlobalTexture("Beam_EagleVision", (Texture)(object)component.targetTexture);
	}

	private void OnDestroy()
	{
		BeamTeam.Destroy<RenderTexture>(_rt);
	}
}
