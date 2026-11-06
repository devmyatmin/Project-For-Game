using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class TerrainOffset : MonoBehaviour
{
	public AimIK aimIK;

	public Vector3 raycastOffset = new Vector3(0f, 2f, 1.5f);

	public LayerMask raycastLayers;

	public float min = -2f;

	public float max = 2f;

	public float lerpSpeed = 10f;

	private RaycastHit hit;

	private Vector3 offset;

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.rotation * raycastOffset;
		Vector3 groundHeightOffset = GetGroundHeightOffset(((Component)this).transform.position + val);
		offset = Vector3.Lerp(offset, groundHeightOffset, Time.deltaTime * lerpSpeed);
		Vector3 val2 = ((Component)this).transform.position + new Vector3(val.x, 0f, val.z);
		aimIK.solver.transform.LookAt(val2);
		aimIK.solver.IKPosition = val2 + offset;
	}

	private Vector3 GetGroundHeightOffset(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Debug.DrawRay(worldPosition, Vector3.down * raycastOffset.y * 2f, Color.green);
		if (Physics.Raycast(worldPosition, Vector3.down, ref hit, raycastOffset.y * 2f))
		{
			return Mathf.Clamp(hit.point.y - ((Component)this).transform.position.y, min, max) * Vector3.up;
		}
		return Vector3.zero;
	}

	public TerrainOffset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
