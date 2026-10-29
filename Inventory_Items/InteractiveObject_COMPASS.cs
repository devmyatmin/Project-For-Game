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
		isUsing = false;
		if (holding && (bool)_lid)
		{
			LeanTween.cancel(base.gameObject);
			_lid.localRotation = Quaternion.identity;
			LeanTween.value(base.gameObject, AnimateLid, 0f, -99f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
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
		if (base.Owner != null && isUsing)
		{
			base.Owner.PlayerCamera.ForceFirstPersonView();
		}
	}

	private void RotateCompass()
	{
		compassPointer.rotation = Quaternion.Lerp(compassPointer.rotation, Quaternion.LookRotation(trueNorth.normalized), Time.deltaTime * turnSpeed);
		compassPointer.localRotation = Quaternion.Euler(new Vector3(0f, compassPointer.localEulerAngles.y, 0f));
	}

	private void AnimateLid(float rotation)
	{
		_lid.localRotation = Quaternion.Euler(rotation * new Vector3(1f, 0f, 0f));
	}

	private void SetWaypoint()
	{
		_waypointPosition = base.Owner.Movement.transform.position;
	}

	private void DisplayWaypoint(bool display)
	{
		if (!(_waypoint == null))
		{
			if (display)
			{
				_waypoint.gameObject.SetActive(value: true);
				_waypoint.position = _waypointPosition;
			}
			else
			{
				_waypoint.gameObject.SetActive(value: false);
			}
		}
	}
}
