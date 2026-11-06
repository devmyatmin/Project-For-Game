using System;
using Beam;
using UnityEngine;

public class Distant_Plane_Manager : MonoBehaviour
{
	[SerializeField]
	private Transform _distantPlane;

	private DateTime _lastFlightTime;

	private int flightInterval = 100;

	private float _speed = 0.5f;

	private bool _activated;

	private void Start()
	{
		Activate(activate: false);
		((MonoBehaviour)this).InvokeRepeating("Poll_FlightPath", 10f, GameTime.OneGameHour);
	}

	private void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (_activated)
		{
			Transform transform = ((Component)_distantPlane).transform;
			transform.position += ((Component)_distantPlane).transform.forward * _speed;
			Vector3 position = ((Component)Singleton<Atmosphere>.Instance).transform.position;
			position.y = ((Component)_distantPlane).transform.position.y;
			if (Vector3.Distance(((Component)_distantPlane).transform.position, position) > 2200f)
			{
				Activate(activate: false);
			}
		}
	}

	private void Poll_FlightPath()
	{
		if (!_activated && GameTime.GetHoursSince(_lastFlightTime) > (float)flightInterval)
		{
			SetNewFlightPath();
			Activate(activate: true);
		}
	}

	private void Activate(bool activate)
	{
		_activated = activate;
		((Component)_distantPlane).gameObject.SetActive(activate);
		if (!activate)
		{
			_lastFlightTime = GameTime.Now;
			flightInterval = Random.Range(12, 48);
		}
	}

	private void SetNewFlightPath()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)Singleton<Atmosphere>.Instance).transform.position;
		((Component)_distantPlane).transform.position = RandomCircle(position, 2000f);
		Vector3 val = position - ((Component)_distantPlane).transform.position;
		val.y = 0f;
		((Component)_distantPlane).transform.rotation = Quaternion.LookRotation(val);
	}

	private Vector3 RandomCircle(Vector3 center, float radius)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.value * 360f;
		Vector3 result = default;
		result.x = center.x + radius * Mathf.Sin(num * ((float)Math.PI / 180f));
		result.z = center.z + radius * Mathf.Cos(num * ((float)Math.PI / 180f));
		result.y = 950f;
		return result;
	}
}
