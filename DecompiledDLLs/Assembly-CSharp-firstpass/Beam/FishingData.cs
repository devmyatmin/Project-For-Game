using System;

namespace Beam;

[Serializable]
public class FishingData
{
	public FishSize FishType;

	public Interactive_FISH FishPrefab;

	public float Strength = 1f;

	public float DodgeSpeed = 1f;

	public float DodgeRate = 1f;

	public float RecoverTime = 1f;

	public float ResistTime = 1f;

	public int Population = 1;
}
