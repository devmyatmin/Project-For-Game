namespace Beam;

public interface ITick
{
	bool Active { get; }

	void Tick();
}
