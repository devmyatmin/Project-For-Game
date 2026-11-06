using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Beam.Utilities;
using Beam.Utilities.UGUI;
using UnityEngine;

namespace Beam.Developer;

public class DeveloperConsole : MonoBehaviour
{
	public class ConsoleCommand
	{
		public string Parameters;

		public string Description;

		public string Help;

		public bool Hidden;

		public Func<string, string> Callback;

		public ConsoleCommand(string description, Func<string, string> callback, string parameters = null, string help = null, bool hidden = false)
		{
			Description = description;
			Parameters = parameters;
			Help = help;
			Hidden = hidden;
			Callback = callback;
		}
	}

	public class ConsoleLog
	{
		public string Message;

		public string Trace;
	}

	public delegate void LoggingEventHandler(ConsoleLog log);

	private static readonly string INVALID_COMMAND_STRING = UGUITools.FormatColor("Invalid Command! ", "FF0000");

	private static readonly string COMMAND_NOT_FOUND_STRING = UGUITools.FormatColor("Unrecognized command: ", "FF0000");

	private static readonly string LOG_STRING = UGUITools.FormatColor("Log: ", "DCDCDC");

	private static readonly string WARNING_STRING = UGUITools.FormatColor("Warning: ", "CCAA00");

	private static readonly string ERROR_STRING = UGUITools.FormatColor("Error: ", "FF6000");

	private static readonly string EXCEPTION_STRING = UGUITools.FormatColor("Exception: ", "FF0000");

	private static readonly string ASSERT_STRING = UGUITools.FormatColor("Assert: ", "0000FF");

	[SerializeField]
	private bool _outputStackTrace = true;

	private Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>();

	private List<ConsoleLog> _cachedLogs = new List<ConsoleLog>();

	public static DeveloperConsole Instance { get; private set; }

	public IDictionary<string, ConsoleCommand> Commands => _commands;

	public List<ConsoleLog> CachedLogs => _cachedLogs;

	public event LoggingEventHandler Log;

	protected virtual void OnLog(ConsoleLog log)
	{
		Log?.Invoke(log);
	}

	private void Awake()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Application.logMessageReceived += Application_LogMessageReceived;
		AddCommand("help", "show help on how to use the console", DeveloperConsoleCommands.Help, "[bool [i]description[/i]]");
		AddCommand("system", "", DeveloperConsoleCommands.System);
		AddCommand("options", "", DeveloperConsoleCommands.DebugOptions, "list");
		AddCommand("fps", "show frames per second counter", DeveloperConsoleCommands.FPS, "[bool [i]state[/i]]");
		AddCommand("dev.log.dump", "", DeveloperConsoleCommands.DumpLog, "");
		AddCommand("dev.log.view", "", DeveloperConsoleCommands.ViewLog, "[bool [i]stack trace[/i]]");
		AddCommand("dev.log.clear", "", DeveloperConsoleCommands.ClearLog, "");
		AddCommand("dev.god", "", DeveloperConsoleCommands.ToggleGodMode, "", hidden: true);
		AddCommand("dev.invincible", "", DeveloperConsoleCommands.ToggleInvincibility, "", hidden: true);
		AddCommand("dev.time", "", DeveloperConsoleCommands.ChangeTime, "[number [i]time[/i]]", hidden: true);
		AddCommand("dev.timescale", "", DeveloperConsoleCommands.TimeScale, "[number [i]scale[/i]]", hidden: true);
		AddCommand("dev.console", "", DeveloperConsoleCommands.TestConsole, "[bool [i]enabled[/i]]", hidden: true);
		AddCommand("dev.debugaudio", "", DeveloperConsoleCommands.DebugAudio, "", hidden: true);
		AddCommand("dev.components.camera.reflections", "", DeveloperConsoleCommands.GlobalReflections, "[bool [i]enabled[/i]]");
		AddCommand("dev.capturemode", "", DeveloperConsoleCommands.CaptureMode, "[bool [i]enabled[/i]]");
	}

	private void OnDestroy()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		Application.logMessageReceived -= Application_LogMessageReceived;
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void Application_LogMessageReceived(string message, string trace, LogType type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		StringBuilder stringBuilder = new StringBuilder();
		switch ((int)type)
		{
		case 1:
			stringBuilder.Append(ASSERT_STRING);
			break;
		case 0:
			stringBuilder.Append(ERROR_STRING);
			break;
		case 4:
			stringBuilder.Append(EXCEPTION_STRING);
			break;
		case 3:
			stringBuilder.Append(LOG_STRING);
			break;
		case 2:
			stringBuilder.Append(WARNING_STRING);
			break;
		}
		stringBuilder.Append(message);
		ConsoleLog consoleLog = new ConsoleLog();
		consoleLog.Message = stringBuilder.ToString();
		consoleLog.Trace = (_outputStackTrace ? UGUITools.FormatColor(trace, "606060") : string.Empty);
		_cachedLogs.Add(consoleLog);
	}

	public string LogToFile()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		string text = folderPath + "/Log.txt";
		int num = 0;
		while (File.Exists(text))
		{
			text = folderPath + "/Log(" + num + ").txt";
			num++;
		}
		try
		{
			using StreamWriter streamWriter = new StreamWriter(text, append: true);
			streamWriter.Write("Stranded Deep Log File: \n\n");
			streamWriter.Write(DeveloperConsoleCommands.System(null));
			streamWriter.Write("\n\n");
			foreach (ConsoleLog cachedLog in _cachedLogs)
			{
				streamWriter.Write($"{cachedLog.Message}\n{cachedLog.Trace}\n");
			}
			return text;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("DeveloperConsole::LogToFile:: Error writing log to file. \n" + ex.ToString()));
			return null;
		}
	}

	public void ClearLogs()
	{
		_cachedLogs.Clear();
	}

	public void AddConsoleCommand(string command, string description, Func<string, string> callback, string parameters = null)
	{
		if (callback != null)
		{
			AddCommand(command, description, callback, parameters);
		}
	}

	private void AddCommand(string command, string description, Func<string, string> callback, string parameters = null, bool hidden = false)
	{
		if (string.IsNullOrEmpty(command))
		{
			Debug.LogError((object)("DeveloperConsole::AddCommand(): Could not add command \"" + command + "\" because it is null or empty."));
		}
		else if (!_commands.ContainsKey(command))
		{
			_commands.Add(command, new ConsoleCommand(description, callback, parameters, "", hidden));
			SortCommands();
		}
	}

	private void SortCommands()
	{
		if (_commands != null)
		{
			_commands = _commands.OrderBy((KeyValuePair<string, ConsoleCommand> command) => command.Key).ToDictionary((KeyValuePair<string, ConsoleCommand> command) => command.Key, (KeyValuePair<string, ConsoleCommand> command) => command.Value);
		}
	}

	public void Execute(string command)
	{
		ConsoleLog consoleLog = new ConsoleLog();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("> {0}\n", UGUITools.FormatColor(command, "2B99E7"));
		if (string.IsNullOrEmpty(command))
		{
			stringBuilder.Append(INVALID_COMMAND_STRING);
			consoleLog.Message = stringBuilder.ToString();
			Print(consoleLog);
			return;
		}
		string text = command.Split(' ')[0];
		if (!_commands.ContainsKey(text))
		{
			stringBuilder.Append(COMMAND_NOT_FOUND_STRING);
			stringBuilder.Append(text);
			consoleLog.Message = stringBuilder.ToString();
			Print(consoleLog);
			return;
		}
		string arg = string.Empty;
		if (command.Length > text.Length)
		{
			arg = command.Substring(text.Length).Trim();
		}
		stringBuilder.Append(_commands[text].Callback(arg));
		consoleLog.Message = stringBuilder.ToString();
		Print(consoleLog);
	}

	private void Print(ConsoleLog log)
	{
		if (!string.IsNullOrEmpty(log.Message))
		{
			OnLog(log);
		}
	}

	public List<string> GetSuggestions(string input)
	{
		return (from command in _commands.Keys
			where command.StartsWith(input, ignoreCase: true, CultureInfo.InvariantCulture) && !Commands[command].Hidden
			orderby command.Length
			select command + " \t" + UGUITools.FormatColor(Commands[command].Description, "CCCCCC") into command
			select UGUITools.FormatColor(command.Substring(0, input.Length), "660090") + command.Substring(input.Length)).ToList();
	}

	public string GetHelpList(bool includeDescription)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in Commands.Keys.Where((string command) => !Commands[command].Hidden))
		{
			stringBuilder.Append(UGUITools.FormatColor(item, "2B99E7"));
			stringBuilder.Append(" ");
			stringBuilder.Append((Commands[item].Parameters == null) ? string.Empty : UGUITools.FormatColor(Commands[item].Parameters, "33EE33"));
			stringBuilder.Append(" ");
			stringBuilder.Append((Commands[item].Description == null) ? string.Empty : UGUITools.FormatColor(Commands[item].Description, "DCDCDC"));
			stringBuilder.Append("\n");
		}
		return stringBuilder.ToString();
	}
}
