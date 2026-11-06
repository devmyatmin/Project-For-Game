using System;
using System.Collections;
using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam.Crafting;

public class User : MonoBehaviour, IGameInputHandler, INeedPlayerComponent
{
	private class ReplicateDoPrimaryAction : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public IUseable Useable;

		[Replicate]
		public ISaveablePrefab RaycasterCurrentObject;

		public ReplicateDoPrimaryAction()
		{
		}

		public ReplicateDoPrimaryAction(IPlayer player, IUseable useable)
		{
			Player = player;
			Useable = useable;
			RaycasterCurrentObject = player.User.Controller.Raycaster.CurrentObject as ISaveablePrefab;
		}

		public override void OnPeer()
		{
			Player.User.Controller.Raycaster.SetCurrentObject(RaycasterCurrentObject as IBase);
			Player.User.DoPrimaryAction(Useable);
		}
	}

	public class ReplicateSingleUse : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public Vector3 RayPosition;

		[Replicate]
		public Vector3 RayForward;

		[Replicate]
		public IUseable Useable;

		[Replicate]
		public Vector3 UseablePosition;

		[Replicate]
		public Quaternion UseableRotation;

		[Replicate]
		public ISaveablePrefab RaycasterCurrentObject;

		public override void OnPeer()
		{
			Player.User.ReplicatedSingleUses.Enqueue(this);
		}
	}

	public class ReplicateUseStop : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.User._primaryActionUseStop = false;
		}
	}

	public enum SecondaryCommand
	{
		Pre_Idle,
		Pre,
		Use_Post,
		Reload,
		Post
	}

	public class ReplicateSecondary : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public IChargeable Chargeable;

		public SecondaryCommand Command;

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)Command);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			Command = (SecondaryCommand)packet.ReadByte();
		}

		public override void OnPeer()
		{
			User user = Player.User;
			((MonoBehaviour)user).StopAllCoroutines();
			((MonoBehaviour)user).StartCoroutine(Secondary());
		}

		private IEnumerator Secondary()
		{
			yield return Player.User.ReplicatedSecondaryCommand(Chargeable, Command, multiplayerMessageConsumer: true);
		}
	}

	[SerializeField]
	private float _zoomSpeed = 2f;

	private IPlayer _player;

	private IGameInputController _controller;

	private bool _isBusy;

	private bool _primaryActionUseStop;

	private bool _poll = true;

	private float _dechargePercent = 1f;

	private Queue<ReplicateSingleUse> _replicatedSingleUses = new Queue<ReplicateSingleUse>();

	private Coroutine _secondaryActionAnimRoutine;

	IPlayer INeedPlayerComponent.Player => _player;

	private Queue<ReplicateSingleUse> ReplicatedSingleUses => _replicatedSingleUses;

	public IGameInputController Controller => _controller;

	public event Action<IBase> UsedOnObject;

	protected virtual void OnUsedOnObject(IBase obj)
	{
		UsedOnObject?.Invoke(obj);
	}

	public void Initialize(IGameInputController controller)
	{
		_controller = controller;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_player.Holder.SelectedItemChanged += Holder_SelectedItemChanged;
	}

	private void Holder_SelectedItemChanged(IPickupable pickupable)
	{
		_isBusy = false;
	}

	public void SetEnabled(bool enabled)
	{
		_poll = !enabled;
	}

	private void OnDestroy()
	{
		UsedOnObject = null;
		if (!BeamTeam.IsNullOrDestroyed(_player) && (Object)(object)_player.Holder != (Object)null)
		{
			_player.Holder.SelectedItemChanged -= Holder_SelectedItemChanged;
		}
	}

	public void CheckInput()
	{
		if (_isBusy || !_poll || !_player.IsOwner || LevelLoader.IsServerJoinInProgress || _player.Movement.IsInCutscene || !_player.Character.CheckIfUseStateNull())
		{
			return;
		}
		if (_player.Input.GetButtonDown(7))
		{
			IUseable useable = _player.Holder.CurrentObject as IUseable;
			if (!BeamTeam.IsNullOrDestroyed(useable))
			{
				ReplicatedDoPrimaryAction(useable);
			}
		}
		else if (_player.Input.GetButtonDown(8) || _player.Input.GetButton(8))
		{
			IChargeable chargeable = _player.Holder.CurrentObject as IChargeable;
			if (ValidateSecondary(chargeable))
			{
				DoSecondaryAction(chargeable);
			}
		}
	}

	private bool ValidatePrimary(IUseable usable)
	{
		if (!BeamTeam.IsNullOrDestroyed(usable) && usable.CanUse && usable.ValidatePrimary(_controller.Raycaster.CurrentObject) && !_player.Movement.ClimbingLadder && !_player.Movement.ClimbingLedge)
		{
			return !_player.Movement.UsingContraints.HasConstraint(usable.UsingConstraints);
		}
		return false;
	}

	private bool ValidateSecondary(IChargeable chargeable)
	{
		if (!BeamTeam.IsNullOrDestroyed(chargeable) && chargeable.CanUseSecondary && chargeable.ValidateSecondary(null) && !_player.Movement.ClimbingLadder && !_player.Movement.ClimbingLedge && !_player.Movement.UsingContraints.HasConstraint(chargeable.UsingConstraints))
		{
			return !_player.Statistics.IsKnockedOut;
		}
		return false;
	}

	private void DoPrimaryAction(IUseable useable)
	{
		if (ValidatePrimary(useable))
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(PrimaryAction_Sequence(useable));
		}
	}

	public void ReplicatedDoPrimaryAction(IUseable useable)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (_player.IsOwner && MPExtensions.IsMultiplayer(Game.Mode))
		{
			new ReplicateDoPrimaryAction(_player, useable).Post();
		}
		DoPrimaryAction(useable);
	}

	private void DoSecondaryAction(IChargeable chargeable)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(SecondaryAction_Sequence(chargeable));
	}

	private IEnumerator PrimaryAction_Sequence(IUseable useable)
	{
		_controller.CurrentHandler = this;
		_isBusy = true;
		_primaryActionUseStop = false;
		while (!_primaryActionUseStop && !BeamTeam.IsNullOrDestroyed(useable))
		{
			if (_player.IsOwner)
			{
				if (!LevelLoader.IsServerJoinInProgress)
				{
					yield return ReplicatedSingleUse(useable, _controller.Raycaster.CurrentObject);
				}
				if (LevelLoader.IsServerJoinInProgress || !_player.Input.GetButton(7) || BeamTeam.IsNullOrDestroyed(_player.Holder.CurrentObject) || useable.SingleUse || !ValidatePrimary(useable))
				{
					_primaryActionUseStop = true;
				}
			}
			else if (_replicatedSingleUses.Count != 0)
			{
				ReplicateSingleUse replicateSingleUse = _replicatedSingleUses.Dequeue();
				useable = replicateSingleUse.Useable;
				if (useable != null)
				{
					useable.transform.position = replicateSingleUse.UseablePosition;
					useable.transform.rotation = replicateSingleUse.UseableRotation;
				}
				_controller.Raycaster.SetCurrentObject(replicateSingleUse.RaycasterCurrentObject as IBase);
				_player.PlayerCamera.EnableOverrideRay(replicateSingleUse.RayPosition, replicateSingleUse.RayForward);
				yield return SingleUse(useable, _controller.Raycaster.CurrentObject, multiplayerMessageConsumer: true);
				_player.PlayerCamera.DisableOverrideRay();
			}
			else
			{
				yield return null;
			}
		}
		_isBusy = false;
		_controller.CurrentHandler = null;
		_controller.Raycaster.Reset();
	}

	private Coroutine SingleUse(IUseable useable, IBase obj, bool multiplayerMessageConsumer)
	{
		IEnumerator enumerator = ((useable.CanUseOnObjects && !BeamTeam.IsNullOrDestroyed(obj)) ? UseOnObject_Sequence(useable, obj, multiplayerMessageConsumer) : Use_Sequence(useable, multiplayerMessageConsumer));
		return ((MonoBehaviour)this).StartCoroutine(enumerator);
	}

	private Coroutine ReplicatedSingleUse(IUseable useable, IBase obj)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode) && _player.IsOwner)
		{
			ReplicateSingleUse replicateSingleUse = new ReplicateSingleUse();
			replicateSingleUse.Player = _player;
			replicateSingleUse.RayPosition = _player.PlayerCamera.GetRayPosition();
			replicateSingleUse.RayForward = _player.PlayerCamera.GetRayForward();
			replicateSingleUse.Useable = useable;
			replicateSingleUse.UseablePosition = ((useable != null) ? useable.transform.position : Vector3.zero);
			replicateSingleUse.UseableRotation = ((useable != null) ? useable.transform.rotation : Quaternion.identity);
			replicateSingleUse.RaycasterCurrentObject = _controller.Raycaster.CurrentObject as ISaveablePrefab;
			replicateSingleUse.Post();
		}
		return SingleUse(useable, obj, multiplayerMessageConsumer: false);
	}

	private IEnumerator Use_Sequence(IUseable useable, bool multiplayerMessageConsumer)
	{
		if (MPExtensions.IsMultiplayer(Game.Mode) && !multiplayerMessageConsumer)
		{
			MultiplayerMng.MessageSyncFenceAllocate();
		}
		yield return _player.Character.Animater.Play(useable.Primary, () =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (MPExtensions.IsMultiplayer(Game.Mode) & multiplayerMessageConsumer)
			{
				MultiplayerMng.MessageSyncFenceConsume();
			}
			try
			{
				useable.Use();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error in Use_Sequence\n" + ex.Message + "\n" + ex.StackTrace));
			}
		});
	}

	private IEnumerator UseOnObject_Sequence(IUseable useable, IBase obj, bool multiplayerMessageConsumer)
	{
		if (MPExtensions.IsMultiplayer(Game.Mode) && !multiplayerMessageConsumer)
		{
			MultiplayerMng.MessageSyncFenceAllocate();
		}
		yield return _player.Character.Animater.Play(obj.IsDamageable ? useable.PrimaryOnObject : useable.Primary, () =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (MPExtensions.IsMultiplayer(Game.Mode) & multiplayerMessageConsumer)
			{
				MultiplayerMng.MessageSyncFenceConsume();
			}
			try
			{
				useable.UseOnObject(obj);
				OnUsedOnObject(obj);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error in UseOnObject_Sequence\n" + ex.Message + "\n" + ex.StackTrace));
			}
		});
	}

	private IEnumerator SecondaryAction_Sequence(IChargeable chargeable)
	{
		_controller.CurrentHandler = this;
		_isBusy = true;
		bool stop = false;
		bool used = false;
		bool zoom = true;
		bool charged = false;
		bool interrupted = false;
		bool reloading = true;
		float chargedPercent = 1f;
		while (_player.Input.GetButton(8) && !stop && !interrupted && !_player.Movement.ClimbingLadder && !_player.Movement.ClimbingLedge)
		{
			if (!charged)
			{
				AnimationClip clip = _player.Character.Animater.GetAnimation(chargeable.SecondaryIdle);
				if ((Object)(object)clip != (Object)null)
				{
					yield return ReplicatedSecondaryCommand(chargeable, SecondaryCommand.Pre_Idle, multiplayerMessageConsumer: false);
					float dechargePercent = _dechargePercent;
					float chargeDuration = clip.length * dechargePercent / _zoomSpeed;
					Coroutine zoomRoutine = null;
					if (zoom)
					{
						zoomRoutine = ((MonoBehaviour)this).StartCoroutine(ZoomIn_Sequence(chargeDuration, dechargePercent));
						zoom = false;
					}
					float timer = 0f;
					while (timer < chargeDuration && !interrupted)
					{
						timer += Time.deltaTime;
						if (_player.Input.GetButtonUp(8))
						{
							interrupted = true;
							chargedPercent = Mathf.Clamp01(1f - _dechargePercent + timer / (clip.length / _zoomSpeed));
						}
						yield return null;
					}
					if (interrupted)
					{
						if (_secondaryActionAnimRoutine != null)
						{
							BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, _secondaryActionAnimRoutine);
							_secondaryActionAnimRoutine = null;
						}
						BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, zoomRoutine);
					}
					charged = !interrupted;
				}
				else
				{
					yield return ReplicatedSecondaryCommand(chargeable, SecondaryCommand.Pre, multiplayerMessageConsumer: false);
				}
			}
			_player.PlayerCamera.MouseLook.DoAimAssist();
			if ((_player.Input.GetButton(7) & charged) && chargeable.Reloaded)
			{
				_player.Character.NoNull(value: true);
				yield return ReplicatedSecondaryCommand(chargeable, SecondaryCommand.Use_Post, multiplayerMessageConsumer: false);
				reloading = _player.Holder.CurrentObject == chargeable && chargeable.ValidateSecondary(null);
				if (reloading)
				{
					yield return ReplicatedSecondaryCommand(chargeable, SecondaryCommand.Reload, multiplayerMessageConsumer: false);
				}
				used = reloading;
				charged = false;
				_player.Character.NoNull(value: false);
			}
			if (!_player.Input.GetButton(8) || !reloading)
			{
				stop = true;
			}
			yield return null;
		}
		if (_player.Holder.CurrentObject != chargeable)
		{
			yield return ZoomOut(GetAnimationClipLength(chargeable.SecondaryIdle), chargedPercent);
		}
		else
		{
			yield return ReplicatedSecondaryCommand(chargeable, SecondaryCommand.Post, multiplayerMessageConsumer: false);
			yield return used ? ZoomOut(GetAnimationClipLength(chargeable.SecondaryIdle), chargedPercent) : ((MonoBehaviour)this).StartCoroutine(Decharge(chargeable.SecondaryIdle, chargedPercent, reloading));
			ReturnToIdleFromPostSecondary(chargeable);
		}
		_player.PlayerCamera.MouseLook.ResetAimAssist();
		_isBusy = false;
		_controller.CurrentHandler = null;
	}

	private void ReturnToIdleFromPostSecondary(IChargeable chargeable)
	{
		_player.Character.Animater.Play(chargeable.Idle, null, 1f, 1f, useAnimation: false, AnimationBlendMode.None);
	}

	public Coroutine ReplicatedSecondaryCommand(IChargeable chargeable, SecondaryCommand command, bool multiplayerMessageConsumer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode) && _player.IsOwner)
		{
			ReplicateSecondary replicateSecondary = new ReplicateSecondary();
			replicateSecondary.Player = _player;
			replicateSecondary.Chargeable = chargeable;
			replicateSecondary.Command = command;
			replicateSecondary.Post();
		}
		IEnumerator enumerator = null;
		switch (command)
		{
		case SecondaryCommand.Pre_Idle:
			enumerator = SecondaryCommand_Pre_Idle(chargeable);
			break;
		case SecondaryCommand.Pre:
			enumerator = SecondaryCommand_Pre(chargeable);
			break;
		case SecondaryCommand.Use_Post:
			enumerator = SecondaryCommand_Use_Post(chargeable, multiplayerMessageConsumer);
			break;
		case SecondaryCommand.Reload:
			enumerator = SecondaryCommand_Reload(chargeable, multiplayerMessageConsumer);
			break;
		case SecondaryCommand.Post:
			enumerator = SecondaryCommand_Post(chargeable);
			break;
		}
		return ((MonoBehaviour)this).StartCoroutine(enumerator);
	}

	private IEnumerator SecondaryCommand_Pre_Idle(IChargeable chargeable)
	{
		chargeable.PreSecondary();
		_secondaryActionAnimRoutine = _player.Character.Animater.Play(chargeable.SecondaryIdle, null, _zoomSpeed, 1f - _dechargePercent, useAnimation: false, AnimationBlendMode.None);
		yield return null;
	}

	private IEnumerator SecondaryCommand_Pre(IChargeable chargeable)
	{
		chargeable.PreSecondary();
		yield return null;
	}

	private IEnumerator SecondaryCommand_Use_Post(IChargeable chargeable, bool multiplayerMessageConsumer)
	{
		if (MPExtensions.IsMultiplayer(Game.Mode) && !multiplayerMessageConsumer)
		{
			MultiplayerMng.MessageSyncFenceAllocate();
		}
		yield return _player.Character.Animater.Play(chargeable.Secondary, () =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (MPExtensions.IsMultiplayer(Game.Mode) & multiplayerMessageConsumer)
			{
				MultiplayerMng.MessageSyncFenceConsume();
			}
			chargeable.UseSecondary();
		});
		chargeable.PostSecondary();
	}

	private IEnumerator SecondaryCommand_Reload(IChargeable chargeable, bool multiplayerMessageConsumer)
	{
		_player.Character.Animater.Play(chargeable.Idle, null, 1f, 1f, useAnimation: false);
		if (MPExtensions.IsMultiplayer(Game.Mode) && !multiplayerMessageConsumer)
		{
			MultiplayerMng.MessageSyncFenceAllocate();
		}
		yield return _player.Character.Animater.Play(chargeable.ReloadAnimation, () =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (MPExtensions.IsMultiplayer(Game.Mode) & multiplayerMessageConsumer)
			{
				MultiplayerMng.MessageSyncFenceConsume();
			}
			chargeable.Reload(null);
		});
	}

	private IEnumerator SecondaryCommand_Post(IChargeable chargeable)
	{
		chargeable.PostSecondary();
		yield return null;
		if (!_player.IsOwner)
		{
			ReturnToIdleFromPostSecondary(chargeable);
		}
	}

	private IEnumerator Decharge(AnimationType type, float percentage, bool canRecharge)
	{
		AnimationClip clip = _player.Character.Animater.GetAnimation(type);
		if (!((Object)(object)clip != (Object)null))
		{
			yield break;
		}
		float chargeDuration = clip.length * percentage / _zoomSpeed;
		Coroutine zoomRoutine = ((MonoBehaviour)this).StartCoroutine(ZoomOut_Sequence(chargeDuration, percentage));
		Coroutine animRoutine = _player.Character.Animater.Play(type, null, 0f - _zoomSpeed, percentage, useAnimation: false, AnimationBlendMode.None);
		bool interrupted = false;
		float timer = 0f;
		while (timer < chargeDuration && !interrupted)
		{
			timer += Time.deltaTime;
			if (_player.Input.GetButton(8) & canRecharge)
			{
				interrupted = true;
				_dechargePercent = Mathf.Clamp01(1f - percentage + timer / (clip.length / _zoomSpeed));
			}
			yield return null;
		}
		if (interrupted)
		{
			BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, zoomRoutine);
			BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, animRoutine);
		}
	}

	private Coroutine ZoomOut(float duration, float percentage)
	{
		if (duration > 0f)
		{
			return ((MonoBehaviour)this).StartCoroutine(ZoomOut_Sequence(duration * percentage / _zoomSpeed, percentage));
		}
		return null;
	}

	private IEnumerator ZoomIn_Sequence(float duration, float percentage)
	{
		float rate = 20f * percentage / duration;
		float targetFov = Cameras.DefaultFOV - 20f;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float fov = Mathf.Max(targetFov, _player.PlayerCamera.Camera.fieldOfView - rate * Time.deltaTime);
			((IGameCamera)_player.PlayerCamera).SetFieldOfView(fov);
			yield return null;
		}
	}

	private IEnumerator ZoomOut_Sequence(float duration, float percentage)
	{
		float rate = 20f * percentage / duration;
		float targetFov = Cameras.DefaultFOV;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float fov = Mathf.Min(targetFov, _player.PlayerCamera.Camera.fieldOfView + rate * Time.deltaTime);
			((IGameCamera)_player.PlayerCamera).SetFieldOfView(fov);
			yield return null;
		}
		_dechargePercent = 1f;
		((IGameCamera)_player.PlayerCamera).ResetFieldOfView();
	}

	private float GetAnimationClipLength(AnimationType type)
	{
		AnimationClip animation = _player.Character.Animater.GetAnimation(type);
		if ((Object)(object)animation != (Object)null)
		{
			return animation.length;
		}
		return 0f;
	}
}
