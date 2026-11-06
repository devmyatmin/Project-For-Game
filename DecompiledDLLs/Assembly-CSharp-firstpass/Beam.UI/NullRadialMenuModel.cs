using UnityEngine;

namespace Beam.UI;

public class NullRadialMenuModel : NullObject<NullRadialMenuModel>, IRadialMenuModel
{
	public string GetDisplayName()
	{
		Debug.LogError((object)"NullRadialMenuModel::GetDisplayName:: Not implemented!");
		return string.Empty;
	}

	public string GetDescription()
	{
		Debug.LogError((object)"NullRadialMenuModel::GetDescription:: Not implemented!");
		return string.Empty;
	}
}
