using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ceto;

public class FoamOverlay : WaveOverlay
{
	public Vector3 Momentum
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public float Spin { get; set; }

	public float Expansion { get; set; }

	public float Size { get; set; }

	public FoamOverlay(Vector3 pos, float rotation, float size, float duration, Texture texture)
		: base(pos, rotation, new Vector2(size * 0.5f, size * 0.5f), duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Size = size;
		FoamTex.tex = texture;
	}

	public void Reset(Vector3 pos, float rotation, float size, float duration, Texture texture)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Reset(pos, rotation, new Vector2(size * 0.5f, size * 0.5f), duration);
		Size = size;
		FoamTex.tex = texture;
		Momentum = Vector3.zero;
		Spin = 0f;
		Expansion = 0f;
	}

	public override void UpdateOverlay()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Position += Momentum * Time.deltaTime;
		Size += Expansion * Time.deltaTime;
		Rotation += Spin * Time.deltaTime;
		HalfSize = new Vector2(Size * 0.5f, Size * 0.5f);
		base.UpdateOverlay();
	}
}
