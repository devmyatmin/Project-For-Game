namespace Beam.Intro;

public struct RefillArgs
{
	public IPlayer Player { get; set; }

	public IHaveFuel Refillable { get; set; }

	public IBase Obj { get; set; }
}
