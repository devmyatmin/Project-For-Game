using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ceto;

public class WaveOverlay
{
	private static readonly Vector4[] CORNERS = new Vector4[4]
	{
		new Vector4(-1f, 0f, -1f, 1f),
		new Vector4(1f, 0f, -1f, 1f),
		new Vector4(1f, 0f, 1f, 1f),
		new Vector4(-1f, 0f, 1f, 1f)
	};

	public bool Kill { get; set; }

	public bool Hide { get; set; }

	public Vector3 Position
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

	public Vector2 HalfSize
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

	public float Rotation { get; set; }

	public float Creation { get; protected set; }

	public float Age => OceanTime() - Creation;

	public float Duration { get; protected set; }

	public float NormalizedAge => Mathf.Clamp01(Age / Duration);

	public Vector4[] Corners { get; private set; }

	public OverlayHeightTexture HeightTex { get; set; }

	public OverlayNormalTexture NormalTex { get; set; }

	public OverlayFoamTexture FoamTex { get; set; }

	public OverlayClipTexture ClipTex { get; set; }

	public Matrix4x4 LocalToWorld
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Bounds BoundingBox
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public WaveOverlay(Vector3 pos, float rotation, Vector2 halfSize, float duration)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Position = pos;
		HalfSize = halfSize;
		Rotation = rotation;
		Creation = OceanTime();
		Duration = Mathf.Max(duration, 0.001f);
		Corners = new Vector4[4];
		HeightTex = new OverlayHeightTexture();
		NormalTex = new OverlayNormalTexture();
		FoamTex = new OverlayFoamTexture();
		ClipTex = new OverlayClipTexture();
		CalculateLocalToWorld();
		CalculateBounds();
	}

	public WaveOverlay()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		HalfSize = Vector2.one;
		Rotation = 0f;
		Creation = OceanTime();
		Duration = 0.001f;
		Corners = new Vector4[4];
		HeightTex = new OverlayHeightTexture();
		NormalTex = new OverlayNormalTexture();
		FoamTex = new OverlayFoamTexture();
		ClipTex = new OverlayClipTexture();
		CalculateLocalToWorld();
		CalculateBounds();
	}

	private float OceanTime()
	{
		if ((Object)(object)Ocean.Instance == (Object)null)
		{
			return 0f;
		}
		return Ocean.Instance.OceanTime.Now;
	}

	public void Reset(Vector3 pos, float rotation, Vector2 halfSize, float duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Position = pos;
		HalfSize = halfSize;
		Rotation = rotation;
		Creation = OceanTime();
		Duration = Mathf.Max(duration, 0.001f);
		Kill = false;
		Hide = false;
		CalculateLocalToWorld();
		CalculateBounds();
	}

	public virtual void UpdateOverlay()
	{
		CalculateLocalToWorld();
		CalculateBounds();
	}

	public virtual void CalculateLocalToWorld()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(HalfSize.x, 1f, HalfSize.y);
		LocalToWorld = Matrix4x4.TRS(new Vector3(Position.x, 0f, Position.z), Quaternion.Euler(0f, Rotation, 0f), val);
	}

	public virtual void CalculateBounds()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(HalfSize.x, 1f, HalfSize.y);
		float num = 0f;
		if ((Object)(object)Ocean.Instance != (Object)null)
		{
			num = Ocean.Instance.level;
			val.y = Ocean.MAX_WAVE_HEIGHT;
		}
		float num2 = float.PositiveInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = float.NegativeInfinity;
		for (int i = 0; i < 4; i++)
		{
			Corners[i] = LocalToWorld * CORNERS[i];
			Corners[i].y = num;
			if (Corners[i].x < num2)
			{
				num2 = Corners[i].x;
			}
			if (Corners[i].z < num3)
			{
				num3 = Corners[i].z;
			}
			if (Corners[i].x > num4)
			{
				num4 = Corners[i].x;
			}
			if (Corners[i].z > num5)
			{
				num5 = Corners[i].z;
			}
		}
		Vector3 val2 = new Vector3(Position.x, num, Position.z);
		Vector3 val3 = new Vector3(num4 - num2, val.y, num5 - num3);
		BoundingBox = new Bounds(val2, val3);
	}

	public bool Contains(float x, float z)
	{
		float u;
		float v;
		return Contains(x, z, out u, out v);
	}

	public bool Contains(float x, float z, out float u, out float v)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		u = 0f;
		v = 0f;
		float num = x - Position.x;
		float num2 = z - Position.z;
		float rotation = Rotation;
		float num3 = Mathf.Cos(rotation * (float)Math.PI / 180f);
		float num4 = Mathf.Sin(rotation * (float)Math.PI / 180f);
		u = num * num3 - num2 * num4;
		v = num * num4 + num2 * num3;
		if (u > 0f - HalfSize.x && u < HalfSize.x && v > 0f - HalfSize.y && v < HalfSize.y)
		{
			u /= HalfSize.x;
			v /= HalfSize.y;
			u = u * 0.5f + 0.5f;
			v = v * 0.5f + 0.5f;
			return true;
		}
		return false;
	}

	static WaveOverlay()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
	}
}
