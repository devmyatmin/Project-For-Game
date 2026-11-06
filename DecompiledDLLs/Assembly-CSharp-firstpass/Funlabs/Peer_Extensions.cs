using System;
using Beam;
using Beam.Utilities;
using Photon.Bolt;
using UdpKit;
using UnityEngine;

namespace Funlabs;

public static class Peer_Extensions
{
	private class ReplicateDisconnect : MultiplayerMessage
	{
		[Replicate]
		public byte PeerId;

		public DisconnectDetail DisconnectDetail;

		public override void OnPeer()
		{
			PlayerUtilities.GetPeerFromId(PeerId).Disconnect(DisconnectDetail);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			DisconnectDetail = (DisconnectDetail)packet.ReadByte();
		}

		public override string ToString()
		{
			return base.ToString() + $" {DisconnectDetail}";
		}

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)DisconnectDetail);
		}
	}

	private static void Disconnect(this Peer peer, DisconnectDetail disconnectDetail)
	{
		if (BeamTeam.IsNullOrDestroyed(peer))
		{
			return;
		}
		int id = peer.Id;
		BoltEntity entity = ((EntityBehaviour)peer).entity;
		MultiplayerMng.LogError($"Disconnect peer {id} {disconnectDetail}");
		MultiplayerMng.DisconnectPayload.Set(disconnectDetail);
		PlayerRegistry.Unlink(peer);
		if (id == 1)
		{
			try
			{
				if (BoltNetwork.IsServer)
				{
					entity.Source.Disconnect();
				}
				if (BoltNetwork.IsClient)
				{
					BoltConnection server = BoltNetwork.Server;
					if (server != null)
					{
						server.Disconnect();
					}
				}
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		MultiplayerMng.Shutdown();
	}

	public static void ReplicatedDisconnect(this Peer peer, DisconnectDetail disconnectDetail, Action localAction = null, bool forceLocalShutdown = false)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerMng.LogError($"ReplicatedDisconnect {peer?.Id ?? (-1)} {disconnectDetail} forceLocalShutdown {forceLocalShutdown}");
		if (forceLocalShutdown)
		{
			MultiplayerMng.DisconnectPayload.Set(disconnectDetail);
			MultiplayerMng.Shutdown();
			localAction?.Invoke();
			return;
		}
		if (BeamTeam.IsNullOrDestroyed(peer))
		{
			localAction?.Invoke();
			return;
		}
		ReplicateDisconnect replicateDisconnect = new ReplicateDisconnect();
		replicateDisconnect.PeerId = (byte)peer.Id;
		replicateDisconnect.DisconnectDetail = disconnectDetail;
		replicateDisconnect.Post();
		BeamTiming.WaitAnd(0.33f, () =>
		{
			try
			{
				peer.Disconnect(disconnectDetail);
			}
			finally
			{
				localAction?.Invoke();
			}
		});
		((Behaviour)peer).enabled = false;
	}
}
