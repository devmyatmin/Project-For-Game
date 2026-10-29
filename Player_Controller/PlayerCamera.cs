using System;
using System.Runtime.CompilerServices;
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

	public IPlayerCameraState MultiplayerState => GetMultiplayerState<IPlayerCameraState>();

	public bool IsInitialized => _initialized;

	public event Action<bool> ToggleUnderwater;

	public event Action<bool> RenderingCameraChanged;

	public event Action<bool> ToggleRunning;

	public event Action<bool> OutOfBreath;

	protected virtual void OnToggleUnderwater(bool underwater)
	{
		this.ToggleUnderwater?.Invoke(underwater);
	}

	protected virtual void OnRenderingCameraChanged(bool mine)
	{
		this.RenderingCameraChanged?.Invoke(mine);
	}

	protected virtual void OnToggleRunning(bool running)
	{
		this.ToggleRunning?.Invoke(running);
	}

	protected virtual void OnOutOfBreath(bool outOfBreath)
	{
		this.OutOfBreath?.Invoke(outOfBreath);
	}

	public static PlayerCamera Create(IPlayer player)
	{
		PlayerCamera playerCamera = MultiplayerMng.Instantiate<PlayerCamera>(BoltPrefabs.PlayerCamera, new MultiplayerTokens.ID
		{
			Id = player.Id
		});
		player.SetCamera(playerCamera);
		return playerCamera;
	}

	public static PlayerCamera CreatePeerlessCamera(IPlayer player)
	{
		PlayerCamera playerCamera = Resources.Load<PlayerCamera>("Prefabs/Player/PlayerCamera");
		if (playerCamera == null)
		{
			Debug.LogError("PlayerCamera::Create:: No PlayerCamera prefab found!");
			return null;
		}
		return UnityEngine.Object.Instantiate(playerCamera);
	}

	public override void Attached()
	{
		base.Attached();
		MultiplayerState.SetTransforms(MultiplayerState.Transform, base.transform);
		Id = ((MultiplayerTokens.ID)base.entity.AttachToken).Id;
		Cameras.AddCamera(this);
		if (!base.entity.IsOwner)
		{
			_mouseLook.enabled = false;
			Camera.gameObject.SetActive(value: false);
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
		this.ToggleRunning = null;
		this.ToggleUnderwater = null;
		this.RenderingCameraChanged = null;
		Cameras.OnPreCull -= Cameras_OnPreCull;
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	private void Cameras_OnPreCull(Camera camera)
	{
		if (camera != _lastCamera)
		{
			bool mine = camera == _camera;
			OnRenderingCameraChanged(mine);
			_lastCamera = camera;
		}
	}

	public void SetCameraMode(CameraViewMode mode)
	{
		_cameraMode = ((mode != 0) ? PlayerCameraMode.Third : PlayerCameraMode.First);
		_optionsCameraView = _cameraMode;
		if (Game.State == GameState.INTRO)
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
			CameraViewMode cameraMode = ((_optionsCameraView != 0) ? CameraViewMode.THIRD_PERSON : CameraViewMode.FIRST_PERSON);
			SetCameraMode(cameraMode);
		}
		if (_forcingFirstPerson)
		{
			if (_cameraMode != 0)
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
		Vector3 cameraTargetPosition = GetCameraTargetPosition(_cameraMode);
		if (force)
		{
			_camera.transform.position = cameraTargetPosition;
			return;
		}
		if (_camSmoothTimer < 2f)
		{
			_camSmoothTimer += Time.deltaTime;
		}
		else if (_cameraMode == PlayerCameraMode.First)
		{
			_camera.transform.position = base.transform.position;
			return;
		}
		_camera.transform.position = Vector3.Lerp(_camera.transform.position, cameraTargetPosition, Time.deltaTime * _camSmooth);
	}

	public Vector3 GetCameraTargetPosition(PlayerCameraMode mode)
	{
		if (mode != PlayerCameraMode.Third)
		{
			return GetFirstPersonTargetPosition();
		}
		return GetThirdPersonTargetPosition();
	}

	private Vector3 GetFirstPersonTargetPosition()
	{
		_camSmooth = 10f;
		_currentCameraDistance = 0f;
		return base.transform.position;
	}

	private Vector3 GetThirdPersonTargetPosition()
	{
		_camSmooth = 4f;
		_currentCameraDistance = 2.23f;
		if (_player.Movement.Vehicle != null)
		{
			_currentCameraDistance = 3.345f;
		}
		Vector3 end = base.transform.position + GetThirdPersonCameraOffset() + _camera.transform.forward * (0f - _currentCameraDistance);
		if (Physics.Linecast(base.transform.position, end, out var hitInfo, _occlusionMask.value, QueryTriggerInteraction.Ignore) && !hitInfo.collider.CompareTag("NoCameraAvoidance"))
		{
			_camSmooth = 10f;
			_currentCameraDistance = Vector3.Distance(base.transform.position, hitInfo.point) - 0.5f;
		}
		return base.transform.position + GetThirdPersonCameraOffset() + _camera.transform.forward * (0f - _currentCameraDistance);
	}

	private Vector3 GetCameraOffset(PlayerCameraMode mode)
	{
		if (mode != PlayerCameraMode.Third)
		{
			return Vector3.zero;
		}
		return GetThirdPersonCameraOffset();
	}

	private Vector3 GetThirdPersonCameraOffset()
	{
		float num = 0.44f;
		if (_player.Statistics.IsKnockedOut)
		{
			num = 0f;
		}
		else if (_player.Movement.Vehicle != null)
		{
			num = 0.22f;
		}
		return _camera.transform.right * num;
	}

	private void CheckCameraHeight()
	{
		if (Cameras.IsUnderwater(base.transform))
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
		if (!_overrideRay)
		{
			return base.transform.position + GetCameraOffset(_cameraMode);
		}
		return _overrideRayPosition;
	}

	public Vector3 GetRayForward()
	{
		if (!_overrideRay)
		{
			return base.transform.forward;
		}
		return _overrideRayForward;
	}

	public void EnableOverrideRay(Vector3 overrideRayPosition, Vector3 overrideRayForward)
	{
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
		return new BeamRay(GetRayPosition(), GetRayForward(), distance);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawCube(GetCameraTargetPosition(_cameraMode), Vector3.one * 0.3f);
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(GetRay(0f).OriginPoint, 0.15f);
	}

	[SpecialName]
	GameObject IGameCamera.get_gameObject()
	{
		return base.gameObject;
	}
}
