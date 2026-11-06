using Beam.Utilities;
using UnityEngine;

namespace Beam.UI;

public class PersistentUserInterfaceProvider : UserInterfaceProvider
{
	public static PersistentUserInterfaceProvider Instance { get; private set; }

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}
}
