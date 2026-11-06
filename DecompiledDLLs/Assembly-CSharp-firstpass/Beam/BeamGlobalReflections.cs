using Beam.PostEffects;
using Beam.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beam;

public class BeamGlobalReflections : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private CommandBuffer _commandBuffer;

	private Material _material;

	private void OnEnable()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		_material = new Material(Shader.Find("Beam Team/Command Buffers/Global Reflections"));
		_commandBuffer = new CommandBuffer();
		_commandBuffer.name = "Beam Global Reflections";
		_commandBuffer.SetRenderTarget(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)14));
		_commandBuffer.DrawMesh(CommandBufferExtensions.FullscreenQuad, Matrix4x4.identity, _material, 0, 0);
		_camera.AddCommandBuffer((CameraEvent)21, _commandBuffer);
	}

	private void OnDisable()
	{
		_camera.RemoveCommandBuffer((CameraEvent)21, _commandBuffer);
		_commandBuffer.Release();
		BeamTeam.Destroy<Material>(_material);
	}
}
