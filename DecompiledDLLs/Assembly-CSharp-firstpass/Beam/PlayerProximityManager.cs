using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

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
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _handlers.Count - 1; num >= 0; num--)
		{
			IPlayerProximityHandler playerProximityHandler = _handlers[num];
			if (BeamTeam.IsNullOrDestroyed(playerProximityHandler))
			{
				_handlers.RemoveAt(num);
			}
			else
			{
				bool flag = false;
				Vector3 val;
				for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
				{
					IPlayer player = PlayerRegistry.AllPlayers[i];
					_playerData.Position = player.transform.position;
					_playerData.Rotation = player.transform.rotation;
					val = player.transform.position - playerProximityHandler.Position;
					if (val.sqrMagnitude < playerProximityHandler.ProximityDistance * playerProximityHandler.ProximityDistance)
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
						val = player2.transform.position - playerProximityHandler.Position;
						if (val.sqrMagnitude > playerProximityHandler.ProximityDistance * playerProximityHandler.ProximityDistance && playerProximityHandler.InProximity)
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
