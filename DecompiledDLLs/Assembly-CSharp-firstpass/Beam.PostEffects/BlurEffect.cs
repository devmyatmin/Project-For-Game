using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public sealed class BlurEffect : PostProcessEffectRenderer<BlurSettings>
{
	public enum Mode
	{
		StandardGaussian,
		SgxGaussian
	}

	public enum Pass
	{
		Downsample = 0,
		BlurVertical = 1,
		BlurHorizontal = 2,
		Composite = 5
	}

	public override void Render(PostProcessRenderContext context)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer command = context.command;
		command.BeginSample("BlurPostEffect");
		int num = ParameterOverride<int>.op_Implicit((ParameterOverride<int>)(object)base.settings.Downsample);
		int num2 = ParameterOverride<int>.op_Implicit((ParameterOverride<int>)(object)base.settings.BlurIterations);
		float num3 = ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.BlurSize);
		float num4 = 1f / (1f * (float)(1 << num));
		int num5 = context.width >> num;
		int num6 = context.height >> num;
		PropertySheet val = context.propertySheets.Get(Shader.Find("Beam Team/Post Effects/Blur"));
		val.properties.Clear();
		val.properties.SetVector("_Parameter", new Vector4(num3 * num4, (0f - num3) * num4, 0f, 0f));
		val.properties.SetTexture("_MaskTex", (Texture)(((object)((ParameterOverride<Texture>)(object)base.settings.BlurMask).value) ?? ((object)Texture2D.blackTexture)));
		val.properties.SetFloat("_Fade", ((ParameterOverride<float>)(object)base.settings.Alpha).value);
		int num7 = Shader.PropertyToID("_BlurPostProcessEffect");
		command.GetTemporaryRT(num7, num5, num6, 0, (FilterMode)1);
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, RenderTargetIdentifier.op_Implicit(num7), val, 0, false);
		int num8 = ((((ParameterOverride<Mode>)base.settings.Mode).value == Mode.SgxGaussian) ? 2 : 0);
		int num9 = 0;
		for (int i = 0; i < num2; i++)
		{
			float num10 = (float)i * 1f;
			val.properties.SetVector("_Parameter", new Vector4(num3 * num4 + num10, (0f - num3) * num4 - num10, 0f, 0f));
			int num11 = Shader.PropertyToID("_BlurPostProcessEffect" + num9++);
			command.GetTemporaryRT(num11, num5, num6, 0, (FilterMode)1);
			RuntimeUtilities.BlitFullscreenTriangle(command, RenderTargetIdentifier.op_Implicit(num7), RenderTargetIdentifier.op_Implicit(num11), val, 1 + num8, false);
			command.ReleaseTemporaryRT(num7);
			num7 = num11;
			num11 = Shader.PropertyToID("_BlurPostProcessEffect" + num9++);
			command.GetTemporaryRT(num11, num5, num6, 0, (FilterMode)1);
			RuntimeUtilities.BlitFullscreenTriangle(command, RenderTargetIdentifier.op_Implicit(num7), RenderTargetIdentifier.op_Implicit(num11), val, 2 + num8, false);
			command.ReleaseTemporaryRT(num7);
			num7 = num11;
		}
		command.SetGlobalTexture("_BlurTex", RenderTargetIdentifier.op_Implicit(num7));
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, val, 5, false);
		command.ReleaseTemporaryRT(num7);
		command.EndSample("BlurPostEffect");
	}
}
