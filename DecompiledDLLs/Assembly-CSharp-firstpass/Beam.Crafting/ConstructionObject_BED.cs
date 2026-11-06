using System;
using System.Collections.Generic;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using Rewired;
using UnityEngine;

namespace Beam.Crafting;

public class ConstructionObject_BED : ConstructionObject
{
	private struct Result
	{
		public bool Succeeded;

		public string Message;
	}

	private class ReplicateSaveGameOnMasterMsg : MultiplayerMessage
	{
		[Replicate]
		public IPlayer InvokingPlayer;

		public override void OnPeer()
		{
			SaveManager.SaveGame(InvokingPlayer);
		}
	}

	private class ReplicateSleep_Sequence : MultiplayerMessage
	{
		[Replicate]
		public ConstructionObject_BED Bed;

		[Replicate]
		public float RandomValue;

		[Replicate]
		public float MilitaryTime;

		public override void OnPeer()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			Timing.RunCoroutine(Bed.Sleep_Sequence(RandomValue, MilitaryTime), (Segment)4);
		}
	}

	private const float SLEEP_TIME = 9f;

	private const float MINIMUM_HOURS_BETWEEN_SLEEP = 4f;

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Buoyancy _buoyancy;

	private static DateTime _LastTimeSlept;

	private ICheckBounds _boundsChecker;

	private bool _isSleeping;

	private DialogueBoxViewAdapter saveDialogueBox;

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_isSleeping)
		{
			return string.Empty;
		}
		return "ITEM_INTERACTION_DESCRIPTION_SLEEP_SAVE";
	}

	protected override void Awake()
	{
		base.Awake();
		if (CanPickUp)
		{
			SetLOD(1);
		}
	}

	protected override void Start()
	{
		base.Start();
		ICheckBounds instance = Singleton<StrandedWorld>.Instance;
		_boundsChecker = instance ?? NullObject<NullCheckBounds>.Instance;
		_LastTimeSlept = DateTimeTools.RemoveHours(GameTime.Now, 4f);
		EventManager.AddListener<EndGameSequenceEvent>(AbortInteraction);
	}

	protected override void OnPlayerDisconnect(PlayerDisconnectedEvent disconnectedEvent)
	{
		if ((Object)(object)saveDialogueBox != (Object)null)
		{
			saveDialogueBox.Destroy();
			HideDialogue(PlayerRegistry.LocalPlayer);
		}
		base.OnPlayerDisconnect(disconnectedEvent);
	}

	private void AbortInteraction(EndGameSequenceEvent e)
	{
		CanInteract = false;
		CanPickUp = false;
	}

	public override void Hold(bool holding)
	{
		base.Hold(holding);
		SetLOD((!holding) ? 1 : 0);
	}

	public override bool Interact(IPlayer player)
	{
		if (_isSleeping || !player.IsOwner)
		{
			return false;
		}
		player.Movement.StopAutoWalk();
		Result result = CheckCanUseBed(player);
		if (!result.Succeeded)
		{
			string message = result.Message;
			if (!string.IsNullOrWhiteSpace(message))
			{
				LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, message);
			}
			return false;
		}
		if (Time.timeScale != 0f)
		{
			ShowDialogue(player);
		}
		return true;
	}

	private Result CheckCanUseBed(IPlayer player)
	{
		Result result = new Result
		{
			Succeeded = false
		};
		if (LevelLoader.IsCreatingOrLoading)
		{
			result.Message = "BED_NTF_CANT_USE_LOADING";
			return result;
		}
		if (AnyPlayerIsBusy())
		{
			result.Message = "BED_NTF_CANT_USE_BUSY";
			return result;
		}
		result.Message = string.Empty;
		result.Succeeded = true;
		return result;
	}

	private Result CheckCanSave(IPlayer player)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Result result = CheckCanUseBed(player);
		if (!result.Succeeded)
		{
			return result;
		}
		result.Succeeded = false;
		if (AnyPlayerKnockedOut())
		{
			result.Message = "BED_NTF_CANT_SAVE_KNOCKED_OUT";
			return result;
		}
		if (!AllPlayersOverLand())
		{
			result.Message = (((int)Game.Mode == 0) ? "BED_NTF_CANT_SAVE_DESC" : "BED_NTF_CANT_SAVE_COOP_DESC");
			return result;
		}
		result.Message = string.Empty;
		result.Succeeded = true;
		return result;
	}

	private Result CheckCanSleep(IPlayer player)
	{
		Result result = CheckCanUseBed(player);
		if (!result.Succeeded)
		{
			return result;
		}
		result.Succeeded = false;
		if (Object.op_Implicit((Object)(object)_buoyancy) && (_buoyancy.Floating || _buoyancy.IsInWater))
		{
			result.Message = "BED_NTF_CANT_SLEEP_UNDERWATER_DESC";
			return result;
		}
		if (GameTime.GetHoursSince(_LastTimeSlept) < 4f)
		{
			result.Message = "BED_NTF_CANT_SLEEP_DESC";
			return result;
		}
		if (Singleton<GameTime>.Instance.MilitaryTime > 4f && Singleton<GameTime>.Instance.MilitaryTime < 18f)
		{
			result.Message = "BED_NTF_CANT_SLEEP_NIGHT";
			return result;
		}
		if (!AllPlayersInProximity(player))
		{
			result.Message = "BED_NTF_CANT_SLEEP_APART_DESC";
			return result;
		}
		result.Message = string.Empty;
		result.Succeeded = true;
		return result;
	}

	private void DoSave(IPlayer player)
	{
		HideDialogue(player);
		Result result = CheckCanSave(player);
		if (!result.Succeeded)
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
			string message = result.Message;
			if (!string.IsNullOrWhiteSpace(message))
			{
				LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, message);
			}
		}
		else
		{
			SaveOnMaster(player);
		}
	}

	private void DoSleep(IPlayer player)
	{
		HideDialogue(player);
		Result result = CheckCanSleep(player);
		if (!result.Succeeded)
		{
			MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
			string message = result.Message;
			if (!string.IsNullOrWhiteSpace(message))
			{
				LocalizedNotification.Post(player, NotificationPriority.Immediate, 4f, message);
			}
		}
		else
		{
			ReplicatedSleep_Sequence(Random.value);
		}
	}

	private void DoCancel(IPlayer player)
	{
		HideDialogue(player);
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
	}

	private void ShowDialogue(IPlayer player)
	{
		Time.timeScale = 0f;
		RewiredUtilities.DisableMaps(RewiredUtilities.GetOtherPlayers(player.Input));
		RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: true, 16);
		RewiredUtilities.SetCursorEnabled(player.Input, enabled: true);
		player.IsBusy = true;
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: true);
		saveDialogueBox = DialogueBox.Show(player.Input, "BED_DB_SLEEP_TITLE", "BED_DB_SLEEP_SAVE_DESC", "BED_DB_BUTTON_SLEEP", "BED_DB_BUTTON_SAVE", "BED_DB_BUTTON_CANCEL");
		saveDialogueBox.Accept += () =>
		{
			DoSleep(player);
		};
		saveDialogueBox.Decline += () =>
		{
			DoSave(player);
		};
		saveDialogueBox.Cancel += () =>
		{
			DoCancel(player);
		};
	}

	private void HideDialogue(IPlayer player)
	{
		Time.timeScale = 1f;
		RewiredUtilities.RestoreMapsEnabled(RewiredUtilities.GetOtherPlayers(player.Input));
		RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: false, 16);
		RewiredUtilities.SetAndCacheMapsEnabled(player.Input, state: true, 0);
		RewiredUtilities.SetCursorEnabled(player.Input, enabled: false);
		player.IsBusy = false;
	}

	private IEnumerator<float> Sleep_Sequence(float randomValue, float militaryTime)
	{
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		_isSleeping = true;
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			++allPlayer.Movement.IsBusy;
		}
		Singleton<Fader>.Instance.FadeOut(3f);
		float timer = 0f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			AudioListener.volume -= 1f / 3f * Time.deltaTime;
			yield return 0f;
		}
		AudioListener.volume = 0f;
		if (MPExtensions.IsClient(Game.Mode))
		{
			Singleton<GameTime>.Instance.MilitaryTime_NoEvents = militaryTime;
		}
		if (Singleton<GameTime>.Instance.MilitaryTime > 18f)
		{
			CheckShelter();
			Singleton<GameTime>.Instance.MilitaryTime = 24f;
			yield return Timing.WaitForSeconds(0.5f);
		}
		float num = 4f + randomValue * 3f;
		if (Singleton<GameTime>.Instance.MilitaryTime < num)
		{
			float num2 = num - Singleton<GameTime>.Instance.MilitaryTime;
			CheckShelter();
			Singleton<GameTime>.Instance.MilitaryTime += num2;
		}
		yield return Timing.WaitForSeconds(1.5f);
		_LastTimeSlept = GameTime.Now;
		Singleton<Fader>.Instance.FadeIn(3f);
		timer = 0f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			AudioListener.volume += 1f / 3f * Time.deltaTime;
			yield return 0f;
		}
		AudioListener.volume = 1f;
		_isSleeping = false;
		foreach (IPlayer allPlayer2 in PlayerRegistry.AllPlayers)
		{
			--allPlayer2.Movement.IsBusy;
		}
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 0);
		MultiplayerMng.UpdateSessionToken_IsBusy(isBusy: false);
	}

	private void CheckShelter()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Sunburn playerEffect = allPlayer.Statistics.GetPlayerEffect<Sunburn>();
			if ((Object)(object)playerEffect != (Object)null && CraftingType.InteractiveType == InteractiveType.CRAFTING_BED_SHELTER)
			{
				playerEffect.Protected = true;
			}
		}
	}

	private bool AllPlayersOverLand()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = Singleton<StrandedWorld>.Instance.FindEndGameZone();
		RaycastHit val = default;
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Zone zone2 = StrandedWorld.GetZone(((Component)allPlayer.Movement).transform.position);
			if ((Object)(object)zone2 == (Object)null || (zone2.Biome == Zone.BiomeType.MISSION && (Object)(object)zone2 != (Object)(object)zone))
			{
				return false;
			}
			if (!Physics.Raycast(((Component)allPlayer.Movement).transform.position, Vector3.down, ref val, 256f, 1 << Layers.TERRAIN))
			{
				return false;
			}
			if (val.point.y < -5f)
			{
				return false;
			}
		}
		return true;
	}

	private bool AllPlayersInProximity(IPlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Game.Mode == 0)
		{
			return true;
		}
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Vector3 val = player.transform.position - allPlayer.transform.position;
			if (val.magnitude > 3f)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsFightingBoss()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = Singleton<StrandedWorld>.Instance.FindEndGameZone();
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (!BeamTeam.IsNullOrDestroyed(allPlayer))
			{
				Zone zone2 = StrandedWorld.GetZone(allPlayer.transform.position);
				if (zone2.Biome == Zone.BiomeType.MISSION && (Object)(object)zone != (Object)(object)zone2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool AnyPlayerKnockedOut()
	{
		foreach (Player allPlayer in PlayerRegistry.AllPlayers)
		{
			if (!BeamTeam.IsNullOrDestroyed(allPlayer) && allPlayer.Statistics.IsKnockedOut)
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyPlayerIsBusy()
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.Crafter.IsPlacing)
			{
				return true;
			}
			if (allPlayer.Lighter.IsLightingFire)
			{
				return true;
			}
			if (allPlayer.Skinner.IsSkinning)
			{
				return true;
			}
			if (allPlayer.Movement.CheckVehicle())
			{
				return true;
			}
			if (allPlayer.Movement.groundedAnimal)
			{
				return true;
			}
			if (allPlayer.IsBusy)
			{
				return true;
			}
		}
		return false;
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("LastTimeSlept", JSerializer.Serialize((object)_LastTimeSlept));
		jObject.AddField("canDrag", JSerializer.Serialize((object)CanDrag));
		jObject.AddField("canPickup", JSerializer.Serialize((object)CanPickUp));
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		_LastTimeSlept = JSerializer.Deserialize<DateTime>(data.GetField("LastTimeSlept"));
		JObject field = data.GetField("canDrag");
		if (field != null)
		{
			CanDrag = field.GetValue<bool>();
		}
		JObject field2 = data.GetField("canPickup");
		if (field2 != null)
		{
			CanPickUp = field2.GetValue<bool>();
		}
	}

	private static void SaveOnMaster(IPlayer invokingPlayer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			ReplicateSaveGameOnMasterMsg replicateSaveGameOnMasterMsg = new ReplicateSaveGameOnMasterMsg();
			replicateSaveGameOnMasterMsg.InvokingPlayer = invokingPlayer;
			replicateSaveGameOnMasterMsg.Post();
		}
		else
		{
			SaveManager.SaveGame(invokingPlayer);
		}
	}

	private void ReplicatedSleep_Sequence(float randomValue)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Sleep_Sequence(randomValue, Singleton<GameTime>.Instance.MilitaryTime), (Segment)4);
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateSleep_Sequence replicateSleep_Sequence = new ReplicateSleep_Sequence();
			replicateSleep_Sequence.Bed = this;
			replicateSleep_Sequence.RandomValue = randomValue;
			replicateSleep_Sequence.MilitaryTime = Singleton<GameTime>.Instance.MilitaryTime;
			replicateSleep_Sequence.Post();
		}
	}
}
