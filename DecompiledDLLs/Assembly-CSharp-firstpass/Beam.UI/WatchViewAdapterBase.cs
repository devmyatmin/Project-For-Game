using Beam.Obfuscated.Users;
using UnityEngine;

namespace Beam.UI;

public abstract class WatchViewAdapterBase : ViewAdapterBase
{
	public abstract Camera Camera { get; }

	public abstract void SetController(WatchController controller);

	public abstract void SetTemperature(float temperature);

	public abstract void SetTemperatureUnits(Temperature temperature);

	public abstract void SetHeatwave(bool heatwave);

	public abstract void SetUVIndex(string uvIndex);

	public abstract void SetClock(string time);

	public abstract void SetDate(string date);

	public abstract void SetDays(string days);

	public abstract void SetHealth(float health);

	public abstract void SetStatistics(Stats stats);

	public abstract void SetHunger(float hunger);

	public abstract void SetThirst(float thirst);

	public abstract void SetSleep(float sleep);

	public abstract void SetWatchMode(int mode);

	public abstract void CycleWatchModeForward();

	public abstract void CycleWatchModeBackward();

	public abstract void SetHunting(float hunting);

	public abstract void SetCooking(float cooking);

	public abstract void SetHarvesting(float harvesting);

	public abstract void SetPhysical(float physical);

	public abstract void SetCraftsmanship(float craftsmanship);
}
