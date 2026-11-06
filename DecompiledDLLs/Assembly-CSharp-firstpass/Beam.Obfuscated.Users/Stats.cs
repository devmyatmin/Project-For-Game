using System;
using Beam.Serialization.Json;
using UnityEngine;

namespace Beam.Obfuscated.Users;

[Serializable]
public class Stats : ISaveable
{
	public int TimePlayed;

	public int DaysSurvived;

	public int IslandsHopped;

	public int IslandsExplored;

	public int FishFished;

	public int AnimalsKilled;

	public int AnimalsTrapped;

	public int ItemsCrafted;

	public int ItemsLooted;

	public int ShipsScavenged;

	public int StatusEffects;

	public int IslandsInserted;

	public int IslandsCreated;

	public void Reset()
	{
		TimePlayed = 0;
		DaysSurvived = 0;
		IslandsHopped = 0;
		IslandsExplored = 0;
		FishFished = 0;
		AnimalsKilled = 0;
		AnimalsTrapped = 0;
		ItemsCrafted = 0;
		ItemsLooted = 0;
		ShipsScavenged = 0;
		StatusEffects = 0;
		IslandsInserted = 0;
		IslandsCreated = 0;
	}

	public static Stats operator +(Stats a, Stats b)
	{
		return new Stats
		{
			TimePlayed = a.TimePlayed + b.TimePlayed,
			DaysSurvived = a.DaysSurvived + b.DaysSurvived,
			IslandsHopped = a.IslandsHopped + b.IslandsHopped,
			IslandsExplored = a.IslandsExplored + b.IslandsExplored,
			FishFished = a.FishFished + b.FishFished,
			AnimalsKilled = a.AnimalsKilled + b.AnimalsKilled,
			AnimalsTrapped = a.AnimalsTrapped + b.AnimalsTrapped,
			ItemsCrafted = a.ItemsCrafted + b.ItemsCrafted,
			ItemsLooted = a.ItemsLooted + b.ItemsLooted,
			ShipsScavenged = a.ShipsScavenged + b.ShipsScavenged,
			StatusEffects = a.StatusEffects + b.StatusEffects,
			IslandsInserted = a.IslandsInserted + b.IslandsInserted,
			IslandsCreated = a.IslandsCreated + b.IslandsCreated
		};
	}

	public static Stats operator -(Stats a, Stats b)
	{
		return new Stats
		{
			TimePlayed = Mathf.Max(0, a.TimePlayed - b.TimePlayed),
			DaysSurvived = Mathf.Max(0, a.DaysSurvived - b.DaysSurvived),
			IslandsHopped = Mathf.Max(0, a.IslandsHopped - b.IslandsHopped),
			IslandsExplored = Mathf.Max(0, a.IslandsExplored - b.IslandsExplored),
			FishFished = Mathf.Max(0, a.FishFished - b.FishFished),
			AnimalsKilled = Mathf.Max(0, a.AnimalsKilled - b.AnimalsKilled),
			AnimalsTrapped = Mathf.Max(0, a.AnimalsTrapped - b.AnimalsTrapped),
			ItemsCrafted = Mathf.Max(0, a.ItemsCrafted - b.ItemsCrafted),
			ItemsLooted = Mathf.Max(0, a.ItemsLooted - b.ItemsLooted),
			ShipsScavenged = Mathf.Max(0, a.ShipsScavenged - b.ShipsScavenged),
			StatusEffects = Mathf.Max(0, a.StatusEffects - b.StatusEffects),
			IslandsInserted = Mathf.Max(0, a.IslandsInserted - b.IslandsInserted),
			IslandsCreated = Mathf.Max(0, a.IslandsCreated - b.IslandsCreated)
		};
	}

	public JObject Save()
	{
		try
		{
			JObject jObject = new JObject();
			jObject.AddField("TimePlayed", JSerializer.Serialize((object)TimePlayed));
			jObject.AddField("DaysSurvived", JSerializer.Serialize((object)DaysSurvived));
			jObject.AddField("IslandsHopped", JSerializer.Serialize((object)IslandsHopped));
			jObject.AddField("IslandsExplored", JSerializer.Serialize((object)IslandsExplored));
			jObject.AddField("FishFished", JSerializer.Serialize((object)FishFished));
			jObject.AddField("AnimalsKilled", JSerializer.Serialize((object)AnimalsKilled));
			jObject.AddField("AnimalsTrapped", JSerializer.Serialize((object)AnimalsTrapped));
			jObject.AddField("ItemsCrafted", JSerializer.Serialize((object)ItemsCrafted));
			jObject.AddField("ItemsLooted", JSerializer.Serialize((object)ItemsLooted));
			jObject.AddField("ShipsScavenged", JSerializer.Serialize((object)ShipsScavenged));
			jObject.AddField("StatusEffects", JSerializer.Serialize((object)StatusEffects));
			jObject.AddField("IslandsInserted", JSerializer.Serialize((object)IslandsInserted));
			jObject.AddField("IslandsCreated", JSerializer.Serialize((object)IslandsCreated));
			return jObject;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Stats::Save:: Error saving stats.\n" + ex));
			return null;
		}
	}

	public void Load(JObject data)
	{
		Reset();
		try
		{
			int value = data.GetField("TimePlayed").GetValue<int>();
			TimePlayed = value;
			int value2 = data.GetField("DaysSurvived").GetValue<int>();
			DaysSurvived = value2;
			int value3 = data.GetField("IslandsHopped").GetValue<int>();
			IslandsHopped = value3;
			int value4 = data.GetField("IslandsExplored").GetValue<int>();
			IslandsExplored = value4;
			int value5 = data.GetField("FishFished").GetValue<int>();
			FishFished = value5;
			int value6 = data.GetField("AnimalsKilled").GetValue<int>();
			AnimalsKilled = value6;
			int value7 = data.GetField("AnimalsTrapped").GetValue<int>();
			AnimalsTrapped = value7;
			int value8 = data.GetField("ItemsCrafted").GetValue<int>();
			ItemsCrafted = value8;
			int value9 = data.GetField("ItemsLooted").GetValue<int>();
			ItemsLooted = value9;
			int value10 = data.GetField("ShipsScavenged").GetValue<int>();
			ShipsScavenged = value10;
			int value11 = data.GetField("StatusEffects").GetValue<int>();
			StatusEffects = value11;
			int value12 = data.GetField("IslandsInserted").GetValue<int>();
			IslandsInserted = value12;
			int value13 = data.GetField("IslandsCreated").GetValue<int>();
			IslandsCreated = value13;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Stats::Load: Error loading stats.\n" + ex));
		}
	}
}
