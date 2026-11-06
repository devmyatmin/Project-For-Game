using System;
using System.Collections.Generic;
using Beam;
using MEC;
using UnityEngine;

namespace Funlabs;

public class MultiplayerFence
{
	private class ReplicateDone : MultiplayerMessage
	{
		[Replicate]
		public byte Fence;

		public override void OnPeer()
		{
			Get((Fence)Fence).Done = true;
		}
	}

	public Fence Fence;

	public bool Done;

	private static MultiplayerFence[] _fences = new MultiplayerFence[Enum.GetValues(typeof(Fence)).Length];

	private static MultiplayerFence Get(Fence fence)
	{
		return _fences[(int)fence];
	}

	public static CoroutineHandle Sync(Fence fence)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Get(fence).Sync();
	}

	public static void Register()
	{
		int length = Enum.GetValues(typeof(Fence)).Length;
		for (int i = 0; i < length; i++)
		{
			_fences[i] = new MultiplayerFence((Fence)i);
		}
	}

	public MultiplayerFence(Fence fence)
	{
		Fence = fence;
		Done = false;
	}

	public CoroutineHandle Sync()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(Sync_Sequence());
	}

	private IEnumerator<float> Sync_Sequence()
	{
		MultiplayerMng.Log($"Waiting for fence {Fence}...");
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			MultiplayerMng.Log($"Waiting for fence {Fence}...Done");
			yield break;
		}
		if (MPExtensions.IsServer(Game.Mode))
		{
			if (!PlayerRegistry.IsClientPeerValid)
			{
				MultiplayerMng.Log($"Waiting for fence {Fence}...Done");
				yield break;
			}
			ReplicateDone replicateDone = new ReplicateDone();
			replicateDone.Fence = (byte)Fence;
			replicateDone.Post();
			float startTime = Time.unscaledTime;
			while (!Done && !(Time.unscaledTime > startTime + PlayerRegistry.LocalPeer.GlobalTimeout))
			{
				yield return 0f;
				if (!PlayerRegistry.IsClientPeerValid)
				{
					Done = true;
				}
			}
		}
		else
		{
			ReplicateDone replicateDone2 = new ReplicateDone();
			replicateDone2.Fence = (byte)Fence;
			replicateDone2.Post();
			float startTime = Time.unscaledTime;
			while (!Done && !(Time.unscaledTime > startTime + PlayerRegistry.LocalPeer.GlobalTimeout))
			{
				yield return 0f;
			}
		}
		MultiplayerMng.Log(string.Format("Waiting for fence {0}...{1}", Fence, Done ? "Done" : "Timeout"));
		if (!Done)
		{
			if (MPExtensions.IsServer(Game.Mode))
			{
				if (PlayerRegistry.IsClientPeerValid)
				{
					PlayerUtilities.GetPeerFromId(1)?.ReplicatedDisconnect(DisconnectDetail.Timeout_Fence);
				}
			}
			else
			{
				PlayerRegistry.LocalPeer.ReplicatedDisconnect(DisconnectDetail.Timeout_Fence);
			}
		}
		Done = false;
	}
}
