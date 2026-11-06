using System;
using System.Collections.Generic;
using Beam;
using Beam.AccountServices;
using MEC;
using Photon.Bolt;
using UdpKit;
using UnityEngine;

namespace Funlabs;

public class Peer : MultiplayerBehaviour, IMultiplayerState<IPeerState>
{
	public enum Params
	{
		Gender,
		Gender2,
		GameDifficulty,
		World,
		Permadeath,
		Wildlife,
		GameStartingCrate,
		WorldSeed,
		Continuing
	}

	public enum StringParams
	{
		OnlineName
	}

	private class StayAliveMessage : MultiplayerMessage
	{
		[Replicate]
		public byte PeerId;

		[Replicate]
		public int AliveId;

		protected override bool IsLogEnabled()
		{
			return false;
		}

		public override void OnPeer()
		{
			PlayerUtilities.GetPeerFromId(PeerId)?.StayAlive(AliveId);
		}
	}

	private class ReplicateSetTimeout : MultiplayerMessage
	{
		[Replicate]
		public byte PeerId;

		[Replicate]
		public float TimeOut;

		public override void OnPeer()
		{
			PlayerUtilities.GetPeerFromId(PeerId)?.SetTimeout_Core(TimeOut);
		}
	}

	private class ReplicateParam : I_MultiplayerMessage
	{
		[I_Replicate]
		public Peer Peer;

		public Params Param;

		[Replicate]
		public int Value;

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)Param);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			Param = (Params)packet.ReadByte();
		}

		public override void OnPeer()
		{
			Peer.Set(Param, Value, informObserver: true, replicate: false, bidirectionalReplication: false);
		}

		public override string ToString()
		{
			return base.ToString() + $" Param: {Param}";
		}
	}

	private class ReplicateStringParam : I_MultiplayerMessage
	{
		[I_Replicate]
		public Peer Peer;

		public StringParams Param;

		[Replicate]
		public string Value;

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)Param);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			Param = (StringParams)packet.ReadByte();
		}

		public override void OnPeer()
		{
			Peer.Set(Param, Value, replicate: false, bidirectionalReplication: false);
		}

		public override string ToString()
		{
			return base.ToString() + $" Param: {Param}";
		}
	}

	private class ReplicateAllParams : I_MultiplayerMessage
	{
		[I_Replicate]
		public Peer Peer;

		public override void OnPeer()
		{
			foreach (object value in Enum.GetValues(typeof(Params)))
			{
				ReplicateParam replicateParam = new ReplicateParam();
				replicateParam.Peer = Peer;
				replicateParam.Param = (Params)value;
				replicateParam.Value = Peer.Get((Params)value);
				replicateParam.Post();
			}
			foreach (object value2 in Enum.GetValues(typeof(StringParams)))
			{
				ReplicateStringParam replicateStringParam = new ReplicateStringParam();
				replicateStringParam.Peer = Peer;
				replicateStringParam.Param = (StringParams)value2;
				replicateStringParam.Value = Peer.Get((StringParams)value2);
				replicateStringParam.Post();
			}
		}
	}

	public static readonly float ALIVE_INTERVAL = 2f;

	public static readonly float DEFAULT_TIMEOUT = 8f;

	private int _id;

	private EGameGenderMode _gender;

	private EGameGenderMode _gender2;

	private EGameDifficultyMode _gameDifficulty;

	private EGameWorldMode _world;

	private EGamePermadeathMode _permadeath;

	private EGameWildlifeMode _wildlife;

	private EGameStartingCrate _gameStartingCrate;

	private int _worldSeed;

	private int _continuing;

	private string _onlineName;

	private Action<Params, int> OnParamChange;

	private float _timeout = DEFAULT_TIMEOUT;

	public bool _disconnect;

	private float _lastSentTime;

	private float _aliveTime;

	private int _aliveId;

	public int Id => _id;

	public IPlayer Player { get; set; }

	public EGameGenderMode Gender
	{
		get
		{
			return _gender;
		}
		set
		{
			Set(Params.Gender, (int)value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public EGameGenderMode Gender2
	{
		get
		{
			return _gender2;
		}
		set
		{
			Set(Params.Gender2, (int)value, informObserver: true, replicate: true, bidirectionalReplication: true);
		}
	}

	public EGameDifficultyMode GameDifficulty
	{
		get
		{
			return _gameDifficulty;
		}
		set
		{
			Set(Params.GameDifficulty, (int)value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public EGameWorldMode World
	{
		get
		{
			return _world;
		}
		set
		{
			Set(Params.World, (int)value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public EGamePermadeathMode Permadeath
	{
		get
		{
			return _permadeath;
		}
		set
		{
			Set(Params.Permadeath, (int)value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public EGameWildlifeMode Wildlife
	{
		get
		{
			return _wildlife;
		}
		set
		{
			Set(Params.Wildlife, (int)value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public EGameStartingCrate StartingCrate
	{
		get
		{
			return _gameStartingCrate;
		}
		set
		{
			Set(Params.GameStartingCrate, (int)value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public int WorldSeed
	{
		get
		{
			return _worldSeed;
		}
		set
		{
			Set(Params.WorldSeed, value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public int Continuing
	{
		get
		{
			return _continuing;
		}
		set
		{
			Set(Params.Continuing, value, informObserver: true, replicate: true, bidirectionalReplication: false);
		}
	}

	public string OnlineName
	{
		get
		{
			return _onlineName;
		}
		set
		{
			Set(StringParams.OnlineName, value, replicate: true, bidirectionalReplication: false);
		}
	}

	public IPeerState MultiplayerState => base.GetMultiplayerState<IPeerState>();

	public float Timeout => _timeout;

	public float GlobalTimeout
	{
		get
		{
			if (PlayerRegistry.AllPeers.Count < 2)
			{
				return _timeout;
			}
			return Mathf.Max(PlayerRegistry.AllPeers[0]._timeout, PlayerRegistry.AllPeers[1]._timeout);
		}
	}

	public bool IsTimeout
	{
		get
		{
			if (_id != 0)
			{
				if (!(Time.unscaledTime - _aliveTime > GlobalTimeout))
				{
					return _disconnect;
				}
				return true;
			}
			return false;
		}
	}

	public void AddListner(Action<Params, int> listener)
	{
		if (listener != null)
		{
			listener(Params.Gender, (int)_gender);
			listener(Params.Gender2, (int)_gender2);
			listener(Params.GameDifficulty, (int)_gameDifficulty);
			listener(Params.World, (int)_world);
			listener(Params.Permadeath, (int)_permadeath);
			listener(Params.Wildlife, (int)_wildlife);
			listener(Params.GameStartingCrate, (int)_gameStartingCrate);
			listener(Params.WorldSeed, _worldSeed);
			listener(Params.Continuing, _continuing);
			OnParamChange = (Action<Params, int>)Delegate.Combine(OnParamChange, listener);
		}
	}

	public void RemoveListener(Action<Params, int> listener)
	{
		OnParamChange = (Action<Params, int>)Delegate.Remove(OnParamChange, listener);
	}

	private int Get(Params param)
	{
		return param switch
		{
			Params.Gender => (int)_gender, 
			Params.Gender2 => (int)_gender2, 
			Params.GameDifficulty => (int)_gameDifficulty, 
			Params.World => (int)_world, 
			Params.Permadeath => (int)_permadeath, 
			Params.Wildlife => (int)_wildlife, 
			Params.GameStartingCrate => (int)_gameStartingCrate, 
			Params.WorldSeed => _worldSeed, 
			Params.Continuing => _continuing, 
			_ => 0, 
		};
	}

	private void Set(Params param, int value, bool informObserver, bool replicate, bool bidirectionalReplication)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (value != Get(param))
		{
			switch (param)
			{
			case Params.Gender:
				_gender = (EGameGenderMode)value;
				break;
			case Params.Gender2:
				_gender2 = (EGameGenderMode)value;
				break;
			case Params.GameDifficulty:
				_gameDifficulty = (EGameDifficultyMode)value;
				break;
			case Params.World:
				_world = (EGameWorldMode)value;
				break;
			case Params.Permadeath:
				_permadeath = (EGamePermadeathMode)value;
				break;
			case Params.Wildlife:
				_wildlife = (EGameWildlifeMode)value;
				break;
			case Params.GameStartingCrate:
				_gameStartingCrate = (EGameStartingCrate)value;
				break;
			case Params.WorldSeed:
				_worldSeed = value;
				break;
			case Params.Continuing:
				_continuing = value;
				break;
			}
			if (informObserver)
			{
				OnParamChange?.Invoke(param, value);
			}
			if ((MPExtensions.IsMultiplayer(Game.Mode) & replicate) && (bidirectionalReplication || ((EntityBehaviour)this).entity.IsOwner))
			{
				ReplicateParam replicateParam = new ReplicateParam();
				replicateParam.Peer = this;
				replicateParam.Param = param;
				replicateParam.Value = value;
				replicateParam.Post();
			}
		}
	}

	private string Get(StringParams param)
	{
		if (param == StringParams.OnlineName)
		{
			return _onlineName;
		}
		return "";
	}

	private void Set(StringParams param, string value, bool replicate, bool bidirectionalReplication)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!string.Equals(value, Get(param)))
		{
			if (param == StringParams.OnlineName)
			{
				_onlineName = value;
			}
			if ((MPExtensions.IsMultiplayer(Game.Mode) & replicate) && (bidirectionalReplication || ((EntityBehaviour)this).entity.IsOwner))
			{
				ReplicateStringParam replicateStringParam = new ReplicateStringParam();
				replicateStringParam.Peer = this;
				replicateStringParam.Param = param;
				replicateStringParam.Value = value;
				replicateStringParam.Post();
			}
		}
	}

	public EGameGenderMode GetGenderById(int id)
	{
		return id switch
		{
			0 => Gender, 
			1 => Gender2, 
			_ => (EGameGenderMode)(id % 2), 
		};
	}

	public override void Attached()
	{
		base.Attached();
		_id = MultiplayerMng.GetEntityId(((EntityBehaviour)this).entity);
		((Object)((Component)this).gameObject).name = $"Peer {_id}";
		PlayerRegistry.Link(this);
		StayAlive(_aliveId);
		if (((EntityBehaviour)this).entity.IsOwner)
		{
			_gender = Options.CustomSettings.Gender;
			_gender2 = Options.CustomSettings.Gender2;
			_gameDifficulty = Options.CustomSettings.Difficulty;
			_permadeath = Options.CustomSettings.Permadeath;
			_wildlife = Options.CustomSettings.Wildlife;
			_gameStartingCrate = Options.CustomSettings.StartingCrate;
			_worldSeed = StrandedWorld.LOCAL_WORLD_SEED;
			_onlineName = GAccountService.Get().LocalUser.UserInfo.DisplayName;
		}
		else
		{
			ReplicateAllParams replicateAllParams = new ReplicateAllParams();
			replicateAllParams.Peer = this;
			replicateAllParams.Post();
		}
	}

	public override void Detached()
	{
		base.Detached();
		PlayerRegistry.Unlink(this);
	}

	private void OnDestroy()
	{
		PlayerRegistry.Unlink(this);
	}

	private void Update()
	{
		UpdateStayAlive();
	}

	private void UpdateStayAlive()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode) && _id != 0)
		{
			if (((EntityBehaviour)this).entity.IsOwner && Time.unscaledTime - _lastSentTime > ALIVE_INTERVAL)
			{
				_lastSentTime = Time.unscaledTime;
				StayAliveMessage stayAliveMessage = new StayAliveMessage();
				stayAliveMessage.PeerId = (byte)Id;
				stayAliveMessage.AliveId = ++_aliveId;
				stayAliveMessage.Post();
			}
			if (IsTimeout)
			{
				this.ReplicatedDisconnect(DisconnectDetail.Timeout_StayAlive);
			}
		}
	}

	private void StayAlive(int aliveId)
	{
		_aliveTime = Time.unscaledTime;
		if (aliveId > _aliveId)
		{
			_aliveId = aliveId;
			StayAliveMessage stayAliveMessage = new StayAliveMessage();
			stayAliveMessage.PeerId = (byte)Id;
			stayAliveMessage.AliveId = aliveId;
			stayAliveMessage.Post();
		}
	}

	public static float GetDefaultTimeout()
	{
		return DEFAULT_TIMEOUT;
	}

	public static float GetSessionTimeout()
	{
		return (float)BoltRuntimeSettings.instance.GetConfigCopy().connectionTimeout / 1000f;
	}

	public float SetTimeout_Async(float timeout)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(SetTimout_Sequence(timeout)));
	}

	private IEnumerator<float> SetTimout_Sequence(float timeout)
	{
		_ = _timeout;
		_ = timeout;
		ReplicatedSetTimeout(timeout);
		yield return Timing.WaitForSeconds(1f);
	}

	public void SetTimeout(float timeout)
	{
		if (_timeout != timeout)
		{
			ReplicatedSetTimeout(timeout);
		}
	}

	private void SetTimeout_Core(float timeout)
	{
		_timeout = timeout;
		StayAlive(_aliveId);
	}

	private void ReplicatedSetTimeout(float timeout)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateSetTimeout replicateSetTimeout = new ReplicateSetTimeout();
			replicateSetTimeout.PeerId = (byte)Id;
			replicateSetTimeout.TimeOut = timeout;
			replicateSetTimeout.Post();
		}
		SetTimeout_Core(timeout);
	}
}
