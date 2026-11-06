using UnityEngine;

namespace Beam;

public class Door : MonoBehaviour
{
	public enum DoorHingeAxis
	{
		X,
		Y,
		Z
	}

	public enum Direction
	{
		In,
		Out
	}

	[SerializeField]
	private DoorHingeAxis _doorHingeAxis = DoorHingeAxis.Y;

	[SerializeField]
	private Direction _direction = Direction.Out;

	[SerializeField]
	private float _rangeOfMotion = 120f;

	[SerializeField]
	private float _openingDuration = 0.4f;

	[SerializeField]
	private float _closingDuration = 1f;

	[SerializeField]
	private LeanTweenType _openingEaseType = (LeanTweenType)7;

	[SerializeField]
	private LeanTweenType _closingEaseType = (LeanTweenType)24;

	private Vector3 _axis;

	public float RangeOfMotion => _rangeOfMotion;

	public float OpeningDuration => _openingDuration;

	public float ClosingDuration => _closingDuration;

	public LeanTweenType OpeningEaseType
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _openingEaseType;
		}
	}

	public LeanTweenType ClosingEaseType
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _closingEaseType;
		}
	}

	public Vector3 Axis
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _axis;
		}
	}

	private void Awake()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		int num = ((_direction != Direction.In) ? 1 : (-1));
		switch (_doorHingeAxis)
		{
		case DoorHingeAxis.X:
			_axis = new Vector3((float)num, 0f, 0f);
			break;
		case DoorHingeAxis.Y:
			_axis = new Vector3(0f, (float)num, 0f);
			break;
		case DoorHingeAxis.Z:
			_axis = new Vector3(0f, 0f, (float)num);
			break;
		}
	}

	public void RotateDoor(float rotation)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localRotation = Quaternion.Euler(rotation * _axis);
	}

	public Door()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
	}
}
