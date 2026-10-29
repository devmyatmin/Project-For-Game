using Beam;
using Beam.Events;
using UnityEngine;

public class MouseLook : MonoBehaviour, IAimAssist, INeedPlayerComponent, IIdleUntilInitialized
{
	private const float _clampedSmoothing = 2f;

	private const float _minimumY = -85f;

	private const float _maximumY = 85f;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private LayerMask _aimAssistLayerMask;

	private IPlayer _player;

	public bool _clampViewUpdate;

	private bool _resetUpdateClamp = true;

	private float _rotationY;

	private float _rotationX;

	private bool _invertMouseY;

	private float _smoothing = 0.5f;

	private float _sensitivityX = 5f;

	private float _sensitivityY = 5f;

	private float _originalSensitivityX;

	private float _originalSensitivityY;

	private float _aimAssistSensitivityX = 2.5f;

	private float _aimAssistSensitivityY = 2.5f;

	private float _verticalMin = -40f;

	private float _verticalMax = 40f;

	private float _horizontalMin = -40f;

	private float _horizontalMax = 40f;

	private Vector3 _angle;

	private bool _lerpAngles;

	private float _timeSinceLastMoved;

	private bool _initialized;

	public LayerMask AimAssistLayerMask => _aimAssistLayerMask;

	IPlayer INeedPlayerComponent.Player => _player;

	public bool ClampSkinning { get; set; }

	public bool ClampLedge { get; set; }

	public bool ClampLadder { get; set; }

	public bool ClampWatch { get; set; }

	public bool ClampIntro { get; set; }

	public float RotationY => _rotationY;

	public float RotationX => _rotationX;

	public Vector3 CamAngle => _angle;

	public float TimeSinceLastMoved => _timeSinceLastMoved;

	public bool IsInitialized => _initialized;

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
			LoadOptions();
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsAppliedEvent);
	}

	private void EventManager_OptionsAppliedEvent(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	public void LoadOptions()
	{
		_smoothing = Options.InputSettings.MouseSmoothness * 2f;
		_invertMouseY = Options.InputSettings.InvertY == Quality_OFF_ON.ON;
		_sensitivityX = Mathf.Max(0.01f, 6f * Options.InputSettings.MouseSensitivity);
		_sensitivityY = Mathf.Max(0.01f, 6f * Options.InputSettings.MouseSensitivity);
		_originalSensitivityX = _sensitivityX;
		_originalSensitivityY = _sensitivityY;
		Debug.Log("MouseLook:: Loaded from settings.");
	}

	public void Update()
	{
		if (!_initialized || LevelLoader.IsLoadingScene())
		{
			return;
		}
		MovementTimer();
		float num = _smoothing;
		if (_lerpAngles)
		{
			float num2 = (_invertMouseY ? _player.Input.GetAxisRelative(3, EDeltaTimeType.Smoothed) : (0f - _player.Input.GetAxisRelative(3, EDeltaTimeType.Smoothed)));
			_rotationY = Mathf.Abs(num2) * ((num2 < 0f) ? _verticalMin : _verticalMax);
			float axis = _player.Input.GetAxis(2);
			_rotationX = Mathf.Abs(axis) * ((axis < 0f) ? _horizontalMin : _horizontalMax);
			_clampViewUpdate = false;
			num = 5f;
			if (Mathf.Abs(axis) + Mathf.Abs(num2) < Mathf.Epsilon)
			{
				float num3 = Mathf.LerpAngle(_rotationX, 0f, Time.deltaTime);
				_rotationX = ((Mathf.Abs(num3 - _rotationX) < 0.01f) ? 0f : num3);
				float num4 = Mathf.LerpAngle(_rotationY, 0f, Time.deltaTime);
				_rotationY = ((Mathf.Abs(num4 - _rotationY) < 0.01f) ? 0f : num4);
			}
			_lerpAngles = false;
		}
		else
		{
			float num5 = _player.Input.GetAxisRelative(3, EDeltaTimeType.Smoothed) * _sensitivityY / _player.GameInput.SpyGlass.ZoomLevel;
			_rotationY += (_invertMouseY ? num5 : (0f - num5));
			_rotationX += _player.Input.GetAxisRelative(2, EDeltaTimeType.Smoothed) * _sensitivityX / _player.GameInput.SpyGlass.ZoomLevel;
			if (_rotationX < -360f)
			{
				_rotationX += 360f;
			}
			if (_rotationX > 360f)
			{
				_rotationX -= 360f;
			}
			if (ClampSkinning || ClampLedge || ClampLadder || ClampWatch || ClampIntro || _clampViewUpdate)
			{
				_clampViewUpdate = false;
				num = 2f;
				_rotationX = ClampAngle(_rotationX, _horizontalMin, _horizontalMax);
				_rotationY = Mathf.Clamp(_rotationY, _verticalMin, _verticalMax);
			}
			else
			{
				_resetUpdateClamp = true;
				_rotationY = Mathf.Clamp(_rotationY, -85f, 85f);
			}
		}
		Quaternion quaternion = Quaternion.Euler(0f, _rotationX, 0f);
		if (_angle != Vector3.zero)
		{
			quaternion *= Quaternion.LookRotation(_angle);
		}
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.x = _rotationY;
		eulerAngles.y = quaternion.eulerAngles.y;
		eulerAngles.z = 0f;
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(eulerAngles), 10f / num * Time.deltaTime);
	}

	public void DoAimAssist()
	{
		if (Physics.SphereCast(new Ray(_camera.transform.position, _camera.transform.forward), 2f, 30f, _aimAssistLayerMask.value))
		{
			_sensitivityX = _aimAssistSensitivityX;
			_sensitivityY = _aimAssistSensitivityY;
		}
		else
		{
			ResetAimAssist();
		}
	}

	public void ResetAimAssist()
	{
		_sensitivityX = _originalSensitivityX;
		_sensitivityY = _originalSensitivityY;
	}

	public void ClampViewToPos(Vector3 rotationAngle)
	{
		_angle = rotationAngle;
		_rotationX = ClampAngle(_angle.y, _horizontalMin, _horizontalMax);
		if (ClampSkinning)
		{
			_verticalMin = -15f;
			_verticalMax = 15f;
			_horizontalMin = -15f;
			_horizontalMax = 15f;
		}
		else if (ClampLedge)
		{
			_verticalMin = -35f;
			_verticalMax = 85f;
			_horizontalMin = -25f;
			_horizontalMax = 25f;
		}
		else if (ClampLadder)
		{
			_verticalMin = -85f;
			_verticalMax = 55f;
			_horizontalMin = -90f;
			_horizontalMax = 90f;
		}
		else if (ClampWatch)
		{
			_verticalMin = -20f;
			_verticalMax = 20f;
			_horizontalMin = -20f;
			_horizontalMax = 20f;
		}
		else if (ClampIntro)
		{
			_verticalMin = -60f;
			_verticalMax = 60f;
			_horizontalMin = -85f;
			_horizontalMax = 85f;
		}
	}

	public void ClampViewToPosUpdate(Vector3 rotationAngle, float xNegClamp, float xClamp, float yNegClamp, float yClamp, bool lerpAngles = false)
	{
		_clampViewUpdate = true;
		_lerpAngles = lerpAngles;
		if (_resetUpdateClamp)
		{
			_rotationX = ClampAngle(_angle.y, _horizontalMin, _horizontalMax);
		}
		_resetUpdateClamp = false;
		_angle = rotationAngle;
		if (xClamp == 0f)
		{
			_horizontalMin = -360f;
			_horizontalMax = 360f;
		}
		else
		{
			_horizontalMin = xNegClamp;
			_horizontalMax = xClamp;
		}
		if (yClamp == 0f)
		{
			_verticalMin = -85f;
			_verticalMax = 85f;
		}
		else
		{
			_verticalMin = yNegClamp;
			_verticalMax = yClamp;
		}
	}

	public void ClampViewToPosUpdate(Vector3 rotationAngle, float xClamp, float yClamp, bool lerpAngles = false)
	{
		ClampViewToPosUpdate(rotationAngle, 0f - xClamp, xClamp, 0f - yClamp, yClamp, lerpAngles);
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	private void MovementTimer()
	{
		if (_player.Input.GetAxis(3) + _player.Input.GetAxis(2) == 0f)
		{
			if (_timeSinceLastMoved < 30f)
			{
				_timeSinceLastMoved += Time.deltaTime;
			}
		}
		else
		{
			_timeSinceLastMoved = 0f;
		}
	}
}
