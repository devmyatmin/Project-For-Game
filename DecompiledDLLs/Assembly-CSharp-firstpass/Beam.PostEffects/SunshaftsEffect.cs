using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public class SunshaftsEffect : PostProcessEffectRenderer<SunshaftsSettings>
{
	public enum Pass
	{
		Screen,
		RadialBlur,
		Depth
	}

	public override void Render(PostProcessRenderContext context)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer command = context.command;
		command.BeginSample("SunshaftsEffect");
		int num = context.width / ParameterOverride<int>.op_Implicit((ParameterOverride<int>)(object)base.settings.Downsampling);
		int num2 = context.height / ParameterOverride<int>.op_Implicit((ParameterOverride<int>)(object)base.settings.Downsampling);
		Vector3 val = context.camera.WorldToViewportPoint(Singleton<Atmosphere>.Instance.CurrentIlluminary.Transform.position);
		PropertySheet val2 = context.propertySheets.Get("Beam Team/Post Effects/Sunshafts");
		val2.properties.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.BlurRadius));
		val2.properties.SetVector("_SunPosition", new Vector4(val.x, val.y, val.z, ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.DistanceFalloff)));
		val2.properties.SetVector("_SunThreshold", Color.op_Implicit(new Color(0.87f, 0.74f, 0.65f)));
		float num3 = ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.BlurRadius) * 0.0013020834f;
		val2.properties.SetVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
		int num4 = Shader.PropertyToID("_SunshaftsBlitSource");
		command.GetTemporaryRT(num4, num, num2, 0);
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, RenderTargetIdentifier.op_Implicit(num4), val2, 2, false);
		int num5 = 0;
		int num6 = ParameterOverride<int>.op_Implicit((ParameterOverride<int>)(object)base.settings.BlurIterations);
		for (int i = 0; i < num6; i++)
		{
			int num7 = Shader.PropertyToID("_SunshaftsIteration" + num5++);
			command.GetTemporaryRT(num7, num, num2, 0);
			RuntimeUtilities.BlitFullscreenTriangle(command, RenderTargetIdentifier.op_Implicit(num4), RenderTargetIdentifier.op_Implicit(num7), val2, 1, false);
			command.ReleaseTemporaryRT(num4);
			num3 = ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.BlurRadius) * (((float)i * 2f + 1f) * 6f) / 768f;
			val2.properties.SetVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
			num4 = Shader.PropertyToID("_SunshaftsIteration" + num5++);
			command.GetTemporaryRT(num4, num, num2, 0);
			RuntimeUtilities.BlitFullscreenTriangle(command, RenderTargetIdentifier.op_Implicit(num7), RenderTargetIdentifier.op_Implicit(num4), val2, 1, false);
			command.ReleaseTemporaryRT(num7);
			num3 = ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.BlurRadius) * (((float)i * 2f + 2f) * 6f) / 768f;
			val2.properties.SetVector("_BlurRadius4", new Vector4(num3, num3, 0f, 0f));
		}
		Color color = Singleton<Atmosphere>.Instance.CurrentIlluminary.Light.color;
		Color linear = color.linear;
		Vector4 val3 = ((val.z >= 0f) ? (new Vector4(linear.r, linear.g, linear.b, linear.a) * ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.Intensity)) : Vector4.zero);
		val2.properties.SetVector("_SunColor", val3);
		command.SetGlobalTexture("_ColorBuffer", RenderTargetIdentifier.op_Implicit(num4));
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, val2, 0, false);
		command.ReleaseTemporaryRT(num4);
		command.EndSample("SunshaftsEffect");
	}
}
