using System.Collections;
using UnityEngine;

namespace Beam;

public class Decal : MonoBehaviour
{
	private ITerrainProvider _terrain;

	[SerializeField]
	private float _size = 1f;

	[SerializeField]
	private float _offset = 0.03f;

	private IEnumerator Start()
	{
		Decal decal = this;
		ITerrainProvider instance = Singleton<StrandedWorld>.Instance;
		decal._terrain = instance ?? NullObject<NullTerrainProvider>.Instance;
		while (LevelLoader.IsLoading())
		{
			yield return null;
		}
		GenerateDecalMesh();
	}

	private void GenerateDecalMesh()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh();
		float size = _size;
		int num = 8;
		int num2 = num + 1;
		int num3 = num + 1;
		int num4 = num2 * num3;
		Vector3[] array = new Vector3[num4];
		Vector2[] array2 = new Vector2[num4];
		Vector2 val2 = new Vector2(1f / ((float)num3 - 1f), 1f / ((float)num2 - 1f));
		Vector3 val3 = new Vector3(size / ((float)num3 - 1f), 1f, size / ((float)num2 - 1f));
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				Vector3 val4 = new Vector3((float)j, 0f, (float)i);
				Vector3 val5 = Vector3.Scale(val3, val4) - new Vector3(size / 2f, 0f, size / 2f);
				Vector3 val6 = ((Component)this).transform.TransformPoint(val5);
				float height = _terrain.GetHeight(val6);
				array[j * num3 + i] = ((Component)this).transform.InverseTransformPoint(new Vector3(val6.x, height + _offset, val6.z));
				array2[j * num3 + i] = Vector2.Scale(new Vector2((float)j, (float)i), val2);
			}
		}
		val.vertices = array;
		val.uv = array2;
		int num5 = num + 1;
		int[] array3 = new int[num * num * 6];
		int k = 0;
		int num6 = 0;
		for (; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				array3[num6++] = k * num5 + l;
				array3[num6++] = k * num5 + (l + 1);
				array3[num6++] = (k + 1) * num5 + (l + 1);
				array3[num6++] = k * num5 + l;
				array3[num6++] = (k + 1) * num5 + l + 1;
				array3[num6++] = (k + 1) * num5 + l;
			}
		}
		val.triangles = array3;
		val.RecalculateNormals();
		val.RecalculateTangents();
		((Component)this).GetComponent<MeshFilter>().mesh = val;
	}
}
