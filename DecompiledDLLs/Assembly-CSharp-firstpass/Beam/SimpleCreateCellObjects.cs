using UnityEngine;

namespace Beam;

public class SimpleCreateCellObjects<TCellObject> : ICreateCellObjects<TCellObject> where TCellObject : Object, ICellObject
{
	public TCellObject CreateCellObject(TCellObject prefab)
	{
		return Object.Instantiate<TCellObject>(prefab);
	}
}
