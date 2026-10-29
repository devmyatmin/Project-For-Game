using System;
using System.Collections.Generic;
using Beam.UI;
using Ceto;
using UnityEngine;

namespace Beam;

public class HelicopterVehicleMovement : VehicleMovementBase, IQueryWaves, IHaveFuel
{
	private const float IDLE_GAS_PER_HOUR = 0.2f;

	private const float RUNNING_GAS_PER_HOUR = 0.9090909f;

	private const float ANGULAR_SMOOTHING_FACTOR = 200f;

	private float _fuel;

	private WaveQuery _waveQuery;

	private bool _outOfRange;

	[SerializeField]
	private Transform[] _blades;

	[SerializeField]
	private Transform _bladesGroup;

	[SerializeField]
	private MeshRenderer[] _bladesRenderers;

	[SerializeField]
	private MeshRenderer[] _bladesTransRenderers;

	private MaterialPropertyBlock _materialPropertyBlock;

	[SerializeField]
	private Transform MainMotor;

	[SerializeField]
	private Transform SubMotor;

	private float _verticalVelocity;

	private float _climbRate = 75f;

	private float _speedForward = 60f;

	private int _altitudeInput;

	private float _pitch;

	private float _forwardSpeed;

	private float _yaw;

	private float _roll;

	private float _sideSpeed;

	private float _engineValue;

	private bool _underWater;

	private bool _engineFailure;

	[SerializeField]
	private Transform HUD_FuelNeedle;

	[SerializeField]
	private Transform HUD_Compass;

	private readonly int _Color = Shader.PropertyToID("_Color");

	private readonly int _Cutoff = Shader.PropertyToID("_Cutoff");

	public float FuelCapacity => 4f;

	public float Fuel => _fuel;

	bool IQueryWaves.Active
	{
		get
		{
			if (base.gameObject != null)
			{
				return base.gameObject.activeInHierarchy;
			}
			return false;
		}
	}

	IEnumerable<WaveQuery> IQueryWaves.WaveQueries => new WaveQuery[1] { _waveQuery };

	public event Action<bool> EngineFailed;

	void IQueryWaves.InRange()
	{
		_outOfRange = false;
	}

	void IQueryWaves.OutOfRange()
	{
		if (!_outOfRange)
		{
			_outOfRange = true;
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = NotificationPriority.Immediate;
			localizedNotification.Duration = 8f;
			localizedNotification.MessageText.SetTerm("ITEM_NTF_LOST_AT_SEA_DESC");
			localizedNotification.MessageText.SetArgs("INTERACTIVE_TYPE_BUILDING_VEHICLE_GYRO");
			localizedNotification.Raise();
		}
	}

	private void Awake()
	{
		_waveQuery = new WaveQuery(base.transform.position.x, base.transform.position.z);
		_materialPropertyBlock = new MaterialPropertyBlock();
	}

	protected override void Start()
	{
		base.Start();
		if (LevelLoader.IsLoading(base.gameObject))
		{
			LevelLoader.CurrentLoader.PhysicalLoading.Add(delegate
			{
				Singleton<BuoyancyManager>.Instance?.Add(this);
			});
		}
		else
		{
			Singleton<BuoyancyManager>.Instance?.Add(this);
		}
	}

	protected override void OnDestroy()
	{
		this.EngineFailed = null;
		Singleton<BuoyancyManager>.Instance?.Remove(this);
		base.OnDestroy();
	}

	[ContextMenu("Set to max fuel")]
	private void Debug_SetMaxFuel()
	{
		((IHaveFuel)this).Refill(100f);
	}

	float IHaveFuel.Refill(float availableFuel)
	{
		float num = Mathf.Min(FuelCapacity - _fuel, availableFuel);
		if (num > 0f)
		{
			_fuel += num;
			_fuel = Mathf.Clamp(_fuel, 0f, FuelCapacity);
		}
		return num;
	}

	protected override float GetSteeringInput()
	{
		return base.GetSteeringInput();
	}

	protected override float GetThrottleInput()
	{
		return base.GetThrottleInput();
	}

	private int GetAltitudeInput()
	{
		if (!base.Operator.IsOwner)
		{
			return base.Operator.MultiplayerState.VehicleAltitudeInput;
		}
		if (!base.Operator.Input.GetButton(58))
		{
			if (!base.Operator.Input.GetButton(57))
			{
				return 0;
			}
			return 1;
		}
		return -1;
	}

	public override float GetEngineLoad()
	{
		float num = _engineValue * 0.5f;
		if (base.Operating)
		{
			num += Mathf.Clamp01(_verticalVelocity) * 0.5f;
			if (_engineFailure)
			{
				num = _engineValue;
			}
		}
		return num;
	}

	public override bool EngineIsRunning()
	{
		if (_engineFailure)
		{
			return true;
		}
		return _engineValue > 0f;
	}

	private void Update()
	{
		if (base.Operating)
		{
			base.Operator.PlayerCamera.MouseLook.ClampViewToPosUpdate(base.OperatingPosition.forward, 90f, 0f);
		}
		_waveQuery.posX = base.transform.position.x;
		_waveQuery.posZ = base.transform.position.z;
		_underWater = base.transform.position.y < _waveQuery.result.height;
		if (base.transform.position.y < 0f)
		{
			base.Rigidbody.drag = 50f;
		}
		else
		{
			base.Rigidbody.drag = 0f;
		}
		MotorRotateControl();
		MotorVelocityContorl();
		EngineControl();
		Update_HUD();
	}

	public override bool StartOperating(IPlayer player)
	{
		if (Mathf.Approximately(_fuel, 0f))
		{
			OnOperatingFailed();
			return false;
		}
		return base.StartOperating(player);
	}

	protected override void Input()
	{
		base.Input();
		_altitudeInput = GetAltitudeInput();
		if (base.Operator.IsOwner)
		{
			base.Operator.MultiplayerState.VehicleAltitudeInput = _altitudeInput;
		}
	}

	public override void Move()
	{
		float throttle = base.Throttle;
		float steering = base.Steering;
		float steering2 = base.Steering;
		CheckEngineFailure();
		if (!_engineFailure)
		{
			if (!Mathf.Approximately(throttle, 0f))
			{
				_pitch += throttle * Time.fixedDeltaTime * 60f;
				_pitch = Mathf.Clamp(_pitch, -20f, 20f);
				_forwardSpeed = throttle * _speedForward;
			}
			else
			{
				_pitch = Mathf.Lerp(_pitch, 0f, Time.fixedDeltaTime * 3f);
				_forwardSpeed = 0f;
			}
			_yaw += steering * Time.fixedDeltaTime * 60f;
			if (!Mathf.Approximately(steering2, 0f))
			{
				_roll += (0f - steering2) * Time.fixedDeltaTime * 60f;
				_roll = Mathf.Clamp(_roll, -20f, 20f);
				_sideSpeed = steering2 * 7f;
			}
			else
			{
				float b = Mathf.PingPong(Time.time * 10f, 10f) - 5f;
				_roll = Mathf.Lerp(_roll, b, Time.fixedDeltaTime * 3f);
				_sideSpeed = 0f;
			}
		}
		base.Rigidbody.rotation = Quaternion.Lerp(base.Rigidbody.rotation, Quaternion.Euler(_pitch, _yaw, _roll), Time.fixedDeltaTime * 1.5f);
		if (!Mathf.Approximately(base.Rigidbody.angularVelocity.magnitude, 0f))
		{
			base.Rigidbody.AddTorque(-base.Rigidbody.angularVelocity.normalized * Time.fixedDeltaTime * 200f, ForceMode.Force);
		}
		base.Rigidbody.AddRelativeForce(Vector3.up * _climbRate * _verticalVelocity * _engineValue);
		base.Rigidbody.AddRelativeForce(Vector3.right * _sideSpeed * _engineValue);
		base.Rigidbody.AddForce(Vector3.up * Mathf.Abs(_sideSpeed * 0.33f) * _engineValue);
		base.Rigidbody.AddRelativeForce(Vector3.forward * _forwardSpeed * _engineValue);
		base.Rigidbody.AddForce(Vector3.up * Mathf.Abs(_forwardSpeed * 0.33f) * _engineValue);
		ClampAltitude();
		base.Rigidbody.drag = 2f;
		OnUsing();
		base.Operator.Movement.transform.position = base.OperatingPosition.position;
	}

	private void ClampAltitude()
	{
		if (_underWater)
		{
			base.Rigidbody.AddRelativeForce(Vector3.up * _climbRate * _verticalVelocity);
		}
		else if (base.Rigidbody.position.y > 75f)
		{
			base.Rigidbody.AddRelativeForce(-Vector3.up * _climbRate * _verticalVelocity);
		}
	}

	public override bool IsOverturned()
	{
		bool flag = base.transform.position.y < _waveQuery.result.height - 2f;
		if (base.IsOverturned() || flag)
		{
			return true;
		}
		return false;
	}

	public void MotorVelocityContorl()
	{
		if (!base.Operating)
		{
			return;
		}
		float num = Mathf.Abs(base.Rigidbody.mass * Physics.gravity.y) / _climbRate;
		float num2 = Mathf.PingPong(Time.time * 0.75f, 0.8f) - 0.4f;
		if ((float)_altitudeInput != 0f)
		{
			if (_altitudeInput < 0)
			{
				_verticalVelocity += (float)_altitudeInput * 0.05f;
			}
			else
			{
				_verticalVelocity += (float)_altitudeInput * 0.1f;
			}
		}
		else
		{
			_verticalVelocity = Mathf.Lerp(_verticalVelocity, num2 + num, Time.deltaTime);
		}
		if (_verticalVelocity > 1f)
		{
			_verticalVelocity = 1f;
		}
		else if (_verticalVelocity < -0.5f)
		{
			_verticalVelocity = -0.5f;
		}
	}

	public void MotorRotateControl()
	{
		if (_engineValue > 0f)
		{
			if (!_bladesGroup.gameObject.activeSelf)
			{
				_bladesGroup.gameObject.SetActive(value: true);
			}
			float num = 2000f * _engineValue * Time.deltaTime;
			MainMotor.transform.Rotate(0f, num, 0f);
			SubMotor.transform.Rotate(0f, 0f, num);
			_bladesGroup.transform.Rotate(0f, num * 0.1f, 0f);
			_materialPropertyBlock.Clear();
			_materialPropertyBlock.SetFloat(_Cutoff, _engineValue);
			MeshRenderer[] bladesRenderers = _bladesRenderers;
			for (int i = 0; i < bladesRenderers.Length; i++)
			{
				bladesRenderers[i].SetPropertyBlock(_materialPropertyBlock, 0);
			}
			_materialPropertyBlock.Clear();
			_materialPropertyBlock.SetColor(_Color, new Color(0f, 0f, 0f, _engineValue));
			bladesRenderers = _bladesTransRenderers;
			for (int i = 0; i < bladesRenderers.Length; i++)
			{
				bladesRenderers[i].SetPropertyBlock(_materialPropertyBlock);
			}
			for (int j = 0; j < _blades.Length; j++)
			{
				_blades[j].transform.localRotation = Quaternion.Euler(new Vector3(0f, Mathf.PingPong(Time.time * (35f * ((float)j * 0.3f)), 20f) - 10f, 0f));
			}
		}
		else if (_bladesGroup.gameObject.activeSelf)
		{
			_materialPropertyBlock.Clear();
			_materialPropertyBlock.SetFloat(_Cutoff, 0f);
			MeshRenderer[] bladesRenderers = _bladesRenderers;
			for (int i = 0; i < bladesRenderers.Length; i++)
			{
				bladesRenderers[i].SetPropertyBlock(_materialPropertyBlock, 0);
			}
			_materialPropertyBlock.Clear();
			_materialPropertyBlock.SetColor(_Color, new Color(0f, 0f, 0f, 0f));
			bladesRenderers = _bladesTransRenderers;
			for (int i = 0; i < bladesRenderers.Length; i++)
			{
				bladesRenderers[i].SetPropertyBlock(_materialPropertyBlock);
			}
		}
	}

	private void CheckEngineFailure()
	{
		_fuel -= ((base.Throttle > 0f) ? 0.9090909f : 0.2f) * GameTime.DeltaTimeHour;
		if (_fuel < 0f)
		{
			_fuel = 0f;
			EngineFailure();
		}
		else if (base.transform.position.y > 50f)
		{
			EngineFailure();
		}
		else if (_engineFailure)
		{
			this.EngineFailed?.Invoke(obj: false);
			_engineFailure = false;
		}
	}

	private void EngineFailure()
	{
		if (_engineFailure)
		{
			_pitch = -20f;
			_roll = -20f;
			_yaw += Time.fixedDeltaTime * 60f;
		}
		else
		{
			this.EngineFailed?.Invoke(obj: true);
			_engineFailure = true;
		}
	}

	public void EngineControl()
	{
		if (base.Operating)
		{
			if (_engineFailure)
			{
				if (_engineValue > 0f)
				{
					_engineValue -= Time.deltaTime;
				}
				else
				{
					_engineValue = 0f;
				}
			}
			else if (_engineValue < 1f)
			{
				_engineValue += Time.deltaTime * 0.25f;
			}
			else
			{
				_engineValue = 1f;
			}
		}
		else if (_engineValue > 0f)
		{
			_engineValue -= Time.deltaTime * 0.12f;
		}
		else
		{
			_engineValue = 0f;
		}
	}

	private void Update_HUD()
	{
		Vector3 vector = new Vector3(-60f, 0f, -20f);
		float num = 3f;
		HUD_Compass.rotation = Quaternion.Lerp(HUD_Compass.rotation, Quaternion.LookRotation(vector.normalized), Time.deltaTime * num);
		HUD_Compass.localRotation = Quaternion.Euler(new Vector3(0f, HUD_Compass.localEulerAngles.y, 0f));
		float t = _fuel / FuelCapacity;
		HUD_FuelNeedle.localRotation = Quaternion.Euler(new Vector3(0f, Mathf.Lerp(0f, 70f, t), 0f));
	}
}
