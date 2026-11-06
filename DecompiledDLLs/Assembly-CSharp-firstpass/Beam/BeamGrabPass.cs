using UnityEngine;
using UnityEngine.Rendering;

namespace Beam;

[RequireComponent(typeof(Camera))]
public class BeamGrabPass : MonoBehaviour
{
	[SerializeField]
	private int _downsampling = 2;

	private Camera _camera;

	private CommandBuffer _cmd1;

	private CommandBuffer _cmd2;

	private void OnEnable()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected Obj, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		_camera = ((Component)this).GetComponent<Camera>();
		int num = _camera.pixelWidth / _downsampling;
		int num2 = _camera.pixelHeight / _downsampling;
		_cmd1 = new CommandBuffer();
		_cmd1.name = "Beam Grab Pass Opaque";
		int num3 = Shader.PropertyToID("_BeamGrabPassOpaque");
		_cmd1.GetTemporaryRT(num3, num, num2, 0, (FilterMode)1);
		_cmd1.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), RenderTargetIdentifier.op_Implicit(num3));
		_cmd1.SetGlobalTexture("Beam_Grab_Opaque", RenderTargetIdentifier.op_Implicit(num3));
		_camera.AddCommandBuffer((CameraEvent)11, _cmd1);
		_cmd2 = new CommandBuffer();
		_cmd2.name = "Beam Grab Pass";
		int num4 = Shader.PropertyToID("_BeamGrabPass");
		_cmd2.GetTemporaryRT(num4, num, num2, 0, (FilterMode)1);
		_cmd2.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), RenderTargetIdentifier.op_Implicit(num4));
		_cmd2.SetGlobalTexture("Beam_Grab", RenderTargetIdentifier.op_Implicit(num4));
		_camera.AddCommandBuffer((CameraEvent)16, _cmd2);
	}

	private void OnDisable()
	{
		_camera.RemoveCommandBuffer((CameraEvent)11, _cmd1);
		_cmd1.Release();
		_cmd1 = null;
		_camera.RemoveCommandBuffer((CameraEvent)16, _cmd2);
		_cmd2.Release();
		_cmd2 = null;
	}
}
