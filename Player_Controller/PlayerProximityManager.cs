using System.Collections.Generic;
using Beam.Utilities;

namespace Beam;

public class PlayerProximityManager : ITick, ISceneClassInstance
{
	private static PlayerProximityManager _Instance;

	private readonly List<IPlayerProximityHandler> _handlers = new List<IPlayerProximityHandler>();

	private PlayerData _playerData = new PlayerData();

	public static PlayerProximityManager Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = Game.CreateSceneClassInstance<PlayerProximityManager>();
			}
			return _Instance;
		}
	}

	bool ITick.Active => true;

	public PlayerProximityManager()
	{
		Ticker.Instance?.Add(this);
	}

	void ISceneClassInstance.OnSceneDestroy()
	{
		Ticker.Instance?.Remove(this);
		_Instance = null;
	}

	public void Subcribe(IPlayerProximityHandler handler)
	{
		_handlers.Add(handler);
	}

	public void Unsubscribe(IPlayerProximityHandler handler)
	{
		if (handler.InProximity)
		{
			handler.OnLeftPlayerProximity(_playerData);
			handler.InProximity = false;
		}
		_handlers.Remove(handler);
	}

	void ITick.Tick()
	{
		for (int num = _handlers.Count - 1; num >= 0; num--)
		{
			IPlayerProximityHandler playerProximityHandler = _handlers[num];
			if (playerProximityHandler.IsNullOrDestroyed())
			{
				_handlers.RemoveAt(num);
			}
			else
			{
				bool flag = false;
				for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
				{
					IPlayer player = PlayerRegistry.AllPlayers[i];
					_playerData.Position = player.transform.position;
					_playerData.Rotation = player.transform.rotation;
					if ((player.transform.position - playerProximityHandler.Position).sqrMagnitude < playerProximityHandler.ProximityDistance * playerProximityHandler.ProximityDistance)
					{
						if (!playerProximityHandler.InProximity)
						{
							playerProximityHandler.OnEnteredPlayerProximity(_playerData);
							playerProximityHandler.InProximity = true;
						}
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					for (int j = 0; j < PlayerRegistry.AllPlayers.Count; j++)
					{
						IPlayer player2 = PlayerRegistry.AllPlayers[j];
						_playerData.Position = player2.transform.position;
						_playerData.Rotation = player2.transform.rotation;
						if ((player2.transform.position - playerProximityHandler.Position).sqrMagnitude > playerProximityHandler.ProximityDistance * playerProximityHandler.ProximityDistance && playerProximityHandler.InProximity)
						{
							playerProximityHandler.OnLeftPlayerProximity(_playerData);
							playerProximityHandler.InProximity = false;
							break;
						}
					}
				}
			}
		}
	}
}
