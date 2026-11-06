using System;
using System.Collections.Generic;
using Beam;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Exp Rewards", menuName = "Player Skills/Exp Rewards")]
public class SkillsExpRewardsAsset : ScriptableObject
{
	[Serializable]
	private class ExpPair
	{
		public PlayerSkills.ExpSource source;

		public float value;

		public ExpPair(PlayerSkills.ExpSource source, float value)
		{
			this.source = source;
			this.value = value;
		}
	}

	[SerializeField]
	private List<ExpPair> _expRewards;

	[SerializeField]
	private Dictionary<PlayerSkills.ExpSource, float> _rewardsDictionary;

	private void OnValidate()
	{
		if (_expRewards != null && _expRewards.Count != Enum.GetValues(typeof(PlayerSkills.ExpSource)).Length)
		{
			_expRewards = new List<ExpPair>();
			foreach (PlayerSkills.ExpSource value in Enum.GetValues(typeof(PlayerSkills.ExpSource)))
			{
				_expRewards.Add(new ExpPair(value, (_rewardsDictionary != null && _rewardsDictionary.ContainsKey(value)) ? _rewardsDictionary[value] : 1f));
			}
		}
		RecreateDictionary();
	}

	private void RecreateDictionary()
	{
		_rewardsDictionary = new Dictionary<PlayerSkills.ExpSource, float>();
		foreach (ExpPair expReward in _expRewards)
		{
			_rewardsDictionary.Add(expReward.source, expReward.value);
		}
	}

	public float GetExp(PlayerSkills.ExpSource source)
	{
		if (_rewardsDictionary == null)
		{
			RecreateDictionary();
		}
		if (!_rewardsDictionary.ContainsKey(source))
		{
			Debug.LogError((object)$"PlayerSkills:: {source} xp not set, check the Exp Rewards asset!");
			return 0f;
		}
		return _rewardsDictionary[source];
	}
}
