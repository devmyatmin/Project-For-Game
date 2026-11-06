using Beam.Events;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

namespace Beam;

public class GameSettings : ISaveable
{
	public EGameMode Mode;

	public EGameDifficultyMode Difficulty;

	public EGameWorldMode World;

	public EGameGenderMode Gender;

	public EGameGenderMode Gender2 = EGameGenderMode.Female;

	public EGamePermadeathMode Permadeath;

	public EGameWildlifeMode Wildlife;

	public EGameStartingCrate StartingCrate;

	public EGameGenderMode GetGenderById(int id)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			return PlayerRegistry.ServerPeer?.GetGenderById(id) ?? EGameGenderMode.Male;
		}
		return id switch
		{
			0 => Gender, 
			1 => Gender2, 
			_ => (EGameGenderMode)(id % 2), 
		};
	}

	public GameSettings()
	{
	}

	public GameSettings(EGameMode mode, EGameDifficultyMode difficulty, EGameWorldMode world, EGameGenderMode gender, EGameGenderMode gender2, EGamePermadeathMode permadeath, EGameWildlifeMode wildlife, EGameStartingCrate startingCrate)
	{
		Mode = mode;
		Difficulty = difficulty;
		World = world;
		Gender = gender;
		Gender2 = gender2;
		Permadeath = permadeath;
		Wildlife = wildlife;
		StartingCrate = startingCrate;
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("Difficulty", JSerializer.Serialize((object)Difficulty));
		jObject.AddField("Gender", JSerializer.Serialize((object)Gender));
		jObject.AddField("Gender2", JSerializer.Serialize((object)Gender2));
		jObject.AddField("Permadeath", JSerializer.Serialize((object)Permadeath));
		jObject.AddField("Wildlife", JSerializer.Serialize((object)Wildlife));
		jObject.AddField("StartingCrate", JSerializer.Serialize((object)StartingCrate));
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Difficulty");
		if (field != null)
		{
			Difficulty = JSerializer.Deserialize<EGameDifficultyMode>(field);
		}
		JObject jObject = data.GetField("CharacterGender") ?? data.GetField("Gender");
		if (jObject != null)
		{
			Gender = JSerializer.Deserialize<EGameGenderMode>(jObject);
		}
		JObject field2 = data.GetField("Gender2");
		if (field2 != null)
		{
			Gender2 = JSerializer.Deserialize<EGameGenderMode>(field2);
		}
		JObject field3 = data.GetField("Permadeath");
		if (field3 != null)
		{
			Permadeath = JSerializer.Deserialize<EGamePermadeathMode>(field3);
		}
		JObject field4 = data.GetField("Wildlife");
		if (field4 != null)
		{
			Wildlife = JSerializer.Deserialize<EGameWildlifeMode>(field4);
		}
		JObject field5 = data.GetField("StartingCrate");
		if (field5 != null)
		{
			StartingCrate = JSerializer.Deserialize<EGameStartingCrate>(field5);
		}
		EventManager.RaiseEvent(new DifficultyChangedEvent());
		Debug.Log((object)("CustomSettings::Load::Loaded:: Difficulty " + Difficulty));
	}
}
