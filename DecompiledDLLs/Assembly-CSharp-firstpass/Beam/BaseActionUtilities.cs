using Beam.Utilities;

namespace Beam;

public static class BaseActionUtilities
{
	public static void Populate(this BaseActionEventData data, IBase owner, IBase other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		data.Position = owner.transform.position;
		data.Value = 0f;
		InteractiveObject interactiveObject = other as InteractiveObject;
		if (!BeamTeam.IsNullOrDestroyed(interactiveObject))
		{
			data.OtherOwnerID = interactiveObject.OwnerId;
		}
		if (!BeamTeam.IsNullOrDestroyed(other))
		{
			data.OtherPosition = other.transform.position;
			data.OtherType = other.CraftingType;
		}
		else
		{
			data.OtherPosition = BaseActionEventData.Empty.OtherPosition;
			data.OtherType = BaseActionEventData.Empty.OtherType;
		}
	}
}
