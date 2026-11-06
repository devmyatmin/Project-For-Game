using UnityEngine;
using UnityEngine.Rendering;

namespace Ceto;

public class RefractionCommand : IRefractionCommand
{
	public class Command
	{
		public CommandBuffer buffer;

		public Material material;

		public bool enabled;

		public CameraEvent camEvent;
	}

	private Camera m_camera;

	private Command CopyScreenCmd { get; set; }

	private Command CopyDepthCmd { get; set; }

	private Command NormalFadeCmd { get; set; }

	public bool DisableCopyDepthCmd
	{
		get
		{
			return CopyDepthCmd.enabled;
		}
		set
		{
			CopyDepthCmd.enabled = !value;
		}
	}

	public bool DisableNormalFadeCmd
	{
		get
		{
			return NormalFadeCmd.enabled;
		}
		set
		{
			NormalFadeCmd.enabled = !value;
		}
	}

	public RefractionCommand(Camera cam, Shader copyDepth, Shader normalFade)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected Obj, but got Unknown
		CopyScreenCmd = new Command();
		CopyDepthCmd = new Command();
		NormalFadeCmd = new Command();
		m_camera = cam;
		CopyDepthCmd.material = new Material(copyDepth);
		CopyDepthCmd.enabled = true;
		NormalFadeCmd.material = new Material(normalFade);
		NormalFadeCmd.enabled = true;
	}

	public void ClearCommands()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (CopyScreenCmd.buffer != null)
		{
			m_camera.RemoveCommandBuffer(CopyScreenCmd.camEvent, CopyScreenCmd.buffer);
			CopyScreenCmd.buffer = null;
		}
		if (CopyDepthCmd.buffer != null)
		{
			m_camera.RemoveCommandBuffer(CopyDepthCmd.camEvent, CopyDepthCmd.buffer);
			CopyDepthCmd.buffer = null;
		}
		if (NormalFadeCmd.buffer != null)
		{
			m_camera.RemoveCommandBuffer(NormalFadeCmd.camEvent, NormalFadeCmd.buffer);
			NormalFadeCmd.buffer = null;
		}
	}

	public void UpdateCommands()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Expected Obj, but got Unknown
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Expected Obj, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected Obj, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		RenderingPath actualRenderingPath = m_camera.actualRenderingPath;
		CameraEvent val;
		CameraEvent val2;
		CameraEvent val3;
		int num;
		if ((int)actualRenderingPath == 3)
		{
			val = (CameraEvent)15;
			val2 = (CameraEvent)7;
			val3 = (CameraEvent)7;
			num = 1;
		}
		else if ((int)actualRenderingPath == 2)
		{
			val = (CameraEvent)15;
			val2 = (CameraEvent)7;
			val3 = (CameraEvent)7;
			num = 0;
		}
		else
		{
			val = (CameraEvent)15;
			val2 = (CameraEvent)1;
			val3 = (CameraEvent)3;
			num = 0;
		}
		if ((!CopyScreenCmd.enabled || CopyScreenCmd.camEvent != val) && CopyScreenCmd.buffer != null)
		{
			m_camera.RemoveCommandBuffer(CopyScreenCmd.camEvent, CopyScreenCmd.buffer);
			CopyScreenCmd.buffer = null;
		}
		if ((!CopyDepthCmd.enabled || CopyDepthCmd.camEvent != val2) && CopyDepthCmd.buffer != null)
		{
			m_camera.RemoveCommandBuffer(CopyDepthCmd.camEvent, CopyDepthCmd.buffer);
			CopyDepthCmd.buffer = null;
		}
		if ((!NormalFadeCmd.enabled || NormalFadeCmd.camEvent != val3) && NormalFadeCmd.buffer != null)
		{
			m_camera.RemoveCommandBuffer(NormalFadeCmd.camEvent, NormalFadeCmd.buffer);
			NormalFadeCmd.buffer = null;
		}
		if (CopyScreenCmd.enabled && CopyScreenCmd.buffer == null)
		{
			RenderTextureFormat val4 = ((!m_camera.allowHDR) ? ((RenderTextureFormat)0) : ((RenderTextureFormat)2));
			if (!SystemInfo.SupportsRenderTextureFormat(val4))
			{
				val4 = (RenderTextureFormat)0;
			}
			CommandBuffer val5 = new CommandBuffer();
			val5.name = "Ceto CopyScreen Cmd: " + ((Object)m_camera).name;
			int num2 = Shader.PropertyToID("Ceto_CopyScreenTexture_Tmp");
			val5.GetTemporaryRT(num2, -1, -1, 0, (FilterMode)1, val4, (RenderTextureReadWrite)0);
			val5.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), RenderTargetIdentifier.op_Implicit(num2));
			val5.SetGlobalTexture(Ocean.REFRACTION_GRAB_TEXTURE_NAME, RenderTargetIdentifier.op_Implicit(num2));
			CopyScreenCmd.buffer = val5;
			CopyScreenCmd.camEvent = val;
			m_camera.AddCommandBuffer(CopyScreenCmd.camEvent, CopyScreenCmd.buffer);
		}
		if (CopyDepthCmd.enabled && CopyDepthCmd.buffer == null)
		{
			RenderTextureFormat val6 = (RenderTextureFormat)14;
			if (!SystemInfo.SupportsRenderTextureFormat(val6))
			{
				val6 = (RenderTextureFormat)15;
			}
			if (!SystemInfo.SupportsRenderTextureFormat(val6))
			{
				val6 = (RenderTextureFormat)2;
			}
			CommandBuffer val7 = new CommandBuffer();
			val7.name = "Ceto Copy Depth Cmd: " + ((Object)m_camera).name;
			int num3 = Shader.PropertyToID("Ceto_DepthCopyTexture_Tmp");
			val7.GetTemporaryRT(num3, -1, -1, 0, (FilterMode)0, val6, (RenderTextureReadWrite)1);
			val7.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), RenderTargetIdentifier.op_Implicit(num3), CopyDepthCmd.material, 0);
			val7.SetGlobalTexture(Ocean.DEPTH_GRAB_TEXTURE_NAME, RenderTargetIdentifier.op_Implicit(num3));
			CopyDepthCmd.buffer = val7;
			CopyDepthCmd.camEvent = val2;
			m_camera.AddCommandBuffer(CopyDepthCmd.camEvent, CopyDepthCmd.buffer);
		}
		if (NormalFadeCmd.enabled && NormalFadeCmd.buffer == null)
		{
			RenderTextureFormat val8 = (RenderTextureFormat)16;
			if (!SystemInfo.SupportsRenderTextureFormat(val8))
			{
				val8 = (RenderTextureFormat)0;
			}
			CommandBuffer val9 = new CommandBuffer();
			val9.name = "Ceto Normal Fade Cmd: " + ((Object)m_camera).name;
			int num4 = Shader.PropertyToID("Ceto_NormalFadeTexture_Tmp");
			val9.GetTemporaryRT(num4, -1, -1, 0, (FilterMode)1, val8, (RenderTextureReadWrite)1);
			val9.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1), RenderTargetIdentifier.op_Implicit(num4), NormalFadeCmd.material, num);
			val9.SetGlobalTexture(Ocean.NORMAL_FADE_TEXTURE_NAME, RenderTargetIdentifier.op_Implicit(num4));
			NormalFadeCmd.buffer = val9;
			NormalFadeCmd.camEvent = val3;
			m_camera.AddCommandBuffer(NormalFadeCmd.camEvent, NormalFadeCmd.buffer);
		}
		if (NormalFadeCmd.buffer != null)
		{
			NormalFadeCmd.material.SetMatrix("Ceto_NormalFade_MV", m_camera.cameraToWorldMatrix);
		}
	}
}
