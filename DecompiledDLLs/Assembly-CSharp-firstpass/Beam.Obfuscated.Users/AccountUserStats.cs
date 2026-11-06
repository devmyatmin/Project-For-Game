using System;
using Beam.Obfuscated.Serialization;
using UnityEngine;

namespace Beam.Obfuscated.Users;

[Serializable]
public class AccountUserStats
{
	[SerializeField]
	private Stats _sessionStats = new Stats();

	[SerializeField]
	private Stats _totalStats = new Stats();

	private Stats _base = new Stats();

	public Stats SessionStats
	{
		get
		{
			return _sessionStats;
		}
		set
		{
			_sessionStats = value;
		}
	}

	public Stats TotalStats
	{
		get
		{
			return _totalStats;
		}
		set
		{
			_totalStats = value;
		}
	}

	public void Reset()
	{
		_sessionStats.Reset();
		_totalStats.Reset();
		_base.Reset();
	}

	public void Rebase()
	{
		Stats stats = _sessionStats - _base;
		_totalStats += stats;
		_base.Reset();
		_base += _sessionStats;
	}

	public void Load(GameMode mode)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		Stats stats = SavingUtilities.LoadFromPrefs<Stats>(FilePath.GetPrefsTotalKey(Options.GeneralSettings.LastSaveSlotUsed, mode));
		if (stats != null)
		{
			_totalStats = stats;
		}
		Stats stats2 = SavingUtilities.LoadFromPrefs<Stats>(FilePath.GetPrefsSessionKey(Options.GeneralSettings.LastSaveSlotUsed, mode));
		if (stats2 != null)
		{
			_sessionStats = stats2;
		}
		_base.Reset();
		_base += _sessionStats;
	}
}
