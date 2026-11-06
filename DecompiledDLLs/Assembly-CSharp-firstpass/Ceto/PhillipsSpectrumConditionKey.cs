namespace Ceto;

public class PhillipsSpectrumConditionKey : WaveSpectrumConditionKey
{
	public float WindSpeed { get; private set; }

	public PhillipsSpectrumConditionKey(float windSpeed, int size, float windDir, SPECTRUM_TYPE spectrumType, int numGrids)
		: base(size, windDir, spectrumType, numGrids)
	{
		WindSpeed = windSpeed;
	}

	protected override bool Matches(WaveSpectrumConditionKey k)
	{
		PhillipsSpectrumConditionKey phillipsSpectrumConditionKey = k as PhillipsSpectrumConditionKey;
		if (phillipsSpectrumConditionKey == null)
		{
			return false;
		}
		if (WindSpeed != phillipsSpectrumConditionKey.WindSpeed)
		{
			return false;
		}
		return true;
	}

	protected override int AddToHashCode(int hashcode)
	{
		hashcode = hashcode * 37 + WindSpeed.GetHashCode();
		return hashcode;
	}

	public override string ToString()
	{
		return string.Format("[PhillipsSpectrumConditionKey WindSpeed={0}, Size={1}, WindDir={2}, Type={3}, NumGrids={4}", new object[5] { WindSpeed, Size, WindDir, SpectrumType, NumGrids });
	}
}
