namespace Beam;

public interface IReloadable
{
	AnimationType ReloadAnimation { get; }

	bool Reloaded { get; }

	bool CanReloadWith(IPickupable pickupable);

	void Reload(IPickupable pickupable);
}
