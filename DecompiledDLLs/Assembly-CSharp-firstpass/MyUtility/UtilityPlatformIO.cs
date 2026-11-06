using System.IO;

namespace MyUtility;

public static class UtilityPlatformIO
{
	public static void SaveToFile(string p_filePath, string p_fileContentAsString)
	{
		using FileStream stream = File.Open(p_filePath, FileMode.Create);
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.Write(p_fileContentAsString);
	}
}
