using System;
using Beam.Crafting;
using Beam.Events;
using Funlabs;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beam;

public class PlayerCamera : MultiplayerBehaviour, IGameCamera, INeedPlayerComponent, IIdleUntilInitialized
{
	private const float CAMERA_DISTANCE_THIRD_PERSON = 2.23f;

	private const float CAMERA_OFFSET_THIRD_PERSON = 0.44f;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private CameraMovement _cameraMovement;

	[SerializeField]
	private MouseLook _mouseLook;

	[SerializeField]
	private DraggingJoint _draggingJoint;

	[FormerlySerializedAs("CamOcclusion")]
	[SerializeField]
	public LayerMask _occlusionMask;

	private float _currentCameraDistance;

	private PlayerCameraMode _cameraMode;

	private IPlayer _player;

	public MultiplayerPickupableLocalCorrection PickupableLocalCorrection;

	private float _camSmooth;

	private float _camSmoothTimer;

	private PlayerCameraMode _optionsCameraView;

	private Camera _lastCamera;

	private bool _isUnderwater;

	private bool _forcingFirstPerson;

	private bool _overrideRay;

	private Vector3 _overrideRayPosition;

	private Vector3 _overrideRayForward;

	private bool _initialized;

	public Camera Camera => _camera;

	public CameraMovement CameraMovement => _cameraMovement;

	public MouseLook MouseLook => _mouseLook;

	public DraggingJoint DraggingJoint => _draggingJoint;

	public float CurrentCameraDistance => _currentCameraDistance;

	public PlayerCameraMode CameraMode => _cameraMode;

	public IPlayer Player => _player;

	public int Id { get; set; }

	public IPlayerCameraState MultiplayerState => base.GetMultiplayerState<IPlayerCameraState>();

	public bool IsInitialized => _initialized;

	public event Action<bool> ToggleUnderwater;

	public event Action<bool> RenderingCameraChanged;

	public event Action<bool> ToggleRunning;

	public event Action<bool> OutOfBreath;

	protected virtual void OnToggleUnderwater(bool underwater)
	{
		ToggleUnderwater?.Invoke(underwater);
	}

	protected virtual void OnRenderingCameraChanged(bool mine)
	{
		RenderingCameraChanged?.Invoke(mine);
	}

	protected virtual void OnToggleRunning(bool running)
	{
		ToggleRunning?.Invoke(running);
	}

	protected virtual void OnOutOfBreath(bool outOfBreath)
	{
		OutOfBreath?.Invoke(outOfBreath);
	}

	public static PlayerCamera Create(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		PlayerCamera playerCamera = MultiplayerMng.Instantiate<PlayerCamera>(BoltPrefabs.PlayerCamera, (IProtocolToken)(object)new MultiplayerTokens.ID
		{
			Id = player.Id
		});
		player.SetCamera(playerCamera);
		return playerCamera;
	}

	public static PlayerCamera CreatePeerlessCamera(IPlayer player)
	{
		PlayerCamera playerCamera = Resources.Load<PlayerCamera>("Prefabs/Player/PlayerCamera");
		if ((Object)(object)playerCamera == (Object)null)
		{
			Debug.LogError((object)"PlayerCamera::Create:: No PlayerCamera prefab found!");
			return null;
		}
		return Object.Instantiate<PlayerCamera>(playerCamera);
	}

	public override void Attached()
	{
		base.Attached();
		((IState)MultiplayerState).SetTransforms(MultiplayerState.Transform, ((Component)this).transform);
		Id = ((MultiplayerTokens.ID)(object)((EntityBehaviour)this).entity.AttachToken).Id;
		Cameras.AddCamera(this);
		if (!((EntityBehaviour)this).entity.IsOwner)
		{
			((Behaviour)_mouseLook).enabled = false;
			((Component)Camera).gameObject.SetActive(false);
		}
	}

	public override void Detached()
	{
		base.Detached();
		Cameras.RemoveCamera(this);
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			Cameras.OnPreCull += Cameras_OnPreCull;
			LoadOptions();
			if (_player.IsOwner)
			{
				EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
			}
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_cameraMovement.SetPlayer(_player);
		_mouseLook.SetPlayer(_player);
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void LoadOptions()
	{
		CameraViewMode cameraViewMode = Options.GeneralSettings.CameraViewMode;
		SetCameraMode(cameraViewMode);
	}

	private void OnDestroy()
	{
		ToggleRunning = null;
		ToggleUnderwater = null;
		RenderingCameraChanged = null;
		Cameras.OnPreCull -= Cameras_OnPreCull;
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	private void Cameras_OnPreCull(Camera camera)
	{
		if ((Object)(object)camera != (Object)(object)_lastCamera)
		{
			bool mine = (Object)(object)camera == (Object)(object)_camera;
			OnRenderingCameraChanged(mine);
			_lastCamera = camera;
		}
	}

	public void SetCameraMode(CameraViewMode mode)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		_cameraMode = ((mode != CameraViewMode.FIRST_PERSON) ? PlayerCameraMode.Third : PlayerCameraMode.First);
		_optionsCameraView = _cameraMode;
		if ((int)Game.State == 1)
		{
			_cameraMode = PlayerCameraMode.First;
		}
		ChangeCameraMode(_cameraMode);
	}

	private void CheckCameraMode()
	{
		if (_player.Statistics.IsKnockedOut)
		{
			LoadOptions();
			if (_cameraMode == PlayerCameraMode.First)
			{
				ChangeCameraMode(PlayerCameraMode.Third);
			}
			return;
		}
		if (!_forcingFirstPerson && _cameraMode != _optionsCameraView)
		{
			CameraViewMode cameraMode = ((_optionsCameraView != PlayerCameraMode.First) ? CameraViewMode.THIRD_PERSON : CameraViewMode.FIRST_PERSON);
			SetCameraMode(cameraMode);
		}
		if (_forcingFirstPerson)
		{
			if (_cameraMode != PlayerCameraMode.First)
			{
				ChangeCameraMode(PlayerCameraMode.First);
			}
			_forcingFirstPerson = false;
		}
	}

	private void ChangeCameraMode(PlayerCameraMode mode)
	{
		_camSmoothTimer = 0f;
		_cameraMode = mode;
		_player.Character.SetCameraMode(_cameraMode);
	}

	public void ForceFirstPersonView()
	{
		_forcingFirstPerson = true;
	}

	private void Update()
	{
		if (!LevelLoader.IsLoadingScene() && _initialized)
		{
			CheckCameraMode();
			CheckCameraHeight();
			CheckRunning();
		}
	}

	private void LateUpdate()
	{
		if (!LevelLoader.IsLoadingScene() && _initialized)
		{
			UpdateCameraPosition();
		}
	}

	public void UpdateCameraPosition(bool force = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 cameraTargetPosition = GetCameraTargetPosition(_cameraMode);
		if (force)
		{
			((Component)_camera).transform.position = cameraTargetPosition;
			return;
		}
		if (_camSmoothTimer < 2f)
		{
			_camSmoothTimer += Time.deltaTime;
		}
		else if (_cameraMode == PlayerCameraMode.First)
		{
			((Component)_camera).transform.position = ((Component)this).transform.position;
			return;
		}
		((Component)_camera).transform.position = Vector3.Lerp(((Component)_camera).transform.position, cameraTargetPosition, Time.deltaTime * _camSmooth);
	}

	public Vector3 GetCameraTargetPosition(PlayerCameraMode mode)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (mode != PlayerCameraMode.Third)
		{
			return GetFirstPersonTargetPosition();
		}
		return GetThirdPersonTargetPosition();
	}

	private Vector3 GetFirstPersonTargetPosition()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_camSmooth = 10f;
		_currentCameraDistance = 0f;
		return ((Component)this).transform.position;
	}

	private Vector3 GetThirdPersonTargetPosition()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		_camSmooth = 4f;
		_currentCameraDistance = 2.23f;
		if (_player.Movement.Vehicle != null)
		{
			_currentCameraDistance = 3.345f;
		}
		Vector3 val = ((Component)this).transform.position + GetThirdPersonCameraOffset() + ((Component)_camera).transform.forward * (0f - _currentCameraDistance);
		RaycastHit val2 = default;
		if (Physics.Linecast(((Component)this).transform.position, val, ref val2, _occlusionMask.value, (QueryTriggerInteraction)1) && !((Component)val2.collider).CompareTag("NoCameraAvoidance"))
		{
			_camSmooth = 10f;
			_currentCameraDistance = Vector3.Distance(((Component)this).transform.position, val2.point) - 0.5f;
		}
		return ((Component)this).transform.position + GetThirdPersonCameraOffset() + ((Component)_camera).transform.forward * (0f - _currentCameraDistance);
	}

	private Vector3 GetCameraOffset(PlayerCameraMode mode)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (mode != PlayerCameraMode.Third)
		{
			return Vector3.zero;
		}
		return GetThirdPersonCameraOffset();
	}

	private Vector3 GetThirdPersonCameraOffset()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.44f;
		if (_player.Statistics.IsKnockedOut)
		{
			num = 0f;
		}
		else if (_player.Movement.Vehicle != null)
		{
			num = 0.22f;
		}
		return ((Component)_camera).transform.right * num;
	}

	private void CheckCameraHeight()
	{
		if (Cameras.IsUnderwater(((Component)this).transform))
		{
			if (!_isUnderwater)
			{
				_isUnderwater = true;
				OnToggleUnderwater(_isUnderwater);
			}
		}
		else if (_isUnderwater)
		{
			_isUnderwater = false;
			OnToggleUnderwater(_isUnderwater);
		}
	}

	private void CheckRunning()
	{
		if (_player.Statistics.OutOfBreath)
		{
			OnOutOfBreath(outOfBreath: true);
		}
		else if (_player.Movement.sprinting)
		{
			OnToggleRunning(running: true);
		}
		else
		{
			OnToggleRunning(running: false);
		}
	}

	public Vector3 GetRayPosition()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!_overrideRay)
		{
			return ((Component)this).transform.position + GetCameraOffset(_cameraMode);
		}
		return _overrideRayPosition;
	}

	public Vector3 GetRayForward()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!_overrideRay)
		{
			return ((Component)this).transform.forward;
		}
		return _overrideRayForward;
	}

	public void EnableOverrideRay(Vector3 overrideRayPosition, Vector3 overrideRayForward)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_overrideRay = true;
		_overrideRayPosition = overrideRayPosition;
		_overrideRayForward = overrideRayForward;
	}

	public void DisableOverrideRay()
	{
		_overrideRay = false;
	}

	public BeamRay GetRay(float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new BeamRay(GetRayPosition(), GetRayForward(), distance);
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.magenta;
		Gizmos.DrawCube(GetCameraTargetPosition(_cameraMode), Vector3.one * 0.3f);
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(GetRay(0f).OriginPoint, 0.15f);
	}
}
