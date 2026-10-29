using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Beam;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class InteractiveObject_PLAYER_RAGDOLL : InteractiveObject, IRagdoll, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, IHaveAttribute
{
	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	[SerializeField]
	private GameObject _playerModel;

	[PREFAB_PROCESSOR_GetComponentsInChildren]
	[SerializeField]
	private RagdollChild[] _ragdollChildren;

	[SerializeField]
	private RagdollHelper _ragdollHelper;

	public Transform RagdollCameraJoint;

	public Transform RagdollRevivePos;

	private IPlayer _player;

	IconType IHaveAttribute.Type => _type;

	public RagdollChild[] RagdollChildren => _ragdollChildren;

	public RagdollHelper RagdollHelper => _ragdollHelper;

	public override bool IsValidPrefab => true;

	float IHaveAttribute.GetValue()
	{
		return base.HealthPoints / base.OriginalHealthPoints;
	}

	public override string GetInteractionDescription(int playerId, IBase obj)
	{
		if (obj.IsNullOrDestroyed() || _player.Statistics.IsReviving || _player.Id == playerId)
		{
			return string.Empty;
		}
		if (IsRevivable(obj))
		{
			return "ITEM_INTERACTION_DESCRIPTION_REVIVE";
		}
		return string.Empty;
	}

	public override void Attached()
	{
		base.Attached();
		Player player = (Player)PlayerUtilities.GetPlayerFromId(MiniGuid.PLAYER_ID_FROM_RAGDOLL(base.ReferenceId));
		if (player.IsValid())
		{
			Initialize(player);
		}
	}

	public void Initialize(IPlayer player)
	{
		_player = player;
		_ragdollHelper.player = _player;
		_ragdollHelper.ragdolled = true;
		player.Character.CharacterFirstPerson.AnimController.transform.CopyTransformsToRecursive(base.transform);
		player.PlayerCamera.CameraMovement.SetCameraTarget(RagdollCameraJoint);
		if (PlayerRegistry.AllPlayers.Count > 1)
		{
			StartCoroutine(Decay());
		}
		EventManager.AddListener<PlayerDisconnectedEvent>(PlayerDisconnected);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(PlayerDisconnected);
	}

	private IEnumerator Decay()
	{
		while (_ragdollHelper.ragdolled)
		{
			_player.transform.position = base.transform.position;
			base.HealthPoints -= 0.25f * Time.deltaTime;
			if (base.HealthPoints <= 0f)
			{
				Die();
				break;
			}
			yield return null;
		}
	}

	public override bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (obj.IsNullOrDestroyed() || _player.Statistics.IsReviving)
		{
			return Interact(player);
		}
		if (!_player.Statistics.IsReviving && IsRevivable(obj))
		{
			_player.Statistics.IsReviving = true;
			player.Holder.ReleaseAndDestroyCurrent();
			Revive();
			return Interact(player);
		}
		return Interact(player);
	}

	private bool IsRevivable(IBase obj)
	{
		return obj.CraftingType.InteractiveType == InteractiveType.MEDICAL_BANDAGE;
	}

	private void Revive()
	{
		StartCoroutine(ReviveAnimation());
	}

	private IEnumerator ReviveAnimation()
	{
		float timer = 0f;
		float reviveTime = 3f;
		_ragdollHelper.ragdolled = false;
		while (timer < reviveTime)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		_player.Statistics.Revive(RagdollRevivePos.transform);
		MultiplayerMng.DestroyMultiplayerEntity(base.gameObject);
	}

	private void Die()
	{
		base.CanInteract = false;
		base.CanDamage = false;
		_player.Statistics.Die();
	}

	private void PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (Game.Mode.IsServer() && _player.Id == 0 && disconnectedEvent.PlayerId == 1)
		{
			PlayerRegistry.GameOver();
		}
		if (_player.Id != disconnectedEvent.PlayerId)
		{
			return;
		}
		try
		{
			if (!base.gameObject.IsNullOrDestroyed())
			{
				SaveablePrefab component = base.gameObject.GetComponent<SaveablePrefab>();
				if (!component.IsNullOrDestroyed())
				{
					MultiplayerMng.Destroy(component);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject data = this.SaveRagdoll();
		jObject.AddField("Ragdolls", data);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject ragdollData = data.GetField("Ragdolls");
		if (ragdollData != null)
		{
			LevelLoader.CurrentLoader.PhysicalLoading.Add(delegate
			{
				this.LoadRagdoll(ragdollData);
			});
		}
	}

	[SpecialName]
	GameObject IBase.get_gameObject()
	{
		return base.gameObject;
	}
}
