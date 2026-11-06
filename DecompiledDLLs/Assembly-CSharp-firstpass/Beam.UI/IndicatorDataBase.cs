using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam.UI;

[Serializable]
public class IndicatorDataBase
{
	public Vector3 Direction
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

	public IndicatorDataBase(Vector3 direction)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Direction = direction;
	}
}
