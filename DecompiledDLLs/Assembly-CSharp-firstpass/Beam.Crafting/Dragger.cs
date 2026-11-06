using System.Collections;
using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Dragger : MonoBehaviour, IGameInputHandler, INeedPlayerComponent, IDragger
{
	private class ReplicateStartDrag : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public ISaveablePrefab Draggable;

		[Replicate]
		public float Distance;

		[Replicate]
		public Vector3 JointPosition;

		public override void OnPeer()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			Player.Dragger.StartDraging(Draggable, Distance, JointPosition);
		}
	}

	private class ReplicateStopDrag : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public bool ThrowObject;

		public override void OnPeer()
		{
			Dragger dragger = Player.Dragger;
			dragger._throwObject = ThrowObject;
			dragger._isDragging = false;
		}
	}

	private const float DRAGGING_OFFSET_Y = 0.2f;

	private const float WAIT_TO_ENABLE_AFTER_THROW = 1f;

	[SerializeField]
	private LayerMask _draggingLayerMask;

	private IPlayer _player;

	private IDraggable _draggable;

	private bool _localCanDrag = true;

	private IGameInputController _controller;

	private bool _isDragging;

	private bool _throwObject;

	IPlayer INeedPlayerComponent.Player => _player;

	public IDraggable Draggable
	{
		get
		{
			if (!IsPickedUp(_draggable) && !IsOwned(_draggable))
			{
				return _draggable;
			}
			return null;
		}
	}

	public bool CanDrag { get; set; } = true;

	public bool IsDragging => _isDragging;

	public void Initialize(IGameInputController controller)
	{
		_controller = controller;
		_localCanDrag = true;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	public void CheckInput()
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (_isDragging || _controller.Raycaster.CurrentObject == null || !_player.IsOwner || _player.Input.GetButton(7) || !_player.Input.GetButtonDown(8) || !CanDrag || !_localCanDrag || !(_controller.Raycaster.CurrentObject is IDraggable { CanDrag: not false, IsDragging: false } draggable))
		{
			return;
		}
		_draggable = draggable;
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer != _player && allPlayer.Dragger._draggable != null && allPlayer.Dragger._draggable == draggable)
			{
				return;
			}
		}
		ISaveablePrefab draggable2 = _draggable as ISaveablePrefab;
		RaycastHit lastHitInfo = _controller.Raycaster.LastHitInfo;
		float distance = lastHitInfo.distance;
		lastHitInfo = _controller.Raycaster.LastHitInfo;
		ReplicatedStartDrag(draggable2, distance, lastHitInfo.point);
	}

	private bool IsOwned(IDraggable draggable)
	{
		return ((draggable as InteractiveObject)?.OwnerId ?? (-1)) != -1;
	}

	private bool IsPickedUp(IDraggable draggable)
	{
		return (draggable as InteractiveObject)?.IsPickedUp ?? false;
	}

	private IEnumerator Drag(ISaveablePrefab draggable, float distance, Vector3 jointPosition)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_draggable = draggable as IDraggable;
		_controller.CurrentHandler = this;
		if (_player.IsOwner)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 0);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 14);
		}
		_isDragging = true;
		_throwObject = false;
		bool isKinematic = _draggable.rigidbody.isKinematic;
		RigidbodyInterpolation interpolation = _draggable.rigidbody.interpolation;
		CollisionDetectionMode collisionDetection = _draggable.rigidbody.collisionDetectionMode;
		SaveablePrefab saveablePrefab = _draggable as SaveablePrefab;
		if (MPExtensions.IsMultiplayer(Game.Mode) && (Object)(object)saveablePrefab != (Object)null)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(SetLocalDrag(saveablePrefab, enabled: true));
		}
		((Joint)_player.PlayerCamera.DraggingJoint.Joint).connectedBody = ((IDraggable)draggable).rigidbody;
		((Component)_player.PlayerCamera.DraggingJoint.Joint).transform.position = jointPosition;
		((Joint)_player.PlayerCamera.DraggingJoint.Joint).anchor = Vector3.zero;
		_draggable.IsDragging = true;
		_draggable.BeginDrag();
		_draggable.rigidbody.drag = 8f;
		_draggable.rigidbody.angularDrag = 8f;
		_draggable.rigidbody.isKinematic = false;
		_draggable.rigidbody.interpolation = (RigidbodyInterpolation)2;
		_draggable.rigidbody.collisionDetectionMode = (CollisionDetectionMode)2;
		float currentMass = _draggable.rigidbody.mass;
		RaycastHit val = default;
		while (!BeamTeam.IsNullOrDestroyed(_draggable) && _isDragging && !IsOwned(_draggable) && !IsPickedUp(_draggable))
		{
			if (BeamTeam.IsNullOrDestroyed(_draggable))
			{
				yield return null;
			}
			_draggable.rigidbody.mass = Mathf.Lerp(_draggable.rigidbody.mass, BeamTeam.IsNullOrDestroyed(_player.Movement.ActivePlatform) ? currentMass : 0.01f, Time.deltaTime * 3f);
			BeamRay ray = _player.PlayerCamera.GetRay(distance);
			if (Physics.Raycast(ray.BaseRay, ref val, ray.Length, LayerMask.op_Implicit(_draggingLayerMask)))
			{
				_player.PlayerCamera.DraggingJoint.Rigidbody.MovePosition(val.point + new Vector3(0f, 0.2f, 0f));
			}
			else
			{
				_player.PlayerCamera.DraggingJoint.Rigidbody.MovePosition(ray.EndPoint + new Vector3(0f, 0.2f, 0f));
			}
			if (_player.IsOwner)
			{
				if (!_player.Input.GetButton(8) || !_draggable.CanDrag || !CanDrag || !_localCanDrag)
				{
					ReplicatedStopDrag(throwObject: false);
				}
				if (_player.Input.GetButtonDown(7))
				{
					ReplicatedStopDrag(throwObject: true);
					if (MPExtensions.IsMultiplayer(Game.Mode))
					{
						_localCanDrag = false;
						((MonoBehaviour)this).StartCoroutine(WaitToEnableDrag(1f));
					}
				}
			}
			yield return null;
		}
		if (!BeamTeam.IsNullOrDestroyed(_draggable))
		{
			_draggable.rigidbody.WakeUp();
			_draggable.IsDragging = false;
			_draggable.EndDrag();
			_draggable.rigidbody.drag = _draggable.CurrentDrag;
			_draggable.rigidbody.angularDrag = _draggable.CurrentAngularDrag;
			_draggable.rigidbody.isKinematic = isKinematic;
			_draggable.rigidbody.interpolation = interpolation;
			_draggable.rigidbody.collisionDetectionMode = collisionDetection;
			_draggable.rigidbody.mass = currentMass;
			((Joint)_player.PlayerCamera.DraggingJoint.Joint).connectedBody = null;
			if (MPExtensions.IsMultiplayer(Game.Mode) && (Object)(object)saveablePrefab != (Object)null)
			{
				yield return ((MonoBehaviour)this).StartCoroutine(SetLocalDrag(saveablePrefab, enabled: false));
			}
			if (_throwObject)
			{
				_draggable.rigidbody.AddForce(((Component)_player.PlayerCamera.Camera).transform.forward * 6f, (ForceMode)2);
			}
			else
			{
				_draggable.rigidbody.AddForce(((Component)_player.PlayerCamera.Camera).transform.up * -0.1f, (ForceMode)1);
			}
		}
		_isDragging = false;
		_draggable = null;
		_controller.CurrentHandler = null;
		if (_player.IsOwner)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 0);
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 14);
		}
	}

	private IEnumerator SetLocalDrag(SaveablePrefab saveablePrefab, bool enabled)
	{
		TransformId parentId = (enabled ? TransformId.Unknown : MPExtensions.GetTransfromId(((Component)saveablePrefab).gameObject.transform.parent));
		InteractiveObject_PALM_LOG interactiveObject_PALM_LOG = saveablePrefab as InteractiveObject_PALM_LOG;
		if (!BeamTeam.IsNullOrDestroyed(interactiveObject_PALM_LOG))
		{
			HashSet<InteractiveObject_PALM_LOG> island = new HashSet<InteractiveObject_PALM_LOG>();
			GatherPalmLogsIsland(island, interactiveObject_PALM_LOG);
			if (enabled && MPExtensions.IsClient(Game.Mode))
			{
				yield return ((MonoBehaviour)this).StartCoroutine(RecreatePhysicsSetupForPalmLogsIsland(island));
			}
			{
				foreach (InteractiveObject_PALM_LOG item in island)
				{
					item.ParentId = parentId;
					item.SetKinematic(!enabled && MPExtensions.IsClient(Game.Mode));
				}
				yield break;
			}
		}
		saveablePrefab.ParentId = parentId;
		InteractiveObject interactiveObject = saveablePrefab as InteractiveObject;
		if (!BeamTeam.IsNullOrDestroyed(interactiveObject))
		{
			interactiveObject.SetKinematic(!enabled && MPExtensions.IsClient(Game.Mode));
		}
	}

	private void GatherPalmLogsIsland(HashSet<InteractiveObject_PALM_LOG> island, InteractiveObject_PALM_LOG log)
	{
		if (!BeamTeam.IsNullOrDestroyed(log) && !island.Contains(log))
		{
			island.Add(log);
			GatherPalmLogsIsland(island, log.ConnectedAboveLog);
			GatherPalmLogsIsland(island, log.ConnectedBelowLog);
		}
	}

	private IEnumerator RecreatePhysicsSetupForPalmLogsIsland(HashSet<InteractiveObject_PALM_LOG> island)
	{
		foreach (InteractiveObject_PALM_LOG item in island)
		{
			if (!BeamTeam.IsNullOrDestroyed(item.FixedJoint))
			{
				((Joint)item.FixedJoint).connectedBody = null;
				BeamTeam.Destroy<FixedJoint>(item.FixedJoint);
				item.FixedJoint = null;
			}
		}
		yield return null;
		foreach (InteractiveObject_PALM_LOG item2 in island)
		{
			item2.RemoveRigidbody(IsOwner: true);
		}
		yield return null;
		foreach (InteractiveObject_PALM_LOG item3 in island)
		{
			item3.CheckRigidbody(IsOwner: true);
		}
		yield return null;
		foreach (InteractiveObject_PALM_LOG item4 in island)
		{
			if (!BeamTeam.IsNullOrDestroyed(item4.ConnectedAboveLog))
			{
				item4.FixedJoint = ((Component)item4).gameObject.AddComponent<FixedJoint>();
				((Joint)item4.FixedJoint).connectedBody = item4.ConnectedAboveLog.rigidbody;
			}
		}
		yield return null;
	}

	public void StartDraging(ISaveablePrefab draggable, float distance, Vector3 jointPosition)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).StartCoroutine(Drag(draggable, distance, jointPosition));
	}

	public void ReplicatedStartDrag(ISaveablePrefab draggable, float distance, Vector3 jointPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(draggable))
		{
			StartDraging(draggable, distance, jointPosition);
			if (MPExtensions.IsMultiplayer(Game.Mode))
			{
				ReplicateStartDrag replicateStartDrag = new ReplicateStartDrag();
				replicateStartDrag.Player = _player;
				replicateStartDrag.Draggable = draggable;
				replicateStartDrag.Distance = distance;
				replicateStartDrag.JointPosition = jointPosition;
				replicateStartDrag.Post();
			}
		}
	}

	public void ReplicatedStopDrag(bool throwObject)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		_isDragging = false;
		_throwObject = throwObject;
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateStopDrag replicateStopDrag = new ReplicateStopDrag();
			replicateStopDrag.Player = _player;
			replicateStopDrag.ThrowObject = throwObject;
			replicateStopDrag.Post();
		}
	}

	public IEnumerator WaitToEnableDrag(float waitTime)
	{
		yield return (object)new WaitForSeconds(waitTime);
		_localCanDrag = true;
	}
}
