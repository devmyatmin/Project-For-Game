using Beam.Obfuscated.Users;
using UnityEngine;

namespace Beam.UI;

public class NullWatchViewAdapter : WatchViewAdapterBase
{
	private static readonly NullWatchViewAdapter _Instance = new NullWatchViewAdapter();

	public static NullWatchViewAdapter Instance => _Instance;

	public override Camera Camera
	{
		get
		{
			Debug.LogError((object)"NullWatchViewAdapter::Camera:: Not implemented!");
			return null;
		}
	}

	public override void SetController(WatchController controller)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetController:: Not implemented!");
	}

	public override void SetTemperature(float temperature)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetTemperature:: Not implemented!");
	}

	public override void SetTemperatureUnits(Temperature temperature)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetTemperatureUnits:: Not implemented!");
	}

	public override void SetHeatwave(bool heatwave)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetHeatwave:: Not implemented!");
	}

	public override void SetUVIndex(string uvIndex)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetUVIndex:: Not implemented!");
	}

	public override void SetClock(string time)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetClock:: Not implemented!");
	}

	public override void SetDate(string date)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetDate:: Not implemented!");
	}

	public override void SetDays(string days)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetDays:: Not implemented!");
	}

	public override void SetStatistics(Stats stats)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetStatistics:: Not implemented!");
	}

	public override void SetHealth(float health)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetHealth:: Not implemented!");
	}

	public override void SetHunger(float hunger)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetHunger:: Not implemented!");
	}

	public override void SetThirst(float thirst)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetThirst:: Not implemented!");
	}

	public override void SetSleep(float sleep)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetSleep:: Not implemented!");
	}

	public override void SetWatchMode(int mode)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetWatchMode:: Not implemented!");
	}

	public override void CycleWatchModeForward()
	{
		Debug.LogError((object)"NullWatchViewAdapter::CycleWatchModeForward:: Not implemented!");
	}

	public override void CycleWatchModeBackward()
	{
		Debug.LogError((object)"NullWatchViewAdapter::CycleWatchModeBackward:: Not implemented!");
	}

	public override void SetHunting(float hunting)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetHunting:: Not implemented!");
	}

	public override void SetCooking(float cooking)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetCooking:: Not implemented!");
	}

	public override void SetHarvesting(float harvesting)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetHarvesting:: Not implemented!");
	}

	public override void SetPhysical(float physical)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetPhysical:: Not implemented!");
	}

	public override void SetCraftsmanship(float craftsmanship)
	{
		Debug.LogError((object)"NullWatchViewAdapter::SetCraftsmanship:: Not implemented!");
	}
}
