using System.Collections.Generic;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Beam;

public class CharacterRenderer : MonoBehaviour, INeedPlayerComponent
{
	public enum RAGDOLL_TYPE
	{
		Male_Ragdoll,
		Female_Ragdoll
	}

	public bool THIRD_PERSON_CHARACTER;

	public Transform CameraPosition;

	public Transform CameraRigPosition;

	public Transform WatchPosition;

	public Transform HoldingPosition;

	public Transform ProxyBodyPosition;

	[SerializeField]
	private Animator _animatorController;

	[SerializeField]
	private AnimationEventPlayer _animationEventPlayer;

	[SerializeField]
	private DynamicBone _tiePhysics;

	[SerializeField]
	private LimbIK Left_Arm;

	private float _leftHandIKWeight;

	[SerializeField]
	private AimIK _aimIK;

	private Transform _aimTarget;

	private Vector3 _aimTargetSmooth;

	private float armCameraFollowSmoothness = 15f;

	public Transform watchTargetParent;

	[SerializeField]
	private Transform _aimTargetTransform;

	[FormerlySerializedAs("_characterRenderers")]
	[SerializeField]
	private List<Renderer> _renderers;

	[FormerlySerializedAs("_characterHeadRenderers")]
	[SerializeField]
	private Renderer[] _headRenderers;

	[SerializeField]
	private ArmEffectBase[] _armEffects;

	[SerializeField]
	private RAGDOLL_TYPE _playerRagdollType;

	[SerializeField]
	private RendererPlayerStatusEffectView _sunburnEffect;

	[SerializeField]
	private GameObject _hatEasy;

	[SerializeField]
	private GameObject _hatHard;

	private IPlayer _player;

	public Animator AnimController => _animatorController;

	public AnimationEventPlayer AnimationEventPlayer => _animationEventPlayer;

	public RAGDOLL_TYPE PlayerRagdollType => _playerRagdollType;

	public RendererPlayerStatusEffectView SunburnEffect => _sunburnEffect;

	IPlayer INeedPlayerComponent.Player => _player;

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Game.State == 0)
		{
			_animatorController.SetTrigger("Menu_Pose");
			_animatorController.updateMode = (AnimatorUpdateMode)2;
			Object.Destroy((Object)(object)_aimIK);
			Object.Destroy((Object)(object)Left_Arm);
			Object.Destroy((Object)(object)_tiePhysics);
			Object.Destroy((Object)(object)this);
			return;
		}
		if ((Object)(object)_tiePhysics != (Object)null)
		{
			if (THIRD_PERSON_CHARACTER)
			{
				((Behaviour)_tiePhysics).enabled = true;
			}
			else
			{
				((Behaviour)_tiePhysics).enabled = false;
			}
		}
		SetDifficultyHatRenderers();
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_animationEventPlayer.SetPlayer(player);
		for (int i = 0; i < _armEffects.Length; i++)
		{
			_armEffects[i].SetPlayer(player);
		}
		_aimTarget = _player.Character.AimTarget;
		SetupWatchIK();
	}

	private void SetDifficultyHatRenderers()
	{
		bool active = PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Creative;
		bool active2 = PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard;
		if ((Object)(object)_hatEasy != (Object)null)
		{
			_hatEasy.SetActive(active);
		}
		if ((Object)(object)_hatHard != (Object)null)
		{
			_hatHard.SetActive(active2);
		}
	}

	public void SetSpyglassRenderers(bool shadowsOnly = false)
	{
		for (int i = 0; i < _renderers.Count; i++)
		{
			_renderers[i].shadowCastingMode = (ShadowCastingMode)((!shadowsOnly) ? 1 : 3);
			_renderers[i].receiveShadows = true;
		}
	}

	public void SetHeadRenderers(bool enabled, bool shadowsOnly = false)
	{
		for (int i = 0; i < _headRenderers.Length; i++)
		{
			_headRenderers[i].enabled = enabled;
			_headRenderers[i].shadowCastingMode = (ShadowCastingMode)((!shadowsOnly) ? 1 : 3);
			_headRenderers[i].receiveShadows = enabled;
		}
	}

	public void SetArmEffectActive(bool enabled)
	{
		for (int i = 0; i < _armEffects.Length; i++)
		{
			_armEffects[i].SetActive(enabled);
		}
	}

	public void SetRenderers(bool enabled)
	{
		for (int i = 0; i < _renderers.Count; i++)
		{
			_renderers[i].enabled = enabled;
		}
	}

	private void SetupWatchIK()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = new GameObject("Watch_IK_Target").transform;
		((Component)transform).transform.parent = watchTargetParent;
		transform.localPosition = new Vector3(-0.2382f, -0.0417f, 0.118f);
		transform.localRotation = Quaternion.Euler(new Vector3(52.206f, -181.599f, -6.622f));
		((IKSolverTrigonometric)Left_Arm.solver).target = transform;
	}

	private void Update()
	{
		if (BeamTeam.IsNullOrDestroyed(_player))
		{
			return;
		}
		SetupAnimatorMasks();
		if (_player.IsOwner)
		{
			_animatorController.SetBool("HoldingItem", _player.Holder.CheckCurrent());
			_animatorController.SetFloat("HoldingItemFloat", (float)((_animatorController.GetBool("HoldingItem") && !THIRD_PERSON_CHARACTER) ? 1 : 0));
		}
		if (_animatorController.GetFloat("watch") > 0.1f)
		{
			if (!THIRD_PERSON_CHARACTER && _leftHandIKWeight <= 1f)
			{
				_leftHandIKWeight += 3.5f * Time.deltaTime;
			}
		}
		else if (_leftHandIKWeight > 0f)
		{
			_leftHandIKWeight -= 3.5f * Time.deltaTime;
		}
		((IKSolver)Left_Arm.solver).IKPositionWeight = _leftHandIKWeight;
		((IKSolverTrigonometric)Left_Arm.solver).IKRotationWeight = _leftHandIKWeight;
		if (_player.Character.PlayingEventAnimation || _player.Movement.ClimbingLedge || _player.Movement.ClimbingLadder || _player.Movement.Vehicle != null || _player.GameInput.FireLighter.IsLightingFire)
		{
			((Point)((IKSolverHeuristic)_aimIK.solver).bones[0]).weight = 0f;
			((Point)((IKSolverHeuristic)_aimIK.solver).bones[1]).weight = 0f;
		}
		else if (THIRD_PERSON_CHARACTER)
		{
			((Point)((IKSolverHeuristic)_aimIK.solver).bones[0]).weight = 0.1f;
			((Point)((IKSolverHeuristic)_aimIK.solver).bones[1]).weight = 0.1f;
		}
		else
		{
			((Point)((IKSolverHeuristic)_aimIK.solver).bones[0]).weight = 1f;
			((Point)((IKSolverHeuristic)_aimIK.solver).bones[1]).weight = 1f;
		}
	}

	private void LateUpdate()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		if (!BeamTeam.IsNullOrDestroyed(_player))
		{
			if (_player.Movement.InWater)
			{
				_aimTargetTransform.localPosition = new Vector3(_aimTargetTransform.localPosition.x, 4.35f, _aimTargetTransform.localPosition.z);
			}
			else
			{
				_aimTargetTransform.localPosition = new Vector3(_aimTargetTransform.localPosition.x, 0f, _aimTargetTransform.localPosition.z);
			}
			_aimTargetSmooth = Vector3.Lerp(_aimTargetSmooth, ((Component)_player.PlayerCamera).transform.position + ((Component)_player.PlayerCamera).transform.up * 3.8f + ((Component)_player.PlayerCamera).transform.forward * 20f, Time.deltaTime * armCameraFollowSmoothness);
			((Component)_aimTarget).transform.position = _aimTargetSmooth;
			_aimIK.solver.transform.LookAt(_aimTarget);
			_aimIK.solver.axis = _aimIK.solver.transform.InverseTransformDirection(((Component)this).transform.forward);
			((IKSolver)_aimIK.solver).IKPosition = _aimTarget.position;
		}
	}

	private void SetupAnimatorMasks()
	{
		if (THIRD_PERSON_CHARACTER)
		{
			_animatorController.SetLayerWeight(1, 0f);
			_animatorController.SetLayerWeight(2, 0f);
			_animatorController.SetLayerWeight(3, 0f);
			_animatorController.SetLayerWeight(4, 1f);
			_animatorController.SetLayerWeight(5, 1f);
			_animatorController.SetLayerWeight(6, 1f);
			_animatorController.SetLayerWeight(7, 1f);
		}
		else
		{
			_animatorController.SetLayerWeight(4, 0f);
			_animatorController.SetLayerWeight(5, 0f);
			_animatorController.SetLayerWeight(6, 0f);
			_animatorController.SetLayerWeight(7, 0f);
			_animatorController.SetLayerWeight(1, 1f);
			_animatorController.SetLayerWeight(2, 1f);
			_animatorController.SetLayerWeight(3, 1f);
		}
		if (_player.Character.PlayingEventAnimation || _player.Movement.ClimbingLadder || _player.Movement.ClimbingLedge)
		{
			_animatorController.SetLayerWeight(1, 0f);
			_animatorController.SetLayerWeight(2, 0f);
			_animatorController.SetLayerWeight(3, 0f);
			_animatorController.SetLayerWeight(4, 0f);
			_animatorController.SetLayerWeight(5, 0f);
			_animatorController.SetLayerWeight(6, 0f);
			_animatorController.SetLayerWeight(7, 0f);
		}
	}

	public void InstantiatePlayerRagdoll(Vector3 position)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerMng.Instantiate<InteractiveObject_PLAYER_RAGDOLL>((_playerRagdollType == RAGDOLL_TYPE.Male_Ragdoll) ? BoltPrefabs.Player_Ragdoll_Male : BoltPrefabs.Player_Ragdoll_Female, (IProtocolToken)(object)new MultiplayerTokens.SaveablePrefab
		{
			Id = MiniGuid.PLAYER_RAGDOLL(_player)
		}).transform.position = position;
	}
}
