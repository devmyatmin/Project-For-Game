using UnityEngine;

namespace Beam;

public class NullPlayerSpeech : NullObject<NullPlayerSpeech>, IPlayerSpeech
{
	public void Play(string name, int gender, bool underwater)
	{
		Debug.LogError("NullPlayerSpeech::Play:: Not implemented!");
	}
}
