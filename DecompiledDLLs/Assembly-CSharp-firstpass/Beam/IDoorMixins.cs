using System;
using UnityEngine;

namespace Beam;

public static class IDoorMixins
{
	public static void Open(this IDoor door)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!door.IsOpening)
		{
			for (int i = 0; i < door.Doors.Length; i++)
			{
				Door door2 = door.Doors[i];
				float num = (door.IsOpened ? door2.ClosingDuration : door2.OpeningDuration);
				LeanTweenType ease = (door.IsOpened ? door2.ClosingEaseType : door2.OpeningEaseType);
				door.IsOpening = true;
				((Component)door2).gameObject.layer = Layers.INTERACTIVE_OBJECTS;
				LeanTween.value(((Component)door2).gameObject, (Action<float>)door2.RotateDoor, door.IsOpened ? door2.RangeOfMotion : 0f, door.IsOpened ? 0f : door2.RangeOfMotion, num).setEase(ease).setOnComplete((Action)door.SetDoorState);
			}
		}
	}

	public static void SetDoorState(this IDoor door)
	{
		if (door.IsOpening)
		{
			door.IsOpened = !door.IsOpened;
			door.IsOpening = false;
			for (int i = 0; i < door.Doors.Length; i++)
			{
				((Component)door.Doors[i]).gameObject.layer = door.gameObject.layer;
			}
		}
	}
}
