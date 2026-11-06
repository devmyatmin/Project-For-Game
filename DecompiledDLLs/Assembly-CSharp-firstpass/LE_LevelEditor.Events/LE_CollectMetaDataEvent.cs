using System;
using System.Collections.Generic;

namespace LE_LevelEditor.Events;

public class LE_CollectMetaDataEvent : EventArgs
{
	private readonly Dictionary<string, string> m_levelMetaData = new Dictionary<string, string>();

	public Dictionary<string, string> LevelMetaData => m_levelMetaData;

	public KeyValuePair<string, string>[] GetCollectedMetaData()
	{
		KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[m_levelMetaData.Count];
		int num = 0;
		foreach (KeyValuePair<string, string> levelMetaDatum in m_levelMetaData)
		{
			array[num] = levelMetaDatum;
			num++;
		}
		return array;
	}
}
