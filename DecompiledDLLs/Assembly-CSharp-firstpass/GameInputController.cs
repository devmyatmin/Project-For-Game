using System.Collections.Generic;
using Beam;
using Beam.Crafting;
using Beam.Events;
using Funlabs;
using UnityEngine;

public class GameInputController : MonoBehaviour, IGameInputController, INeedPlayerComponent, IIdleUntilInitialized
{
	[SerializeField]
	private Raycaster _raycaster;

	[SerializeField]
	private Interacter _interacter;

	[SerializeField]
	private Dragger _dragger;

	[SerializeField]
	private User _user;

	[SerializeField]
	private Selecter _selecter;

	[SerializeField]
	private Skinner _skinner;

	[SerializeField]
	private FireLighter _fireLighter;

	[SerializeField]
	private SpyGlass _spyGlass;

	private IPlayer _player;

	private List<IGameInputHandler> _handlers = new List<IGameInputHandler>();

	private bool _initialized;

	public Raycaster Raycaster => _raycaster;

	public Interacter Interacter => _interacter;

	public Dragger Dragger => _dragger;

	public User User => _user;

	public FireLighter FireLighter => _fireLighter;

	public SpyGlass SpyGlass => _spyGlass;

	IPlayer INeedPlayerComponent.Player => _player;

	public IGameInputHandler CurrentHandler { get; set; }

	public bool IsInitialized => _initialized;

	private void Start()
	{
		EventManager.AddListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<EndGameSequenceEvent>(EventManager_EndGameSequenceEvent);
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			Subscribe(_raycaster);
			Subscribe(_selecter);
			Subscribe(_interacter);
			Subscribe(_user);
			Subscribe(_dragger);
			Subscribe(_skinner);
			Subscribe(_fireLighter);
			Subscribe(_spyGlass);
		}
	}

	public void SetPlayer(IPlayer player)
	{
		_player = player;
		_raycaster.SetPlayer(_player);
		_interacter.SetPlayer(_player);
		_user.SetPlayer(_player);
		_dragger.SetPlayer(_player);
		_skinner.SetPlayer(_player);
		_fireLighter.SetPlayer(_player);
		_selecter.SetPlayer(_player);
		_spyGlass.SetPlayer(_player);
	}

	public void Subscribe(IGameInputHandler handler)
	{
		handler.Initialize(this);
		_handlers.Add(handler);
	}

	public void Unsubscribe(IGameInputHandler handler)
	{
		_handlers.Remove(handler);
	}

	private void EventManager_EndGameSequenceEvent(EndGameSequenceEvent e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Game.Mode == 1 && _player.Id == 1)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void Update()
	{
		if (!_initialized)
		{
			return;
		}
		_raycaster.CheckInput();
		if (_player.Statistics.IsKnockedOut)
		{
			return;
		}
		if (CurrentHandler != null)
		{
			CurrentHandler.CheckInput();
			return;
		}
		for (int i = 0; i < _handlers.Count; i++)
		{
			_handlers[i].CheckInput();
			if (CurrentHandler != null)
			{
				break;
			}
		}
	}
}
