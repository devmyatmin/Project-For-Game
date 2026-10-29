using System.Runtime.CompilerServices;
using Beam.Crafting;
using Beam.Obfuscated.Serialization;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using Rewired;
using UnityEngine;

namespace Beam;

[ExecuteInEditMode]
public class Player : MultiplayerBehaviour, IPlayer, ISaveable, IIdleUntilInitialized
{
	private PlayerCamera _playerCamera;

	private PlayerUI _playerUI;

	[SerializeField]
	private UiThing PlayerSpeechControllerView;

	[SerializeField]
	private Character _character;

	[SerializeField]
	private Movement _movement;

	[SerializeField]
	private Statistics _statistics;

	[SerializeField]
	private Inventory _inventory;

	[SerializeField]
	private Holder _holder;

	[SerializeField]
	private User _user;

	[SerializeField]
	private Interacter _interacter;

	[SerializeField]
	private Crafter _crafter;

	[SerializeField]
	private FireLighter _lighter;

	[SerializeField]
	private Skinner _skinner;

	[SerializeField]
	private Dragger _dragger;

	[SerializeField]
	private GameInputController _gameInput;

	[SerializeField]
	private WatchController _watch;

	[SerializeField]
	private Audio _audio;

	[SerializeField]
	private DeveloperMode _developerMode;

	[SerializeField]
	private PlayerSkills _playerSkills;

	[SerializeField]
	private PlayerExperience _playerExperience;

	[SerializeField]
	private HotkeyController _hotkeys;

	[SerializeField]
	private Achievements _achievements;

	[SerializeField]
	private WatchLoader _watchLoader;

	private bool _initialized;

	public int Id { get; set; }

	public int Gender { get; set; }

	public PlayerCamera PlayerCamera => _playerCamera;

	public PlayerUI PlayerUI => _playerUI;

	public PlayerSpeech PlayerSpeech => PlayerSpeechControllerView.Controller as PlayerSpeech;

	public Character Character => _character;

	public Movement Movement => _movement;

	public Statistics Statistics => _statistics;

	public Inventory Inventory => _inventory;

	public Holder Holder => _holder;

	public User User => _user;

	public Interacter Interacter => _interacter;

	public Crafter Crafter => _crafter;

	public FireLighter Lighter => _lighter;

	public Skinner Skinner => _skinner;

	public Dragger Dragger => _dragger;

	public GameInputController GameInput => _gameInput;

	public WatchController Watch => _watch;

	public Audio Audio => _audio;

	public DeveloperMode DeveloperMode => _developerMode;

	public PlayerSkills PlayerSkills => _playerSkills;

	public PlayerExperience PlayerExperience => _playerExperience;

	public HotkeyController Hotkeys => _hotkeys;

	public Achievements Achievements => _achievements;

	public bool IsInitialized => _initialized;

	public Peer Peer { get; set; }

	public IPlayerState MultiplayerState => GetMultiplayerState<IPlayerState>();

	public bool IsOwner => base.entity.IsOwner;

	public bool IsBusy
	{
		get
		{
			return MultiplayerState.IsBusy;
		}
		set
		{
			if (base.entity.IsOwner)
			{
				MultiplayerState.IsBusy = value;
			}
		}
	}

	public IMissionGoal CurrentMissionGoal { get; set; }

	public Rewired.Player Input { get; private set; }

	public override void Attached()
	{
		base.Attached();
		Id = ((MultiplayerTokens.ID)base.entity.AttachToken).Id;
		PlayerRegistry.Link(this);
	}

	public override void Detached()
	{
		base.Detached();
		PlayerRegistry.Unlink(this);
	}

	private void OnDestroy()
	{
		if (_watchLoader.IsValid())
		{
			BeamTeam.Destroy(_watchLoader.gameObject);
		}
	}

	public void SetStateAnimator()
	{
		CharacterRenderer characterThirdPerson = Character.CharacterThirdPerson;
		bool num = Gender == 1;
		Transform transform = characterThirdPerson.transform.Find("Stranded_Deep_Main_Character");
		if (num)
		{
			transform = characterThirdPerson.transform.Find("Stranded_Deep_Main_FemaleCharacter");
		}
		if ((bool)transform)
		{
			Animator component = transform.GetComponent<Animator>();
			if ((bool)component)
			{
				base.state.SetAnimator(component);
			}
		}
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			_character.SetPlayer(this);
			_movement.SetPlayer(this);
			_statistics.SetPlayer(this);
			_inventory.SetPlayer(this);
			_holder.SetPlayer(this);
			_crafter.SetPlayer(this);
			_gameInput.SetPlayer(this);
			_watch.SetPlayer(this);
			_audio.SetPlayer(this);
			_developerMode.SetPlayer(this);
			_playerSkills.SetPlayer(this);
			_playerExperience.SetPlayer(this);
			_hotkeys.SetPlayer(this);
			_achievements.SetPlayer(this);
			if (!IsOwner)
			{
				SetSpeechUI();
			}
			string text = "";
			text += $"Initialize Player {Id} Owner {IsOwner}";
			IIdleUntilInitialized[] componentsInChildren = GetComponentsInChildren<IIdleUntilInitialized>();
			foreach (IIdleUntilInitialized idleUntilInitialized in componentsInChildren)
			{
				text = text + "\n ֎ " + idleUntilInitialized.GetType().Name;
				idleUntilInitialized.Initialize();
			}
			componentsInChildren = _playerCamera.GetComponentsInChildren<IIdleUntilInitialized>();
			foreach (IIdleUntilInitialized idleUntilInitialized2 in componentsInChildren)
			{
				text = text + "\n ֎ " + idleUntilInitialized2.GetType().Name;
				idleUntilInitialized2.Initialize();
			}
			MultiplayerMng.Log(text, "attach/detach");
		}
	}

	public void SetInput(Rewired.Player input)
	{
		Input = input;
	}

	public void SetVibration(int motorIndex, float motorLevel, float duration)
	{
		if (IsOwner)
		{
			Input.SetVibration(motorIndex, motorLevel, duration);
		}
	}

	public void SetCamera(PlayerCamera playerCamera)
	{
		_playerCamera = playerCamera;
	}

	public void SetUI(PlayerUI playerUI)
	{
		_playerUI = playerUI;
		SetSpeechUI();
	}

	public void SetSpeechUI()
	{
		ViewAdapterBase viewAdapterBase;
		if (IsOwner)
		{
			if (PlayerSpeechControllerView == null || PlayerSpeechControllerView.Controller == null || PlayerSpeechControllerView.ViewPrefab == null)
			{
				return;
			}
			viewAdapterBase = Object.Instantiate(PlayerSpeechControllerView.ViewPrefab);
			viewAdapterBase.transform.SetParent(_playerUI.Canvas.transform, worldPositionStays: false);
			viewAdapterBase.transform.SetAsLastSibling();
			viewAdapterBase.transform.localPosition = Vector3.zero;
			viewAdapterBase.transform.localRotation = Quaternion.identity;
		}
		else
		{
			viewAdapterBase = PlayerRegistry.LocalPlayer.PlayerSpeech.View;
			if (viewAdapterBase == null)
			{
				return;
			}
		}
		PlayerSpeechControllerView.Controller.SetView(viewAdapterBase);
		((PlayerSpeech)PlayerSpeechControllerView.Controller).SetPlayer(this);
	}

	public static string GetAchivementSaveFieldName(int id)
	{
		return $"pId{id}_a";
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		JObject jObject2 = Beam.Serialization.SavingUtilities.Save(_movement);
		if (jObject2 != null)
		{
			jObject.AddField("Movement", jObject2);
		}
		JObject jObject3 = Beam.Serialization.SavingUtilities.Save(_statistics);
		if (jObject3 != null)
		{
			jObject.AddField("Statistics", jObject3);
		}
		JObject jObject4 = Beam.Serialization.SavingUtilities.Save(_inventory);
		if (jObject4 != null && !jObject4.IsNull())
		{
			jObject.AddField("Inventory", jObject4);
		}
		else
		{
			Debug.LogError("NULL INVENTORY!");
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = NotificationPriority.Immediate;
			localizedNotification.Duration = 8f;
			localizedNotification.PlayerId = 0;
			localizedNotification.MessageText.SetTerm("NULL INVENTORY! THIS SHOULD NOT HAPPEN, PLEASE CONTACT BEAM TEAM ON THE STEAM FORUMS.");
			localizedNotification.Raise();
		}
		JObject jObject5 = Beam.Serialization.SavingUtilities.Save(_holder);
		if (jObject5 != null)
		{
			jObject.AddField("Holder", jObject5);
		}
		JObject jObject6 = Beam.Serialization.SavingUtilities.Save(_crafter);
		if (jObject6 != null)
		{
			jObject.AddField("Crafter", jObject6);
		}
		JObject jObject7 = Beam.Serialization.SavingUtilities.Save(_playerSkills);
		if (jObject7 != null)
		{
			jObject.AddField("Skills", jObject7);
		}
		JObject jObject8 = Beam.Serialization.SavingUtilities.Save(_hotkeys);
		if (jObject8 != null)
		{
			jObject.AddField("Hotkeys", jObject8);
		}
		JObject data = Beam.Serialization.SavingUtilities.Save(_achievements);
		Beam.Obfuscated.Serialization.SavingUtilities.SaveToPrefs(GetAchivementSaveFieldName(Id), data);
		return jObject;
	}

	public void Load(JObject data)
	{
		JObject field = data.GetField("Crafter");
		Beam.Serialization.SavingUtilities.Load(_crafter, field);
		JObject field2 = data.GetField("Movement");
		if (field2.IsValid())
		{
			Beam.Serialization.SavingUtilities.Load(_movement, field2);
		}
		else if (Game.Mode.IsMultiplayer() && Id != 0)
		{
			this.SetTransformCloseTo(PlayerUtilities.GetPlayerFromId(0));
		}
		JObject field3 = data.GetField("Statistics");
		Beam.Serialization.SavingUtilities.Load(_statistics, field3);
		JObject field4 = data.GetField("Inventory");
		Beam.Serialization.SavingUtilities.Load(_inventory, field4);
		JObject field5 = data.GetField("Holder");
		Beam.Serialization.SavingUtilities.Load(_holder, field5);
		JObject field6 = data.GetField("Skills");
		Beam.Serialization.SavingUtilities.Load(_playerSkills, field6);
		JObject field7 = data.GetField("Hotkeys");
		Beam.Serialization.SavingUtilities.Load(_hotkeys, field7);
		JObject data2 = Beam.Obfuscated.Serialization.SavingUtilities.LoadFromPrefs(GetAchivementSaveFieldName(Id));
		_achievements.Load(data2);
	}

	[SpecialName]
	Transform IPlayer.get_transform()
	{
		return base.transform;
	}
}
