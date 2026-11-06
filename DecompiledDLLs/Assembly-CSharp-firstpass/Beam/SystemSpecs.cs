using UnityEngine;

namespace Beam;

public static class SystemSpecs
{
	private const int MINIMUM_SYSTEM_MEMORY = 2000;

	private const int MINIMUM_GRAPHICS_MEMORY = 500;

	public static bool CheckSystemMemory()
	{
		if (Mathf.Approximately((float)SystemInfo.systemMemorySize, 0f))
		{
			return true;
		}
		return SystemInfo.systemMemorySize >= 2000;
	}

	public static bool CheckGraphicsMemory()
	{
		if (Mathf.Approximately((float)SystemInfo.graphicsMemorySize, 0f))
		{
			return true;
		}
		return SystemInfo.graphicsMemorySize >= 500;
	}

	public static bool CheckOperatingSystem()
	{
		return true;
	}

	public static bool CheckDirectX11Support()
	{
		string text = SystemInfo.graphicsDeviceVersion.ToLower();
		if (!text.Contains("direct3d 11"))
		{
			return text.Contains("directx 11");
		}
		return true;
	}

	public static bool CheckGPUInstancingSupport()
	{
		return SystemInfo.supportsInstancing;
	}
}
