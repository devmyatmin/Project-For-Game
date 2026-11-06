using System.Runtime.CompilerServices;
using Beam.Crafting;
using UnityEngine;

namespace Beam;

public class BaseActionEventData : IBaseActionEventData
{
	public static BaseActionEventData Empty { get; } = new BaseActionEventData();

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Vector3 OtherPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public CraftingType OtherType { get; set; }

	public int OtherOwnerID { get; set; }

	public float Value { get; set; }

	public BaseActionEventData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Position = Vector3.zero;
		OtherPosition = Vector3.zero;
		OtherType = CraftingType.Empty;
		OtherOwnerID = -1;
		base._002Ector();
	}
}
