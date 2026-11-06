using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

public sealed class HighlighterEffect : PostProcessEffectRenderer<HighlighterSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer command = context.command;
		command.BeginSample("HighlightingPostEffect");
		PropertySheet val = context.propertySheets.Get(Shader.Find("Beam Team/Post Effects/Highlighter Composite"));
		val.properties.Clear();
		RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, val, 0, false);
		command.EndSample("HighlightingPostEffect");
	}
}
