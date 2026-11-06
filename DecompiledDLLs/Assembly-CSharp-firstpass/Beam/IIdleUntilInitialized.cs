namespace Beam;

public interface IIdleUntilInitialized
{
	bool IsInitialized { get; }

	void Initialize();
}
