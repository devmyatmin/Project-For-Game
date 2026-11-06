using System;
using UnityEngine;

namespace Beam.Crafting;

public class InteractiveObject_COMPASS : InteractiveObject
{
	private float turnSpeed = 3f;

	private Vector3 trueNorth = new Vector3(-60f, 0f, -20f);

	private bool isUsing;

	[SerializeField]
	private Transform _lid;

	[SerializeField]
	private Transform compassPointer;

	[SerializeField]
	private Transform _waypoint;

	private Vector3 _waypointPosition;

	public override void Hold(bool holding)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		isUsing = false;
		if (holding && Object.op_Implicit((Object)(object)_lid))
		{
			LeanTween.cancel(((Component)this).gameObject);
			_lid.localRotation = Quaternion.identity;
			LeanTween.value(((Component)this).gameObject, (Action<float>)AnimateLid, 0f, -99f, 0.5f).setEase((LeanTweenType)4);
		}
		base.Hold(holding);
	}

	public override void Use()
	{
		isUsing = !isUsing;
		base.Use();
	}

	private void Update()
	{
		RotateCompass();
		if (Owner != null && isUsing)
		{
			Owner.PlayerCamera.ForceFirstPersonView();
		}
	}

	private void RotateCompass()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		compassPointer.rotation = Quaternion.Lerp(compassPointer.rotation, Quaternion.LookRotation(trueNorth.normalized), Time.deltaTime * turnSpeed);
		compassPointer.localRotation = Quaternion.Euler(new Vector3(0f, compassPointer.localEulerAngles.y, 0f));
	}

	private void AnimateLid(float rotation)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_lid.localRotation = Quaternion.Euler(rotation * new Vector3(1f, 0f, 0f));
	}

	private void SetWaypoint()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_waypointPosition = ((Component)Owner.Movement).transform.position;
	}

	private void DisplayWaypoint(bool display)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_waypoint == (Object)null))
		{
			if (display)
			{
				((Component)_waypoint).gameObject.SetActive(true);
				_waypoint.position = _waypointPosition;
			}
			else
			{
				((Component)_waypoint).gameObject.SetActive(false);
			}
		}
	}

	public InteractiveObject_COMPASS()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
	}
}
