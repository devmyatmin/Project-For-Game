using System;
using UnityEngine;

public class Sinoid : MonoBehaviour
{
	public float AngularVelocity = 2f;

	public float SineFreq = 0.2f;

	public float Amplitude = 0.25f;

	private float accuTime;

	private Vector3 startPos;

	public void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		startPos = ((Component)this).transform.position;
	}

	public void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		accuTime += Time.deltaTime;
		((Component)this).transform.position = startPos + Vector3.up * Amplitude * Mathf.Sin(accuTime * 2f * (float)Math.PI * SineFreq);
		((Component)this).transform.Rotate((Vector3.up + Vector3.forward) * AngularVelocity * Time.deltaTime);
	}
}
