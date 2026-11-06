using System;
using System.Collections.Generic;
using System.Threading;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam;

public static class BeamTiming
{
	public class ThreadedAction
	{
		private bool _isDone;

		private string _tag;

		public ThreadedAction(Action action)
		{
			ThreadedAction threadedAction = this;
			new Thread(() =>
			{
				try
				{
					action?.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogError((object)("Error exectuing ThreadedAction\n" + ex.Message + "\n" + ex.StackTrace));
				}
				threadedAction._isDone = true;
			}).Start();
		}

		public float WaitUntilDone(string tag)
		{
			return Timing.WaitUntilDone(WaitUntilDone_Sequence(tag));
		}

		private IEnumerator<float> WaitUntilDone_Sequence(string tag)
		{
			MultiplayerMng.Log("Wait for " + tag + "...");
			while (!_isDone)
			{
				yield return 0f;
			}
			MultiplayerMng.Log("Wait for " + tag + "...Done");
		}
	}

	public static float WaitForSeconds(float seconds, TimingUpdateMode mode = TimingUpdateMode.Scaled)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (mode != TimingUpdateMode.Scaled)
		{
			return Timing.WaitUntilDone(Timing.RunCoroutine(WaitForSecondsUnscaled_Sequence(seconds)));
		}
		return Timing.WaitForSeconds(seconds);
	}

	public static CoroutineHandle RunCoroutine(IEnumerator<float> routine)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(routine);
	}

	private static IEnumerator<float> WaitForSecondsUnscaled_Sequence(float seconds)
	{
		float timer = 0f;
		while (timer < seconds)
		{
			timer += Time.unscaledDeltaTime;
			yield return 0f;
		}
	}

	public static float WaitForAllPeers()
	{
		return Timing.WaitUntilDone(WaitForAllPeers_Sequence());
	}

	private static IEnumerator<float> WaitForAllPeers_Sequence()
	{
		MultiplayerMng.Log("WaitForAllPeers...");
		while (!PlayerUtilities.AllPeersPresent())
		{
			yield return 0f;
		}
		MultiplayerMng.Log("WaitForAllPeers...Done");
	}

	public static float WaitForPeer(int id, float timeout = 0f)
	{
		return Timing.WaitUntilDone(WaitForPeer_Sequence(id, timeout));
	}

	private static IEnumerator<float> WaitForPeer_Sequence(int id, float timeout)
	{
		MultiplayerMng.Log($"WaitForPeer {id}...");
		float start = Time.unscaledTime;
		bool isFound;
		bool hasTime;
		do
		{
			isFound = (Object)(object)PlayerUtilities.GetPeerFromId(id) != (Object)null;
			hasTime = timeout == 0f || Time.unscaledTime - start < timeout;
			yield return 0f;
		}
		while (!isFound & hasTime);
		MultiplayerMng.Log(string.Format("WaitForPeer {0}...{1}", id, isFound ? "Done" : "Timeout"));
	}

	public static float WaitForAllPlayers()
	{
		return Timing.WaitUntilDone(WaitForAllPlayers_Sequence());
	}

	private static IEnumerator<float> WaitForAllPlayers_Sequence()
	{
		MultiplayerMng.Log("WaitForAllPlayers...");
		yield return Timing.WaitForSeconds(1f);
		while (!PlayerUtilities.AllPlayersPresent())
		{
			yield return 0f;
		}
		MultiplayerMng.Log("WaitForAllPlayers...Done");
	}

	public static float WaitForPeersToUnspawn()
	{
		return Timing.WaitUntilDone(WaitForPeersToUnspawn_Sequence());
	}

	private static IEnumerator<float> WaitForPeersToUnspawn_Sequence()
	{
		MultiplayerMng.Log("WaitForPeersToUnspawn...");
		while (PlayerUtilities.AnyPeersPresent())
		{
			yield return 0f;
		}
		MultiplayerMng.Log("WaitForPeersToUnspawn...Done");
	}

	public static float WaitForPlayersToUnspawn()
	{
		return Timing.WaitUntilDone(WaitForPlayersToUnSpawn_Sequence());
	}

	private static IEnumerator<float> WaitForPlayersToUnSpawn_Sequence()
	{
		MultiplayerMng.Log("WaitForPlayersToUnspawn...");
		while (PlayerUtilities.AnyPlayersPresent())
		{
			yield return 0f;
		}
		MultiplayerMng.Log("WaitForPlayersToUnspawn...Done");
	}

	public static CoroutineHandle WaitAnd(float waitPeriod, Action action)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(WaitAnd_Sequence(waitPeriod, action));
	}

	private static IEnumerator<float> WaitAnd_Sequence(float waitPeriod, Action action)
	{
		yield return Timing.WaitForSeconds(waitPeriod);
		action?.Invoke();
	}

	public static CoroutineHandle BreakedWait(float waitPeriod, Func<bool> breakCondition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(WaitWhile_Sequence(waitPeriod, breakCondition));
	}

	private static IEnumerator<float> WaitWhile_Sequence(float waitPeriod, Func<bool> breakCondition)
	{
		float start = Time.time;
		while (Time.time < start + waitPeriod && !(breakCondition?.Invoke() ?? false))
		{
			yield return 0f;
		}
	}
}
