using System;

namespace Ceto;

public abstract class WaveSpectrumConditionKey : IEquatable<WaveSpectrumConditionKey>
{
	public int Size { get; private set; }

	public int NumGrids { get; private set; }

	public float WindDir { get; private set; }

	public SPECTRUM_TYPE SpectrumType { get; private set; }

	public WaveSpectrumConditionKey(int size, float windDir, SPECTRUM_TYPE spectrumType, int numGrids)
	{
		Size = size;
		NumGrids = numGrids;
		WindDir = windDir;
		SpectrumType = spectrumType;
	}

	protected abstract bool Matches(WaveSpectrumConditionKey k);

	protected abstract int AddToHashCode(int hashcode);

	public static bool operator ==(WaveSpectrumConditionKey k1, WaveSpectrumConditionKey k2)
	{
		if ((object)k1 == k2)
		{
			return true;
		}
		if ((object)k1 == null || (object)k2 == null)
		{
			return false;
		}
		if (k1.Size != k2.Size)
		{
			return false;
		}
		if (k1.NumGrids != k2.NumGrids)
		{
			return false;
		}
		if (k1.WindDir != k2.WindDir)
		{
			return false;
		}
		if (k1.SpectrumType != k2.SpectrumType)
		{
			return false;
		}
		return k1.Matches(k2);
	}

	public static bool operator !=(WaveSpectrumConditionKey k1, WaveSpectrumConditionKey k2)
	{
		return !(k1 == k2);
	}

	public override bool Equals(object o)
	{
		WaveSpectrumConditionKey waveSpectrumConditionKey = o as WaveSpectrumConditionKey;
		if (waveSpectrumConditionKey == null)
		{
			return false;
		}
		return waveSpectrumConditionKey == this;
	}

	public bool Equals(WaveSpectrumConditionKey k)
	{
		return k == this;
	}

	public override int GetHashCode()
	{
		int num = 23;
		num = num * 37 + Size.GetHashCode();
		num = num * 37 + NumGrids.GetHashCode();
		num = num * 37 + WindDir.GetHashCode();
		num = num * 37 + SpectrumType.GetHashCode();
		return AddToHashCode(num);
	}
}
