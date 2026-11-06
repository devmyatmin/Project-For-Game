using UnityEngine;
using UnityEngine.UI;

namespace Beam.UI;

public static class NewGameMenuPresenter_Extensions
{
	public static void SetNavigationOrder(this IButtonViewAdapter that, IButtonViewAdapter up, IButtonViewAdapter down)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Button component = ((Component)that.transform).GetComponent<Button>();
		if (!((Object)(object)component == (Object)null))
		{
			Navigation navigation = default;
			navigation.mode = (Mode)4;
			navigation.selectOnUp = (Selectable)(object)((up != null) ? ((Component)up.transform).GetComponent<Button>() : null);
			navigation.selectOnDown = (Selectable)(object)((down != null) ? ((Component)down.transform).GetComponent<Button>() : null);
			((Selectable)component).navigation = navigation;
		}
	}

	public static IButtonViewAdapter GetActive(this IButtonViewAdapter[] items, int index, int direction)
	{
		for (int i = index; i < items.Length && i >= 0; i += direction)
		{
			if (items[i] != null && items[i].Interactable && ((Component)items[i].transform).gameObject.activeSelf)
			{
				return items[i];
			}
		}
		return null;
	}

	public static void SetVerticalNaviationOrder(this IButtonViewAdapter[] items)
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].SetNavigationOrder(items.GetActive(i - 1, -1), items.GetActive(i + 1, 1));
		}
	}
}
