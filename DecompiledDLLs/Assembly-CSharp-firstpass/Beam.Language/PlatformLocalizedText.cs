using System;
using System.Linq;
using UnityEngine;

namespace Beam.Language;

public class PlatformLocalizedText : LocalizedText, ISerializationCallbackReceiver
{
	[Serializable]
	public struct RewiredActionIdWrapper(int id)
	{
		[RewiredActionDropdown]
		public int Id = id;
	}

	[SerializeField]
	private RewiredActionIdWrapper[] _actions = new RewiredActionIdWrapper[1]
	{
		new RewiredActionIdWrapper(-1)
	};

	[SerializeField]
	[HideInInspector]
	private int[] _rewiredActions = new int[1] { -1 };

	private int _playerId;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (_actions != null && _actions.Length != 0)
		{
			_rewiredActions = _actions.Select((RewiredActionIdWrapper a) => a.Id).ToArray();
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_actions = _rewiredActions?.Select((int r) => new RewiredActionIdWrapper(r)).ToArray();
	}

	public void SetActions(params int[] actionIds)
	{
		_rewiredActions = actionIds;
	}

	public void SetPlayer(int playerId)
	{
		_playerId = playerId;
	}

	public override string GetLocalizedText()
	{
		string localizedText = base.GetLocalizedText();
		return PlatformLocalization.GetHandler().SubstituteControllerGlyphs(localizedText, _playerId, _rewiredActions);
	}
}
