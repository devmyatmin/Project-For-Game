using System;
using UdpKit;
using UnityEngine;

namespace Beam.UI;

public abstract class LobbyEntryElement : MonoBehaviour
{
	public abstract void SetRoomname(string username);

	public abstract void SetRank(int rank);

	public abstract void SetValue(int value, ELeaderboardDisplayType displayType);

	public abstract void SetDisplayType(ELeaderboardDisplayType displayType);

	public abstract void SetSession(Guid guid, UdpSession session, Session_Filter session_Filter);
}
