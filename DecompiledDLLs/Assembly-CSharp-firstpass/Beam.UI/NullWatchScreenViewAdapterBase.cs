using UnityEngine;

namespace Beam.UI;

public class NullWatchScreenViewAdapterBase : WatchScreenViewAdapterBase
{
	private static readonly NullWatchScreenViewAdapterBase _Instance = new NullWatchScreenViewAdapterBase();

	public static NullWatchScreenViewAdapterBase Instance => _Instance;

	public override void SetWatchTexture(Texture texture)
	{
		Debug.LogError((object)"NullWatchScreenViewAdapterBase::SetWatchTexture:: Not implemented!");
	}

	public override void SetWatchMesh(int type)
	{
		Debug.LogError((object)"NullWatchScreenViewAdapterBase::SetWatchMesh:: Not implemented!");
	}

	public override void SetBacklight(bool enabled)
	{
		Debug.LogError((object)"NullWatchScreenViewAdapterBase::SetBacklight:: Not implemented!");
	}
}
