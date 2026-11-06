using UnityEngine;

namespace Ceto;

public static class OceanUtility
{
	public static int ShowLayer(int mask, string layer)
	{
		return mask | (1 << LayerMask.NameToLayer(layer));
	}

	public static int HideLayer(int mask, string layer)
	{
		return mask & ~(1 << LayerMask.NameToLayer(layer));
	}

	public static int ToggleLayer(int mask, string layer)
	{
		return mask ^ (1 << LayerMask.NameToLayer(layer));
	}

	public static int ShowLayer(int mask, LayerMask layer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return mask | (1 << LayerMask.op_Implicit(layer));
	}

	public static int HideLayer(int mask, LayerMask layer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return mask & ~(1 << LayerMask.op_Implicit(layer));
	}

	public static int ToggleLayer(int mask, LayerMask layer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return mask ^ (1 << LayerMask.op_Implicit(layer));
	}

	public static Mesh CreateQuadMesh()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected Obj, but got Unknown
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		array[0] = new Vector3(-1f, 0f, -1f);
		array[1] = new Vector3(1f, 0f, -1f);
		array[2] = new Vector3(-1f, 0f, 1f);
		array[3] = new Vector3(1f, 0f, 1f);
		array2[0] = new Vector2(0f, 0f);
		array2[1] = new Vector2(1f, 0f);
		array2[2] = new Vector2(0f, 1f);
		array2[3] = new Vector2(1f, 1f);
		return new Mesh
		{
			vertices = array,
			uv = array2,
			triangles = triangles
		};
	}
}
