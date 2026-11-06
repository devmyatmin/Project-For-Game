using System.Collections.Generic;

namespace LE_LevelEditor.Core;

public class LE_ObjectMap
{
	public string CategoryName;

	public List<LE_Object> ObjectPrefabs = new List<LE_Object>();

	public List<LE_ObjectMap> SubObjectMaps = new List<LE_ObjectMap>();
}
