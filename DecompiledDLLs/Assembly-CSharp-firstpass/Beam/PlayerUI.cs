using System;
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
		if ((Object)(object)playerUI == (Object)null)
		{
			Debug.LogError((object)"PlayerUI::Create:: No UI prefab found!");
			return null;
		}
		return Object.Instantiate<PlayerUI>(playerUI);
	}

	private void Start()
	{
		_uiFallthrough.FellThrough += UIFallthrough_FellThrough;
	}

	public void SetPlayer(IPlayer player)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		_player = player;
		CreateUI();
		EventManager.AddListener<MainMenuOpenedEvent>(EventManager_MainMenuOpenedEvent);
		EventManager.AddListener<DialogueOpenedEvent>(EventManager_DialogueOpenedEvent);
		RewiredUtilities.RegisterCanvasForPlayer(_canvas, (!MPExtensions.IsMultiplayer(Game.Mode)) ? _player.Id : 0);
	}

	private void OnDestroy()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		EventManager.RemoveListener<MainMenuOpenedEvent>(EventManager_MainMenuOpenedEvent);
		EventManager.RemoveListener<DialogueOpenedEvent>(EventManager_DialogueOpenedEvent);
		RewiredUtilities.UnregisterCanvasForPlayer(_canvas, (!MPExtensions.IsMultiplayer(Game.Mode)) ? _player.Id : 0);
		EventManager.RemoveListener<PlayerDisconnectedEvent>(OnDisconnect);
		if ((Object)(object)_uiFallthrough != (Object)null)
		{
			_uiFallthrough.FellThrough -= UIFallthrough_FellThrough;
		}
	}

	private void CreateUI()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected Obj, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		m_userInterface = ((MPExtensions.IsMultiplayer(Game.Mode) || _player.Id == 0) ? PersistentUserInterfaceProvider.Instance.GetUserInterface() : Object.Instantiate<UserInterfaceProvider>(m_pxUserInterfacePrefab, ((Component)this).transform, false).GetUserInterface());
		UserInterfaceContext val = new UserInterfaceContext(m_userInterface);
		try
		{
			for (int i = 0; i < _controllerViews.Count; i++)
			{
				UiThing uiThing = _controllerViews[i];
				if (!((Object)(object)uiThing.ViewPrefab == (Object)null))
				{
					ViewAdapterBase viewAdapterBase = Object.Instantiate<ViewAdapterBase>(uiThing.ViewPrefab);
					((Component)viewAdapterBase).transform.SetParent(((Component)_canvas).transform, false);
					((Component)viewAdapterBase).transform.SetAsLastSibling();
					((Component)viewAdapterBase).transform.localPosition = Vector3.zero;
					((Component)viewAdapterBase).transform.localRotation = Quaternion.identity;
					uiThing.Controller?.SetView(viewAdapterBase);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		MonoBehaviour[] componentsInChildren = ((Component)_canvas).GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour obj in componentsInChildren)
		{
			(obj as INeedPlayerComponent)?.SetPlayer(_player);
			if (obj is ICloseOnFallthrough item)
			{
				fallthroughs.Add(item);
			}
		}
		_uiCamera.rect = _player.PlayerCamera.Camera.rect;
		if (MPExtensions.GetLocalPlayersCount(Game.Mode) > 1)
		{
			CanvasScaler component = ((Component)((Component)_canvas).transform).GetComponent<CanvasScaler>();
			component.screenMatchMode = (ScreenMatchMode)0;
			component.matchWidthOrHeight = 0.7f;
			Vector2 referenceResolution = component.referenceResolution;
			referenceResolution.y *= 2f;
			component.referenceResolution = referenceResolution;
		}
		if (_player.Id > 0 && MPExtensions.IsLocalCoop(Game.Mode))
		{
			((Behaviour)((Component)_canvas).GetComponent<GraphicRaycaster>()).enabled = false;
		}
		if (MPExtensions.IsServer(Game.Mode))
		{
			EventManager.AddListener<PlayerDisconnectedEvent>(OnDisconnect);
		}
	}

	public void Suspend(bool suspend)
	{
		this.GetController<HudPresenter>()?.Suspend(suspend);
		this.GetController<RadialBarPresenter>()?.Suspend(suspend);
	}

	public void Refresh(RefreshUIMode mode)
	{
		this.GetController<HudPresenter>()?.Refresh(mode);
		this.GetController<RadialBarPresenter>()?.Refresh(mode);
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
		TController result = default;
		foreach (UiThing controllerView in _controllerViews)
		{
			ControllerBase controller = controllerView.Controller;
			TController val = (TController)(object)((controller is TController) ? controller : null);
			if ((Object)(object)val != (Object)null)
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsServer(Game.Mode) && @event.PlayerId == 1 && !LevelLoader.IsLoadingScene())
		{
			LocalizedNotification.Post(_player, NotificationPriority.Immediate, 4f, "CLIENT_DISCONNECTED_NTF");
		}
	}
}
