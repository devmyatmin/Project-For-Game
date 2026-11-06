namespace Beam;

public interface IFixedTick
{
	FixedTickRate Rate { get; }

	bool Active { get; }

	void FixedTick();
}
