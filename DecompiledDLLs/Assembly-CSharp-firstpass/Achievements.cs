using System;
using System.Collections.Generic;
using System.Linq;
using Beam;
using Beam.AccountServices;
using Beam.Crafting;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Beam.Utilities.Reflection;
using Funlabs;
using UnityEngine;

public class Achievements : MonoBehaviour, INeedPlayerComponent, ISaveable
{
	private const int FISH_ARE_FRIENDS_GOAL = 10;

	private const int VEGETARIAN_GOAL = 10;

	private const int ISLAND_HERMIT_GOAL = 10;

	private const int DAY_10_GOAL = 10;

	private const int DAY_20_GOAL = 20;

	private const int DAY_50_GOAL = 50;

	private const int DAY_100_GOAL = 100;

	private const int FISHING_SEASON_OPEN_GOAL = 1;

	private const int SEA_HARVEST_GOAL = 10;

	private const int ISLAND_HOPPER_GOAL = 1;

	private const int ANOTHER_DAY_SHORE_GOAL = 5;

	private const int COLUMBUS_GOAL = 10;

	private const int WORKING_WITH_HANDS_GOAL = 20;

	private const int LEAN_MEAN_CRAFTING_GOAL = 40;

	private const int CALL_ME_AHAB_GOAL = 3;

	private const int INDUSTRIAL_FASHION_GOAL = 10;

	private const int THIS_SPARKS_JOY_GOAL = 100;

	private IPlayer _player;

	private bool _usedLoom;

	private bool _usedCompass;

	private int _daysWithoutHop;

	private int _daysWithoutFish;

	private int _daysWithoutMeat;

	private int _toolsCrafted;

	private int _corrugatedFurnitureCrafted;

	private int _sheltersScavenged;

	private int _pilesCrafted;

	private bool[] _achieved;

	IPlayer INeedPlayerComponent.Player => _player;

	private bool GetAchieved(EAchievement ach)
	{
		if (ach > EAchievement.HORRIFIC_PACIFIC && (int)ach < _achieved.Length)
		{
			return _achieved[(int)ach];
		}
		return false;
	}

	private void SetAchieved(EAchievement ach, bool value)
	{
		if (ach > EAchievement.HORRIFIC_PACIFIC && (int)ach < _achieved.Length)
		{
			_achieved[(int)ach] = value;
		}
	}

	private void Awake()
	{
		int num = Enum.GetNames(typeof(EAchievement)).Length;
		_achieved = new bool[num];
	}

	public void SetPlayer(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		if ((MPExtensions.IsMultiplayer(Game.Mode) && !player.IsOwner) || (!MPExtensions.IsMultiplayer(Game.Mode) && player.Id != 0))
		{
			((Behaviour)this).enabled = false;
			return;
		}
		_player = player;
		if ((int)Game.State == 2 || (int)Game.State == 3)
		{
			_player.Crafter.Crafted += Crafter_Crafted;
			_player.Statistics.Ate += Statistics_Ate;
			_player.Statistics.PlayerEffectApplied += Statistics_PlayerEffectApplied;
			Singleton<GameTime>.Instance.DayTick += GameTime_DayTick;
			Tutorial.Instance.Completed += Tutorial_Completed;
			Singleton<StrandedWorld>.Instance.ZoneEvents.CampFirePlaced += ZoneEvents_CampFirePlaced;
			Singleton<StrandedWorld>.Instance.ZoneEvents.CropPlanted += ZoneEvents_CropPlanted;
			EventManager.AddListener<AddStatsEvent>(EventManager_AddStatsEvent);
			EventManager.AddListener<ScavengedShelterEvent>(EventManager_ScavengedShelterEvent);
			EventManager.AddListener<AchievementUnlockEvent>(EventManager_AchievementUnlock);
			EventManager.AddListener<CompletedEndGameEvent>(EventManager_CompletedEndGameEvent);
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.DayTick -= GameTime_DayTick;
		}
		if (Object.op_Implicit((Object)(object)Tutorial.Instance))
		{
			Tutorial.Instance.Completed -= Tutorial_Completed;
		}
		if (Object.op_Implicit((Object)(object)Singleton<StrandedWorld>.Instance))
		{
			Singleton<StrandedWorld>.Instance.ZoneEvents.CampFirePlaced -= ZoneEvents_CampFirePlaced;
			Singleton<StrandedWorld>.Instance.ZoneEvents.CropPlanted -= ZoneEvents_CropPlanted;
		}
		EventManager.RemoveListener<AddStatsEvent>(EventManager_AddStatsEvent);
		EventManager.RemoveListener<ScavengedShelterEvent>(EventManager_ScavengedShelterEvent);
		EventManager.RemoveListener<AchievementUnlockEvent>(EventManager_AchievementUnlock);
		EventManager.RemoveListener<CompletedEndGameEvent>(EventManager_CompletedEndGameEvent);
	}

	private void EventManager_AchievementUnlock(AchievementUnlockEvent e)
	{
		DoAchievement(e.Achievement);
	}

	private void EventManager_CompletedEndGameEvent(CompletedEndGameEvent e)
	{
		AwardEndGameAchievement();
	}

	private void Tutorial_Completed()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode))
		{
			DoAchievement(EAchievement.HORRIFIC_PACIFIC);
		}
	}

	private void GameTime_DayTick()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Game.State != 1)
		{
			if (!AchievementTools.IsCheating(((Component)this).transform.position))
			{
				_daysWithoutFish++;
				_daysWithoutMeat++;
				_daysWithoutHop++;
			}
			if (_daysWithoutFish >= 10)
			{
				DoAchievement(EAchievement.FISH_ARE_FRIENDS);
			}
			if (_daysWithoutMeat >= 10)
			{
				DoAchievement(EAchievement.VEGETARIAN);
			}
			if (_daysWithoutHop >= 10)
			{
				DoAchievement(EAchievement.HERMIT);
			}
			if (Singleton<GameCalendar>.Instance.DaysElapsed >= 10)
			{
				DoAchievement(EAchievement.DAY_10);
			}
			if (Singleton<GameCalendar>.Instance.DaysElapsed >= 20)
			{
				DoAchievement(EAchievement.DAY_20);
			}
			if (Singleton<GameCalendar>.Instance.DaysElapsed >= 50)
			{
				DoAchievement(EAchievement.DAY_50);
			}
			if (Singleton<GameCalendar>.Instance.DaysElapsed >= 100)
			{
				DoAchievement(EAchievement.DAY_100);
			}
		}
	}

	public void ZoneEvents_CampFirePlaced(Zone zone)
	{
		int num = Singleton<StrandedWorld>.Instance.Zones.Sum((Zone z) => (z.Attributes.CampFires > 0) ? 1 : 0);
		if (num >= 5)
		{
			DoAchievement(EAchievement.ANOTHER_DAY_ANOTHER_SHORE);
		}
		if (num >= 10)
		{
			DoAchievement(EAchievement.COLUMBUS);
		}
	}

	public void ZoneEvents_CropPlanted(Zone zone, PlantType plantType)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (Beam.Utilities.ExtensionMethods.All_NonAlloc(zone.Attributes.Crops, (KeyValuePair<PlantType, int> c) => c.Value > 0) && !AchievementTools.IsCheating(_player.transform.position))
		{
			DoAchievement(EAchievement.HORTICULTURALIST);
		}
	}

	private void EventManager_AddStatsEvent(AddStatsEvent e)
	{
		if (e.PlayerId != _player.Id)
		{
			return;
		}
		GAccountService.Get().LocalUser.UserStats.SessionStats += e.StatsToAdd;
		if (e.StatsToAdd.FishFished > 0)
		{
			int fishFished = GAccountService.Get().LocalUser.UserStats.SessionStats.FishFished;
			if (fishFished >= 1)
			{
				DoAchievement(EAchievement.FISHING_SEASON_IS_OPEN);
			}
			if (fishFished >= 10)
			{
				DoAchievement(EAchievement.THE_SEAS_HARVEST);
			}
		}
		if (e.StatsToAdd.IslandsHopped > 0)
		{
			if (GAccountService.Get().LocalUser.UserStats.SessionStats.IslandsHopped >= 1)
			{
				DoAchievement(EAchievement.ISLAND_HOPPER);
			}
			_daysWithoutHop = 0;
		}
	}

	private void EventManager_ScavengedShelterEvent(ScavengedShelterEvent e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!AchievementTools.IsCheating(((Component)this).transform.position))
		{
			_sheltersScavenged++;
		}
		if (_sheltersScavenged >= Singleton<StrandedWorld>.Instance.ShelterCount)
		{
			DoAchievement(EAchievement.ARCHAEOLOGIST);
		}
	}

	private void Crafter_Crafted(CraftingCombination combination)
	{
		if (combination.Name == "ITEM PILE")
		{
			OnItemPileCrafted();
		}
		else if (combination.CraftingType.AttributeType == AttributeType.Corrugated && (combination.CraftingType.InteractiveType == InteractiveType.SHELF || combination.CraftingType.InteractiveType == InteractiveType.TABLE || combination.CraftingType.InteractiveType == InteractiveType.CHAIR))
		{
			OnCorrugatedFurnitureCrafted();
		}
		else
		{
			if (combination.IsExtension)
			{
				CraftingType craftingType = Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(combination.Materials);
				if ((object)craftingType != null && craftingType.InteractiveType == InteractiveType.STATION_LOOM)
				{
					DoAchievement(EAchievement.NEW_THREADS);
					goto IL_00d5;
				}
			}
			if (combination.CraftingType.InteractiveType == InteractiveType.TROPHY)
			{
				OnTrophyCrafted();
			}
			else if (combination.CraftingType.InteractiveType.ToString().Contains("TOOL"))
			{
				OnToolCrafted();
			}
		}
		goto IL_00d5;
		IL_00d5:
		CheckCraftedThemAll();
	}

	private void OnItemPileCrafted()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!AchievementTools.IsCheating(((Component)this).transform.position))
		{
			_pilesCrafted++;
		}
		if (_pilesCrafted >= 100)
		{
			DoAchievement(EAchievement.THIS_SPARKS_JOY);
		}
	}

	private void OnCorrugatedFurnitureCrafted()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!AchievementTools.IsCheating(((Component)this).transform.position))
		{
			_corrugatedFurnitureCrafted++;
		}
		if (_corrugatedFurnitureCrafted >= 10)
		{
			DoAchievement(EAchievement.INDUSTRIAL_FASHION);
		}
	}

	private void OnTrophyCrafted()
	{
		if (CheckMissionTrophiesCrafted() >= 3)
		{
			DoAchievement(EAchievement.CALL_ME_AHAB);
		}
	}

	private int CheckMissionTrophiesCrafted()
	{
		int num = 0;
		foreach (CraftingCombination combination in _player.Crafter.CraftingCombinations.Combinations)
		{
			if ((combination.Name == "SHARK TROPHY" || combination.Name == "EEL TROPHY" || combination.Name == "SQUID TROPHY") && combination.Unlocked && combination.CraftedCount > 0)
			{
				num++;
			}
		}
		return num;
	}

	private void OnToolCrafted()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!AchievementTools.IsCheating(((Component)this).transform.position))
		{
			_toolsCrafted++;
		}
		if (_toolsCrafted >= 20)
		{
			DoAchievement(EAchievement.WORKING_WITH_MY_HANDS);
		}
		if (_toolsCrafted >= 40)
		{
			DoAchievement(EAchievement.LEAN_MEAN_CRAFTING_MACHINE);
		}
	}

	public void Statistics_Ate(InteractiveType type, MeatProvenance provenance)
	{
		if (type == InteractiveType.FOOD_MEAT)
		{
			_daysWithoutMeat = 0;
			if (provenance == MeatProvenance.Fish)
			{
				_daysWithoutFish = 0;
			}
		}
	}

	private void Statistics_PlayerEffectApplied(PlayerEffect effect)
	{
		GAccountService.Get().LocalUser.UserStats.SessionStats.StatusEffects++;
	}

	private void CheckCraftedThemAll()
	{
		if (!GetAchieved(EAchievement.GOTTA_CRAFT_EM_ALL) && Beam.Utilities.ExtensionMethods.All_NonAlloc(_player.Crafter.CraftingCombinations.Combinations.Where((CraftingCombination c) => !c.IsDynamic), (CraftingCombination c) => c.CraftedCount > 0))
		{
			DoAchievement(EAchievement.GOTTA_CRAFT_EM_ALL);
		}
	}

	public void AwardEndGameAchievement()
	{
		if (!_usedCompass)
		{
			EventManager.RaiseEvent(new AchievementUnlockEvent(EAchievement.MAGNETS));
		}
		EventManager.RaiseEvent(new AchievementUnlockEvent(EAchievement.OUT_OF_THE_FRYING_PAN));
		AccountUserStats userStats = GAccountService.Get().LocalUser.UserStats;
		GLeaderboardHandler.Get().SubmitScore("QUICKEST_ESCAPE", userStats.SessionStats.DaysSurvived);
	}

	public void UsedCompass()
	{
		_usedCompass = true;
	}

	private void DoAchievement(EAchievement ach)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((!MPExtensions.IsMultiplayer(Game.Mode) || _player.IsOwner) && !GetAchieved(ach))
		{
			SetAchieved(ach, value: true);
			UnlockAchievement(ach);
		}
	}

	private void UnlockAchievement(EAchievement ach)
	{
		GAchievementsHandler.Get().UnlockAchievement(ach);
	}

	private void NotifyMissedAchievement(EAchievement ach)
	{
		string text = Reflection.GetAttribute<DescriptionAttribute>(ach).Text;
		LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
		localizedNotification.Priority = NotificationPriority.Immediate;
		localizedNotification.Duration = 8f;
		localizedNotification.TitleText.SetTerm("ACHIEVEMENT_NTF_MISSED_ACHIEVEMENT_TITLE");
		localizedNotification.MessageText.SetTerm("ACHIEVEMENT_NTF_MISSED_ACHIEVEMENT_DESC");
		localizedNotification.MessageText.SetArgs(text);
		localizedNotification.Raise();
	}

	private void CheckAchievementsGoals()
	{
		if (CheckMissionTrophiesCrafted() >= 3)
		{
			DoAchievement(EAchievement.CALL_ME_AHAB);
		}
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		JObject jObject2 = new JObject();
		for (int i = 0; i < _achieved.Length; i++)
		{
			bool instance = _achieved[i];
			jObject2.Add(instance);
		}
		jObject.AddField("achieved", jObject2);
		jObject.AddField("usedLoom", JSerializer.Serialize((object)_usedLoom));
		jObject.AddField("usedCompass", JSerializer.Serialize((object)_usedCompass));
		jObject.AddField("daysWithoutHop", JSerializer.Serialize((object)_daysWithoutHop));
		jObject.AddField("daysWithoutFish", JSerializer.Serialize((object)_daysWithoutFish));
		jObject.AddField("daysWithoutMeat", JSerializer.Serialize((object)_daysWithoutMeat));
		jObject.AddField("toolsCrafted", JSerializer.Serialize((object)_toolsCrafted));
		jObject.AddField("corrugatedFurnitureCrafted", JSerializer.Serialize((object)_corrugatedFurnitureCrafted));
		jObject.AddField("sheltersScavenged", JSerializer.Serialize((object)_sheltersScavenged));
		jObject.AddField("pilesCrafted", JSerializer.Serialize((object)_pilesCrafted));
		return jObject;
	}

	public void Load(JObject data)
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (data == null || data.IsNull())
		{
			Debug.LogWarning((object)$"Achievements:: Data was null or empty for P{_player.Id}");
			return;
		}
		JObject field = data.GetField("achieved");
		if (field != null)
		{
			for (int i = 0; i < _achieved.Length && i < field.Children.Count; i++)
			{
				if (field.Children[i].GetValue<bool>())
				{
					_achieved[i] = true;
					EAchievement ach = (EAchievement)i;
					UnlockAchievement(ach);
				}
			}
		}
		bool value = data.GetField("usedLoom").GetValue<bool>();
		_usedLoom = value;
		bool value2 = data.GetField("usedCompass").GetValue<bool>();
		_usedCompass = value2;
		int value3 = data.GetField("daysWithoutHop").GetValue<int>();
		_daysWithoutHop = value3;
		int value4 = data.GetField("daysWithoutFish").GetValue<int>();
		_daysWithoutFish = value4;
		int value5 = data.GetField("daysWithoutMeat").GetValue<int>();
		_daysWithoutMeat = value5;
		int value6 = data.GetField("toolsCrafted").GetValue<int>();
		_toolsCrafted = value6;
		int value7 = data.GetField("corrugatedFurnitureCrafted").GetValue<int>();
		_corrugatedFurnitureCrafted = value7;
		int value8 = data.GetField("sheltersScavenged").GetValue<int>();
		_sheltersScavenged = value8;
		JObject field2 = data.GetField("pilesCrafted");
		if (field2 != null)
		{
			_pilesCrafted = field2.GetValue<int>();
		}
		CheckAchievementsGoals();
	}
}
