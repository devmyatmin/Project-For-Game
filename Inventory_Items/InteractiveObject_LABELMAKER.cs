using Beam.UI;
using Funlabs;

namespace Beam.Crafting;

public class InteractiveObject_LABELMAKER : InteractiveObject
{
	public override void UseOnObject(IBase obj)
	{
		if (Game.Mode.IsMultiplayer())
		{
			LocalizedNotification.Post(base.Owner, NotificationPriority.Immediate, 4f, "LABELMAKER_NTF_BROKEN_DESC");
		}
		else
		{
			base.Owner.PlayerUI?.GetController<LabelmakerController>()?.Show(obj);
		}
	}
}
