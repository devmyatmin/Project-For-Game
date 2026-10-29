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

	public float CameraOffset => Mathf.Abs(_cameraRotation.transform.localPosition.z);

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
		Debug.Log("CameraMovement:: Loaded settings.");
	}

	public void Freeze(bool freeze)
	{
		base.enabled = !freeze;
	}

	public void SetPosition(Transform target)
	{
		base.transform.position = target.position;
	}

	public void SetRotation(Transform target)
	{
		Vector3 localEulerAngles = target.localEulerAngles;
		if (target == _player.Character.CameraRigTransform)
		{
			localEulerAngles.y = 0f;
		}
		_cameraRotation.rotation *= Quaternion.Euler(localEulerAngles);
	}

	public void SetCameraTarget(Transform target)
	{
		if (!(target == null))
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
		if (!(Time.timeScale > 0f) || !(Time.deltaTime > 0f))
		{
			return;
		}
		_targetSpeed = Mathf.MoveTowards(_targetSpeed, 256f, Time.smoothDeltaTime * 8f);
		Vector3 position = Vector3.Lerp(base.transform.position, _cameraTarget.position, Time.smoothDeltaTime * _targetSpeed);
		if (_shakeElapsed < _duration || _duration == -1f)
		{
			_shakeElapsed += Time.deltaTime;
			float num = _shakeElapsed / 20f;
			float num2 = 1f - Mathf.Clamp(4f * num - 3f, 0f, 1f);
			float num3 = Random.value * 2f - 1f;
			float num4 = Random.value * 2f - 1f;
			num3 *= _cameraShakeMagnitude * num2;
			num4 *= _cameraShakeMagnitude * num2;
			position += new Vector3(num3, num4, 0f);
		}
		Vector3 euler = Vector3.zero;
		float num5 = _oceanRotationIntensity;
		Transform transform = ((_player.Movement.Vehicle != null) ? _player.Movement.Vehicle.VehicleMovement.OperatingPosition : _player.Movement.ActivePlatform);
		if (transform != null)
		{
			float num6 = Vector3.Dot(transform.up, Vector3.up);
			euler = transform.rotation.eulerAngles;
			euler.y = 0f;
			if (num6 < 0f)
			{
				euler.z -= 180f;
			}
		}
		else
		{
			num5 = 0.3f;
		}
		_movingPlatformRotation = Quaternion.Lerp(_movingPlatformRotation, Quaternion.Euler(euler), Time.smoothDeltaTime * _rotationSmoothness * num5);
		base.transform.position = position;
		if (_cameraTarget == _player.Character.CharacterFirstPerson.CameraRigPosition)
		{
			_cameraRotation.rotation = _cameraTarget.rotation;
			return;
		}
		Vector3 localEulerAngles = _cameraTarget.localEulerAngles;
		if (_cameraTarget == _player.Character.CameraRigTransform)
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
}
