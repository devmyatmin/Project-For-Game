using UnityEngine;

public class ModuleConnector : MonoBehaviour
{
	public string[] Tags;

	public bool IsDefault;

	private void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + ((Component)this).transform.forward * num);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position - ((Component)this).transform.right * num);
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + ((Component)this).transform.right * num);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + Vector3.up * num);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(((Component)this).transform.position, 0.125f);
	}
}
