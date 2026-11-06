using System;
using Beam;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Level Thresholds", menuName = "Player Skills/Level Thresholds")]
public class SkillsLevelThresholdsAsset : ScriptableObject
{
	[Serializable]
	private class LevelThresholds
	{
		public EGameDifficultyMode Difficulty;

		public float[] HuntingLevelThresholds;

		public float[] CookingLevelThresholds;

		public float[] HarvestingLevelThresholds;

		public float[] PhysicalLevelThresholds;

		public float[] CraftsmanshipLevelThresholds;
	}

	[SerializeField]
	private LevelThresholds[] _thresholds;

	public float[] GetHunting()
	{
		return _thresholds[(int)PlayerUtilities.GetServerGameDifficulty()].HuntingLevelThresholds;
	}

	public float[] GetCooking()
	{
		return _thresholds[(int)PlayerUtilities.GetServerGameDifficulty()].CookingLevelThresholds;
	}

	public float[] GetHarvesting()
	{
		return _thresholds[(int)PlayerUtilities.GetServerGameDifficulty()].HarvestingLevelThresholds;
	}

	public float[] GetPhysical()
	{
		return _thresholds[(int)PlayerUtilities.GetServerGameDifficulty()].PhysicalLevelThresholds;
	}

	public float[] GetCraftsmanship()
	{
		return _thresholds[(int)PlayerUtilities.GetServerGameDifficulty()].CraftsmanshipLevelThresholds;
	}
}
