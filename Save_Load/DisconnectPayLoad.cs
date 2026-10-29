using Beam.Language;
using UnityEngine;

namespace Funlabs;

public class DisconnectPayLoad
{
	public static readonly string TITLE = "TEXT_ERROR";

	public static readonly string CONNECTION_ERROR = "ConnectionError";

	public static readonly string KICKED_BY_SERVER_MESSAGE = "KICKED_BY_SERVER_MESSAGE";

	public static float PERSISTENCE = 3f;

	private DisconnectDetail? _disconnectDetail;

	private float _setTime;

	public void Set(DisconnectDetail disconnectDetail)
	{
		_disconnectDetail = disconnectDetail;
		_setTime = Time.unscaledTime;
	}

	public DisconnectDetail? Get()
	{
		if (!(Time.unscaledTime - _setTime > PERSISTENCE))
		{
			return _disconnectDetail;
		}
		return null;
	}

	public bool TryGetDisconnectDialog(out string title, out string content)
	{
		DisconnectDetail? disconnectDetail = Get();
		switch (disconnectDetail)
		{
		case DisconnectDetail.Intended:
			title = (content = string.Empty);
			return false;
		case DisconnectDetail.Timeout_PalmSegments:
			title = TITLE;
			content = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", disconnectDetail.ToString());
			return true;
		case DisconnectDetail.Timeout_Fence:
			title = TITLE;
			content = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", disconnectDetail.ToString());
			return true;
		case DisconnectDetail.Timeout_MissionStart:
			title = TITLE;
			content = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", disconnectDetail.ToString());
			return true;
		case DisconnectDetail.Timeout_Messages:
			title = TITLE;
			content = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", disconnectDetail.ToString());
			return true;
		case DisconnectDetail.Timeout_StayAlive:
			title = TITLE;
			content = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", disconnectDetail.ToString());
			return true;
		case DisconnectDetail.Timeout_Linking:
			title = TITLE;
			content = Localization.GetLanguageHandler().Localize("DISCONNECTED_INTERNAL", disconnectDetail.ToString());
			return true;
		case DisconnectDetail.Kicked_By_Server:
			title = TITLE;
			content = "TEXT_DISCONNECTED";
			return true;
		default:
			title = TITLE;
			content = "TEXT_DISCONNECTED";
			return true;
		}
	}

	public string GetDisconnetDialogContent()
	{
		return string.Empty;
	}
}
