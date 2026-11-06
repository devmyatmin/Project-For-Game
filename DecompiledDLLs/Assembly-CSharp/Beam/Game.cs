using System;
using System.Collections.Generic;
using System.Diagnostics;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Beam;

public class Game : MonoBehaviour
{
	private sealed class GameFinalizer
	{
		~GameFinalizer()
		{
			Unload();
		}
	}

	public static int MultiplayerSessionRegionIndex;

	public static bool MultiplayerSessionPrivate;

	public static string MultiplayerSessionName;

	private static readonly GameFinalizer _finalizer;

	private static Game _Instance;

	private const string PLAYERS_EDITOR_PREF = "__GamePlayers";

	public const int MAX_PLAYERS = 2;

	private static bool _isQuitting;

	private static GameState _state;

	private static GameState _previousState;

	private static GameMode _mode;

	private static readonly List<ISceneClassInstance> _classInstances;

	private static readonly Rect[][] SCREEN_LAYOUTS;

	public static bool IsQuitting => _isQuitting;

	public static GameState State
	{
		get
		{
			return _state;
		}
		set
		{
			_previousState = _state;
			_state = value;
		}
	}

	public static GameState PreviousState => _previousState;

	public static GameMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	public static event Action Quitting;

	public static Rect[] GetScreenLayoutForPlayers(int localPlayersCount)
	{
		return SCREEN_LAYOUTS[localPlayersCount - 1];
	}

	private static void OnQuit()
	{
		Quitting?.Invoke();
		Quitting = null;
	}

	static Game()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		_finalizer = new GameFinalizer();
		_state = GameState.MAIN_MENU;
		_previousState = _state;
		_classInstances = new List<ISceneClassInstance>();
		SCREEN_LAYOUTS = new Rect[4][]
		{
			new Rect[1]
			{
				new Rect(0f, 0f, 1f, 1f)
			},
			new Rect[2]
			{
				new Rect(0f, 0f, 0.5f, 1f),
				new Rect(0.5f, 0f, 0.5f, 1f)
			},
			new Rect[3]
			{
				new Rect(0f, 0.5f, 1f, 0.5f),
				new Rect(0f, 0f, 0.5f, 0.5f),
				new Rect(0.5f, 0f, 0.5f, 0.5f)
			},
			new Rect[4]
			{
				new Rect(0f, 0.5f, 0.5f, 0.5f),
				new Rect(0.5f, 0.5f, 0.5f, 0.5f),
				new Rect(0f, 0f, 0.5f, 0.5f),
				new Rect(0.5f, 0f, 0.5f, 0.5f)
			}
		};
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	private static void Unload()
	{
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
		Debug.Log((object)"Game::Unload:: Success.");
	}

	private static void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_Instance == (Object)null)
		{
			Game game = new GameObject("~Game").AddComponent<Game>();
			((Component)game).transform.position = Vector3.zero;
			((Component)game).transform.rotation = Quaternion.identity;
			((Component)game).transform.SetAsFirstSibling();
		}
	}

	private void Awake()
	{
		if ((Object)(object)_Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		_Instance = this;
		_isQuitting = false;
	}

	private void OnDestroy()
	{
		_isQuitting = true;
		ReleaseSceneClassInstances();
		if ((Object)(object)_Instance == (Object)(object)this)
		{
			_Instance = null;
		}
	}

	public static void QuitToMenu()
	{
		OnQuit();
		SceneManager.LoadScene(1);
	}

	public static void QuitToDesktop()
	{
		OnQuit();
		SteamAPI.Shutdown();
		Application.Quit();
		Process.GetCurrentProcess().Kill();
	}

	public static T CreateSceneClassInstance<T>() where T : class, ISceneClassInstance
	{
		if (_isQuitting)
		{
			return null;
		}
		T val = Activator.CreateInstance<T>();
		_classInstances.Add(val);
		return val;
	}

	private static void ReleaseSceneClassInstances()
	{
		for (int i = 0; i < _classInstances.Count; i++)
		{
			_classInstances[i].OnSceneDestroy();
		}
		_classInstances.Clear();
	}
}
