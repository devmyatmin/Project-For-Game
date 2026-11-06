using Beam.Crafting;
using Funlabs;
using Photon.Bolt;
using Rewired;
using UnityEngine;

namespace Beam;

public interface IPlayer : ISaveable, IIdleUntilInitialized
{
	Transform transform { get; }

	IPlayerState MultiplayerState { get; }

	bool IsOwner { get; }

	int Id { get; set; }

	int Gender { get; set; }

	PlayerCamera PlayerCamera { get; }

	Movement Movement { get; }

	Player Input { get; }

	bool IsBusy { get; set; }

	PlayerUI PlayerUI { get; }

	PlayerSpeech PlayerSpeech { get; }

	Character Character { get; }

	Statistics Statistics { get; }

	Inventory Inventory { get; }

	User User { get; }

	Interacter Interacter { get; }

	Holder Holder { get; }

	HotkeyController Hotkeys { get; }

	Crafter Crafter { get; }

	FireLighter Lighter { get; }

	Skinner Skinner { get; }

	Dragger Dragger { get; }

	GameInputController GameInput { get; }

	Audio Audio { get; }

	DeveloperMode DeveloperMode { get; }

	PlayerSkills PlayerSkills { get; }

	PlayerExperience PlayerExperience { get; }

	Achievements Achievements { get; }

	Peer Peer { get; set; }

	IMissionGoal CurrentMissionGoal { get; set; }

	void SetInput(Player input);

	void SetCamera(PlayerCamera playerCamera);

	void SetUI(PlayerUI playerUI);

	void SetVibration(int motorIndex, float motorLevel, float duration);
}
