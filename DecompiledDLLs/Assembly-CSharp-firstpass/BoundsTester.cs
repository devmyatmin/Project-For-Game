using System.Linq;
using Beam;
using UnityEngine;

public class BoundsTester : MonoBehaviour
{
	[SerializeField]
	private BoxCollider _collider;

	[SerializeField]
	private LayerMask _layermask;

	public void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		OOBB oobb = new OOBB(((Component)this).transform, _collider.size);
		foreach (BoxCollider item in (from c in Physics.OverlapSphere(((Component)this).transform.position, 3f, LayerMask.op_Implicit(_layermask))
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
