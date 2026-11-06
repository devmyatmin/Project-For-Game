using System.Collections.Generic;
using Beam.Crafting;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Constructing_FUEL_STILL_BOILER : Constructing, IInteractable, IBase, IHasCraftingType, IHaveAttribute
{
	public class ReplicateBoil : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab FuelBoiler;

		[Replicate]
		public int Units;

		public override void OnPeer()
		{
			SaveablePrefab fuelBoiler = FuelBoiler;
			if (fuelBoiler != null)
			{
				((Component)fuelBoiler).GetComponent<Constructing_FUEL_STILL_BOILER>().Boil(Units);
			}
		}
	}

	private const float DECREMENT_INTERVAL_HOURS = 2f;

	private const int DECREMENT_UNIT = 1;

	private const int MAX_MASH = 4;

	[PREFAB_PROCESSOR_GetComponentInChildren]
	[SerializeField]
	private Constructing_FUEL_STILL_COLLECTOR _collector;

	private Construction_CAMPFIRE _fire;

	private CoroutineHandle _boilingSequence;

	private float _boilingTimer;

	private int _mash;

	[SerializeField]
	private ParticleSystem _smokeParticle;

	[HideInInspector]
	public UnityEvent StartedBoiling;

	[HideInInspector]
	public UnityEvent StoppedBoiling;

	[HideInInspector]
	public FloatUnityEvent Boiled;

	[HideInInspector]
	public UnityEvent Refilled;

	public IconType Type => IconType.Fuel;

	public bool CanInteract { get; set; } = true;

	public Constructing_FUEL_STILL_COLLECTOR Collector => _collector;

	public float GetValue()
	{
		return (float)_mash / 4f;
	}

	public string GetInteractionDescription(int playerId, IBase obj)
	{
		if (_mash == 4 || BeamTeam.IsNullOrDestroyed(obj))
		{
			return string.Empty;
		}
		if (obj.CraftingType.InteractiveType == InteractiveType.FOOD_POTATO)
		{
			return "ITEM_INTERACTION_DESCRIPTION_ADD_MASH";
		}
		return string.Empty;
	}

	public override void Place()
	{
		base.Place();
		Construction_CAMPFIRE construction = Structure.GetConstruction<Construction_CAMPFIRE>();
		Initialize(construction);
	}

	private void Initialize(Construction_CAMPFIRE fire)
	{
		_fire = fire;
		_fire.FireStarted += Fire_Started;
		_fire.FireStopped += Fire_Stopped;
		_fire.FireBurned += Fire_Burned;
		if (_fire.IsBurning)
		{
			Fire_Started();
		}
	}

	private void OnDestroy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_boilingSequence);
		((UnityEventBase)StartedBoiling).RemoveAllListeners();
		((UnityEventBase)StoppedBoiling).RemoveAllListeners();
		((UnityEventBase)Boiled).RemoveAllListeners();
		((UnityEventBase)Refilled).RemoveAllListeners();
		StartedBoiling = null;
		StoppedBoiling = null;
		Boiled = null;
		Refilled = null;
		if ((Object)(object)_fire != (Object)null)
		{
			_fire.FireStarted -= Fire_Started;
			_fire.FireStopped -= Fire_Stopped;
			_fire.FireBurned -= Fire_Burned;
		}
	}

	private void Fire_Started()
	{
		StartBoiling();
	}

	private void Fire_Stopped()
	{
		StopBoiling();
	}

	private void Fire_Burned(float hours)
	{
		float num = hours / 2f;
		int num2 = Mathf.FloorToInt(num);
		ReplicatedBoil(num2);
		if (_mash > 0)
		{
			float num3 = num - (float)num2;
			_boilingTimer = num3 * 2f;
		}
	}

	private void StartBoiling()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (_mash != 0 && !_boilingSequence.IsRunning)
		{
			if ((Object)(object)_smokeParticle != (Object)null)
			{
				_smokeParticle.Play();
			}
			if (MPExtensions.IsMaster(Game.Mode))
			{
				_boilingSequence = Timing.RunCoroutine(Boil_Sequence());
			}
			UnityEvent startedBoiling = StartedBoiling;
			if (startedBoiling != null)
			{
				startedBoiling.Invoke();
			}
		}
	}

	private void StopBoiling()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (_boilingSequence.IsRunning)
		{
			if ((Object)(object)_smokeParticle != (Object)null)
			{
				_smokeParticle.Stop();
			}
			if (MPExtensions.IsMaster(Game.Mode))
			{
				Timing.KillCoroutines(_boilingSequence);
			}
			UnityEvent stoppedBoiling = StoppedBoiling;
			if (stoppedBoiling != null)
			{
				stoppedBoiling.Invoke();
			}
			_boilingTimer = 0f;
		}
	}

	private IEnumerator<float> Boil_Sequence()
	{
		while (true)
		{
			_boilingTimer += GameTime.DeltaTimeHour;
			if (_boilingTimer > 2f)
			{
				_boilingTimer = 0f;
				ReplicatedBoil(1);
			}
			yield return 0f;
		}
	}

	private void Boil(int units)
	{
		int num = Mathf.Min(_mash, units);
		if (num > 0)
		{
			_mash -= num;
			_mash = Mathf.Clamp(_mash, 0, 4);
			if (_mash == 0)
			{
				StopBoiling();
			}
			((UnityEvent<float>)Boiled)?.Invoke((float)num);
		}
	}

	public void ForceRefuel(float units)
	{
		((UnityEvent<float>)Boiled)?.Invoke(units);
	}

	private void Refill()
	{
		if (_mash != 4)
		{
			_mash++;
			_mash = Mathf.Clamp(_mash, 0, 4);
			UnityEvent refilled = Refilled;
			if (refilled != null)
			{
				refilled.Invoke();
			}
			if (_fire.IsBurning)
			{
				Fire_Started();
			}
		}
	}

	public bool Interact(IPlayer player)
	{
		return false;
	}

	public bool InteractWithObject(IPlayer player, IBase obj)
	{
		if (_mash == 4)
		{
			return false;
		}
		if (obj.CraftingType.InteractiveType == InteractiveType.FOOD_POTATO)
		{
			player.Holder.ReleaseAndDestroyCurrent();
			Refill();
			return true;
		}
		return Interact(player);
	}

	public void ReplicatedBoil(int units)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateBoil replicateBoil = new ReplicateBoil();
			replicateBoil.FuelBoiler = ((Component)this).GetComponent<SaveablePrefab>();
			replicateBoil.Units = units;
			replicateBoil.Post();
		}
		Boil(units);
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("Mash", _mash);
		jObject.AddField("Fire", _fire.ReferenceId.ToString());
		JObject jObject2 = SavingUtilities.Save(_collector);
		if (jObject2 != null)
		{
			jObject.AddField("Collector", jObject2);
		}
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		int value = data.GetField("Mash").GetValue<int>();
		_mash = value;
		JObject field = data.GetField("Collector");
		if (field != null)
		{
			SavingUtilities.Load(_collector, field);
		}
		MiniGuid fireReferenceId = Beam.Utilities.ExtensionMethods.ToMiniGuid(data.GetField("Fire").GetValue<string>());
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			Construction_CAMPFIRE construction_CAMPFIRE = Structure.transform.FindObject<Construction_CAMPFIRE>(fireReferenceId);
			if ((Object)(object)construction_CAMPFIRE != (Object)null)
			{
				Initialize(construction_CAMPFIRE);
			}
			else
			{
				Debug.LogError((object)"Constructing_FUEL_STILL_BOILER::Load:: Error finding fire.");
			}
		});
	}
}
