using System;
using UnityEngine;

namespace Beam.UI;

[Serializable]
public class TransformViewAdapter : ITransformViewAdapter
{
	[SerializeField]
	private Transform _transform;

	public Transform Transform => _transform;

	public Vector3 Position
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				return _transform.position;
			}
			return default;
		}
		set
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				_transform.position = value;
			}
		}
	}

	public Quaternion Rotation
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				return _transform.rotation;
			}
			return default;
		}
		set
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				_transform.rotation = value;
			}
		}
	}

	public Vector3 LocalPosition
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				return _transform.localPosition;
			}
			return default;
		}
		set
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				_transform.localPosition = value;
			}
		}
	}

	public Quaternion LocalRotation
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				return _transform.localRotation;
			}
			return default;
		}
		set
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_transform != (Object)null)
			{
				_transform.localRotation = value;
			}
		}
	}

	public TransformViewAdapter(Transform t)
	{
		_transform = t;
	}

	public void SetParent(ITransformViewAdapter transform, bool worldPositionStays)
	{
		_transform.SetParent(transform.Transform, worldPositionStays);
	}
}
