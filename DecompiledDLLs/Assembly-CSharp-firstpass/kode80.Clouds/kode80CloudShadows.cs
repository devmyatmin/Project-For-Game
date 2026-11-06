using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace kode80.Clouds;

[ExecuteInEditMode]
public class kode80CloudShadows : MonoBehaviour
{
	public kode80Clouds clouds;

	private Light _light;

	private CommandBuffer _commandBuffer;

	private Material _material;

	private void Start()
	{
	}

	private void ShadowsPreCull(Camera camera)
	{
		UpdateShadows();
	}

	public void UpdateShadows()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)clouds == (Object)null) && !((Object)(object)_light == (Object)null))
		{
			_material.SetTexture("_CloudCoverage", clouds.cloudCoverage);
			_material.SetMatrix("_InvCamera", clouds.targetCamera.cameraToWorldMatrix);
			Material material = _material;
			Matrix4x4 projectionMatrix = clouds.targetCamera.projectionMatrix;
			material.SetMatrix("_InvProjection", projectionMatrix.inverse);
			_material.SetVector("_Offset", Vector4.op_Implicit(Vector3.zero));
			_material.SetFloat("_CoverageScale", 1f / clouds.cloudsSharedProperties.maxDistance);
			_material.SetVector("_CoverageOffset", Vector4.op_Implicit(clouds.coverageOffset));
			_material.SetVector("_LightDirection", Vector4.op_Implicit(((Component)clouds.sunLight).transform.forward));
			_material.SetFloat("_ShadowStrength", _light.shadowStrength);
			clouds.cloudsSharedProperties.ApplyToMaterial(_material);
		}
	}

	private void OnEnable()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected Obj, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected Obj, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		_light = ((Component)this).GetComponent<Light>();
		if ((Object)(object)_light == (Object)null && (int)_light.type != 1)
		{
			Debug.LogWarning((object)"kode80CloudShadows component must be on a directional light");
			((Behaviour)this).enabled = false;
			return;
		}
		if ((Object)(object)_material == (Object)null)
		{
			_material = new Material(Shader.Find("Hidden/kode80/CloudShadowPass"));
			((Object)_material).hideFlags = (HideFlags)61;
		}
		if (_commandBuffer == null)
		{
			_commandBuffer = new CommandBuffer();
			_commandBuffer.name = "kode80 Clouds Shadow Pass";
			_commandBuffer.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)0), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), _material);
			_light.AddCommandBuffer((LightEvent)3, _commandBuffer);
		}
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(ShadowsPreCull));
	}

	private void OnDisable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(ShadowsPreCull));
		if (_commandBuffer != null)
		{
			_light.RemoveCommandBuffer((LightEvent)3, _commandBuffer);
			_commandBuffer = null;
		}
		if ((Object)(object)_material != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)_material);
			_material = null;
		}
	}
}
