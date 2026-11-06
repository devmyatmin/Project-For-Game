using UnityEngine;

public class RaycastPointSampler : IPointSampler
{
	private readonly Vector3 _direction;

	private readonly float _length;

	private readonly int _mask;

	public RaycastPointSampler(Vector3 direction, float length, int mask)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_direction = direction;
		_length = length;
		_mask = mask;
	}

	public void Init(Vector3 worldPosition)
	{
	}

	public bool TryGetSample(Vector3 worldPosition, out Sample sample)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		sample = Sample.Empty;
		RaycastHit hit = default;
		if (Physics.Raycast(worldPosition, _direction, ref hit, _length, _mask))
		{
			sample = new Sample(hit);
			return true;
		}
		return false;
	}
}
