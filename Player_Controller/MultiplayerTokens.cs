using System;
using Beam;
using Beam.Language;
using Photon.Bolt;
using UdpKit;

namespace Funlabs;

public static class MultiplayerTokens
{
	public class ID : IProtocolToken
	{
		public int Id;

		public void Read(UdpPacket packet)
		{
			Id = packet.ReadByte();
		}

		public void Write(UdpPacket packet)
		{
			packet.WriteByte((byte)Id);
		}

		public override string ToString()
		{
			return $"Id {Id}";
		}
	}

	public class SaveablePrefab : IProtocolToken
	{
		public MiniGuid Id;

		public SaveablePrefab()
		{
		}

		public SaveablePrefab(MiniGuid id)
		{
			Id = id;
		}

		public void Read(UdpPacket packet)
		{
			Id = packet.ReadMiniGuid();
		}

		public void Write(UdpPacket packet)
		{
			packet.WriteMiniGuid(Id);
		}
	}

	public class MessageEnvelope : IProtocolToken
	{
		public static readonly int MAX_MESSAGES_TYPES = 256;

		private IMultiplayerMessage _message;

		public T Get<T>() where T : IMultiplayerMessage
		{
			return (T)_message;
		}

		public MessageEnvelope()
		{
		}

		public MessageEnvelope(IMultiplayerMessage message)
		{
			_message = message;
		}

		public void Read(UdpPacket packet)
		{
			try
			{
				_message = MultiplayerMessageManager.New(packet.ReadByte());
				_message.Read(packet);
			}
			catch (Exception ex)
			{
				MultiplayerMng.LogError("Unable to read message envelope " + ex.Message + "\n" + ex.StackTrace);
			}
		}

		public void Write(UdpPacket packet)
		{
			try
			{
				packet.WriteByte((byte)MultiplayerMessageManager.Id(_message.GetType()));
				_message.Write(packet);
			}
			catch (Exception ex)
			{
				MultiplayerMng.LogError("Unable to write message envelope for message type " + _message.GetType().ToStringSafe() + " " + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	public class Session : IProtocolToken
	{
		public enum SessionPlatform
		{
			PS4,
			XB1,
			PC_CONSOLE,
			PC
		}

		private static readonly string SESSION_BUSY = "SESSION_BUSY";

		private static readonly int SessionPlatform_Bits = 2;

		public string SessionId;

		public int RegionIndex;

		public string Name;

		public SessionPlatform Platform = GetPlatform();

		public ushort Version = BuildInformation.MultiplayerVersion;

		public bool IsPrivate;

		public bool IsBusy;

		public bool IsOccupied;

		public float TimePlayed;

		public int DaysSurvived;

		public int IslandsHopped;

		public int IslandsExplored;

		public int FishFished;

		public int EnemiesKilled;

		public int ItemsCrafted;

		public float QuickestScape;

		public int AnimalsTrapped;

		public int ShipsScavenged;

		public bool HasUgc;

		public static SessionPlatform GetPlatform()
		{
			return SessionPlatform.PC;
		}

		public bool IsJoinable()
		{
			if (!IsBusy)
			{
				return !IsOccupied;
			}
			return false;
		}

		public bool IsShowable()
		{
			if (!IsPrivate)
			{
				return !IsOccupied;
			}
			return false;
		}

		public string GetCosmeticName()
		{
			if (IsBusy)
			{
				return "[" + Localization.GetLanguageHandler().Localize("PLAY_ONLINE_MENU_LOBBY_ROOM_BUSY") + "]" + Name;
			}
			return Name;
		}

		void IProtocolToken.Read(UdpPacket packet)
		{
			SessionId = packet.ReadString();
			RegionIndex = packet.ReadInt();
			Name = packet.ReadString();
			Platform = (SessionPlatform)packet.ReadByte(SessionPlatform_Bits);
			Version = packet.ReadUShort();
			IsPrivate = packet.ReadBool();
			IsBusy = packet.ReadBool();
			IsOccupied = packet.ReadBool();
			TimePlayed = packet.ReadFloat();
			DaysSurvived = packet.ReadInt();
			IslandsHopped = packet.ReadInt();
			IslandsExplored = packet.ReadInt();
			FishFished = packet.ReadInt();
			EnemiesKilled = packet.ReadInt();
			ItemsCrafted = packet.ReadInt();
			QuickestScape = packet.ReadFloat();
			AnimalsTrapped = packet.ReadInt();
			ShipsScavenged = packet.ReadInt();
			HasUgc = packet.ReadBool();
		}

		void IProtocolToken.Write(UdpPacket packet)
		{
			packet.WriteString(SessionId);
			packet.WriteInt(RegionIndex);
			packet.WriteString(Name);
			packet.WriteByte((byte)Platform, SessionPlatform_Bits);
			packet.WriteUShort(Version);
			packet.WriteBool(IsPrivate);
			packet.WriteBool(IsBusy);
			packet.WriteBool(IsOccupied);
			packet.WriteFloat(TimePlayed);
			packet.WriteInt(DaysSurvived);
			packet.WriteInt(IslandsHopped);
			packet.WriteInt(IslandsExplored);
			packet.WriteInt(FishFished);
			packet.WriteInt(EnemiesKilled);
			packet.WriteInt(ItemsCrafted);
			packet.WriteFloat(QuickestScape);
			packet.WriteInt(AnimalsTrapped);
			packet.WriteInt(ShipsScavenged);
			packet.WriteBool(HasUgc);
		}
	}

	public class Join : IProtocolToken
	{
		public string Id;

		public string Name;

		public void Read(UdpPacket packet)
		{
			Id = packet.ReadString();
			Name = packet.ReadString();
		}

		public void Write(UdpPacket packet)
		{
			packet.WriteString(Id);
			packet.WriteString(Name);
		}

		public override string ToString()
		{
			return "Id " + Id;
		}
	}

	public class Accept : IProtocolToken
	{
		public GameState ServerGameState;

		public void Read(UdpPacket packet)
		{
			ServerGameState = (GameState)packet.ReadByte();
		}

		public void Write(UdpPacket packet)
		{
			packet.WriteByte((byte)ServerGameState);
		}
	}

	public static void Register()
	{
		BoltNetwork.RegisterTokenClass<ID>();
		BoltNetwork.RegisterTokenClass<SaveablePrefab>();
		BoltNetwork.RegisterTokenClass<MessageEnvelope>();
		BoltNetwork.RegisterTokenClass<Session>();
		BoltNetwork.RegisterTokenClass<Join>();
		BoltNetwork.RegisterTokenClass<Accept>();
	}

	public static string GetDesc(IProtocolToken token)
	{
		if (token != null)
		{
			return token.ToString();
		}
		return "";
	}
}
