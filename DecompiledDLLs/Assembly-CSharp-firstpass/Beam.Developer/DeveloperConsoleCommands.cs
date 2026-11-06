using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Beam.Developer.UI;
using Beam.Utilities;
using Beam.Utilities.UGUI;
using UnityEngine;

namespace Beam.Developer;

public static class DeveloperConsoleCommands
{
	public static string ToggleInvincibility(string param)
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.Statistics.Invincible = !allPlayer.Statistics.Invincible;
		}
		return "Success.";
	}

	public static string ToggleGodMode(string param)
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			allPlayer.DeveloperMode.ReplicatedToggleGodMode();
		}
		return "Success.";
	}

	public static string SetHealth(string param)
	{
		if (!int.TryParse(param, out var result))
		{
			return "Failed.";
		}
		PlayerRegistry.LocalPlayer.Statistics.SetHealth(result);
		return "Success";
	}

	public static string Die(string param)
	{
		PlayerRegistry.LocalPlayer.Statistics.ReplicatedApplyDamage(700f);
		return "Success.";
	}

	public static string ToggleUI(string param)
	{
		return string.Concat(string.Concat(string.Concat("" + "Toggling player UI... ", TogglePlayerUI("")), "\nToggling game UI... "), ToggleGameUI(""));
	}

	public static string ToggleGameUI(string param)
	{
		GameObject val = GameObject.Find("GameUI");
		if ((Object)(object)val == (Object)null)
		{
			return "Failed.";
		}
		Camera componentInChildren = val.GetComponentInChildren<Camera>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			((Behaviour)componentInChildren).enabled = !((Behaviour)componentInChildren).enabled;
		}
		return "Success.";
	}

	public static string TogglePlayerUI(string param)
	{
		foreach (IPlayer allPlayer in PlayerRegistry.AllPlayers)
		{
			Camera componentInChildren = ((Component)allPlayer.PlayerUI).GetComponentInChildren<Camera>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((Behaviour)componentInChildren).enabled = !((Behaviour)componentInChildren).enabled;
			}
		}
		return "Success.";
	}

	public static string CaptureMode(string param)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)DeveloperConsole.Instance == (Object)null)
		{
			return null;
		}
		if (bool.TryParse(param, out var result))
		{
			if (result)
			{
				CaptureMode captureMode = Resources.Load<CaptureMode>("Prefabs/CAPTURE_MODE");
				if ((Object)(object)captureMode != (Object)null)
				{
					Object.Instantiate<CaptureMode>(captureMode, Vector3.zero, Quaternion.identity);
					return "Success. Capture mode enabled";
				}
				return "Failed. No CaptureMode prefab found.";
			}
			CaptureMode captureMode2 = Object.FindObjectOfType<CaptureMode>();
			if ((Object)(object)captureMode2 != (Object)null)
			{
				BeamTeam.Destroy<GameObject>(((Component)captureMode2).gameObject);
				return "Success. Capture mode disabled.";
			}
			return "Success. Capture mode was not enabled.";
		}
		return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
	}

	public static string GlobalReflections(string param)
	{
		if ((Object)(object)DeveloperConsole.Instance == (Object)null)
		{
			return null;
		}
		if (bool.TryParse(param, out var result))
		{
			BeamGlobalReflections beamGlobalReflections = Object.FindObjectOfType<BeamGlobalReflections>();
			if ((Object)(object)beamGlobalReflections != (Object)null)
			{
				((Behaviour)beamGlobalReflections).enabled = result;
				return "Success.";
			}
			return "Failed. No BeamGlobalReflections component found.";
		}
		return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
	}

	public static string Help(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (string.IsNullOrEmpty(param))
		{
			return string.Format("Invalid parameter. Use {0} for a list of possible commands, or {1} for a description of a certain command.", UGUITools.FormatColor("list", "33EE33"), UGUITools.FormatColor("<command>", "33EE33"));
		}
		DeveloperConsole instance = DeveloperConsole.Instance;
		switch (param)
		{
		case "list true":
			return instance.GetHelpList(includeDescription: true);
		case "list":
		case "list false":
			return instance.GetHelpList(includeDescription: false);
		default:
			if (instance.Commands.ContainsKey(param))
			{
				DeveloperConsole.ConsoleCommand consoleCommand = instance.Commands[param];
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(UGUITools.FormatColor(param, "2B99E7"));
				stringBuilder.Append(" ");
				stringBuilder.Append((consoleCommand.Parameters == null) ? string.Empty : UGUITools.FormatColor(consoleCommand.Parameters, "33EE33"));
				stringBuilder.Append(" ");
				stringBuilder.Append(consoleCommand.Description);
				stringBuilder.Append((consoleCommand.Help == null) ? string.Empty : UGUITools.FormatColor(consoleCommand.Help, "DCDCDC"));
				return stringBuilder.ToString();
			}
			return "Failed. Help not found for " + UGUITools.FormatColor(param, "33EE33");
		}
	}

	public static string TimeScale(string param)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (string.IsNullOrEmpty(param))
		{
			return "Invalid parameter. Enter a number greater than 0.";
		}
		if ((int)Game.State != 0)
		{
			return "Failed. Time scale can only be set at main menu.";
		}
		if (int.TryParse(param, out var result) && result > 0)
		{
			GameTime.TIME_SCALE = result;
			return "Success.";
		}
		return "Invalid parameter. Enter a number greater than 0.";
	}

	public static string System(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Operating System:\n");
		stringBuilder.AppendFormat("Version:         {0}\n", SystemInfo.operatingSystem);
		stringBuilder.AppendFormat("Processor Count: {0}\n", SystemInfo.processorCount);
		stringBuilder.AppendFormat("Processor Type:  {0}\n", SystemInfo.processorType);
		stringBuilder.AppendFormat("Memory:          {0} MB\n", SystemInfo.systemMemorySize);
		stringBuilder.Append("\n");
		stringBuilder.Append("Graphics Device:\n");
		stringBuilder.AppendFormat("Device:       {0}\n", SystemInfo.graphicsDeviceName);
		stringBuilder.AppendFormat("Vendor:       {0}\n", SystemInfo.graphicsDeviceVendor);
		stringBuilder.AppendFormat("Vendor Id:    {0}\n", SystemInfo.graphicsDeviceVendorID);
		stringBuilder.AppendFormat("Memory:       {0} MB\n", SystemInfo.graphicsMemorySize);
		stringBuilder.AppendFormat("Shader API:   {0}\n", SystemInfo.graphicsDeviceVersion);
		stringBuilder.AppendFormat("Shader Level: {0}", SystemInfo.graphicsShaderLevel);
		return stringBuilder.ToString();
	}

	public static string DebugOptions(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (string.IsNullOrEmpty(param))
		{
			return string.Format("Invalid parameter. Use {0}.", UGUITools.FormatColor("list", "33EE33"));
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Graphics Settings:\n");
		stringBuilder.AppendFormat("Preset:                 [{0}]\n", Options.GraphicsSettings.Preset);
		stringBuilder.AppendFormat("Resolution:             [{0}]\n", Options.GraphicsSettings.Resolution);
		stringBuilder.AppendFormat("Fixed Frame Rate:       [{0}]\n", Options.GraphicsSettings.FixedFrameRate);
		stringBuilder.AppendFormat("Vertical Sync:          [{0}]\n", Options.GraphicsSettings.VerticalSync);
		stringBuilder.AppendFormat("Full Screen:            [{0}]\n", Options.GraphicsSettings.FullScreen);
		stringBuilder.AppendFormat("Field Of View:          [{0}]\n", Options.GraphicsSettings.FieldOfView);
		stringBuilder.AppendFormat("Draw Distance:          [{0}]\n", Options.GraphicsSettings.DrawDistance);
		stringBuilder.AppendFormat("Texture Quality:        [{0}]\n", Options.GraphicsSettings.TextureQuality);
		stringBuilder.AppendFormat("Shadow Quality:         [{0}]\n", Options.GraphicsSettings.ShadowQuality);
		stringBuilder.AppendFormat("Shader Quality:         [{0}]\n", Options.GraphicsSettings.ShaderQuality);
		stringBuilder.AppendFormat("Antialiasing:           [{0}]\n", Options.GraphicsSettings.Antialiasing);
		stringBuilder.AppendFormat("Ambient Occlusion:      [{0}]\n", Options.GraphicsSettings.AmbientOcclusion);
		stringBuilder.AppendFormat("Motion Blur:            [{0}]\n", Options.GraphicsSettings.MotionBlur);
		stringBuilder.AppendFormat("Bloom:                  [{0}]\n", Options.GraphicsSettings.Bloom);
		stringBuilder.AppendFormat("Lens Effects:           [{0}]\n", Options.GraphicsSettings.LensEffects);
		stringBuilder.Append("\n");
		return stringBuilder.ToString();
	}

	public static string DumpLog(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		try
		{
			string text = DeveloperConsole.Instance.LogToFile();
			return "Success. Log file saved to: " + text;
		}
		catch (Exception ex)
		{
			return "Failed. Error writing log file to desktop. \n" + ex;
		}
	}

	public static string ViewLog(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance) || !Object.op_Implicit((Object)(object)DeveloperConsolePresenter.Instance))
		{
			return null;
		}
		if (string.IsNullOrEmpty(param))
		{
			DeveloperConsolePresenter.Instance.LoadLogs();
			return "Success.";
		}
		if (bool.TryParse(param, out var result))
		{
			DeveloperConsolePresenter.Instance.LoadLogs(result);
			return "Success.";
		}
		return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
	}

	public static string ClearLog(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance) || !Object.op_Implicit((Object)(object)DeveloperConsolePresenter.Instance))
		{
			return null;
		}
		DeveloperConsole.Instance.ClearLogs();
		return "Success.";
	}

	public static string Save(string param)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		SaveManager.SaveInternal(null, (int result) =>
		{
		});
		return "Success.";
	}

	public static string FPS(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (string.IsNullOrEmpty(param))
		{
			return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
		}
		if (bool.TryParse(param, out var result))
		{
			if (Object.op_Implicit((Object)(object)Singleton<FPSCounter>.Instance))
			{
				FPSCounter.Enable(result);
				return "Success.";
			}
			return "Failed. No FPS counter component found.";
		}
		return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
	}

	public static string TestConsole(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (string.IsNullOrEmpty(param))
		{
			return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
		}
		if (bool.TryParse(param, out var result))
		{
			if (Object.op_Implicit((Object)(object)Singleton<TestingConsole>.Instance))
			{
				((Behaviour)Singleton<TestingConsole>.Instance).enabled = result;
				return "Success.";
			}
			return "Failed. No TestingConsole component found.";
		}
		return "Invalid parameter. Use " + UGUITools.FormatColor("true", "33EE33") + " or " + UGUITools.FormatColor("false.", "33EE33");
	}

	public static string ChangeTime(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		if (!Object.op_Implicit((Object)(object)Singleton<Atmosphere>.Instance))
		{
			return "Failed. No Atmosphere component found.";
		}
		if (string.IsNullOrEmpty(param))
		{
			return "Invalid parameter. Enter a number between " + UGUITools.FormatColor("0", "33EE33") + " and " + UGUITools.FormatColor("24", "33EE33");
		}
		if (float.TryParse(param, out var result) && result >= 0f && result <= 24f)
		{
			Singleton<GameTime>.Instance.MilitaryTime = result;
			return "Success.";
		}
		return "Invalid parameter. Enter a number between " + UGUITools.FormatColor("0", "33EE33") + " and " + UGUITools.FormatColor("24", "33EE33");
	}

	public static string DebugAudio(string param)
	{
		if (!Object.op_Implicit((Object)(object)DeveloperConsole.Instance))
		{
			return null;
		}
		AudioManager audioManager = AudioManager.GetAudioPlayer() as AudioManager;
		if ((Object)(object)audioManager == (Object)null)
		{
			return "Failed. No AudioManager component found.";
		}
		string text = string.Empty;
		if (typeof(AudioManager).GetField("_actorPool", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(audioManager) is Stack<AudioActor> stack)
		{
			text = "Pooled Actors: " + stack.Count;
		}
		if (typeof(AudioManager).GetField("_activeActors", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(audioManager) is IList<AudioActor> list)
		{
			text = text + "\nActive Actors: " + list.Count;
		}
		return text + "\nSuccess.";
	}
}
