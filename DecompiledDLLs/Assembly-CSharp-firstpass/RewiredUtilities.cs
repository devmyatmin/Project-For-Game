using System;
using System.Collections.Generic;
using System.Linq;
using Beam;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public static class RewiredUtilities
{
	private sealed class RewiredUtilitiesFinalizer
	{
		~RewiredUtilitiesFinalizer()
		{
			Unload();
		}
	}

	private class PlayerInputData
	{
		public Dictionary<int, bool> MapCategoryStates = new Dictionary<int, bool>();

		public bool Disabled;
	}

	private static RewiredUtilitiesFinalizer _finalizer;

	private static PlayerInputData[] _playerInputData;

	private static readonly Dictionary<Canvas, int> _playerCanvases;

	private static bool _initialized;

	private static Dictionary<int, int> _currentMapId;

	public static IList<Player> Players => ReInput.players.Players;

	public static IEnumerable<Player> ConnectedPlayers => ReInput.players.Players.Where((Player p) => p.controllers.hasKeyboard || p.controllers.joystickCount > 0);

	public static IEnumerable<Player> GetOtherPlayers(this Player player)
	{
		return ReInput.players.Players.Where((Player p) => p.id != player.id);
	}

	static RewiredUtilities()
	{
		_finalizer = new RewiredUtilitiesFinalizer();
		_playerCanvases = new Dictionary<Canvas, int>();
		_currentMapId = new Dictionary<int, int>();
		if (!_initialized)
		{
			CacheDefaultMapStates();
			_initialized = true;
		}
	}

	private static void Unload()
	{
		_initialized = false;
	}

	private static void CacheDefaultMapStates()
	{
		_playerInputData = new PlayerInputData[ReInput.players.AllPlayers.Count];
		for (int i = 0; i < _playerInputData.Length; i++)
		{
			_playerInputData[i] = new PlayerInputData();
			foreach (InputMapCategory mapCategory in ReInput.mapping.MapCategories)
			{
				bool value = ((InputCategory)mapCategory).id == 0;
				_playerInputData[i].MapCategoryStates.Add(((InputCategory)mapCategory).id, value);
			}
		}
	}

	public static bool AnyKey()
	{
		return ReInput.players.AllPlayers.Any((Player player) => player.GetAnyButtonDown());
	}

	public static bool HasJoystick(this Player input)
	{
		return input.controllers.joystickCount > 0;
	}

	public static bool HasJoysticks()
	{
		return ReInput.controllers.GetControllerCount((ControllerType)2) > 0;
	}

	public static float GetAxisRelative(this Player input, int actionId, EDeltaTimeType dtType)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float num = input.GetAxis(actionId);
		if ((int)input.GetAxisCoordinateMode(actionId) == 0)
		{
			num *= dtType switch
			{
				EDeltaTimeType.Regular => Time.deltaTime, 
				EDeltaTimeType.Smoothed => Time.smoothDeltaTime, 
				_ => Time.unscaledDeltaTime, 
			} * 80f;
		}
		return num;
	}

	public static void SetAndCacheMapsEnabled(this IEnumerable<Player> inputs, bool state, int mapCategoryId)
	{
		foreach (Player input in inputs)
		{
			if (state && mapCategoryId != 16)
			{
				_currentMapId[input.id] = mapCategoryId;
			}
			SetAndCacheMapsEnabled(input, state, mapCategoryId);
		}
	}

	public static void SetAndCacheMapsEnabled(this Player input, bool state, int mapCategoryId)
	{
		if (state && mapCategoryId != 16)
		{
			_currentMapId[input.id] = mapCategoryId;
		}
		_playerInputData[input.id].MapCategoryStates[mapCategoryId] = state;
		input.controllers.maps.SetMapsEnabled(state, mapCategoryId);
	}

	public static int GetCurrentMapId(this Player input)
	{
		return _currentMapId[input.id];
	}

	public static void SetCursorEnabled(this Player input, bool enabled)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!HasJoystick(input) || !MPExtensions.IsLocalCoop(Game.Mode))
		{
			CursorUtils.LockState = (CursorLockMode)((!enabled) ? 1 : 2);
			CursorUtils.Visible = enabled;
		}
	}

	public static void DisableMaps(this Player input)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		_playerInputData[input.id].Disabled = true;
		foreach (ControllerMap allMap in input.controllers.maps.GetAllMaps())
		{
			input.controllers.maps.SetMapsEnabled(false, allMap.controllerType, allMap.categoryId);
		}
	}

	public static void DisableMaps(this IEnumerable<Player> inputs)
	{
		foreach (Player input in inputs)
		{
			DisableMaps(input);
		}
	}

	public static void RestoreMapsEnabled(this Player input, ControllerType controllerType)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		_playerInputData[input.id].Disabled = false;
		foreach (ControllerMap allMap in input.controllers.maps.GetAllMaps(controllerType))
		{
			bool flag = IsMapEnabled(input.id, allMap.categoryId);
			input.controllers.maps.SetMapsEnabled(flag, allMap.controllerType, allMap.categoryId);
		}
	}

	public static void RestoreMapsEnabled(this Player input)
	{
		if (input.controllers.hasMouse)
		{
			RestoreMapsEnabled(input, (ControllerType)1);
		}
		if (input.controllers.hasKeyboard)
		{
			RestoreMapsEnabled(input, (ControllerType)0);
		}
		RestoreMapsEnabled(input, (ControllerType)2);
	}

	public static void RestoreMapsEnabled(this IEnumerable<Player> inputs)
	{
		foreach (Player input in inputs)
		{
			RestoreMapsEnabled(input);
		}
	}

	public static void RefreshMapsEnabled(this Player input, ControllerType controllerType)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!_playerInputData[input.id].Disabled)
		{
			RestoreMapsEnabled(input, controllerType);
		}
	}

	public static bool IsMapEnabled(int playerId, int mapCategoryId)
	{
		return _playerInputData[playerId].MapCategoryStates[mapCategoryId];
	}

	public static bool isMapEnabled(this Player player, int mapCategoryId)
	{
		return _playerInputData[player.id].MapCategoryStates[mapCategoryId];
	}

	public static bool IsActionEnabled(int playerId, int actionId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		return Beam.Utilities.ExtensionMethods.Any_NonAlloc(player.controllers.Controllers, (Controller controller, int id) => player.controllers.maps.GetFirstElementMapWithAction(controller, id, true) != null, actionId);
	}

	public static IPlayer GetPlayerFromInput(Player input)
	{
		for (int i = 0; i < PlayerRegistry.AllPlayers.Count; i++)
		{
			IPlayer player = PlayerRegistry.AllPlayers[i];
			if (player.Input == input)
			{
				return player;
			}
		}
		return null;
	}

	public static void RegisterCanvasForPlayer(Canvas canvas, int playerId)
	{
		if (_playerCanvases.TryGetValue(canvas, out var value))
		{
			Debug.LogError((object)("RewiredUtilities::RegisterCanvasForPlayer: Canvas already registered to player: " + value), (Object)(object)((Component)canvas).gameObject);
			return;
		}
		_playerCanvases.Add(canvas, playerId);
		if (!_currentMapId.ContainsKey(playerId))
		{
			_currentMapId.Add(playerId, -1);
		}
	}

	public static void UnregisterCanvasForPlayer(Canvas canvas, int playerId)
	{
		if (_playerCanvases.TryGetValue(canvas, out var value) && value == playerId)
		{
			_playerCanvases.Remove(canvas);
		}
		if (_currentMapId.ContainsKey(playerId))
		{
			_currentMapId.Remove(playerId);
		}
	}

	public static Player GetPlayerFromCanvas(Canvas canvas)
	{
		if (_playerCanvases.TryGetValue(canvas, out var value))
		{
			return ReInput.players.GetPlayer(value);
		}
		return ReInput.players.GetPlayer(0);
	}

	public static UserInterface GetUserInterfaceFromInput(Player input)
	{
		IPlayer playerFromInput = GetPlayerFromInput(input);
		if (!BeamTeam.IsNullOrDestroyed(playerFromInput))
		{
			return playerFromInput.PlayerUI.UserInterface;
		}
		return PersistentUserInterfaceProvider.Instance.GetUserInterface();
	}

	public static void SetUserInterfaceFromInput(IEnumerable<Selectable> selectables, Player input)
	{
		if (selectables == null || input == null)
		{
			return;
		}
		UserInterface userInterfaceFromInput = GetUserInterfaceFromInput(input);
		foreach (Selectable selectable in selectables)
		{
			selectable.SetUserInterface(userInterfaceFromInput);
		}
	}

	public static string GetActionElementName(Player player, int actionId)
	{
		if (player == null)
		{
			return string.Empty;
		}
		ActionElementMap firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)((player.controllers.joystickCount > 0) ? 2 : 0), actionId, false);
		if (firstElementMapWithAction == null)
		{
			firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)1, actionId, false);
		}
		if (firstElementMapWithAction == null)
		{
			return null;
		}
		return firstElementMapWithAction.elementIdentifierName;
	}

	public static string GetActionElementName(int playerId, int actionId)
	{
		return GetActionElementName(ReInput.players.GetPlayer(playerId), actionId);
	}

	public static bool ReplaceElementForAction(this ControllerMap map, int newElementId, int actionId)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		ActionElementMap[] elementMapsWithAction = map.GetElementMapsWithAction(actionId);
		if (elementMapsWithAction.Length == 0)
		{
			return false;
		}
		int num = 0;
		ActionElementMap[] array = elementMapsWithAction;
		ActionElementMap val3 = default;
		foreach (ActionElementMap val in array)
		{
			ElementAssignment val2 = new ElementAssignment(val.controllerMap.controllerType, val.elementType, val.elementIdentifierId, val.axisRange, val.keyCode, val.modifierKeyFlags, val.actionId, val.axisContribution, val.invert, val.id);
			string elementIdentifierName = val.elementIdentifierName;
			if (!map.ReplaceElementMap(val2, ref val3))
			{
				Debug.LogError((object)("Failed to remap action [" + ReInput.mapping.GetAction(actionId).descriptiveName + "] from [" + elementIdentifierName + "] to [" + val3.elementIdentifierName + "]"));
			}
			else
			{
				num++;
			}
		}
		return num > 0;
	}

	public static void CreateActionElementMap(this ControllerMap map, int newElementId, int actionId, KeyCode keyCode)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Element elementById = map.controller.GetElementById(newElementId);
		ElementAssignment val = new ElementAssignment(map.controllerType, elementById.type, elementById.id, (AxisRange)1, keyCode, (ModifierKeyFlags)0, actionId, (Pole)0, false);
		ActionElementMap val2 = default;
		if (!map.CreateElementMap(val, ref val2))
		{
			Debug.LogError((object)("Failed to create new element map for action " + ReInput.mapping.GetAction(actionId).descriptiveName));
		}
	}

	public static void CreateActionElementMap(this Player input, ControllerType controllerType, int controllerId, int actionId, int newElementId, int mapCategoryId, KeyCode keyCode)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		foreach (ControllerMap item in input.controllers.maps.GetMapsInCategory(controllerType, controllerId, mapCategoryId))
		{
			CreateActionElementMap(item, newElementId, actionId, keyCode);
		}
	}

	public static bool MapCategoryHasAction(this Player input, ControllerType controllerType, int controllerId, int actionId, int newElementId, int mapCategoryId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		foreach (ControllerMap item in input.controllers.maps.GetMapsInCategory(controllerType, controllerId, mapCategoryId))
		{
			if (item.GetFirstElementMapMatch((Predicate<ActionElementMap>)((ActionElementMap aem) => aem.actionId == actionId)) != null)
			{
				return true;
			}
		}
		return false;
	}
}
