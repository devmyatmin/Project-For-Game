using System;
using System.IO;
using System.Linq;

namespace Beam.Utilities;

public static class PathTools
{
	public static string FormatDirectorySeparators(string path)
	{
		return path.Replace('/', Path.DirectorySeparatorChar);
	}

	public static string Combine(params string[] paths)
	{
		if (paths == null)
		{
			throw new ArgumentNullException("paths");
		}
		return FormatDirectorySeparators(paths.Aggregate(Path.Combine));
	}
}
