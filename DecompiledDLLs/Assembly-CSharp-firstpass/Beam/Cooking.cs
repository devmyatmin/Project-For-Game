using Beam.AccountServices;
using Beam.Events;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;

namespace Beam;

public class Cooking : MonoBehaviour
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private InteractiveObject_FOOD _food;

	[SerializeField]
	private bool _hasToBeCooked;

	[SerializeField]
	private float _cookingHours;

	[SerializeField]
	private PlayerSkills.ExpSource _skillExpSource = PlayerSkills.ExpSource.FOOD_COOKED_SMALL;

	[SerializeField]
	[HideInInspector]
	private float _originalCookingHours;

	private int _playerChefID = -1;

	private bool _isCooked;

	public UnityEvent BeganCooking;

	public UnityEvent Cooked;

	public InteractiveObject_FOOD Food => _food;

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

	public bool IsCooked
	{
		get
		{
			return _isCooked;
		}
		set
		{
			_isCooked = value;
		}
	}

	public bool IsBeingCooked { get; set; }

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

	public float GetCookingProgress()
	{
		if (_hasToBeCooked && _cookingHours >= 0f)
		{
			float num = 1f / _originalCookingHours;
			float num2 = Mathf.Abs(_cookingHours) * num;
			return 100f * num2;
		}
		return 0f;
	}

	public void Burn(float heat)
	{
		BurnManual(heat * GameTime.DeltaTimeHour);
	}

	public void BurnManual(float hours)
	{
		if (_isCooked)
		{
			return;
		}
		if (_cookingHours == _originalCookingHours)
		{
			UnityEvent beganCooking = BeganCooking;
			if (beganCooking != null)
			{
				beganCooking.Invoke();
			}
		}
		_cookingHours -= hours;
		_cookingHours = Mathf.Clamp(_cookingHours, 0f, _originalCookingHours);
		SetCookingInShader();
		if (_cookingHours <= 0f && !_isCooked)
		{
			OnCooked();
			if (PlayerChefID != -1)
			{
				EventManager.RaiseEvent(new ExperienceEvent(PlayerChefID, PlayerSkills.Skills.COOKING, _skillExpSource));
			}
			else
			{
				Debug.LogWarning((object)"Cooking:: PlayerChefID not assigned - award exp to LocalPlayer (failsafe)");
				EventManager.RaiseEvent(new ExperienceEvent(0, PlayerSkills.Skills.COOKING, _skillExpSource));
			}
			if (Food.Provenance == MeatProvenance.Crab)
			{
				EventManager.RaiseEvent(new AchievementUnlockEvent(EAchievement.CRABBY_PATTY));
			}
		}
	}

	private void OnCooked(bool loading = false)
	{
		_isCooked = true;
		if (!Food.Spoiled)
		{
			if (!loading)
			{
				_food.TimePicked = GameTime.Now;
			}
			Food.DisplayNamePrefixes.Clear();
			Food.DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_COOKED", 0);
		}
		if (!loading)
		{
			UnityEvent cooked = Cooked;
			if (cooked != null)
			{
				cooked.Invoke();
			}
		}
	}

	public void SpoilManual()
	{
		SetSpoilingInShader();
	}

	private void SetSpoilingInShader()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _food.renderers.Length; i++)
		{
			_food.renderers[i].GetPropertyBlock(_food.PropertyBlock);
			_food.PropertyBlock.SetColor("_Color", new Color(0.83f, 1f, 0.73f, 1f));
			_food.renderers[i].SetPropertyBlock(_food.PropertyBlock);
		}
	}

	private void SetCookingInShader()
	{
		float num = (_isCooked ? 1f : Mathf.Clamp01(1f - _cookingHours / _originalCookingHours));
		for (int i = 0; i < _food.renderers.Length; i++)
		{
			_food.renderers[i].GetPropertyBlock(_food.PropertyBlock);
			_food.PropertyBlock.SetFloat("_Cooked", num);
			_food.renderers[i].SetPropertyBlock(_food.PropertyBlock);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			SetCookingInShader();
			if (_food.Spoiled)
			{
				SetSpoilingInShader();
			}
		}
	}

	public JObject Save()
	{
		JObject jObject = new JObject();
		jObject.AddField("cookingPoints", JSerializer.Serialize((object)_cookingHours));
		return jObject;
	}

	public void Load(JObject data)
	{
		_cookingHours = data.GetField("cookingPoints").GetValue<float>();
		if (_cookingHours <= 0f)
		{
			OnCooked(loading: true);
		}
		SetCookingInShader();
	}
}
