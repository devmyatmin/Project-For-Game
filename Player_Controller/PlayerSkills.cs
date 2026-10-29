using System;
using Beam.Serialization.Json;
using Beam.UI;
using Funlabs;
using UdpKit;
using UnityEngine;

namespace Beam;

[RequireComponent(typeof(Movement))]
public class PlayerSkills : MonoBehaviour, INeedPlayerComponent, ISaveable
{
	public enum Skills
	{
		HUNTING,
		COOKING,
		HARVESTING,
		PHYSICAL,
		CRAFTSMANSHIP
	}

	public enum ExpSource
	{
		ANIMAL_HIT,
		SMALL_KILL,
		SHARK_KILL,
		MONSTER_KILL,
		CAMPFIRE_COOKING,
		FOOD_COOKED_SMALL,
		FOOD_COOKED_MEDIUM,
		FOOD_COOKED_LARGE,
		SWIMMING,
		HAMMER_HIT,
		DECONSTRUCTION_HIT,
		CAMPFIRE_FAIL,
		CAMPFIRE_LIGHT
	}

	private class ReplicateSkill : MultiplayerMessage
	{
		public Skills Type;

		[Replicate]
		public IPlayer Player;

		[Replicate]
		public float Exp;

		[Replicate]
		public int Level;

		public override void Write(UdpPacket packet)
		{
			base.Write(packet);
			packet.WriteByte((byte)Type);
		}

		public override void Read(UdpPacket packet)
		{
			base.Read(packet);
			Type = (Skills)packet.ReadByte();
		}

		public override void OnPeer()
		{
			switch (Type)
			{
			case Skills.HUNTING:
				Player.PlayerSkills._huntingExp = Exp;
				Player.PlayerSkills._huntingLevel = Level;
				break;
			case Skills.COOKING:
				Player.PlayerSkills._cookingExp = Exp;
				Player.PlayerSkills._cookingLevel = Level;
				break;
			case Skills.HARVESTING:
				Player.PlayerSkills._harvestingExp = Exp;
				Player.PlayerSkills._harvestingLevel = Level;
				break;
			case Skills.PHYSICAL:
				Player.PlayerSkills._physicalExp = Exp;
				Player.PlayerSkills._physicalLevel = Level;
				break;
			case Skills.CRAFTSMANSHIP:
				Player.PlayerSkills._craftsmanshipExp = Exp;
				Player.PlayerSkills._craftsmanshipLevel = Level;
				break;
			}
		}
	}

	public const int NUM_LEVELS = 7;

	public const float CAMPFIRE_COOKING_RANGE = 8f;

	public const float HARVEST_XP_FACTOR = 0.5f;

	[SerializeField]
	private SkillsExpRewardsAsset _expRewards;

	[SerializeField]
	private SkillsLevelThresholdsAsset _levelThresholds;

	[SerializeField]
	private SkillsEffectMultipliersAsset _effectMultipliers;

	public bool DisplayGUI = true;

	private float HUNTING_POINTS_MAXIMUM;

	private float COOKING_POINTS_MAXIMUM;

	private float HARVESTING_POINTS_MAXIMUM;

	private float PHYSICAL_POINTS_MAXIMUM;

	private float CRAFTSMANSHIP_POINTS_MAXIMUM;

	private int _huntingLevel;

	private int _cookingLevel;

	private int _harvestingLevel;

	private int _physicalLevel;

	private int _craftsmanshipLevel;

	private float _huntingExp;

	private float _cookingExp;

	private float _harvestingExp;

	private float _physicalExp;

	private float _craftsmanshipExp;

	private IPlayer _player;

	public float HuntingDisplayedPercent => GetNormalizedValueInTiers(_huntingExp, _levelThresholds.GetHunting());

	public float CookingDisplayedPercent => GetNormalizedValueInTiers(_cookingExp, _levelThresholds.GetCooking());

	public float HarvestingDisplayedPercent => GetNormalizedValueInTiers(_harvestingExp, _levelThresholds.GetHarvesting());

	public float PhysicalDisplayedPercent => GetNormalizedValueInTiers(_physicalExp, _levelThresholds.GetPhysical());

	public float CraftsmanshipDisplayedPercent => GetNormalizedValueInTiers(_craftsmanshipExp, _levelThresholds.GetCraftsmanship());

	public float PhysicalPercentage => (float)((_physicalLevel == 0) ? 1 : _physicalLevel) / 7f;

	public float HuntingMultiplier
	{
		get
		{
			if (_huntingLevel == 0)
			{
				return 1f;
			}
			return _effectMultipliers.GetHunting(_huntingLevel);
		}
	}

	public float CookingMultiplier
	{
		get
		{
			if (_cookingLevel == 0)
			{
				return 1f;
			}
			return _effectMultipliers.GetCooking(_cookingLevel);
		}
	}

	public float HarvestingMultiplier
	{
		get
		{
			if (_harvestingLevel == 0)
			{
				return 1f;
			}
			return _effectMultipliers.GetHarvesting(_harvestingLevel);
		}
	}

	public float PhysicalMultiplier
	{
		get
		{
			if (_physicalLevel == 0)
			{
				return 1f;
			}
			return _effectMultipliers.GetPhysical(_physicalLevel);
		}
	}

	public float CraftsmanshipMultiplier
	{
		get
		{
			if (_craftsmanshipLevel == 0)
			{
				return _effectMultipliers.GetCraftsmanship(1);
			}
			return _effectMultipliers.GetCraftsmanship(_craftsmanshipLevel);
		}
	}

	public int CraftsmanshipLevel => _craftsmanshipLevel;

	IPlayer INeedPlayerComponent.Player => _player;

	public event EventHandler SkillLevelIncreased;

	protected virtual void OnSkillLevelIncreased(object sender, EventArgs e)
	{
		this.SkillLevelIncreased?.Invoke(sender, e);
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	protected void Awake()
	{
		HUNTING_POINTS_MAXIMUM = _levelThresholds.GetHunting()[6];
		COOKING_POINTS_MAXIMUM = _levelThresholds.GetCooking()[6];
		HARVESTING_POINTS_MAXIMUM = _levelThresholds.GetHarvesting()[6];
		PHYSICAL_POINTS_MAXIMUM = _levelThresholds.GetPhysical()[6];
		CRAFTSMANSHIP_POINTS_MAXIMUM = _levelThresholds.GetCraftsmanship()[6];
	}

	private void OnDestroy()
	{
		this.SkillLevelIncreased = null;
	}

	private void LevelUpNotification(string skillTerm)
	{
		if (_player.IsOwner)
		{
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = NotificationPriority.Immediate;
			localizedNotification.Duration = 8f;
			localizedNotification.PlayerId = _player.Id;
			localizedNotification.MessageText.SetTerm(skillTerm);
			new SkillNotification(localizedNotification).Raise();
		}
	}

	private float GetNormalizedValueInTiers(float value, float[] tiers)
	{
		float num = Mathf.Clamp(value, 0f, tiers[6]);
		for (int i = 0; i < 7; i++)
		{
			if (num <= tiers[i])
			{
				float num2 = ((i > 0) ? tiers[i - 1] : 0f);
				float num3 = (num - num2) / (tiers[i] - num2);
				return ((float)i + num3) / 7f;
			}
		}
		return 0f;
	}

	private void AddHunting(ExpSource source)
	{
		_huntingExp += _expRewards.GetExp(source);
		_huntingExp = Mathf.Clamp(_huntingExp, 0f, HUNTING_POINTS_MAXIMUM);
		if (_huntingLevel != 7 && _huntingExp >= _levelThresholds.GetHunting()[_huntingLevel])
		{
			_huntingLevel++;
			LevelUpNotification("PLAYER_SKILL_HUNTING");
			OnSkillLevelIncreased(this, EventArgs.Empty);
		}
	}

	private void AddCooking(ExpSource source)
	{
		_cookingExp += _expRewards.GetExp(source);
		_cookingExp = Mathf.Clamp(_cookingExp, 0f, COOKING_POINTS_MAXIMUM);
		if (_cookingLevel != 7 && _cookingExp >= _levelThresholds.GetCooking()[_cookingLevel])
		{
			_cookingLevel++;
			LevelUpNotification("PLAYER_SKILL_COOKING");
			OnSkillLevelIncreased(this, EventArgs.Empty);
		}
	}

	private void AddHarvesting(float exp)
	{
		_harvestingExp += exp;
		_harvestingExp = Mathf.Clamp(_harvestingExp, 0f, HARVESTING_POINTS_MAXIMUM);
		if (_harvestingLevel != 7 && _harvestingExp >= _levelThresholds.GetHarvesting()[_harvestingLevel])
		{
			_harvestingLevel++;
			LevelUpNotification("PLAYER_SKILL_HARVESTING");
			OnSkillLevelIncreased(this, EventArgs.Empty);
		}
	}

	private void AddPhysical(ExpSource source)
	{
		_physicalExp += _expRewards.GetExp(source);
		_physicalExp = Mathf.Clamp(_physicalExp, 0f, PHYSICAL_POINTS_MAXIMUM);
		if (_physicalLevel != 7 && _physicalExp >= _levelThresholds.GetPhysical()[_physicalLevel])
		{
			_physicalLevel++;
			LevelUpNotification("PLAYER_SKILL_PHYSICAL");
			OnSkillLevelIncreased(this, EventArgs.Empty);
		}
	}

	private void AddCraftsmanship(float exp)
	{
		_craftsmanshipExp += exp;
		_craftsmanshipExp = Mathf.Clamp(_craftsmanshipExp, 0f, CRAFTSMANSHIP_POINTS_MAXIMUM);
		if (_craftsmanshipLevel != 7 && _craftsmanshipExp >= _levelThresholds.GetCraftsmanship()[_craftsmanshipLevel])
		{
			_craftsmanshipLevel++;
			LevelUpNotification("PLAYER_SKILL_CRAFTSMANSHIP");
			OnSkillLevelIncreased(this, EventArgs.Empty);
		}
	}

	public bool DebugSetHuntingLevel(int level)
	{
		if (level < 0)
		{
			return false;
		}
		if (level > 7)
		{
			level = 7;
		}
		if (level == 0)
		{
			_huntingExp = 0f;
			_huntingLevel = 0;
		}
		else
		{
			_huntingLevel = level;
			_huntingExp = _levelThresholds.GetHunting()[level - 1];
		}
		return true;
	}

	public bool DebugSetCookingLevel(int level)
	{
		if (level < 0)
		{
			return false;
		}
		if (level > 7)
		{
			level = 7;
		}
		if (level == 0)
		{
			_cookingExp = 0f;
			_cookingLevel = 0;
		}
		else
		{
			_cookingLevel = level;
			_cookingExp = _levelThresholds.GetCooking()[level - 1];
		}
		return true;
	}

	public bool DebugSetHarvestingLevel(int level)
	{
		if (level < 0)
		{
			return false;
		}
		if (level > 7)
		{
			level = 7;
		}
		if (level == 0)
		{
			_harvestingExp = 0f;
			_harvestingLevel = 0;
		}
		else
		{
			_harvestingLevel = level;
			_harvestingExp = _levelThresholds.GetHarvesting()[level - 1];
		}
		return true;
	}

	public bool DebugSetPhysicalLevel(int level)
	{
		if (level < 0)
		{
			return false;
		}
		if (level > 7)
		{
			level = 7;
		}
		if (level == 0)
		{
			_physicalExp = 0f;
			_physicalLevel = 0;
		}
		else
		{
			_physicalLevel = level;
			_physicalExp = _levelThresholds.GetPhysical()[level - 1];
		}
		return true;
	}

	public bool DebugSetCraftsmanshipLevel(int level)
	{
		if (level < 0)
		{
			return false;
		}
		if (level > 7)
		{
			level = 7;
		}
		if (level == 0)
		{
			_craftsmanshipExp = 0f;
			_craftsmanshipLevel = 0;
		}
		else
		{
			_craftsmanshipLevel = level;
			_craftsmanshipExp = _levelThresholds.GetCraftsmanship()[level - 1];
		}
		return true;
	}

	public void ReplicatedAddHunting(ExpSource source)
	{
		if (!LevelLoader.IsLoading() && !Singleton<GameTime>.Instance.Paused && _player != null && _player.IsOwner)
		{
			AddHunting(source);
			if (Game.Mode.IsMultiplayer())
			{
				ReplicateSkill replicateSkill = new ReplicateSkill();
				replicateSkill.Type = Skills.HUNTING;
				replicateSkill.Exp = _huntingExp;
				replicateSkill.Level = _huntingLevel;
				replicateSkill.Player = _player;
				replicateSkill.Post();
			}
		}
	}

	public void ReplicatedAddCooking(ExpSource source)
	{
		if (!LevelLoader.IsLoading() && !Singleton<GameTime>.Instance.Paused && _player != null && _player.IsOwner && !_player.Statistics.IsKnockedOut)
		{
			AddCooking(source);
			if (Game.Mode.IsMultiplayer())
			{
				ReplicateSkill replicateSkill = new ReplicateSkill();
				replicateSkill.Type = Skills.COOKING;
				replicateSkill.Exp = _cookingExp;
				replicateSkill.Level = _cookingLevel;
				replicateSkill.Player = _player;
				replicateSkill.Post();
			}
		}
	}

	public void ReplicatedAddHarvesting(float exp)
	{
		if (!LevelLoader.IsLoading() && !Singleton<GameTime>.Instance.Paused && _player != null && _player.IsOwner)
		{
			AddHarvesting(exp);
			if (Game.Mode.IsMultiplayer())
			{
				ReplicateSkill replicateSkill = new ReplicateSkill();
				replicateSkill.Type = Skills.HARVESTING;
				replicateSkill.Exp = _harvestingExp;
				replicateSkill.Level = _harvestingLevel;
				replicateSkill.Player = _player;
				replicateSkill.Post();
			}
		}
	}

	public void ReplicatedAddPhysical(ExpSource source)
	{
		if (!LevelLoader.IsLoading() && !Singleton<GameTime>.Instance.Paused && _player != null && _player.IsOwner)
		{
			AddPhysical(source);
			if (Game.Mode.IsMultiplayer())
			{
				ReplicateSkill replicateSkill = new ReplicateSkill();
				replicateSkill.Type = Skills.PHYSICAL;
				replicateSkill.Exp = _physicalExp;
				replicateSkill.Level = _physicalLevel;
				replicateSkill.Player = _player;
				replicateSkill.Post();
			}
		}
	}

	public void ReplicatedAddCraftsmanship(float exp)
	{
		if (!LevelLoader.IsLoading() && !Singleton<GameTime>.Instance.Paused && _player != null && _player.IsOwner)
		{
			AddCraftsmanship(exp);
			if (Game.Mode.IsMultiplayer())
			{
				ReplicateSkill replicateSkill = new ReplicateSkill();
				replicateSkill.Type = Skills.CRAFTSMANSHIP;
				replicateSkill.Exp = _craftsmanshipExp;
				replicateSkill.Level = _craftsmanshipLevel;
				replicateSkill.Player = _player;
				replicateSkill.Post();
			}
		}
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("hunting", JSerializer.Serialize(_huntingExp));
		jObject.AddField("cooking", JSerializer.Serialize(_cookingExp));
		jObject.AddField("harvesting", JSerializer.Serialize(_harvestingExp));
		jObject.AddField("physical", JSerializer.Serialize(_physicalExp));
		jObject.AddField("craftsmanship", JSerializer.Serialize(_craftsmanshipExp));
		return jObject;
	}

	public void Load(JObject data)
	{
		if (data != null && !data.IsNull())
		{
			_huntingExp = (data.GetField("hunting")?.GetValue<float>()).GetValueOrDefault();
			_huntingLevel = FindLevel(_huntingExp, _levelThresholds.GetHunting());
			_cookingExp = (data.GetField("cooking")?.GetValue<float>()).GetValueOrDefault();
			_cookingLevel = FindLevel(_cookingExp, _levelThresholds.GetCooking());
			_harvestingExp = (data.GetField("harvesting")?.GetValue<float>()).GetValueOrDefault();
			_harvestingLevel = FindLevel(_harvestingExp, _levelThresholds.GetHarvesting());
			_physicalExp = (data.GetField("physical")?.GetValue<float>()).GetValueOrDefault();
			_physicalLevel = FindLevel(_physicalExp, _levelThresholds.GetPhysical());
			_craftsmanshipExp = (data.GetField("craftsmanship")?.GetValue<float>()).GetValueOrDefault();
			_craftsmanshipLevel = FindLevel(_craftsmanshipExp, _levelThresholds.GetCraftsmanship());
		}
	}

	private int FindLevel(float experience, float[] expThresholds)
	{
		if (Mathf.Approximately(experience, 0f))
		{
			return 0;
		}
		if (experience < expThresholds[0])
		{
			return 0;
		}
		for (int i = 0; i < expThresholds.Length - 1; i++)
		{
			if (experience >= expThresholds[i] && experience < expThresholds[i + 1])
			{
				return i + 1;
			}
		}
		return expThresholds.Length;
	}
}
