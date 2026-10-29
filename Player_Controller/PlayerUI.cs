using System.Collections.Generic;
using Beam.Events;
using Beam.UI;
using Funlabs;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Beam;

public class PlayerUI : MonoBehaviour, INeedPlayerComponent
{
	[SerializeField]
	private UserInterfaceProvider m_pxUserInterfacePrefab;

	private UserInterface m_userInterface;

	[SerializeField]
	private Camera _uiCamera;

	[FormerlySerializedAs("_uiCanvas")]
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private UIFallthrough _uiFallthrough;

	[SerializeField]
	private StorageMenuPresenter _storagePresenter;

	[FormerlySerializedAs("_things")]
	[SerializeField]
	private List<UiThing> _controllerViews;

	private IPlayer _player;

	private List<ICloseOnFallthrough> fallthroughs = new List<ICloseOnFallthrough>();

	public UserInterface UserInterface => m_userInterface;

	public Camera UiCamera => _uiCamera;

	public Canvas Canvas => _canvas;

	public StorageMenuPresenter StoragePresenter => _storagePresenter;

	IPlayer INeedPlayerComponent.Player => _player;

	public static PlayerUI Create(IPlayer player)
	{
		PlayerUI playerUI = Resources.Load<PlayerUI>("Prefabs/Player/PlayerUI");
		if (playerUI == null)
		{
			Debug.LogError("PlayerUI::Create:: No UI prefab found!");
			return null;
		}
		return Object.Instantiate(playerUI);
	}

	private void Start()
	{
		_uiFallthrough.FellThrough += UIFallthrough_FellThrough;
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		CreateUI();
		EventManager.AddListener<MainMenuOpenedEvent>(EventManager_MainMenuOpenedEvent);
		EventManager.AddListener<DialogueOpenedEvent>(EventManager_DialogueOpenedEvent);
		RewiredUtilities.RegisterCanvasForPlayer(_canvas, (!Game.Mode.IsMultiplayer()) ? _player.Id : 0);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<MainMenuOpenedEvent>(EventManager_MainMenuOpenedEvent);
		EventManager.RemoveListener<DialogueOpenedEvent>(EventManager_DialogueOpenedEvent);
		RewiredUtilities.UnregisterCanvasForPlayer(_canvas, (!Game.Mode.IsMultiplayer()) ? _player.Id : 0);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
		if (_uiFallthrough != null)
		{
			_uiFallthrough.FellThrough -= UIFallthrough_FellThrough;
		}
	}

	private void CreateUI()
	{
		m_userInterface = ((Game.Mode.IsMultiplayer() || _player.Id == 0) ? PersistentUserInterfaceProvider.Instance.GetUserInterface() : Object.Instantiate(m_pxUserInterfacePrefab, base.transform, worldPositionStays: false).GetUserInterface());
		using (new UserInterfaceContext(m_userInterface))
		{
			for (int i = 0; i < _controllerViews.Count; i++)
			{
				UiThing uiThing = _controllerViews[i];
				if (!(uiThing.ViewPrefab == null))
				{
					ViewAdapterBase viewAdapterBase = Object.Instantiate(uiThing.ViewPrefab);
					viewAdapterBase.transform.SetParent(_canvas.transform, worldPositionStays: false);
					viewAdapterBase.transform.SetAsLastSibling();
					viewAdapterBase.transform.localPosition = Vector3.zero;
					viewAdapterBase.transform.localRotation = Quaternion.identity;
					uiThing.Controller?.SetView(viewAdapterBase);
				}
			}
		}
		MonoBehaviour[] componentsInChildren = _canvas.GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour obj in componentsInChildren)
		{
			(obj as INeedPlayerComponent)?.SetPlayer(_player);
			if (obj is ICloseOnFallthrough item)
			{
				fallthroughs.Add(item);
			}
		}
		_uiCamera.rect = _player.PlayerCamera.Camera.rect;
		if (Game.Mode.GetLocalPlayersCount() > 1)
		{
			CanvasScaler component = _canvas.transform.GetComponent<CanvasScaler>();
			component.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			component.matchWidthOrHeight = 0.7f;
			Vector2 referenceResolution = component.referenceResolution;
			referenceResolution.y *= 2f;
			component.referenceResolution = referenceResolution;
		}
		if (_player.Id > 0 && Game.Mode.IsLocalCoop())
		{
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
		}
		if (Game.Mode.IsServer())
		{
			EventManager.AddListener<PlayerDisconnectedEvent>(OnDisconnect);
		}
	}

	public void Suspend(bool suspend)
	{
		GetController<HudPresenter>()?.Suspend(suspend);
		GetController<RadialBarPresenter>()?.Suspend(suspend);
	}

	public void Refresh(RefreshUIMode mode)
	{
		GetController<HudPresenter>()?.Refresh(mode);
		GetController<RadialBarPresenter>()?.Refresh(mode);
	}

	private void EventManager_MainMenuOpenedEvent(MainMenuOpenedEvent e)
	{
		SuspendInput(e.PlayerId, e.Opened);
	}

	private void EventManager_DialogueOpenedEvent(DialogueOpenedEvent e)
	{
		SuspendInput(e.PlayerId, e.Opened);
	}

	private void SuspendInput(int playerId, bool suspend)
	{
		if (_player.Id != playerId)
		{
			m_userInterface.EventSystem.Suspend(suspend);
		}
	}

	public TController GetController<TController>() where TController : Component
	{
		TController result = null;
		foreach (UiThing controllerView in _controllerViews)
		{
			TController val = controllerView.Controller as TController;
			if ((Object)val != (Object)null)
			{
				result = val;
				return result;
			}
		}
		return result;
	}

	private void UIFallthrough_FellThrough()
	{
		for (int i = 0; i < fallthroughs.Count; i++)
		{
			fallthroughs[i].Close();
		}
	}

	private void OnDisconnect(PlayerDisconnectedEvent @event)
	{
		if (Game.Mode.IsServer() && @event.PlayerId == 1 && !LevelLoader.IsLoadingScene())
		{
			LocalizedNotification.Post(_player, NotificationPriority.Immediate, 4f, "CLIENT_DISCONNECTED_NTF");
		}
	}
}
