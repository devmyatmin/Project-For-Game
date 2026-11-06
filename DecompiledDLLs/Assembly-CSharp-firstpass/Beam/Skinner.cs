using System.Collections.Generic;
using Beam.Crafting;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam;

public class Skinner : MonoBehaviour, IGameInputHandler, INeedPlayerComponent
{
	private class ReplicateStartSkinning : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public ISkinnable Skinnable;

		public override void OnPeer()
		{
			Player.Skinner.StartSkinning(Skinnable);
		}
	}

	private class ReplicateHandleMeat : I_MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[I_Replicate]
		public InteractiveObject Meat;

		[Replicate]
		public byte Provenance;

		public override void OnPeer()
		{
			Player.Skinner.HandleMeat(Meat, (MeatProvenance)Provenance);
		}
	}

	[SerializeField]
	private GameObject _bloodPrefab;

	[SerializeField]
	private Transform _bloodParent;

	[SerializeField]
	private InteractiveObject _rawhidePrefab;

	[SerializeField]
	private InteractiveObject _smallMeatPrefab;

	[SerializeField]
	private InteractiveObject _mediumMeatPrefab;

	[SerializeField]
	private InteractiveObject _largeMeatPrefab;

	[SerializeField]
	private AudioClip[] _skinLeatherAudioClips;

	[SerializeField]
	private AudioClip[] _skinCrabAudioClips;

	[SerializeField]
	private AudioClip[] _skinMeatAudioClips;

	[SerializeField]
	private AudioClip[] _successAudioClips;

	[SerializeField]
	private AudioClip[] _successAudioClipsFemale;

	[SerializeField]
	private AudioClip[] _failureAudioClips;

	[SerializeField]
	private SceneGameAction _action;

	private IPlayer _player;

	private IGameInputController _controller;

	private ISkinnable _currentSkinnable;

	private float _successAudioTimer;

	private CraftingType _toolRequired = new CraftingType(AttributeType.Refined, InteractiveType.TOOLS_KNIFE);

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsSkinning => !BeamTeam.IsNullOrDestroyed(_currentSkinnable);

	public ISkinnable CurrentSkinnable => _currentSkinnable;

	public void Initialize(IGameInputController controller)
	{
		_controller = controller;
		_controller.Interacter.RequestGameActions += Interacter_RequestGameActions;
		_successAudioTimer = Time.time;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
	}

	private void OnDestroy()
	{
		if (BeamTeam.IsValid(_controller))
		{
			_controller.Interacter.RequestGameActions -= Interacter_RequestGameActions;
		}
	}

	private bool IsSkinnedByTheOtherPlayer(ISkinnable skinnable)
	{
		if (BeamTeam.IsNullOrDestroyed(skinnable))
		{
			return false;
		}
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			if (allPlayer.Id != _player.Id && allPlayer.Skinner._currentSkinnable == skinnable)
			{
				return true;
			}
		}
		return false;
	}

	private void Interacter_RequestGameActions(Interacter sender, IBase obj)
	{
		if (!BeamTeam.IsNullOrDestroyed(obj) && !BeamTeam.IsValid(_currentSkinnable) && ((!BeamTeam.IsNullOrDestroyed(_player.Holder.CurrentObject) && _player.Holder.CurrentObject.CraftingType.Equals(_toolRequired)) | (_player.Inventory.GetSlotStorage().Get<IPickupable>(_toolRequired) != null)) && _player.Movement.grounded)
		{
			ISkinnable skinnable = obj.gameObject.GetComponent<BaseObject>() as ISkinnable;
			if (!BeamTeam.IsNullOrDestroyed(skinnable) && skinnable.CanSkin && !IsSkinnedByTheOtherPlayer(skinnable))
			{
				sender.AddGameAction(_action);
			}
		}
	}

	public void CheckInput()
	{
	}

	public void Skinning_GameAction(IBase obj)
	{
		ISkinnable skinnable = obj.gameObject.GetComponent<BaseObject>() as ISkinnable;
		if (!BeamTeam.IsNullOrDestroyed(skinnable))
		{
			SkinningRewardType rewardType = (IsRawhideAvailable(skinnable) ? SkinningRewardType.Rawhide : skinnable.MeatType);
			IPickupable rewardPrefab = GetRewardPrefab(rewardType);
			if (!_player.Holder.Storage.CanPush(rewardPrefab))
			{
				LocalizedNotification.Post(_player, NotificationPriority.Normal, 4f, "SKINNER_NTF_NO_ROOM_FOR_REWARD", rewardPrefab.DisplayName);
			}
			else if (!IsSkinnedByTheOtherPlayer(skinnable))
			{
				ReplicatedStartSkinning(skinnable);
			}
		}
	}

	private void StartSkinning(ISkinnable skinnable)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Skinning_Sequence(skinnable));
	}

	private void FinishSkinning()
	{
		_currentSkinnable = null;
		_player.Character.SetBool("IsSkinning", val: false);
		_player.Character.ShowCurrentItem();
		if (_player.IsOwner)
		{
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: true, 0);
			_controller.CurrentHandler = null;
			_controller.Raycaster.Reset();
		}
	}

	private IEnumerator<float> Skinning_Sequence(ISkinnable skinnable)
	{
		_currentSkinnable = skinnable;
		if (MPExtensions.IsMultiplayer(Game.Mode) && IsSkinnedByTheOtherPlayer(skinnable))
		{
			PlayerUtilities.GetPlayerFromId(1).Skinner.FinishSkinning();
			if (_player.Id == 1)
			{
				yield break;
			}
		}
		if (_player.IsOwner)
		{
			_controller.CurrentHandler = this;
			RewiredUtilities.SetAndCacheMapsEnabled(_player.Input, state: false, 0);
			RewiredUtilities.SetCursorEnabled(_player.Input, enabled: false);
			_player.PlayerUI.Refresh(RefreshUIMode.Clear);
		}
		_player.Character.HideCurrentItem();
		GameObject knifeProxy = _player.Holder.Proxies.GetProxy(new CraftingType(AttributeType.Refined, InteractiveType.TOOLS_KNIFE));
		knifeProxy.SetActive(true);
		bool rawhideAvailable = IsRawhideAvailable(skinnable);
		AudioClip[] clips;
		if (skinnable.CraftingType.InteractiveType != InteractiveType.ANIMALS_CRAB)
		{
			clips = (rawhideAvailable ? _skinLeatherAudioClips : _skinMeatAudioClips);
		}
		else
		{
			clips = _skinCrabAudioClips;
		}
		AudioManager.GetAudioPlayer().Play3D(clips, skinnable.transform.position, AudioMixerChannels.FX);
		_player.Character.SetBool("IsSkinning", val: true);
		_player.Character.CrossFade("SKINNING_PRIMARY", 0.1f, 3);
		_player.Character.CrossFade("SKINNING_PRIMARY", 0.1f, 7);
		yield return Timing.WaitUntilDone(DoMultiplayerArbitration(0.33f));
		if (BeamTeam.IsNullOrDestroyed(_currentSkinnable))
		{
			knifeProxy.SetActive(false);
			FinishSkinning();
			yield break;
		}
		GameObject val = Object.Instantiate<GameObject>(_bloodPrefab);
		val.transform.SetParent(_bloodParent, false);
		val.transform.localPosition = Vector3.zero;
		yield return Timing.WaitUntilDone(DoMultiplayerArbitration(2.67f));
		knifeProxy.SetActive(false);
		if (BeamTeam.IsNullOrDestroyed(_currentSkinnable))
		{
			FinishSkinning();
			yield break;
		}
		InteractiveType interactiveType = (rawhideAvailable ? InteractiveType.CRAFTING_RAWHIDE : InteractiveType.FOOD_MEAT);
		GameObject rewardProxy = _player.Holder.Proxies.GetProxy(new CraftingType(AttributeType.None, interactiveType));
		rewardProxy.SetActive(true);
		yield return Timing.WaitUntilDone(DoMultiplayerArbitration(1.2f));
		rewardProxy.SetActive(false);
		if (BeamTeam.IsNullOrDestroyed(_currentSkinnable))
		{
			FinishSkinning();
			yield break;
		}
		Skin(skinnable, succeeded: true);
		FinishSkinning();
	}

	private CoroutineHandle DoMultiplayerArbitration(float duration)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return BeamTiming.BreakedWait(duration, () =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return MPExtensions.IsMultiplayer(Game.Mode) && _player.Id == 1 && IsSkinnedByTheOtherPlayer(_currentSkinnable);
		});
	}

	private void Skin(ISkinnable skinnable, bool succeeded)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (succeeded)
		{
			if (skinnable.Skinned < skinnable.LeatherQuantity)
			{
				if (MPExtensions.IsMaster(Game.Mode))
				{
					CreateLeather(skinnable);
				}
			}
			else if (MPExtensions.IsMaster(Game.Mode))
			{
				CreateMeat(skinnable);
			}
			skinnable.Skinned++;
			if (skinnable.Skinned >= skinnable.Quantity + skinnable.LeatherQuantity)
			{
				skinnable.Skin(_player);
			}
		}
		else
		{
			skinnable.FailedAttempts++;
			if (skinnable.FailedAttempts >= 3)
			{
				if (_player.IsOwner)
				{
					CreateScraps();
				}
				skinnable.Skin(_player);
			}
		}
		PlayAudio(succeeded);
	}

	private void PlayAudio(bool succeeded)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (succeeded)
		{
			if (Time.time - _successAudioTimer > 30f)
			{
				AudioManager.GetAudioPlayer().Play3D((_player.Gender == 0) ? _successAudioClips : _successAudioClipsFemale, _player.transform.position, AudioMixerChannels.Voice);
				_successAudioTimer = Time.time;
			}
		}
		else
		{
			AudioManager.GetAudioPlayer().Play3D(_failureAudioClips, _player.transform.position, AudioMixerChannels.Voice);
		}
	}

	private void CreateScraps()
	{
	}

	private void CreateLeather(ISkinnable skinnable)
	{
		InteractiveObject obj = MultiplayerMng.Instantiate<InteractiveObject>(_rawhidePrefab.PrefabId, MiniGuid.New());
		_player.Holder.Storage.ReplicatedPush(obj);
	}

	private void CreateMeat(ISkinnable skinnable)
	{
		InteractiveObject meat = MultiplayerMng.Instantiate<InteractiveObject>(GetRewardPrefab(skinnable.MeatType).PrefabId, MiniGuid.New());
		MeatProvenance provenance = GetProvenance(skinnable.CraftingType.InteractiveType);
		ReplicatedHandleMeat(meat, provenance);
	}

	private void HandleMeat(InteractiveObject meat, MeatProvenance provenance)
	{
		InteractiveObject_FOOD interactiveObject_FOOD = meat as InteractiveObject_FOOD;
		if ((Object)(object)interactiveObject_FOOD != (Object)null)
		{
			interactiveObject_FOOD.Provenance = provenance;
		}
		_player.Holder.Storage.Push(meat);
	}

	private bool IsRawhideAvailable(ISkinnable skinnable)
	{
		if (skinnable.LeatherQuantity > 0)
		{
			return skinnable.Skinned < skinnable.LeatherQuantity;
		}
		return false;
	}

	private InteractiveObject GetRewardPrefab(SkinningRewardType rewardType)
	{
		return rewardType switch
		{
			SkinningRewardType.Small => _smallMeatPrefab, 
			SkinningRewardType.Medium => _mediumMeatPrefab, 
			SkinningRewardType.Large => _largeMeatPrefab, 
			SkinningRewardType.Rawhide => _rawhidePrefab, 
			_ => null, 
		};
	}

	private MeatProvenance GetProvenance(InteractiveType type)
	{
		return type switch
		{
			InteractiveType.ANIMALS_CRAB => MeatProvenance.Crab, 
			InteractiveType.ANIMALS_BOAR => MeatProvenance.Boar, 
			InteractiveType.ANIMALS_SNAKE => MeatProvenance.Snake, 
			InteractiveType.ANIMALS_BAT => MeatProvenance.Bat, 
			InteractiveType.ANIMALS_SEAGULL => MeatProvenance.Seagull, 
			InteractiveType.ANIMALS_FISH => MeatProvenance.Fish, 
			InteractiveType.ANIMALS_SHARK => MeatProvenance.Fish, 
			InteractiveType.ANIMALS_STINGRAY => MeatProvenance.Fish, 
			_ => MeatProvenance.Other, 
		};
	}

	private void ReplicatedStartSkinning(ISkinnable skinnable)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateStartSkinning replicateStartSkinning = new ReplicateStartSkinning();
			replicateStartSkinning.Player = _player;
			replicateStartSkinning.Skinnable = skinnable;
			replicateStartSkinning.Post();
		}
		StartSkinning(skinnable);
	}

	private void ReplicatedHandleMeat(InteractiveObject meat, MeatProvenance provenance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateHandleMeat replicateHandleMeat = new ReplicateHandleMeat();
			replicateHandleMeat.Player = _player;
			replicateHandleMeat.Meat = meat;
			replicateHandleMeat.Provenance = (byte)provenance;
			replicateHandleMeat.Post();
		}
		HandleMeat(meat, provenance);
	}
}
