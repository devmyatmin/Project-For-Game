using Beam.Events;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Smoking : MonoBehaviour
{
	private class ReplicateSetSmoked : MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab SaveablePrefab;

		public override void OnPeer()
		{
			((Component)SaveablePrefab).GetComponent<Smoking>().SetSmoked();
		}
	}

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject_FOOD _food;

	[SerializeField]
	private float _smokingHours;

	[SerializeField]
	private PlayerSkills.ExpSource _skillExpSource = PlayerSkills.ExpSource.FOOD_COOKED_SMALL;

	[SerializeField]
	[HideInInspector]
	private float _originalSmokingHours;

	private bool _isSmoked;

	private int _playerChefID = -1;

	public UnityEvent Smoked;

	public InteractiveObject_FOOD Food => _food;

	public bool IsSmoked => _isSmoked;

	public bool IsBeingSmoked { get; set; }

	public int PlayerChefID
	{
		get
		{
			return _playerChefID;
		}
		set
		{
			_playerChefID = value;
		}
	}

	private void Start()
	{
		EventManager.AddListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<PlayerDisconnectedEvent>(EventManager_PlayerDisconnected);
	}

	private void EventManager_PlayerDisconnected(PlayerDisconnectedEvent disconnectedEvent)
	{
		if (_playerChefID == disconnectedEvent.PlayerId)
		{
			_playerChefID = -1;
		}
	}

	public void Smoke()
	{
		SmokeManual(GameTime.DeltaTimeHour);
	}

	public void SmokeManual(float hours)
	{
		if (!_isSmoked)
		{
			_smokingHours -= hours;
			_smokingHours = Mathf.Clamp(_smokingHours, 0f, _originalSmokingHours);
			SetSmokingInShader();
			if (_smokingHours <= 0f)
			{
				ReplicatedSetSmoked();
			}
		}
	}

	private void SetSmoked()
	{
		if (!_isSmoked)
		{
			OnSmoked();
			if (PlayerChefID != -1)
			{
				EventManager.RaiseEvent(new ExperienceEvent(PlayerChefID, PlayerSkills.Skills.COOKING, _skillExpSource));
				return;
			}
			Debug.LogWarning((object)"Smoking:: PlayerChefID not assigned - will not award exp");
			EventManager.RaiseEvent(new ExperienceEvent(0, PlayerSkills.Skills.COOKING, _skillExpSource));
		}
	}

	private void OnSmoked(bool loading = false)
	{
		_isSmoked = true;
		if (!_food.Spoiled)
		{
			_food.Spoilable = false;
			Food.DisplayNamePrefixes.Clear();
			Food.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_SMOKED", 0);
		}
		if (!loading)
		{
			UnityEvent smoked = Smoked;
			if (smoked != null)
			{
				smoked.Invoke();
			}
		}
	}

	private void SetSmokingInShader()
	{
		float num = (_isSmoked ? 1f : Mathf.Clamp01(1f - _smokingHours / _originalSmokingHours));
		for (int i = 0; i < _food.renderers.Length; i++)
		{
			_food.renderers[i].GetPropertyBlock(_food.PropertyBlock);
			_food.PropertyBlock.SetFloat("_Smoked", num);
			_food.renderers[i].SetPropertyBlock(_food.PropertyBlock);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			SetSmokingInShader();
		}
	}

	private void ReplicatedSetSmoked()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateSetSmoked replicateSetSmoked = new ReplicateSetSmoked();
			replicateSetSmoked.SaveablePrefab = ((Component)this).GetComponent<SaveablePrefab>();
			replicateSetSmoked.Post();
		}
		SetSmoked();
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("SmokingHours", _smokingHours);
		return jObject;
	}

	public void Load(JObject data)
	{
		_smokingHours = data.GetField("SmokingHours").GetValue<float>();
		if (_smokingHours <= 0f)
		{
			OnSmoked(loading: true);
		}
		SetSmokingInShader();
	}
}
