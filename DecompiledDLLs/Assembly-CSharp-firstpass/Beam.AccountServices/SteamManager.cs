using System;
using System.Text;
using Beam.Obfuscated.Users;
using Steamworks;
using UnityEngine;

namespace Beam.AccountServices;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour, IAccountService, IAuthenticationHandler
{
	private bool m_bInitialized;

	private SteamAccountUser m_localUser;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public static SteamManager Instance { get; private set; }

	public string Name => "Steam";

	public bool Initialized => m_bInitialized;

	public AccountUser LocalUser => m_localUser;

	public bool LoggedIn => m_localUser != null;

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning((object)pchDebugText);
	}

	public IAuthenticationHandler GetAuthenticationHandler()
	{
		return this;
	}

	public IAchievementsHandler GetAchievementsHandler()
	{
		return SteamAchievements.Instance;
	}

	public ILeaderboardHandler GetLeaderboardsHandler()
	{
		return SteamLeaderboards.Instance;
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError((object)"[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", (Object)(object)this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError((object)"[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", (Object)(object)this);
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError((object)"[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", (Object)(object)this);
		}
	}

	private void OnEnable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	private void OnDestroy()
	{
		if (!((Object)(object)Instance != (Object)(object)this))
		{
			Instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
			}
		}
	}

	private void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public void Login(Action<AuthCallbackInfo> callback)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (m_bInitialized)
		{
			m_localUser = new SteamAccountUser(((object)SteamUser.GetSteamID()/*cast due to constrained. prefix*/).ToString());
			callback?.Invoke(new AuthCallbackInfo
			{
				Result = 0
			});
		}
	}
}
