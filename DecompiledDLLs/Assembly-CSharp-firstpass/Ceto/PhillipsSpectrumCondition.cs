using System;
using UnityEngine;

namespace Ceto;

public class PhillipsSpectrumCondition : WaveSpectrumCondition
{
	public PhillipsSpectrumCondition(int size, float windSpeed, float windDir, float waveAge, int numGrids)
		: base(size, numGrids)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		if (numGrids < 1 || numGrids > 4)
		{
			throw new ArgumentException("PhillipsSpectrumCondition must have 1 to 4 grids not " + numGrids);
		}
		Key = new PhillipsSpectrumConditionKey(windSpeed, size, windDir, SPECTRUM_TYPE.PHILLIPS, numGrids);
		switch (numGrids)
		{
		case 1:
			GridSizes = new Vector4(217f, 1f, 1f, 1f);
			Choppyness = new Vector4(1.5f, 1f, 1f, 1f);
			WaveAmps = new Vector4(0.5f, 1f, 1f, 1f);
			break;
		case 2:
			GridSizes = new Vector4(217f, 97f, 1f, 1f);
			Choppyness = new Vector4(1.5f, 1.2f, 1f, 1f);
			WaveAmps = new Vector4(0.5f, 1f, 1f, 1f);
			break;
		case 3:
			GridSizes = new Vector4(217f, 97f, 31f, 1f);
			Choppyness = new Vector4(1.5f, 1.2f, 1f, 1f);
			WaveAmps = new Vector4(0.5f, 1f, 1f, 1f);
			break;
		case 4:
			GridSizes = new Vector4(1372f, 217f, 97f, 31f);
			Choppyness = new Vector4(1.5f, 1.2f, 1f, 1f);
			WaveAmps = new Vector4(0.25f, 0.5f, 1f, 1f);
			break;
		}
	}

	public override SpectrumTask GetCreateSpectrumConditionTask()
	{
		PhillipsSpectrumConditionKey phillipsSpectrumConditionKey = Key as PhillipsSpectrumConditionKey;
		PhillipsSpectrum phillipsSpectrum = new PhillipsSpectrum(phillipsSpectrumConditionKey.WindSpeed, phillipsSpectrumConditionKey.WindDir);
		return new SpectrumTask(this, multiThreadTask: true, new ISpectrum[4] { phillipsSpectrum, phillipsSpectrum, phillipsSpectrum, phillipsSpectrum });
	}
}
