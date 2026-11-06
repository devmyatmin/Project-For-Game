using System.Collections.Generic;
using System.Text;
using Beam.Utilities;
using Funlabs;
using Rewired;
using UnityEngine;

namespace Beam.Developer.UI;

public class DeveloperConsolePresenter : MonoBehaviour
{
	private static DeveloperConsolePresenter _Instance;

	private IDeveloperConsoleViewAdapter _view;

	private DeveloperConsole _console;

	public static DeveloperConsolePresenter Instance => _Instance;

	public bool IsOpen => _view.Visible;

	public void Initialize(IDeveloperConsoleViewAdapter view)
	{
		if (Object.op_Implicit((Object)(object)_Instance))
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		_Instance = this;
		_view = view;
		_console = DeveloperConsole.Instance;
		if (Object.op_Implicit((Object)(object)_console))
		{
			_console.AddConsoleCommand("clear", "clears the console", Clear);
			_console.Log += DeveloperConsole_Log;
			ClearControlEvents();
			SetControlEvents();
			Hide();
		}
		else
		{
			Debug.LogError((object)"DeveloperConsoleUI::Start:: Error finding DeveloperConsole instance.");
			((Behaviour)this).enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)_console))
		{
			_console.Log -= DeveloperConsole_Log;
		}
	}

	public void Show()
	{
		_view.Show();
		RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, false, 0);
		RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: true, 16);
		_view.InputField.Text = "Enter command...";
		_view.InputField.Select();
	}

	public void Hide()
	{
		if (_view.Visible)
		{
			_view.Hide();
			RewiredUtilities.SetAndCacheMapsEnabled(ReInput.players.GetPlayer(0), state: false, 16);
			RewiredUtilities.SetAndCacheMapsEnabled((IEnumerable<Player>)RewiredUtilities.Players, true, 0);
		}
	}

	private void ClearControlEvents()
	{
	}

	private void SetControlEvents()
	{
		_view.InputField.KeyDown += InputField_KeyDown;
	}

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!MPExtensions.IsMultiplayer(Game.Mode) && Input.GetKeyDown((KeyCode)92))
		{
			if (_view.Visible)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}
	}

	public void Clear()
	{
		_view.OutputLabel.Text = "\n";
	}

	private void DeveloperConsole_Log(DeveloperConsole.ConsoleLog log)
	{
		_view.OutputLabel.Text += $"{log.Message}\n{log.Trace}\n";
	}

	public void LoadLogs(bool stacktrace = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < DeveloperConsole.Instance.CachedLogs.Count; i++)
		{
			DeveloperConsole.ConsoleLog consoleLog = DeveloperConsole.Instance.CachedLogs[i];
			if (stacktrace)
			{
				stringBuilder.Append($"{consoleLog.Message}\n{consoleLog.Trace}\n");
			}
			else
			{
				stringBuilder.Append($"{consoleLog.Message}\n");
			}
		}
		_view.OutputLabel.Text += stringBuilder.ToString();
	}

	public void InputField_KeyDown(string arg)
	{
		if (Input.GetButtonDown("Submit") && !string.IsNullOrEmpty(_view.OutputLabel.Text))
		{
			_console.Execute(_view.InputField.Text);
			_view.InputField.Text = "Enter command...";
			_view.InputField.Select();
		}
	}

	public static string Clear(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (!Object.op_Implicit((Object)(object)Instance))
		{
			return "Failed. No DeveloperConsoleUI component found.";
		}
		Instance.Clear();
		return string.Empty;
	}
}
