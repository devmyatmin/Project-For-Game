using System.Collections.Generic;
using Beam.Developer.UI;
using Beam.Serialization;
using Funlabs;
using UnityEngine;

namespace Beam;

public class DeveloperMode : MonoBehaviour, INeedPlayerComponent
{
	private class ReplicateToggleGod : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.DeveloperMode.ToggleGodMode();
		}
	}

	private class ReplicateCreate : MultiplayerMessage
	{
		[Replicate]
		public short PrefabId;

		[Replicate]
		public Vector3 Pos;

		public override void OnPeer()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			MultiplayerMng.Instantiate<SaveablePrefab>((uint)PrefabId, Pos, Quaternion.identity, MiniGuid.New());
		}
	}

	public bool startInFlyMode;

	public KeyCode toggleMode = (KeyCode)103;

	public KeyCode flyUp = (KeyCode)101;

	public KeyCode flyDown = (KeyCode)113;

	[SerializeField]
	private List<GameObject> _items;

	[SerializeField]
	private List<GameObject> _tools;

	[SerializeField]
	private List<GameObject> _consumables;

	private float curSpeed;

	private float minFlySpeed = 20f;

	private float nrmFlySpeed = 35f;

	private float maxFlySpeed = 75f;

	private bool _inFlyMode;

	private IPlayer _player;

	private CharacterController _characterController;

	public bool InFlyMode => _inFlyMode;

	public IPlayer Player => _player;

	private void Awake()
	{
		_characterController = ((Component)this).GetComponent<CharacterController>();
		_inFlyMode = startInFlyMode;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	public void Create(GameObject prefab, float offset = 0f)
	{
		uint valueOrDefault = ((prefab == null) ? ((uint?)null) : prefab.GetComponent<ISaveablePrefab>()?.PrefabId).GetValueOrDefault();
		if (valueOrDefault != 0)
		{
			Create(valueOrDefault, offset);
		}
	}

	public void Create(uint prefabId, float offset = 0f)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)Camera.main).transform.forward;
		forward.y = 0f;
		forward.Normalize();
		bool flag = Prefabs.TryGetMultiplayerPrefabName(prefabId, out var _);
		Vector3 val = ((Component)this).transform.position + forward + Vector3.up * offset;
		if (flag)
		{
			if (MPExtensions.IsClient(Game.Mode))
			{
				ReplicateCreate replicateCreate = new ReplicateCreate();
				replicateCreate.PrefabId = (short)prefabId;
				replicateCreate.Pos = val;
				replicateCreate.Post();
			}
			else
			{
				MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, val, Quaternion.identity, MiniGuid.New());
			}
		}
		else
		{
			MultiplayerMng.Instantiate<SaveablePrefab>(prefabId, val, Quaternion.identity, MiniGuid.New());
			ReplicateCreate replicateCreate2 = new ReplicateCreate();
			replicateCreate2.PrefabId = (short)prefabId;
			replicateCreate2.Pos = val;
			replicateCreate2.Post();
		}
	}

	private void FixedUpdate()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (_inFlyMode && _player.IsOwner && (!Object.op_Implicit((Object)(object)DeveloperConsolePresenter.Instance) || !DeveloperConsolePresenter.Instance.IsOpen))
		{
			float axis = _player.Input.GetAxis(0);
			float axis2 = _player.Input.GetAxis(1);
			curSpeed = nrmFlySpeed;
			if (_player.Input.GetButton(6))
			{
				curSpeed = maxFlySpeed;
			}
			if (_player.Input.GetButton(5))
			{
				curSpeed = minFlySpeed;
			}
			Vector3 val = new Vector3(axis, 0f, axis2);
			val = ((Component)_player.PlayerCamera).transform.TransformDirection(val);
			val *= curSpeed;
			_characterController.Move(val * Time.deltaTime);
		}
	}

	private void ToggleGodMode()
	{
		if (!_inFlyMode)
		{
			_player.Movement.GodMode = true;
			_player.Movement.falling = false;
			_player.Movement.grounded = false;
			_player.Movement.InWater = false;
			_player.Movement.ActivePlatform = null;
			_player.Movement.activeLadder = null;
			_player.Movement.activeLedge = null;
			_inFlyMode = true;
		}
		else if (_inFlyMode)
		{
			_player.Movement.activeLadder = null;
			_player.Movement.ActivePlatform = null;
			_player.Movement.GodMode = false;
			_inFlyMode = false;
		}
	}

	public bool Teleport(int zoneId)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (zoneId < 0 || zoneId > 48)
		{
			return false;
		}
		Zone zone = Singleton<StrandedWorld>.Instance.Zones[zoneId];
		Teleport(((Component)zone).transform.position);
		return true;
	}

	public bool Teleport(Vector3 position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!_inFlyMode)
		{
			ReplicatedToggleGodMode();
		}
		_player.transform.position = position + Vector3.up * 22.5f;
		return true;
	}

	public void ReplicatedToggleGodMode()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateToggleGod replicateToggleGod = new ReplicateToggleGod();
			replicateToggleGod.Player = _player;
			replicateToggleGod.Post();
		}
		ToggleGodMode();
	}

	public DeveloperMode()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
	}
}
