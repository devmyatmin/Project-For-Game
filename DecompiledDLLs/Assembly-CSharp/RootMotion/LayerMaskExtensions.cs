using System.Collections.Generic;
using UnityEngine;

namespace RootMotion;

public static class LayerMaskExtensions
{
	public static bool Contains(LayerMask mask, int layer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return LayerMask.op_Implicit(mask) == (LayerMask.op_Implicit(mask) | (1 << layer));
	}

	public static LayerMask Create(params string[] layerNames)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return NamesToMask(layerNames);
	}

	public static LayerMask Create(params int[] layerNumbers)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LayerNumbersToMask(layerNumbers);
	}

	public static LayerMask NamesToMask(params string[] layerNames)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = LayerMask.op_Implicit(0);
		foreach (string text in layerNames)
		{
			val = LayerMask.op_Implicit(LayerMask.op_Implicit(val) | (1 << LayerMask.NameToLayer(text)));
		}
		return val;
	}

	public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = LayerMask.op_Implicit(0);
		foreach (int num in layerNumbers)
		{
			val = LayerMask.op_Implicit(LayerMask.op_Implicit(val) | (1 << num));
		}
		return val;
	}

	public static LayerMask Inverse(this LayerMask original)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return LayerMask.op_Implicit(~LayerMask.op_Implicit(original));
	}

	public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return LayerMask.op_Implicit(LayerMask.op_Implicit(original) | LayerMask.op_Implicit(NamesToMask(layerNames)));
	}

	public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return LayerMask.op_Implicit(~(LayerMask.op_Implicit(LayerMask.op_Implicit(~LayerMask.op_Implicit(original))) | LayerMask.op_Implicit(NamesToMask(layerNames))));
	}

	public static string[] MaskToNames(this LayerMask original)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if ((LayerMask.op_Implicit(original) & num) == num)
			{
				string text = LayerMask.LayerToName(i);
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
			}
		}
		return list.ToArray();
	}

	public static int[] MaskToNumbers(this LayerMask original)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if ((LayerMask.op_Implicit(original) & num) == num)
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	public static string MaskToString(this LayerMask original)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return original.MaskToString(", ");
	}

	public static string MaskToString(this LayerMask original, string delimiter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return string.Join(delimiter, original.MaskToNames());
	}
}
