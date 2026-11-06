using Beam.Serialization.Json;
using SharpNeatLib.Maths;

namespace Beam.Utilities;

public class DeterministicFastRandom : ISaveable
{
	private readonly FastRandom _generator;

	private int _seed;

	private int _iter;

	public DeterministicFastRandom()
	{
		_generator = new FastRandom();
	}

	public int GetSeed()
	{
		return _seed;
	}

	public int GetIter()
	{
		return _iter;
	}

	public int Next()
	{
		_iter++;
		return _generator.Next();
	}

	public int Next(int lowerBound, int upperBound)
	{
		_iter++;
		return _generator.Next(lowerBound, upperBound);
	}

	public double NextDouble()
	{
		_iter++;
		return _generator.NextDouble();
	}

	public void RestoreState(int seed, int iter)
	{
		_seed = seed;
		_iter = iter;
		_generator.Reinitialise(_seed);
		for (int i = 0; i < _iter; i++)
		{
			_generator.Next();
		}
	}

	JObject ISaveable.Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Seed", _seed);
		jObject.AddField("Iter", _iter);
		return jObject;
	}

	void ISaveable.Load(JObject data)
	{
		int value = data.GetField("Seed").GetValue<int>();
		int value2 = data.GetField("Iter").GetValue<int>();
		RestoreState(value, value2);
	}
}
