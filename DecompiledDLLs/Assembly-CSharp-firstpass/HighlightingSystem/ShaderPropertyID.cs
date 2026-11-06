using UnityEngine;

namespace HighlightingSystem;

public class ShaderPropertyID
{
	public readonly int _MainTex;

	public readonly int _Color;

	public readonly int _Speed;

	public readonly int _Amplitude;

	public readonly int _Frequency;

	public readonly int _HeadLimit;

	public readonly int _Cutoff;

	public readonly int _Intensity;

	public readonly int _Cull;

	public readonly int _HighlightingBlur1;

	public readonly int _HighlightingBlur2;

	public readonly int _HighlightingBuffer;

	public readonly int _HighlightingBlurOffset;

	public ShaderPropertyID()
	{
		_MainTex = Shader.PropertyToID("_MainTex");
		_Color = Shader.PropertyToID("_Color");
		_Speed = Shader.PropertyToID("_Speed");
		_Amplitude = Shader.PropertyToID("_Amplitude");
		_Frequency = Shader.PropertyToID("_Frequency");
		_HeadLimit = Shader.PropertyToID("_HeadLimit");
		_Cutoff = Shader.PropertyToID("_Cutoff");
		_Intensity = Shader.PropertyToID("_Intensity");
		_Cull = Shader.PropertyToID("_Cull");
		_HighlightingBlur1 = Shader.PropertyToID("_HighlightingBlur1");
		_HighlightingBlur2 = Shader.PropertyToID("_HighlightingBlur2");
		_HighlightingBuffer = Shader.PropertyToID("_HighlightingBuffer");
		_HighlightingBlurOffset = Shader.PropertyToID("_HighlightingBlurOffset");
	}
}
