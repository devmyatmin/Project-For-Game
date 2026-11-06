using System.Linq;
using Beam;
using UnityEngine;

[ExecuteInEditMode]
public class BoundsTest : MonoBehaviour
{
	public Bounds bounds;

	[SerializeField]
	private BoxCollider _collider;

	private Vector3[] _points = new Vector3[8];

	private void OnDrawGizmos()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		OOBB oobb = new OOBB(((Component)this).transform, _collider.size);
		float num = oobb.Size.x / 2f;
		float num2 = oobb.Size.y / 2f;
		float num3 = oobb.Size.z / 2f;
		_points[0] = new Vector3(num, num2, num3);
		_points[1] = new Vector3(0f - num, num2, num3);
		_points[2] = new Vector3(num, 0f - num2, num3);
		_points[3] = new Vector3(0f - num, 0f - num2, num3);
		_points[4] = new Vector3(num, num2, 0f - num3);
		_points[5] = new Vector3(0f - num, num2, 0f - num3);
		_points[6] = new Vector3(num, 0f - num2, 0f - num3);
		_points[7] = new Vector3(0f - num, 0f - num2, 0f - num3);
		Gizmos.color = Color.green;
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < _points.Length; i++)
		{
			array[i] = ((Component)this).transform.TransformPoint(_points[i]);
		}
		Vector3[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			Gizmos.DrawSphere(array2[j], 0.2f);
		}
		Gizmos.color = Color.white;
		Test(oobb);
	}

	public void Test(OOBB oobb)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		foreach (BoxCollider item in (from c in Physics.OverlapSphere(((Component)this).transform.position, 3f)
			where (Object)(object)c != (Object)(object)_collider && c is BoxCollider
			select c).Cast<BoxCollider>())
		{
			if (new OOBB(((Component)item).transform, item.size).Intersects(oobb))
			{
				Debug.Log((object)"INTERSECTING!!");
			}
		}
	}
}
