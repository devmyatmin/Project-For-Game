using System;
using System.Collections.Generic;
using Beam;
using Beam.Events;
using Beam.Serialization.Json;
using MEC;
using Photon.Bolt;
using UdpKit;
using UnityEngine;

namespace Funlabs;

public class MultiplayerZoneSync : GlobalEventListener
{
	public enum Usage
	{
		Unknown,
		Palm,
		Fruit,
		Module,
		PaddleRaft,
		Spear,
		Holder,
		Storage,
		LootContainerSpawner,
		ZoneGeneration,
		ZoneLoading
	}

	public struct Command
	{
		public Usage Usage;

		public JObject Data;

		public Zone Zone;

		public SaveablePrefab Instance;

		public Action<Command> Load;

		public bool IsCompleted()
		{
			return (Object)(object)Instance != (Object)null;
		}
	}

	private enum DownloadState
	{
		Idle,
		Download,
		Abort,
		Completed
	}

	public static float CLIENT_LINK_TIME = 40f;

	public static bool SHOW_UI = false;

	private Dictionary<MiniGuid, Command> _commands = new Dictionary<MiniGuid, Command>();

	private int _zoneUploadIndex = -1;

	private DownloadState _zoneDownloadState;

	private float _zoneDownloadStartTime;

	private float _zoneDownloadElapsed;

	private float _zoneDownloadRatio;

	private int _zoneDownloadIndex = -1;

	public static MultiplayerZoneSync Instance { get; private set; }

	public int ZoneDowloadIndex
	{
		get
		{
			if (_zoneDownloadState != DownloadState.Download)
			{
				return -1;
			}
			return _zoneDownloadIndex;
		}
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	public bool TryGetCommand(MiniGuid referenceId, out Command command)
	{
		return _commands.TryGetValue(referenceId, out command);
	}

	public void LinkData(Usage usage, MiniGuid referenceId, Action<Command> load, JObject data = null, Zone zone = null)
	{
		_commands.TryGetValue(referenceId, out var value);
		value.Usage = usage;
		value.Data = data;
		value.Zone = zone;
		value.Load = load;
		value.Instance = MultiplayerMng.FindObject<SaveablePrefab>(referenceId, includeInactive: true);
		_commands[referenceId] = value;
		if (value.IsCompleted())
		{
			Load(value);
		}
	}

	public void LinkInstance(SaveablePrefab instance)
	{
		MiniGuid referenceId = instance.ReferenceId;
		if (_commands.TryGetValue(referenceId, out var value))
		{
			value.Instance = instance;
			_commands[referenceId] = value;
			if (value.IsCompleted())
			{
				Load(value);
			}
		}
	}

	public bool IsLinked()
	{
		return _commands.Count == 0;
	}

	public void LinkReset()
	{
		_commands.Clear();
	}

	private void Load(Command command)
	{
		command.Load(command);
		_commands.Remove(command.Instance.ReferenceId);
	}

	public void DumpNotCompleted()
	{
		foreach (KeyValuePair<MiniGuid, Command> command in _commands)
		{
			if (!command.Value.IsCompleted())
			{
				MultiplayerMng.LogError(string.Format("{0} {1} data {2} instance {3} zone {4}", new object[5]
				{
					command.Value.Usage,
					command.Key.ToString(),
					command.Value.Data != null,
					(Object)(object)command.Value.Instance != (Object)null,
					(Object)(object)command.Value.Zone != (Object)null
				}));
			}
		}
	}

	public void SendZone(byte index, JObject zone)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(SendZone_Sequence(index, zone), (Segment)0, 100);
	}

	private IEnumerator<float> SendZone_Sequence(byte index, JObject zone)
	{
		_zoneUploadIndex = index;
		byte[] buffer = null;
		BeamTiming.ThreadedAction threadedAction = new BeamTiming.ThreadedAction(() =>
		{
			buffer = zone.ToArray();
		});
		yield return threadedAction.WaitUntilDone("compress json");
		MultiplayerMng.Log($"Send zone {index} {zone.Children.Count} size: {buffer.Length}", "zone_sync", "green");
		foreach (BoltConnection connection in BoltNetwork.Connections)
		{
			connection.StreamBytes(MultiplayerMng.ZoneStreamChannel, new byte[1] { index });
			connection.StreamBytes(MultiplayerMng.ZoneStreamChannel, buffer);
		}
	}

	public override void StreamDataStarted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (((object)channel/*cast due to constrained. prefix*/).Equals((object?)MultiplayerMng.ZoneStreamChannel) && _zoneDownloadState != DownloadState.Download)
		{
			_zoneDownloadState = DownloadState.Download;
			_zoneDownloadRatio = 0f;
			_zoneDownloadElapsed = 0f;
			_zoneDownloadStartTime = Time.unscaledTime;
			_zoneDownloadIndex = -1;
		}
	}

	public override void StreamDataProgress(BoltConnection connection, UdpChannelName channel, ulong streamID, float progress)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (((object)channel/*cast due to constrained. prefix*/).Equals((object?)MultiplayerMng.ZoneStreamChannel) && _zoneDownloadIndex != -1)
		{
			_zoneDownloadRatio = progress;
			_zoneDownloadElapsed = Time.unscaledTime - _zoneDownloadStartTime;
		}
	}

	public override void StreamDataAborted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (((object)channel/*cast due to constrained. prefix*/).Equals((object?)MultiplayerMng.ZoneStreamChannel))
		{
			_zoneDownloadElapsed = Time.unscaledTime - _zoneDownloadStartTime;
			_zoneDownloadState = DownloadState.Abort;
		}
	}

	public override void StreamDataReceived(BoltConnection connection, UdpStreamData data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ref UdpChannelName channel = ref data.Channel;
		object obj = MultiplayerMng.ZoneStreamChannel;
		if (((object)channel/*cast due to constrained. prefix*/).Equals(obj))
		{
			if (_zoneDownloadIndex == -1)
			{
				_zoneDownloadIndex = data.Data[0];
			}
			else
			{
				PostZone(data.Data);
			}
		}
	}

	private void PostZone(byte[] data)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(PostZone_Sequence(data), (Segment)0, 100);
	}

	private IEnumerator<float> PostZone_Sequence(byte[] data)
	{
		_zoneDownloadElapsed = Time.unscaledTime - _zoneDownloadStartTime;
		_zoneDownloadRatio = 1f;
		_zoneDownloadState = DownloadState.Completed;
		JObject zone = null;
		BeamTiming.ThreadedAction threadedAction = new BeamTiming.ThreadedAction(() =>
		{
			zone = MPExtensions.ToJObject(data);
		});
		yield return threadedAction.WaitUntilDone("decompress json");
		MultiplayerMng.Log($"Received zone {_zoneDownloadIndex} {zone.Children.Count} size: {data.Length}", "zone_sync", "green");
		EventManager.RaiseEvent(new ZoneDownloadedEvent(_zoneDownloadIndex, zone));
	}

	private void Label(Rect rect, string text)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		GUI.Box(MPExtensions.Inflate(rect, new Vector2(4f, 0f)), "");
		GUI.Label(rect, text);
	}
}
