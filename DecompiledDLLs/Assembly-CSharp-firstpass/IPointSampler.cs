using UnityEngine;

public interface IPointSampler
{
	void Init(Vector3 worldPosition);

	bool TryGetSample(Vector3 worldPosition, out Sample sample);
}
