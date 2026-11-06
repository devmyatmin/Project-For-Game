using Beam.UI;
using Funlabs;

namespace Beam.Crafting;

public class InteractiveObject_LABELMAKER : InteractiveObject
{
	public override void UseOnObject(IBase obj)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			LocalizedNotification.Post(Owner, NotificationPriority.Immediate, 4f, "LABELMAKER_NTF_BROKEN_DESC");
		}
		else
		{
			Owner.PlayerUI?.GetController<LabelmakerController>()?.Show(obj);
		}
	}
}
