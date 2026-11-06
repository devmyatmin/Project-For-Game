using Beam;

namespace Funlabs;

public static class MultiplayerFenceExtension
{
	public static Fence ToFence(this MissionAssetLoader.MissionType missionType)
	{
		return (Fence)missionType;
	}
}
