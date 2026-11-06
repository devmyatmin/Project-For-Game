using UnityEngine;

namespace Beam.UI;

public abstract class LeaderboardEntryElement : MonoBehaviour
{
	public abstract void SetUsername(string username);

	public abstract void SetRank(int rank);

	public abstract void SetValue(int value);

	public abstract void SetDisplayType(ELeaderboardDisplayType displayType);
}
