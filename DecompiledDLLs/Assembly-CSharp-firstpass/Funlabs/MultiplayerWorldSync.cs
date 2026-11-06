using System;
using System.Collections.Generic;
using Beam;
using Beam.Serialization.Json;
using Beam.Terrain;
using MEC;
using Photon.Bolt;
using UdpKit;
using UnityEngine;

namespace Funlabs;

public class MultiplayerWorldSync : GlobalEventListener
{
	private enum Slot
	{
		Save,
		Maps,
		None
	}

	private enum State
	{
		Idle,
		Working,
		Abort,
		Completed
	}

	private struct SlotData
	{
		public State State;

		public float StartTime;

		public float Ratio;

		public byte[] Buffer;
	}

	private class ReplicateState : MultiplayerMessage
	{
		[Replicate]
		public byte Slot;

		[Replicate]
		public float Ratio;

		[Replicate]
		public byte State;

		public override void OnPeer()
		{
			Instance._slots[Slot].State = (State)State;
			Instance._slots[Slot].Ratio = Ratio;
		}
	}

	public static bool SHOW_UI;

	private readonly float PROGRESS_UPDATE_TIME = 1f;

	private SlotData[] _slots = new SlotData[Enum.GetValues(typeof(Slot)).Length - 1];

	private Slot _receiving = Slot.None;

	public static MultiplayerWorldSync Instance { get; private set; }

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

	private CoroutineHandle Send_Async(Slot slot, byte[] buffer, Action<float> onProgress = null, Action onSuccess = null, Action onAbort = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(Send_Sequence(slot, buffer, onProgress, onSuccess, onAbort));
	}

	private IEnumerator<float> Send_Sequence(Slot slot, byte[] buffer, Action<float> onProgress = null, Action onSuccess = null, Action onAbort = null)
	{
		_slots[(int)slot].State = State.Working;
		_slots[(int)slot].StartTime = Time.unscaledTime;
		_slots[(int)slot].Ratio = 0f;
		byte[] array = new byte[1] { (byte)slot };
		foreach (BoltConnection connection in BoltNetwork.Connections)
		{
			connection.StreamBytes(MultiplayerMng.WorldStreamChannel, array);
			connection.StreamBytes(MultiplayerMng.WorldStreamChannel, buffer);
		}
		float lastProgressTime = 0f;
		while (_slots[(int)slot].State != State.Completed && _slots[(int)slot].State != State.Abort)
		{
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime - lastProgressTime > PROGRESS_UPDATE_TIME)
			{
				lastProgressTime = unscaledTime;
				_ = ref _slots[(int)slot];
				onProgress?.Invoke(_slots[(int)slot].Ratio);
			}
			yield return 0f;
		}
		if (_slots[(int)slot].State == State.Abort)
		{
			onAbort?.Invoke();
			SwitchToIdle(slot);
		}
		else
		{
			onProgress?.Invoke(1f);
			onSuccess?.Invoke();
			SwitchToIdle(slot);
		}
	}

	private CoroutineHandle Receive_Async(Slot slot, Action<float> onProgress = null, Action<byte[]> onSuccess = null, Action onAbort = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(Receive_Sequence(slot, onProgress, onSuccess, onAbort));
	}

	private IEnumerator<float> Receive_Sequence(Slot slot, Action<float> onProgress = null, Action<byte[]> onSuccess = null, Action onAbort = null)
	{
		float lastProgressTime = 0f;
		int progressCount = 0;
		while (_slots[(int)slot].State != State.Completed && _slots[(int)slot].State != State.Abort)
		{
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime - lastProgressTime > PROGRESS_UPDATE_TIME)
			{
				lastProgressTime = unscaledTime;
				_ = ref _slots[(int)slot];
				onProgress?.Invoke(_slots[(int)slot].Ratio);
				progressCount++;
				if (progressCount % 2 == 0)
				{
					SendState(slot);
				}
			}
			yield return 0f;
		}
		SendState(slot);
		if (_slots[(int)slot].State == State.Abort)
		{
			onAbort?.Invoke();
			SwitchToIdle(slot);
		}
		else
		{
			onProgress?.Invoke(1f);
			onSuccess?.Invoke(_slots[(int)slot].Buffer);
			SwitchToIdle(slot);
		}
	}

	private void SwitchToIdle(Slot slot)
	{
		_slots[(int)slot].State = State.Idle;
		_slots[(int)slot].Ratio = 0f;
		_slots[(int)slot].StartTime = 0f;
		_slots[(int)slot].Buffer = null;
	}

	public override void StreamDataStarted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (((object)channel/*cast due to constrained. prefix*/).Equals((object?)MultiplayerMng.WorldStreamChannel) && _receiving != Slot.None)
		{
			_slots[(int)_receiving].State = State.Working;
			_slots[(int)_receiving].Ratio = 0f;
			_slots[(int)_receiving].Buffer = null;
			_slots[(int)_receiving].StartTime = Time.unscaledTime;
		}
	}

	public override void StreamDataProgress(BoltConnection connection, UdpChannelName channel, ulong streamID, float progress)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (((object)channel/*cast due to constrained. prefix*/).Equals((object?)MultiplayerMng.WorldStreamChannel) && _receiving != Slot.None)
		{
			_slots[(int)_receiving].Ratio = progress;
		}
	}

	public override void StreamDataAborted(BoltConnection connection, UdpChannelName channel, ulong streamID)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (((object)channel/*cast due to constrained. prefix*/).Equals((object?)MultiplayerMng.WorldStreamChannel) && _receiving != Slot.None)
		{
			_slots[(int)_receiving].State = State.Abort;
			_slots[(int)_receiving].Ratio = 0f;
			_slots[(int)_receiving].Buffer = null;
			_slots[(int)_receiving].StartTime = 0f;
			_receiving = Slot.None;
		}
	}

	public override void StreamDataReceived(BoltConnection connection, UdpStreamData data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ref UdpChannelName channel = ref data.Channel;
		object obj = MultiplayerMng.WorldStreamChannel;
		if (((object)channel/*cast due to constrained. prefix*/).Equals(obj))
		{
			if (_receiving == Slot.None)
			{
				_receiving = (Slot)data.Data[0];
				return;
			}
			_slots[(int)_receiving].Ratio = 1f;
			_slots[(int)_receiving].Buffer = data.Data;
			_slots[(int)_receiving].State = State.Completed;
			_receiving = Slot.None;
		}
	}

	public void Reset()
	{
		foreach (object value in Enum.GetValues(typeof(Slot)))
		{
			if ((Slot)value != Slot.None)
			{
				SwitchToIdle((Slot)value);
			}
		}
	}

	public float SendWorld_Async(World.Bundle world, Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action onSuccess, Action onAbort)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(SendWorld_Sequence(world, onCoroutine, onProgress, onSuccess, onAbort)));
	}

	private IEnumerator<float> SendWorld_Sequence(World.Bundle world, Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action onSuccess, Action onAbort)
	{
		byte[] buffer = null;
		onProgress?.Invoke(0f);
		BeamTiming.ThreadedAction threadedAction = new BeamTiming.ThreadedAction(() =>
		{
			buffer = world.ToArray();
		});
		yield return threadedAction.WaitUntilDone("compress maps");
		MultiplayerMng.Log($"Send maps size: {buffer.Length}", "world_sync", "green");
		CoroutineHandle val = Send_Async(Slot.Maps, buffer, onProgress, onSuccess, onAbort);
		onCoroutine?.Invoke(val);
		yield return Timing.WaitUntilDone(val);
	}

	public float ReceiveWorld_Async(Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action<World.Bundle> onSuccess, Action onAbort)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(ReceiveWorld_Sequence(onCoroutine, onProgress, onSuccess, onAbort)));
	}

	private IEnumerator<float> ReceiveWorld_Sequence(Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action<World.Bundle> onSuccess, Action onAbort)
	{
		byte[] buffer = null;
		CoroutineHandle val = Receive_Async(Slot.Maps, onProgress, (byte[] data) =>
		{
			buffer = data;
		}, onAbort);
		onCoroutine?.Invoke(val);
		yield return Timing.WaitUntilDone(val);
		if (buffer != null)
		{
			MultiplayerMng.Log($"Received maps size: {buffer.Length}", "world_sync", "green");
			World.Bundle world = World.Bundle.New();
			BeamTiming.ThreadedAction threadedAction = new BeamTiming.ThreadedAction(() =>
			{
				world = MPExtensions.ToWorld(buffer, createTextures: false);
			});
			yield return threadedAction.WaitUntilDone("decompress maps");
			Map[] maps = world.Maps;
			for (int num = 0; num < maps.Length; num++)
			{
				maps[num].CreateTextures();
			}
			onSuccess?.Invoke(world);
		}
	}

	public float SendSave_Async(JObject save, Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action<JObject> onSuccess, Action onAbort)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(SendSave_Sequence(save, onCoroutine, onProgress, onSuccess, onAbort)));
	}

	private IEnumerator<float> SendSave_Sequence(JObject save, Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action<JObject> onSuccess, Action onAbort)
	{
		byte[] buffer = null;
		onProgress?.Invoke(0f);
		BeamTiming.ThreadedAction threadedAction = new BeamTiming.ThreadedAction(() =>
		{
			buffer = save.ToArray();
		});
		yield return threadedAction.WaitUntilDone("compress save");
		onProgress?.Invoke(0.1f);
		MultiplayerMng.Log($"Send save size: {buffer.Length}", "world_sync", "green");
		CoroutineHandle val = Send_Async(Slot.Save, buffer, onProgress, () =>
		{
			onSuccess?.Invoke(save);
		}, onAbort);
		onCoroutine?.Invoke(val);
		yield return Timing.WaitUntilDone(val);
	}

	public float ReceiveSave_Async(Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action<JObject> onSuccess, Action onAbort)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Timing.WaitUntilDone(Timing.RunCoroutine(ReceiveSave_Sequence(onCoroutine, onProgress, onSuccess, onAbort)));
	}

	private IEnumerator<float> ReceiveSave_Sequence(Action<CoroutineHandle> onCoroutine, Action<float> onProgress, Action<JObject> onSuccess, Action onAbort)
	{
		byte[] buffer = null;
		CoroutineHandle val = Receive_Async(Slot.Save, onProgress, (byte[] data) =>
		{
			buffer = data;
		}, onAbort);
		onCoroutine?.Invoke(val);
		yield return Timing.WaitUntilDone(val);
		if (buffer != null)
		{
			JObject save = null;
			MultiplayerMng.Log($"Received save size: {buffer.Length}", "world_sync", "green");
			BeamTiming.ThreadedAction threadedAction = new BeamTiming.ThreadedAction(() =>
			{
				save = MPExtensions.ToJObject(buffer);
			});
			yield return threadedAction.WaitUntilDone("decompress save");
			onProgress?.Invoke(1f);
			onSuccess?.Invoke(save);
		}
	}

	private void ApplyReadOnlyPatch(JObject save, JObject patch, string[] paths)
	{
		for (int i = 0; i < paths.Length; i++)
		{
			string[] array = paths[i].Split('/');
			JObject jObject = patch;
			JObject jObject2 = save;
			for (int j = 0; j < array.Length - 1; j++)
			{
				string name = array[j];
				jObject = jObject.GetField(name);
				jObject2 = jObject2.GetField(name);
				if (jObject == null || jObject2 == null)
				{
					break;
				}
			}
			if (jObject != null && jObject2 != null)
			{
				string text = array[^1];
				JObject field = jObject.GetField(text);
				JObject field2 = jObject2.GetField(text);
				if (field != null && field2 != null)
				{
					jObject2.SetField(text, field);
				}
			}
		}
	}

	private void SendState(Slot slot)
	{
		ReplicateState replicateState = new ReplicateState();
		replicateState.Slot = (byte)slot;
		replicateState.State = (byte)_slots[(int)slot].State;
		replicateState.Ratio = _slots[(int)slot].Ratio;
		replicateState.Post();
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
