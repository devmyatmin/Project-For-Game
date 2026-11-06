using UnityEngine;

public class SailTest : MonoBehaviour
{
	public Transform Sail;

	public Transform Rudder;

	public float bowDir;

	public float headWind;

	public float keelDir;

	public float sailDir;

	public float sailDotWind;

	public float directionalFactor;

	[Header("Force")]
	public float Force;

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		Vector3 left = Vector3.left;
		if (Object.op_Implicit((Object)(object)Rudder))
		{
			bowDir = Vector3.Dot(((Component)this).transform.forward, left);
			headWind = Mathf.Clamp01(bowDir * 0.5f + 0.5f);
			keelDir = Mathf.Abs(bowDir);
			keelDir += 1f - Mathf.Abs(Vector3.Dot(Sail.forward, ((Component)this).transform.forward));
			keelDir = Mathf.Clamp01(keelDir);
			sailDir = 1f - Mathf.Abs(Vector3.Dot(Sail.forward, left));
			sailDotWind = Mathf.Clamp01(0f - Vector3.Dot(Sail.forward, left));
			directionalFactor = Mathf.Lerp(sailDotWind, 1f, Mathf.Clamp01(bowDir));
			sailDir *= directionalFactor;
			float num = keelDir * sailDir * headWind;
			Vector3 val = ((Component)this).transform.forward * Mathf.Cos(Rudder.localRotation.y) - ((Component)this).transform.right * Mathf.Sin(Rudder.localRotation.y);
			float num2 = 30f * num;
			num2 *= 5f;
			num2 = Mathf.Clamp(num2, 0f, 30f);
			Vector3 val2 = val * num2;
			Force = val2.magnitude;
			Vector3 val3 = val2;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + Sail.forward);
			Gizmos.DrawWireSphere(((Component)this).transform.position + Sail.forward, 0.2f);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + left);
			Gizmos.DrawWireSphere(((Component)this).transform.position + left, 0.2f);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + val3);
			Gizmos.DrawWireSphere(((Component)this).transform.position + val3, 0.1f);
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + ((Component)this).transform.forward);
			Gizmos.DrawWireSphere(((Component)this).transform.position + ((Component)this).transform.forward, 0.2f);
			Gizmos.color = Color.white;
		}
	}
}
