using System;

namespace Beam.IO;

public static class Path
{
	public const char DirectorySeparatorChar = '/';

	public static string Combine(params string[] paths)
	{
		if (paths == null)
		{
			throw new ArgumentNullException("paths");
		}
		for (int i = 0; i < paths.Length; i++)
		{
			string text = paths[i];
			if (text == null)
			{
				throw new ArgumentNullException("path");
			}
			paths[i] = Sanitize(text);
			if (i != paths.Length - 1)
			{
				paths[i] = paths[i].TrimEnd('/');
			}
		}
		return Sanitize(string.Join($"{'/'}", paths));
	}

	public static string Sanitize(string filePath)
	{
		if (filePath == null)
		{
			throw new ArgumentNullException("filePath");
		}
		string text = filePath;
		if (text.Contains('\\'))
		{
			text = text.Replace('\\', '/');
		}
		return text.TrimStart('/');
	}
}
