using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public class AtmosphericFogEffect : PostProcessEffectRenderer<AtmosphericFogSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer command = context.command;
		command.BeginSample("AtmosphericFogPostEffect");
		PropertySheet val = context.propertySheets.Get(Shader.Find("Beam Team/Post Effects/Atmospheric Fog"));
		val.properties.SetFloat("_Height", 1f / ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.Height));
		val.properties.SetFloat("_StartDistance", 1f / Mathf.Max(ParameterOverride<float>.op_Implicit((ParameterOverride<float>)(object)base.settings.StartDistance), 0f));
		CommandBufferExtensions.BlitFullscreenQuad(command, context.source, context.destination, val, 0);
		command.EndSample("AtmosphericFogPostEffect");
	}
}
