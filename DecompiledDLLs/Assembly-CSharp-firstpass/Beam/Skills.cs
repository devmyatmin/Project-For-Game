using System;
using System.Collections.Generic;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam;

public class Skills : MonoBehaviour, ISaveable
{
	public Dictionary<SkillCategoryType, List<int>> _skillPoints;

	private Dictionary<SkillCategoryType, SkillCategory> _skills;

	public static void AddSkillPoints(SkillCategoryType type, int points)
	{
	}

	public static int GetSkillLevel(SkillCategoryType type)
	{
		return 0;
	}

	public static bool CheckSkillUnlocked(SkillType type)
	{
		return true;
	}

	public JObject Save()
	{
		throw new NotImplementedException();
	}

	public void Load(JObject data)
	{
		throw new NotImplementedException();
	}
}
