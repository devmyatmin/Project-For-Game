using UnityEngine;

namespace RootMotion.Demos;

public class TransferMotion : MonoBehaviour
{
	[Tooltip("The Transform to transfer motion to.")]
	public Transform to;

	[Tooltip("The amount of motion to transfer.")]
	[Range(0f, 1f)]
	public float transferMotion = 0.9f;

	private Vector3 lastPosition;

	private void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		lastPosition = ((Component)this).transform.position;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - lastPosition;
		Transform val2 = to;
		val2.position += val * transferMotion;
		lastPosition = ((Component)this).transform.position;
	}
}
