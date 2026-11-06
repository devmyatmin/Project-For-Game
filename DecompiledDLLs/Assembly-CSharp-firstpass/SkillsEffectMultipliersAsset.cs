using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Effect Multipliers", menuName = "Player Skills/Effect Multipliers")]
public class SkillsEffectMultipliersAsset : ScriptableObject
{
	[SerializeField]
	private float[] _hunting;

	[SerializeField]
	private float[] _cooking;

	[SerializeField]
	private float[] _harvesting;

	[SerializeField]
	private float[] _physical;

	[SerializeField]
	private float[] _craftsmanship;

	private void OnValidate()
	{
		if (_hunting.Length != 8)
		{
			_hunting = new float[8] { 1f, 1.1f, 1.15f, 1.25f, 1.35f, 1.5f, 1.75f, 2.15f };
		}
		if (_cooking.Length != 8)
		{
			_cooking = new float[8] { 1f, 1.1f, 1.15f, 1.25f, 1.35f, 1.5f, 1.75f, 2.15f };
		}
		if (_harvesting.Length != 8)
		{
			_harvesting = new float[8] { 1f, 1.1f, 1.15f, 1.25f, 1.35f, 1.5f, 1.75f, 2.15f };
		}
		if (_physical.Length != 8)
		{
			_physical = new float[8] { 1f, 1.05f, 1.1f, 1.15f, 1.2f, 1.25f, 1.35f, 1.5f };
		}
		if (_craftsmanship.Length != 8)
		{
			_craftsmanship = new float[8] { 0.3f, 0.3f, 0.3f, 0.6f, 0.6f, 0.6f, 0.9f, 0.9f };
		}
	}

	public float GetHunting(int level)
	{
		if (level < 7)
		{
			return _hunting[level - 1];
		}
		return _hunting[6];
	}

	public float GetCooking(int level)
	{
		if (level < 7)
		{
			return _cooking[level - 1];
		}
		return _cooking[7];
	}

	public float GetHarvesting(int level)
	{
		if (level < 7)
		{
			return _harvesting[level - 1];
		}
		return _harvesting[7];
	}

	public float GetPhysical(int level)
	{
		if (level < 7)
		{
			return _physical[level - 1];
		}
		return _physical[7];
	}

	public float GetCraftsmanship(int level)
	{
		if (level < 7)
		{
			return _craftsmanship[level - 1];
		}
		return _craftsmanship[7];
	}
}
