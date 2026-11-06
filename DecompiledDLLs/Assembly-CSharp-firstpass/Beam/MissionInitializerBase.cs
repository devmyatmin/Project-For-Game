using Funlabs;
using UnityEngine;

namespace Beam;

public abstract class MissionInitializerBase : MonoBehaviour
{
	public enum LinkWithEnvironmentType
	{
		None,
		Client_LuscaTheSquid
	}

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private MissionControllerBase _mission;

	[SerializeField]
	private LinkWithEnvironmentType LinkWithEnvironment;

	protected MissionControllerBase Mission => _mission;

	protected virtual void Start()
	{
		WireMission();
		LinkWithEnvironmentGameobject();
	}

	protected virtual void OnDestroy()
	{
		UnwireMission();
	}

	private void LinkWithEnvironmentGameobject()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		LinkWithEnvironmentType linkWithEnvironment = LinkWithEnvironment;
		if (linkWithEnvironment != LinkWithEnvironmentType.None && linkWithEnvironment == LinkWithEnvironmentType.Client_LuscaTheSquid && MPExtensions.IsClient(Game.Mode))
		{
			BuoyMultiplayerSync buoyMultiplayerSync = Object.FindObjectOfType<BuoyMultiplayerSync>();
			if (Object.op_Implicit((Object)(object)buoyMultiplayerSync))
			{
				buoyMultiplayerSync.TryToAttachToBouy();
			}
		}
	}

	protected abstract void WireMission();

	protected abstract void UnwireMission();
}
