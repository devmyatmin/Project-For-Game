using System;
using System.Collections.Generic;
using System.Linq;
using Beam.Events;
using Beam.Obfuscated.Users;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using MEC;
using UnityEngine;

namespace Beam;

[RequireComponent(typeof(Movement))]
public class Statistics : MonoBehaviour, INeedPlayerComponent, ISaveable, IIdleUntilInitialized
{
	private class FoodRecord
	{
		public int AmountEaten;

		public int AmountSick;

		public DateTime LastTimeEaten;
	}

	private enum DebugCommand
	{
		Knockout,
		Bleed,
		Poison,
		Vomit
	}

	private class ReplicateApply : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public byte Command;

		public override void OnPeer()
		{
			Player.Statistics.Apply((DebugCommand)Command);
		}
	}

	private class ReplicateKnockOut : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		public override void OnPeer()
		{
			Player.Statistics.Knockout();
		}
	}

	private class ReplicateApplyDamage : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public float Damage;

		[Replicate]
		public string AttackCause;

		public override void OnPeer()
		{
			Player.Statistics.ApplyDamage(Damage, AttackCause);
		}
	}

	private class ReplicateApplyDamageDir : MultiplayerMessage
	{
		[Replicate]
		public IPlayer Player;

		[Replicate]
		public float Damage;

		[Replicate]
		public Vector3 Direction;

		[Replicate]
		public string AttackCause;

		public override void OnPeer()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Player.Statistics.ApplyDamage(Damage, Direction, AttackCause);
		}
	}

	public const float ONE_HEALTH_BAR = 100f;

	public const float HEALTH_POINTS_MAXIMUM = 700f;

	public const float CALORIE_POINTS_MAXIMUM = 700f;

	public const float THIRST_POINTS_MAXIMUM = 700f;

	private const float SLEEP_POINTS_MAXIMUM = 700f;

	private const float NUTRIENT_POINTS_MAXIMUM = 700f;

	private float CALORIE_POINTS_PER_DAY;

	private float THIRST_POINTS_PER_DAY;

	private float SLEEP_POINTS_PER_DAY;

	private float NUTRIENT_POINTS_PER_DAY;

	private const float DAYS_TO_DEPLETE_HUNGER = 2f;

	private const float DAYS_TO_DEPLETE_NUTRIENT = 1f;

	private const float DAYS_TO_DEPLETE_THIRST = 1f;

	private const float DAYS_TO_DEPLETE_SLEEP = 3f;

	private const float HARD_DAYS_TO_DEPLETE_HUNGER = 1f;

	private const float HARD_DAYS_TO_DEPLETE_NUTRIENT = 0.5f;

	private const float HARD_DAYS_TO_DEPLETE_THIRST = 0.5f;

	private const float HARD_DAYS_TO_DEPLETE_SLEEP = 1.5f;

	private const float DAY_TO_SLEEP_CALORIE_RATIO = 0.024f;

	public bool DisplayGUI;

	private CharacterController _controller;

	private IPlayerSpeech _speech;

	private float _calorieBurnFactor;

	private bool _extremeCalorieBurn;

	private float _nutrientBurnFactor;

	[SerializeField]
	private float _health = 700f;

	private float _calories = 700f;

	private float _fluids = 700f;

	private float _sleep = 700f;

	private float _fibres = 700f;

	private float _fats = 700f;

	private float _proteins = 700f;

	private float _stamina = 100f;

	private float _maxStamina = 100f;

	private bool _outOfBreath;

	private float _drownTime = 15f;

	private float _drownTimer;

	private float _oxygen = 100f;

	private float _maxOxygen = 100f;

	private float _oxygenRegainSpeed = 40f;

	private float _oxygenDrainSpeed = 3.5f;

	public float bodyTemperature = 98f;

	public float environmentTemperature = 75f;

	private float _underWaterTime;

	private bool _inWater;

	private float _velocity;

	private float _lastCough;

	private float _coughTimer;

	private bool _poll = true;

	private bool _isKnockedOut;

	private bool _invincible;

	[SerializeField]
	private ScenePlayerEffect[] _scenePlayerEffects;

	private CoroutineHandle _poopSequence;

	private CoroutineHandle _vomitSequence;

	private bool _oxygenRestored;

	private bool _drowningRestored;

	private HashSet<PlayerEffect> _playerEffects = new HashSet<PlayerEffect>(new PlayerEffectComparer());

	private readonly Dictionary<InteractiveType, FoodRecord> _foodRecords = new Dictionary<InteractiveType, FoodRecord>
	{
		{
			InteractiveType.FOOD_COCONUT_DRINKABLE,
			new FoodRecord
			{
				AmountSick = 3
			}
		},
		{
			InteractiveType.FOOD_COCONUT_HALF,
			new FoodRecord
			{
				AmountSick = 3
			}
		},
		{
			InteractiveType.FOOD_POTATO,
			new FoodRecord
			{
				AmountSick = 3
			}
		}
	};

	private IPlayer _player;

	private float _clientCopyLastUpdateTime;

	private bool _initialized;

	public float HealthPercent => _health / 700f;

	public float CaloriesPercent => _calories / 700f;

	public float FibresPercent => _fibres / 700f;

	public float FatsPercent => _fats / 700f;

	public float ProteinsPercent => _proteins / 700f;

	public float FluidsPercent => _fluids / 700f;

	public float SleepPercent => _sleep / 700f;

	public bool OutOfBreath => _outOfBreath;

	public float DrowningTimer => _drownTimer;

	public float Oxygen => _oxygen;

	public float MaxOxygen => _maxOxygen * _player.PlayerSkills.PhysicalMultiplier;

	public bool IsKnockedOut
	{
		get
		{
			return _isKnockedOut;
		}
		set
		{
			_isKnockedOut = value;
		}
	}

	public bool IsReviving { get; set; }

	public bool CanEat { get; set; } = true;

	public bool Invincible
	{
		get
		{
			if (!_invincible && !LevelLoader.IsLoadingScene())
			{
				return LevelLoader.IsServerJoinInProgress;
			}
			return true;
		}
		set
		{
			_invincible = value;
		}
	}

	public IEnumerable<PlayerEffect> PlayerEffects => _playerEffects;

	IPlayer INeedPlayerComponent.Player => _player;

	public bool IsInitialized => _initialized;

	public event Action<bool> KnockedOut;

	public event Action<PlayerEffect> PlayerEffectApplied;

	public event Action<PlayerEffect> PlayerEffectRemoved;

	public event Action<IndicatorDataBase> DirectionalDamage;

	public event Action Damaged;

	public event Action Poisoned;

	public event Action<float> Vomited;

	public event Action<float> Pooped;

	public event Action Dived;

	public event Action<float> Resurfaced;

	public event Action<float> StaminaUpdated;

	public event Action<float> DrowningUpdated;

	public event Action DrowningRestored;

	public event Action<float> OxygenUpdated;

	public event Action OxygenRestored;

	public event Action<InteractiveType, MeatProvenance> Ate;

	public event Action<InteractiveType, MeatProvenance> Drank;

	public TEffect GetPlayerEffect<TEffect>() where TEffect : class, IPlayerEffect
	{
		return _scenePlayerEffects.FirstOrDefault((ScenePlayerEffect effect) => ((object)effect).GetType() == typeof(TEffect)) as TEffect;
	}

	protected virtual void OnPlayerEffectApplied(PlayerEffect effect)
	{
		PlayerEffectApplied?.Invoke(effect);
	}

	protected virtual void OnPlayerEffectRemoved(PlayerEffect effect)
	{
		PlayerEffectRemoved?.Invoke(effect);
	}

	protected virtual void OnDirectionalDamage(IndicatorDataBase data)
	{
		DirectionalDamage?.Invoke(data);
	}

	protected virtual void OnDamaged()
	{
		Damaged?.Invoke();
	}

	protected virtual void OnPoisoned()
	{
		Poisoned?.Invoke();
	}

	protected virtual void OnVomited(float vomitTime)
	{
		Vomited?.Invoke(vomitTime);
	}

	protected virtual void OnPooped(float poopTime)
	{
		Pooped?.Invoke(poopTime);
	}

	protected virtual void OnDived()
	{
		Dived?.Invoke();
	}

	protected virtual void OnResurfaced(float underwaterTime)
	{
		Resurfaced?.Invoke(underwaterTime);
	}

	protected virtual void OnStaminaUpdated(float stamina)
	{
		StaminaUpdated?.Invoke(stamina);
	}

	protected virtual void OnDrowningUpdated(float value)
	{
		DrowningUpdated?.Invoke(value);
	}

	protected virtual void OnDrowningRestored()
	{
		DrowningRestored?.Invoke();
	}

	protected virtual void OnOxygenUpdated(float value)
	{
		OxygenUpdated?.Invoke(value);
	}

	protected virtual void OnOxygenRestored()
	{
		OxygenRestored?.Invoke();
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			Singleton<GameTime>.Instance.HourChanged += GameTime_HourChanged;
			EventManager.AddListener<FreezePlayerEffectsEvent>(EventManager_FreezePlayerEffects);
			EventManager.AddListener<DifficultyChangedEvent>(EventManager_DifficultyChangedEvent);
			SetDifficultyParams();
			((MonoBehaviour)this).InvokeRepeating("UpdateFoods_InvokeRepeating", 0f, 1f);
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_controller = _player.Movement.CharacterController;
		IPlayerSpeech playerSpeech = _player.PlayerSpeech;
		_speech = playerSpeech ?? NullObject<NullPlayerSpeech>.Instance;
		ClampClientVitals();
	}

	private void ClampClientVitals()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode) && BeamTeam.IsValid(_player) && _player.Id == 1)
		{
			float num = 0.15f;
			if (_health < 700f * num)
			{
				_health = 700f * num;
			}
			if (_calories < 700f * num)
			{
				_calories = 700f * num;
			}
			if (_fluids < 700f * num)
			{
				_fluids = 700f * num;
			}
		}
	}

	private void SetDifficultyParams()
	{
		bool flag = PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Hard;
		CALORIE_POINTS_PER_DAY = 700f / (flag ? 1f : 2f);
		NUTRIENT_POINTS_PER_DAY = 700f / (flag ? 0.5f : 1f);
		THIRST_POINTS_PER_DAY = 700f / (flag ? 0.5f : 1f);
		SLEEP_POINTS_PER_DAY = 700f / (flag ? 1.5f : 3f);
		Invincible = PlayerUtilities.GetServerGameDifficulty() == EGameDifficultyMode.Creative;
		StatusEffect.CreateEffects();
	}

	private void EventManager_FreezePlayerEffects(FreezePlayerEffectsEvent e)
	{
	}

	private void EventManager_DifficultyChangedEvent(DifficultyChangedEvent e)
	{
		SetDifficultyParams();
	}

	private void OnDestroy()
	{
		PlayerEffectApplied = null;
		PlayerEffectRemoved = null;
		DirectionalDamage = null;
		Damaged = null;
		Poisoned = null;
		Vomited = null;
		Dived = null;
		Resurfaced = null;
		StaminaUpdated = null;
		OxygenUpdated = null;
		DrowningUpdated = null;
		DrowningRestored = null;
		OxygenRestored = null;
		Ate = null;
		Drank = null;
		KnockedOut = null;
		if (Object.op_Implicit((Object)(object)Singleton<GameTime>.Instance))
		{
			Singleton<GameTime>.Instance.HourChanged -= GameTime_HourChanged;
		}
		EventManager.RemoveListener<FreezePlayerEffectsEvent>(EventManager_FreezePlayerEffects);
		EventManager.RemoveListener<DifficultyChangedEvent>(EventManager_DifficultyChangedEvent);
	}

	private void FixedUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized || LevelLoader.IsLoadingScene() || !_poll || Time.timeScale == 0f)
		{
			return;
		}
		Vector3 velocity = _controller.velocity;
		_velocity = Mathf.Clamp(velocity.magnitude, 0f, 20f);
		_inWater = _player.Movement.InWater;
		_extremeCalorieBurn = _player.Movement.sprinting;
		if (Invincible)
		{
			_player.Movement.CanSprint = true;
		}
		else
		{
			if (_isKnockedOut)
			{
				return;
			}
			float num = 0f;
			bool healthy = true;
			IEnumerator<PlayerEffect> enumerator = _playerEffects.GetEnumerator();
			while (enumerator.MoveNext())
			{
				PlayerEffect current = enumerator.Current;
				if (current.Duration != -1f && GameTime.GetHoursSince(current.StartedTime) > current.Duration)
				{
					RemoveStatusEffect(current);
					return;
				}
				_health += current.HealthPerHour * GameTime.DeltaTimeHour;
				_fluids += current.FluidsPerHour * GameTime.DeltaTimeHour;
				_calories += current.CaloriesPerHour * GameTime.DeltaTimeHour;
				num += current.TemperaturePerHour * GameTime.DeltaTimeHour;
				if (!current.PositiveEffect)
				{
					healthy = false;
				}
				if (_health <= 0f)
				{
					ReplicatedKnockout();
					return;
				}
			}
			enumerator.Dispose();
			UpdateVitals(healthy);
			UpdateServerClientCopy();
		}
	}

	private void UpdateVitals(bool healthy = true)
	{
		UpdatePlayerHunger();
		UpdatePlayerThirst();
		UpdateOxygen();
		UpdatePlayerHealth(healthy);
		UpdatePlayerSleep();
		UpdateStamina();
	}

	private void UpdatePlayerHealth(bool healthy)
	{
		if (_health <= 0f)
		{
			ReplicatedKnockout();
			return;
		}
		if (healthy && _calories >= 500f && _fluids >= 500f)
		{
			ApplyStatusEffect(StatusEffect.HEALTH_REGENERATION);
		}
		else
		{
			RemoveStatusEffect(StatusEffect.HEALTH_REGENERATION);
		}
		_health = Mathf.Clamp(_health, 0f, 700f);
	}

	private void UpdatePlayerHunger()
	{
		float num = (_extremeCalorieBurn ? 1.2f : 1f);
		_calorieBurnFactor = CALORIE_POINTS_PER_DAY * num;
		_nutrientBurnFactor = NUTRIENT_POINTS_PER_DAY * num;
		_calories -= _calorieBurnFactor * GameTime.DeltaTimeDay;
		_calories = Mathf.Clamp(_calories, 0f, 700f);
		if (Mathf.Approximately(_calories, 0f))
		{
			ApplyStatusEffect(StatusEffect.STARVATION);
		}
		else
		{
			RemoveStatusEffect(StatusEffect.STARVATION);
		}
		if (_fibres > 0f && _fats > 0f && _proteins > 0f)
		{
			RemoveStatusEffect(StatusEffect.MALNUTRITION);
		}
		else
		{
			ApplyStatusEffect(StatusEffect.MALNUTRITION);
		}
	}

	private void UpdatePlayerThirst()
	{
		float num = (_extremeCalorieBurn ? 1.2f : 1f);
		_calorieBurnFactor = THIRST_POINTS_PER_DAY * num;
		_fluids -= GameTime.DeltaTimeDay * _calorieBurnFactor;
		_fluids = Mathf.Clamp(_fluids, 0f, 700f);
		if (Mathf.Approximately(_fluids, 0f))
		{
			ApplyStatusEffect(StatusEffect.DEHYDRATION);
		}
		else
		{
			RemoveStatusEffect(StatusEffect.DEHYDRATION);
		}
	}

	private void UpdatePlayerSleep()
	{
		_player.Movement.CanSprint = true;
	}

	private void UpdateOxygen()
	{
		bool flag = Cameras.IsUnderwater(((Component)_player.PlayerCamera).transform);
		if (_isKnockedOut)
		{
			flag = false;
		}
		if (!flag)
		{
			if (_oxygen < _maxOxygen * _player.PlayerSkills.PhysicalMultiplier)
			{
				_oxygen += GameTime.RealDeltaTime * _oxygenRegainSpeed;
			}
			if (_underWaterTime > 0f)
			{
				OnResurfaced(_underWaterTime);
				_underWaterTime = 0f;
			}
		}
		else
		{
			if (Mathf.Approximately(_underWaterTime, 0f))
			{
				OnDived();
			}
			_underWaterTime += GameTime.RealDeltaTime;
			_oxygenRestored = false;
			if (_oxygen > 0f)
			{
				bool flag2 = HasStatusEffect(StatusEffect.BOOST_BREATH);
				_oxygen -= GameTime.RealDeltaTime * (_oxygenDrainSpeed * (flag2 ? 0.5f : 1f));
			}
		}
		if (_oxygen <= 0f)
		{
			_drowningRestored = false;
			_drownTimer += GameTime.RealDeltaTime;
			if (_drownTimer > _drownTime)
			{
				ReplicatedKnockout();
			}
			ApplyStatusEffect(StatusEffect.DROWNING);
		}
		else
		{
			_drownTimer = 0f;
			RemoveStatusEffect(StatusEffect.DROWNING);
		}
		_drownTimer = Mathf.Clamp(_drownTimer, 0f, _drownTime);
		if (_drownTimer > 0f && !Mathf.Approximately(_drownTimer, _drownTime))
		{
			float value = _drownTimer / _drownTime;
			OnDrowningUpdated(value);
		}
		else if (!_drowningRestored)
		{
			OnDrowningRestored();
			_drowningRestored = true;
		}
		_oxygen = Mathf.Clamp(_oxygen, 0f, _maxOxygen * _player.PlayerSkills.PhysicalMultiplier);
		if (_oxygen >= 0f && !Mathf.Approximately(_oxygen, _maxOxygen * _player.PlayerSkills.PhysicalMultiplier))
		{
			float value2 = _oxygen / (_maxOxygen * _player.PlayerSkills.PhysicalMultiplier);
			OnOxygenUpdated(value2);
		}
		else if (!_oxygenRestored)
		{
			OnOxygenRestored();
			_oxygenRestored = true;
		}
	}

	private void UpdateStamina()
	{
		float num = (_extremeCalorieBurn ? (-5f) : 5f);
		if (_fibres <= 0f || _fats <= 0f || _proteins <= 0f)
		{
			_stamina += num * 0.5f * GameTime.RealDeltaTime;
		}
		else
		{
			_stamina += num * GameTime.RealDeltaTime;
		}
		_stamina = Mathf.Clamp(_stamina, 0f, _maxStamina * _player.PlayerSkills.PhysicalMultiplier);
		if (_stamina == 0f)
		{
			_outOfBreath = true;
		}
		else if (_outOfBreath && _stamina >= _maxStamina * _player.PlayerSkills.PhysicalMultiplier - 0.01f)
		{
			_outOfBreath = false;
		}
		float stamina = _stamina / (_maxStamina * _player.PlayerSkills.PhysicalMultiplier);
		OnStaminaUpdated(stamina);
	}

	private void UpdateFoods_InvokeRepeating()
	{
		if (LevelLoader.IsLoadingScene() || LevelLoader.IsServerJoinInProgress)
		{
			return;
		}
		foreach (KeyValuePair<InteractiveType, FoodRecord> foodRecord in _foodRecords)
		{
			if (foodRecord.Value.AmountEaten > 0 && GameTime.GetMinutesSince(foodRecord.Value.LastTimeEaten) >= 10f)
			{
				Debug.Log((object)("Reset " + foodRecord.Key));
				foodRecord.Value.AmountEaten = 0;
			}
		}
	}

	private void UpdatePlayerTemperature(float effectTemperature)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (Singleton<AtmosphereTemperature>.Instance.GetOceanTemperatureForDepth(((Component)this).transform.position.y) <= 45f)
		{
			ApplyStatusEffect(StatusEffect.ILLNESS);
		}
	}

	private void UpdateServerClientCopy()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode) && _player != null && _player.Id == 1 && !(Time.unscaledTime <= _clientCopyLastUpdateTime + 30f))
		{
			_clientCopyLastUpdateTime = Time.unscaledTime;
			LevelLoader.Instance?.UpdateServerClientCopy(_player);
		}
	}

	public void SetHealth(float health)
	{
		_health = health;
		_health = Mathf.Min(_health, 700f);
	}

	public void AddHealth(float health)
	{
		_health += health;
		_health = Mathf.Min(_health, 700f);
	}

	public void AddOxygen(float oxygen)
	{
		_oxygen += oxygen;
		_oxygen = Mathf.Min(_oxygen, _maxOxygen * _player.PlayerSkills.PhysicalMultiplier);
	}

	private void SleepForHours(float sleptTime)
	{
		_sleep += SLEEP_POINTS_PER_DAY;
		_sleep = Mathf.Clamp(_sleep, 0f, 700f);
		_calories -= 16.8f * sleptTime;
		_calories = Mathf.Clamp(_calories, 0f, 700f);
		_fluids -= 16.8f * sleptTime;
		_fluids = Mathf.Clamp(_fluids, 0f, 700f);
		PlayerEffect healthStatusEffect = GetHealthStatusEffect();
		if (BeamTeam.IsValid(healthStatusEffect))
		{
			_health += healthStatusEffect.HealthPerHour * sleptTime;
		}
	}

	private PlayerEffect GetHealthStatusEffect()
	{
		foreach (PlayerEffect playerEffect in _playerEffects)
		{
			if (playerEffect.Name == "HEALTH_REGENERATION")
			{
				return playerEffect;
			}
		}
		return null;
	}

	private void GameTime_HourChanged(float hours)
	{
		SleepForHours(hours);
		RemoveStatusEffect(StatusEffect.ILLNESS);
	}

	public void ApplyDamage(float damage, Vector3 direction, string attackCause = "")
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!Invincible)
		{
			OnDirectionalDamage(new IndicatorDataBase(direction));
			ApplyDamage(damage, attackCause);
		}
	}

	public void ApplyDamage(float damage, string attackCause = "")
	{
		if (!Invincible)
		{
			_health -= damage;
			if (_health <= 10f)
			{
				ReplicatedKnockout();
			}
			else
			{
				OnDamaged();
			}
		}
	}

	public void Eat(InteractiveType type, MeatProvenance provenance, int calorieGain, int fibresGain, int fatGain, int proteinsGain, float fluidGain = 0f, float vomitChance = 0f, bool canCauseDiarrhoea = false)
	{
		((fluidGain > (float)calorieGain) ? Drank : Ate)?.Invoke(type, provenance);
		bool flag = (float)Random.Range(0, 100) < vomitChance;
		if (flag)
		{
			Vomit();
			_fluids -= 100f;
		}
		bool flag2 = canCauseDiarrhoea && CheckDiarrhoea(type);
		if (flag2 && _fluids > 0f)
		{
			Poop();
			_fluids -= 100f;
		}
		if (!(flag | flag2))
		{
			calorieGain = (int)((float)calorieGain * _player.PlayerSkills.CookingMultiplier);
			_calories += calorieGain;
			_fibres += fibresGain;
			_proteins += proteinsGain;
			_fats += fatGain;
			_fluids += fluidGain;
		}
	}

	private bool CheckDiarrhoea(InteractiveType type)
	{
		_foodRecords.TryGetValue(type, out var value);
		if (value != null)
		{
			value.AmountEaten++;
			value.LastTimeEaten = GameTime.Now;
			return value.AmountEaten >= value.AmountSick;
		}
		return false;
	}

	private void Poop(float poopTime = 3f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_poopSequence);
		_poopSequence = Timing.RunCoroutine(Poop_Sequence(poopTime));
	}

	private IEnumerator<float> Poop_Sequence(float poopTime)
	{
		CanEat = false;
		yield return Timing.WaitForSeconds(2f);
		++_player.Movement.IsBusy;
		OnPoisoned();
		OnPooped(poopTime);
		yield return Timing.WaitForSeconds(poopTime);
		ApplyStatusEffect(StatusEffect.DIARRHOEA);
		--_player.Movement.IsBusy;
		CanEat = true;
	}

	private void Vomit(float vomitTime = 3f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Timing.KillCoroutines(_vomitSequence);
		_vomitSequence = Timing.RunCoroutine(Vomit_Sequence(vomitTime));
	}

	private IEnumerator<float> Vomit_Sequence(float vomitTime)
	{
		CanEat = false;
		yield return Timing.WaitForSeconds(2f);
		++_player.Movement.IsBusy;
		OnPoisoned();
		OnVomited(vomitTime);
		yield return Timing.WaitForSeconds(vomitTime);
		--_player.Movement.IsBusy;
		CanEat = true;
	}

	private void ReplicatedKnockout()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateKnockOut replicateKnockOut = new ReplicateKnockOut();
			replicateKnockOut.Player = _player;
			replicateKnockOut.Post();
		}
		Knockout();
	}

	private void Knockout()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Timing.RunCoroutine(Knockout_Sequence());
	}

	private IEnumerator<float> Knockout_Sequence()
	{
		if (!Invincible && !_isKnockedOut)
		{
			KnockedOut?.Invoke(obj: true);
			IsKnockedOut = true;
			++_player.Movement.IsBusy;
			_player.Movement.ResetChecks();
			_player.transform.localScale = new Vector3(0f, 0f, 0f);
			if (_player.Crafter.IsPlacing)
			{
				_player.Crafter.CancelPlacing();
				yield return Timing.WaitForSeconds(0.2f);
			}
			if (_player.IsOwner)
			{
				_player.Character.CharacterFirstPerson.InstantiatePlayerRagdoll(((Component)this).transform.position);
			}
			PlayerRegistry.CheckAndHandleGameOver();
		}
	}

	public void Die()
	{
	}

	public void Revive(Transform RagdollRevivePos)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		KnockedOut?.Invoke(obj: false);
		IsKnockedOut = false;
		Vector3 val = RagdollRevivePos.position + RagdollRevivePos.TransformDirection(Vector3.up) * 4f;
		Vector3 val2 = RagdollRevivePos.TransformDirection(Vector3.down);
		RaycastHit val3 = default;
		if (Physics.Raycast(val, val2, ref val3, 10f, (1 << Layers.TERRAIN) | (1 << Layers.TERRAIN_OBJECTS)))
		{
			if (val3.point.y + 0.2f >= RagdollRevivePos.position.y)
			{
				_player.transform.position = new Vector3(RagdollRevivePos.position.x, val3.point.y + 1f, RagdollRevivePos.position.z);
			}
			else
			{
				_player.transform.position = RagdollRevivePos.position;
			}
		}
		else
		{
			_player.transform.position = RagdollRevivePos.position;
		}
		_player.transform.rotation = Quaternion.Euler(_player.transform.rotation.x, RagdollRevivePos.rotation.y, _player.transform.rotation.z);
		_player.transform.localScale = new Vector3(1f, 1f, 1f);
		_player.Movement.ResetChecks();
		_player.Character.PlayingEventAnimation = false;
		_player.PlayerCamera.CameraMovement.ResetCameraTarget();
		Reset_Vitals();
		IsReviving = false;
	}

	private void Reset_Vitals()
	{
		RemoveNegativeStatusEffects();
		int num = 100;
		_oxygen = _maxOxygen;
		if (_sleep < (float)num)
		{
			_sleep = num;
		}
		if (_health < (float)num)
		{
			_health = num;
		}
		if (_calories < (float)num)
		{
			_calories = num;
		}
		if (_fluids < (float)num)
		{
			_fluids = num;
		}
		UpdateVitals();
	}

	public bool HasStatusEffect(PlayerEffect effect)
	{
		return _playerEffects.Contains(effect);
	}

	public bool ApplyStatusEffect(PlayerEffect effect, bool force = false)
	{
		if (Invincible && !force)
		{
			return false;
		}
		bool flag = _playerEffects.Add(effect);
		if (flag)
		{
			effect.StartedTime = GameTime.Now;
			OnPlayerEffectApplied(effect);
			if (!effect.PositiveEffect)
			{
				EventManager.RaiseEvent(new AddStatsEvent(_player.Id, new Stats
				{
					StatusEffects = 1
				}));
			}
			bool isUnderwater = _player.Movement.IsUnderwater;
			int gender = _player.Gender;
			switch (effect.Name)
			{
			case "SUNBLOCK":
				_speech.Play("STATUS_SUNBLOCK", gender, isUnderwater);
				break;
			case "BOOST_BREATH":
				_speech.Play("STATUS_BREATH", gender, isUnderwater);
				break;
			case "BROKEN_BONES":
				RemoveStatusEffect(StatusEffect.SPLINT);
				_speech.Play("STATUS_BROKEN_BONES", gender, isUnderwater);
				break;
			case "POISON":
				_speech.Play("STATUS_POISON", gender, isUnderwater);
				OnPoisoned();
				break;
			case "ILLNESS":
				_speech.Play("STATUS_ILLNESS", gender, isUnderwater);
				break;
			case "HYPOTHERMIA":
				_speech.Play("STATUS_HYPOTHERMIA", gender, isUnderwater);
				break;
			case "DEHYDRATION":
				_speech.Play("STATUS_DEHYDRATION", gender, isUnderwater);
				break;
			case "STARVING":
				_speech.Play("STATUS_STARVING", gender, isUnderwater);
				break;
			case "BLEEDING":
				_speech.Play("STATUS_BLEEDING", gender, isUnderwater);
				break;
			case "TIRED":
				_speech.Play("STATUS_TIRED", gender, isUnderwater);
				break;
			case "DIARRHOEA":
				_speech.Play("STATUS_DIARRHOEA", gender, isUnderwater);
				break;
			case "MALNUTRITION":
				_speech.Play("STATUS_MALNUTRITION", gender, isUnderwater);
				break;
			}
		}
		return flag;
	}

	public bool RemoveStatusEffect(PlayerEffect effect)
	{
		bool flag = _playerEffects.Remove(effect);
		if (flag)
		{
			OnPlayerEffectRemoved(effect);
		}
		return flag;
	}

	private void RemoveNegativeStatusEffects()
	{
		HashSet<PlayerEffect> hashSet = new HashSet<PlayerEffect>();
		foreach (PlayerEffect playerEffect in _playerEffects)
		{
			if (!playerEffect.PositiveEffect)
			{
				hashSet.Add(playerEffect);
			}
		}
		foreach (PlayerEffect item in hashSet)
		{
			RemoveStatusEffect(item);
		}
	}

	public IList<string> GetStatusEffects()
	{
		return _playerEffects.Select((PlayerEffect effect) => effect.DisplayName).ToList();
	}

	public IList<string> GetNegativeStatusEffects()
	{
		return (from effect in _playerEffects
			where !effect.PositiveEffect
			select effect.DisplayName).ToList();
	}

	public IList<string> GetStatusEffectsWatch()
	{
		return _playerEffects.Select((PlayerEffect effect) => (effect.PositiveEffect ? "+ " : "- ") + effect.DisplayName).ToList();
	}

	private void Apply(DebugCommand command)
	{
		switch (command)
		{
		case DebugCommand.Knockout:
			Knockout();
			break;
		case DebugCommand.Bleed:
			ApplyStatusEffect(StatusEffect.BLEEDING);
			break;
		case DebugCommand.Poison:
			ApplyStatusEffect(StatusEffect.POISON);
			break;
		case DebugCommand.Vomit:
			Vomit();
			break;
		}
	}

	private void ReplicatedApply(DebugCommand command)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateApply replicateApply = new ReplicateApply();
			replicateApply.Player = _player;
			replicateApply.Command = (byte)command;
			replicateApply.Post();
		}
		Apply(command);
	}

	public void ReplicatedApplyDamage(float damage, Vector3 direction, string attackCause = "")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateApplyDamageDir replicateApplyDamageDir = new ReplicateApplyDamageDir();
			replicateApplyDamageDir.Player = _player;
			replicateApplyDamageDir.Damage = damage;
			replicateApplyDamageDir.Direction = direction;
			replicateApplyDamageDir.AttackCause = attackCause;
			replicateApplyDamageDir.Post();
		}
		ApplyDamage(damage, direction, attackCause);
	}

	public void ReplicatedApplyDamage(float damage, string attackCause = "")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateApplyDamage replicateApplyDamage = new ReplicateApplyDamage();
			replicateApplyDamage.Player = _player;
			replicateApplyDamage.Damage = damage;
			replicateApplyDamage.AttackCause = attackCause;
			replicateApplyDamage.Post();
		}
		ApplyDamage(damage, attackCause);
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("player_health", JSerializer.Serialize((object)_health));
		jObject.AddField("playerCalorie", JSerializer.Serialize((object)_calories));
		jObject.AddField("playerBodyfluid", JSerializer.Serialize((object)_fluids));
		jObject.AddField("playerBreath", JSerializer.Serialize((object)_oxygen));
		jObject.AddField("playerBodytemp", JSerializer.Serialize((object)bodyTemperature));
		jObject.AddField("Sleep", JSerializer.Serialize((object)_sleep));
		if (_scenePlayerEffects != null && _scenePlayerEffects.Length != 0)
		{
			JObject jObject2 = new JObject();
			for (int i = 0; i < _scenePlayerEffects.Length; i++)
			{
				ScenePlayerEffect scenePlayerEffect = _scenePlayerEffects[i];
				JObject jObject3 = SavingUtilities.Save(scenePlayerEffect);
				if (jObject3 != null)
				{
					jObject2.AddField(((object)scenePlayerEffect).GetType().Name, jObject3);
				}
			}
			jObject.AddField("ScenePlayerEffects", jObject2);
		}
		JObject jObject4 = new JObject();
		foreach (PlayerEffect playerEffect in _playerEffects)
		{
			JObject data = SavingUtilities.Save(playerEffect);
			jObject4.Add(data);
		}
		jObject.AddField("PlayerEffects", jObject4);
		return jObject;
	}

	public void Load(JObject data)
	{
		if (data != null && !data.IsNull())
		{
			float value = data.GetField("player_health").GetValue<float>();
			_health = value;
			float value2 = data.GetField("playerCalorie").GetValue<float>();
			_calories = value2;
			float value3 = data.GetField("playerBodyfluid").GetValue<float>();
			_fluids = value3;
			float value4 = data.GetField("playerBreath").GetValue<float>();
			_oxygen = value4;
			float value5 = data.GetField("playerBodytemp").GetValue<float>();
			bodyTemperature = value5;
			float value6 = data.GetField("Sleep").GetValue<float>();
			_sleep = value6;
			if (_scenePlayerEffects != null && _scenePlayerEffects.Length != 0)
			{
				JObject field = data.GetField("ScenePlayerEffects");
				if (field != null && !field.IsNull())
				{
					for (int i = 0; i < _scenePlayerEffects.Length; i++)
					{
						ScenePlayerEffect scenePlayerEffect = _scenePlayerEffects[i];
						JObject field2 = field.GetField(((object)scenePlayerEffect).GetType().Name);
						SavingUtilities.Load(scenePlayerEffect, field2);
					}
				}
			}
			JObject field3 = data.GetField("PlayerEffects");
			if (field3 != null && !field3.IsNull() && field3.Children.Count > 0)
			{
				for (int j = 0; j < field3.Children.Count; j++)
				{
					PlayerEffect playerEffect = new PlayerEffect();
					JObject data2 = field3.Children[j];
					if (SavingUtilities.Load(playerEffect, data2))
					{
						if (!ApplyStatusEffect(playerEffect, force: true))
						{
							Debug.Log((object)"Statistics::Load:: Effect already added.", (Object)(object)this);
						}
					}
					else
					{
						Debug.LogError((object)"Statistics::Load:: Error loading effect.", (Object)(object)this);
					}
				}
			}
			ClampClientVitals();
			Debug.Log((object)"Statistics::Load:: Loaded.", (Object)(object)this);
		}
		else
		{
			Debug.LogError((object)"Statistics::Load:: Save data was null or empty.", (Object)(object)this);
		}
	}
}
