using System;
using UnityEngine;

namespace Ceto;

public class UnifiedSpectrumCondition : WaveSpectrumCondition
{
	public UnifiedSpectrumCondition(int size, float windSpeed, float windDir, float waveAge, int numGrids)
		: base(size, numGrids)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		if (numGrids < 1 || numGrids > 4)
		{
			throw new ArgumentException("UnifiedSpectrumCondition must have 1 to 4 grids not " + numGrids);
		}
		Key = new UnifiedSpectrumConditionKey(windSpeed, waveAge, size, windDir, SPECTRUM_TYPE.UNIFIED, numGrids);
		switch (numGrids)
		{
		case 1:
			GridSizes = new Vector4(772f, 1f, 1f, 1f);
			Choppyness = new Vector4(2.3f, 1f, 1f, 1f);
			WaveAmps = new Vector4(1f, 1f, 1f, 1f);
			break;
		case 2:
			GridSizes = new Vector4(772f, 57f, 1f, 1f);
			Choppyness = new Vector4(2.3f, 2.1f, 1f, 1f);
			WaveAmps = new Vector4(1f, 1f, 1f, 1f);
			break;
		case 3:
			GridSizes = new Vector4(1372f, 392f, 28f, 1f);
			Choppyness = new Vector4(2.3f, 2.1f, 1.6f, 1f);
			WaveAmps = new Vector4(1f, 1f, 1f, 1f);
			break;
		case 4:
			GridSizes = new Vector4(1372f, 392f, 28f, 4f);
			Choppyness = new Vector4(2.3f, 2.1f, 1.6f, 0.9f);
			WaveAmps = new Vector4(1f, 1f, 1f, 1f);
			break;
		}
	}

	public override SpectrumTask GetCreateSpectrumConditionTask()
	{
		UnifiedSpectrumConditionKey unifiedSpectrumConditionKey = Key as UnifiedSpectrumConditionKey;
		UnifiedSpectrum unifiedSpectrum = new UnifiedSpectrum(unifiedSpectrumConditionKey.WindSpeed, unifiedSpectrumConditionKey.WindDir, unifiedSpectrumConditionKey.WaveAge);
		return new SpectrumTask(this, multiThreadTask: true, new ISpectrum[4] { unifiedSpectrum, unifiedSpectrum, unifiedSpectrum, unifiedSpectrum });
	}
}
