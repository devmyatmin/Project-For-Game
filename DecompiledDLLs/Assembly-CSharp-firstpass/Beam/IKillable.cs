namespace Beam;

public interface IKillable
{
	bool IsAlive { get; }

	void Kill(int killerId);
}
