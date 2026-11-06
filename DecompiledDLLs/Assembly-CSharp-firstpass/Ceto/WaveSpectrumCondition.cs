using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ceto;

public abstract class WaveSpectrumCondition
{
	public bool Done { get; set; }

	public Vector4 GridSizes
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

	public Vector4 Choppyness
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

	public Vector4 WaveAmps
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

	public WaveSpectrumConditionKey Key { get; protected set; }

	public Texture2D Spectrum01 { get; private set; }

	public Color[] SpectrumData01 { get; private set; }

	public Texture2D Spectrum23 { get; private set; }

	public Color[] SpectrumData23 { get; private set; }

	public Texture2D WTable { get; private set; }

	public Color[] WTableData { get; private set; }

	public int LastUpdated { get; set; }

	public bool SupportsJacobians { get; protected set; }

	public WaveSpectrumCondition(int size, int numGrids)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		GridSizes = Vector4.one;
		Choppyness = Vector4.one;
		WaveAmps = Vector4.one;
		SpectrumData01 = new Color[size * size];
		if (numGrids > 2)
		{
			SpectrumData23 = new Color[size * size];
		}
		WTableData = new Color[size * size];
		LastUpdated = -1;
		SupportsJacobians = true;
	}

	public abstract SpectrumTask GetCreateSpectrumConditionTask();

	public InitSpectrumDisplacementsTask GetInitSpectrumDisplacementsTask(DisplacementBufferCPU buffer, float time)
	{
		return new InitSpectrumDisplacementsTask(buffer, this, time);
	}

	public Vector4 InverseGridSizes()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI * 2f * (float)Key.Size;
		return new Vector4(num / GridSizes.x, num / GridSizes.y, num / GridSizes.z, num / GridSizes.w);
	}

	public void Release()
	{
		if ((Object)(object)Spectrum01 != (Object)null)
		{
			Object.Destroy((Object)(object)Spectrum01);
			Spectrum01 = null;
		}
		if ((Object)(object)Spectrum23 != (Object)null)
		{
			Object.Destroy((Object)(object)Spectrum23);
			Spectrum23 = null;
		}
		if ((Object)(object)WTable != (Object)null)
		{
			Object.Destroy((Object)(object)WTable);
			WTable = null;
		}
	}

	public void CreateTextures()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected Obj, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		if ((Object)(object)Spectrum01 == (Object)null)
		{
			Spectrum01 = new Texture2D(Key.Size, Key.Size, (TextureFormat)20, false, true);
			((Texture)Spectrum01).filterMode = (FilterMode)0;
			((Texture)Spectrum01).wrapMode = (TextureWrapMode)0;
			((Object)Spectrum01).hideFlags = (HideFlags)61;
			((Object)Spectrum01).name = "Ceto Spectrum01 Condition";
		}
		if ((Object)(object)Spectrum23 == (Object)null && Key.NumGrids > 2)
		{
			Spectrum23 = new Texture2D(Key.Size, Key.Size, (TextureFormat)20, false, true);
			((Texture)Spectrum23).filterMode = (FilterMode)0;
			((Texture)Spectrum23).wrapMode = (TextureWrapMode)0;
			((Object)Spectrum23).hideFlags = (HideFlags)61;
			((Object)Spectrum23).name = "Ceto Spectrum23 Condition";
		}
		if ((Object)(object)WTable == (Object)null)
		{
			WTable = new Texture2D(Key.Size, Key.Size, (TextureFormat)20, false, true);
			((Texture)WTable).filterMode = (FilterMode)0;
			((Texture)WTable).wrapMode = (TextureWrapMode)1;
			((Object)WTable).hideFlags = (HideFlags)61;
			((Object)WTable).name = "Ceto Wave Spectrum GPU WTable";
		}
	}

	public void Apply(Color[] spectrum01, Color[] spectrum23, Color[] wtable)
	{
		if ((Object)(object)Spectrum01 != (Object)null && spectrum01 != null)
		{
			Spectrum01.SetPixels(spectrum01);
			Spectrum01.Apply();
			Array.Copy(spectrum01, SpectrumData01, spectrum01.Length);
		}
		if ((Object)(object)Spectrum23 != (Object)null && spectrum23 != null)
		{
			Spectrum23.SetPixels(spectrum23);
			Spectrum23.Apply();
			Array.Copy(spectrum23, SpectrumData23, spectrum23.Length);
		}
		if ((Object)(object)WTable != (Object)null && wtable != null)
		{
			WTable.SetPixels(wtable);
			WTable.Apply();
			Array.Copy(wtable, WTableData, wtable.Length);
		}
	}
}
