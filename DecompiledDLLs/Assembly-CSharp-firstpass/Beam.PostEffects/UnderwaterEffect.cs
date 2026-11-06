using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public class UnderwaterEffect : PostProcessEffectRenderer<UnderwaterSettings>
{
	public enum Pass
	{
		UnderwaterFog,
		ApplyMask
	}

	public override void Render(PostProcessRenderContext context)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer command = context.command;
		command.BeginSample("UnderwaterPostEffect");
		PropertySheet val = context.propertySheets.Get(Shader.Find("Beam Team/Post Effects/Underwater"));
		int num = Shader.PropertyToID("_UnderwaterPostProcessEffect");
		command.GetTemporaryRT(num, context.width, context.height, 0);
		CommandBufferExtensions.BlitFullscreenQuad(command, context.source, RenderTargetIdentifier.op_Implicit(num), val, 0);
		command.SetGlobalTexture("_BelowTex", RenderTargetIdentifier.op_Implicit(num));
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, val, 1, false);
		command.ReleaseTemporaryRT(num);
		command.EndSample("UnderwaterPostEffect");
	}
}
