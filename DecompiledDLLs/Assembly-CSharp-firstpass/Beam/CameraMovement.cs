using Beam.Events;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public class CameraMovement : MonoBehaviour, INeedPlayerComponent, IIdleUntilInitialized
{
	public Condition IsBusy;

	[SerializeField]
	private Transform _cameraRotation;

	private float _rotationSmoothness = 75f;

	private Transform _cameraTarget;

	private float _targetSpeed;

	private float _shakeElapsed;

	private float _duration;

	private float _cameraShakeMagnitude;

	private float _oceanRotationIntensity;

	private Quaternion _movingPlatformRotation = Quaternion.identity;

	private IPlayer _player;

	private bool _initialized;

	public float CameraOffset
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return Mathf.Abs(((Component)_cameraRotation).transform.localPosition.z);
		}
	}

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsInitialized => _initialized;

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
			LoadOptions();
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		ResetCameraTarget();
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	public void LoadOptions()
	{
		_oceanRotationIntensity = Options.GeneralSettings.OceanRotationIntensity * 0.9f;
		Debug.Log((object)"CameraMovement:: Loaded settings.");
	}

	public void Freeze(bool freeze)
	{
		((Behaviour)this).enabled = !freeze;
	}

	public void SetPosition(Transform target)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = target.position;
	}

	public void SetRotation(Transform target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = target.localEulerAngles;
		if ((Object)(object)target == (Object)(object)_player.Character.CameraRigTransform)
		{
			localEulerAngles.y = 0f;
		}
		_cameraRotation.rotation *= Quaternion.Euler(localEulerAngles);
	}

	public void SetCameraTarget(Transform target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			_cameraTarget = target;
		}
	}

	public void ResetCameraTarget()
	{
		_cameraTarget = _player.Character.CameraRigTransform;
	}

	private void LateUpdate()
	{
		if (_initialized && !LevelLoader.IsLoadingScene())
		{
			DoMovement();
		}
	}

	private void DoMovement()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.timeScale > 0f) || !(Time.deltaTime > 0f))
		{
			return;
		}
		_targetSpeed = Mathf.MoveTowards(_targetSpeed, 256f, Time.smoothDeltaTime * 8f);
		Vector3 val = Vector3.Lerp(((Component)this).transform.position, _cameraTarget.position, Time.smoothDeltaTime * _targetSpeed);
		if (_shakeElapsed < _duration || _duration == -1f)
		{
			_shakeElapsed += Time.deltaTime;
			float num = _shakeElapsed / 20f;
			float num2 = 1f - Mathf.Clamp(4f * num - 3f, 0f, 1f);
			float num3 = Random.value * 2f - 1f;
			float num4 = Random.value * 2f - 1f;
			num3 *= _cameraShakeMagnitude * num2;
			num4 *= _cameraShakeMagnitude * num2;
			val += new Vector3(num3, num4, 0f);
		}
		Vector3 val2 = Vector3.zero;
		float num5 = _oceanRotationIntensity;
		Transform val3 = ((_player.Movement.Vehicle != null) ? _player.Movement.Vehicle.VehicleMovement.OperatingPosition : _player.Movement.ActivePlatform);
		if ((Object)(object)val3 != (Object)null)
		{
			float num6 = Vector3.Dot(val3.up, Vector3.up);
			Quaternion rotation = val3.rotation;
			val2 = rotation.eulerAngles;
			val2.y = 0f;
			if (num6 < 0f)
			{
				val2.z -= 180f;
			}
		}
		else
		{
			num5 = 0.3f;
		}
		_movingPlatformRotation = Quaternion.Lerp(_movingPlatformRotation, Quaternion.Euler(val2), Time.smoothDeltaTime * _rotationSmoothness * num5);
		((Component)this).transform.position = val;
		if ((Object)(object)_cameraTarget == (Object)(object)_player.Character.CharacterFirstPerson.CameraRigPosition)
		{
			_cameraRotation.rotation = _cameraTarget.rotation;
			return;
		}
		Vector3 localEulerAngles = _cameraTarget.localEulerAngles;
		if ((Object)(object)_cameraTarget == (Object)(object)_player.Character.CameraRigTransform)
		{
			localEulerAngles.y = 0f;
		}
		_cameraRotation.localRotation = Quaternion.Euler(localEulerAngles) * _movingPlatformRotation;
	}

	public void ShakeCamera(float duration, float magnitude)
	{
		_shakeElapsed = 0f;
		_duration = duration;
		_cameraShakeMagnitude = magnitude;
	}

	public CameraMovement()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
