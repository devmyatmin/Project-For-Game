using System;
using UnityEngine;

[Serializable]
public struct AudioRollOff
{
	[SerializeField]
	private float _minDistance;

	[SerializeField]
	private float _maxDistance;

	[SerializeField]
	private AudioRollOffCurveType _type;

	public float MinDistance => _minDistance;

	public float MaxDistance => _maxDistance;

	public float Evaluate(float distance)
	{
		if (_type == AudioRollOffCurveType.Constant || distance < _minDistance)
		{
			return 1f;
		}
		if (distance > _maxDistance)
		{
			return 0f;
		}
		return _type switch
		{
			AudioRollOffCurveType.Linear => Mathf.Clamp01((Mathf.Max(distance, _minDistance) - _maxDistance) / (_minDistance - _maxDistance)), 
			AudioRollOffCurveType.Logarithmic => Mathf.Clamp01(Mathf.Pow((Mathf.Max(distance, _minDistance) - _maxDistance) / (_minDistance - _maxDistance), 2f)), 
			_ => 1f, 
		};
	}
}
