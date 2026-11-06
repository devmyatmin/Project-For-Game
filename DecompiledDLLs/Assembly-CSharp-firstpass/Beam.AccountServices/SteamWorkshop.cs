using System;
using System.Collections.Generic;
using System.Text;
using Beam.Utilities;
using Steamworks;
using UnityEngine;

namespace Beam.AccountServices;

public class SteamWorkshop : MonoBehaviour, IUserContentProvider
{
	private static readonly string DEBUG_HEADER = "<color=blue>SteamWorkshop::</color>";

	private CallResult<SteamUGCQueryCompleted_t> m_ugcQueryResult;

	private List<LoadedContent> m_content;

	private bool m_requested;

	private bool m_received;

	public static SteamWorkshop Instance { get; private set; }

	public event Action<LoadedContentArgs> LoadedUserContent;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void OnDestroy()
	{
		LoadedUserContent = null;
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	public void RequestUserContent()
	{
		if (SteamManager.Instance.Initialized)
		{
			if (m_received)
			{
				LoadedUserContent?.Invoke(new LoadedContentArgs
				{
					LoadedContent = m_content
				});
			}
			else if (!m_requested)
			{
				GetSubscribedContent();
				m_requested = true;
			}
		}
	}

	private void GetSubscribedContent()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamID = SteamUser.GetSteamID();
		SteamAPICall_t val = SteamUGC.SendQueryUGCRequest(SteamUGC.CreateQueryUserUGCRequest(steamID.GetAccountID(), (EUserUGCList)6, (EUGCMatchingUGCType)0, (EUserUGCListSortOrder)1, (AppId_t)0u, SteamUtils.GetAppID(), 1u));
		if (m_ugcQueryResult != null)
		{
			m_ugcQueryResult.Dispose();
		}
		m_ugcQueryResult = CallResult<SteamUGCQueryCompleted_t>.Create((APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
		m_ugcQueryResult.Set(val, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)OnSteamUGCQueryCompleted);
		Debug.Log((object)(DEBUG_HEADER + " Requesting UGC.."));
	}

	private void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t result, bool failed)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (failed)
		{
			Debug.LogError((object)(DEBUG_HEADER + "OnSteamUGCQueryCompleted: Query failed."));
			m_requested = false;
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		m_content = new List<LoadedContent>();
		SteamUGCDetails_t details = default;
		for (int i = 0; i < result.m_unNumResultsReturned; i++)
		{
			if (SteamUGC.GetQueryUGCResult(result.m_handle, (uint)i, ref details))
			{
				LoadedContent loadedContent = LoadContent(details);
				if (loadedContent != null)
				{
					m_content.Add(loadedContent);
					stringBuilder.Append("Loaded mod content: '" + details.m_rgchTitle + "' at path: " + loadedContent.Path + "\n");
				}
			}
		}
		Debug.Log((object)(DEBUG_HEADER + ((stringBuilder.Length == 0) ? "No mod content found." : ("Loaded mods:\n" + stringBuilder.ToString()))));
		LoadedUserContent?.Invoke(new LoadedContentArgs
		{
			LoadedContent = m_content
		});
		SteamUGC.ReleaseQueryUGCRequest(result.m_handle);
		m_received = true;
	}

	private LoadedContent LoadContent(SteamUGCDetails_t details)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		PublishedFileId_t nPublishedFileId = details.m_nPublishedFileId;
		if (!((Enum)(EItemState)SteamUGC.GetItemState(nPublishedFileId)).HasFlag((Enum)(object)(EItemState)4))
		{
			Debug.Log((object)(DEBUG_HEADER + details.m_rgchTitle + " is not installed."));
			return null;
		}
		ulong num = default;
		string path = default;
		uint num2 = default;
		if (!SteamUGC.GetItemInstallInfo(nPublishedFileId, ref num, ref path, 2048u, ref num2))
		{
			Debug.Log((object)(DEBUG_HEADER + details.m_rgchTitle + " is not installed or contains no content."));
			return null;
		}
		LoadedContent loadedContent = new LoadedContent();
		loadedContent.Path = path;
		if (!string.IsNullOrEmpty(details.m_rgchTags))
		{
			loadedContent.Tags = details.m_rgchTags.Split(',');
		}
		return loadedContent;
	}
}
