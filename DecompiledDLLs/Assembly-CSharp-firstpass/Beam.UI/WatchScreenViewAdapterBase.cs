using UnityEngine;

namespace Beam.UI;

public abstract class WatchScreenViewAdapterBase : ViewAdapterBase
{
	public abstract void SetWatchTexture(Texture texture);

	public abstract void SetWatchMesh(int type);

	public abstract void SetBacklight(bool enabled);
}
