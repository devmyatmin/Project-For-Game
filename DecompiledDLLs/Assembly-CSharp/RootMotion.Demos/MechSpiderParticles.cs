using UnityEngine;

namespace RootMotion.Demos;

public class MechSpiderParticles : MonoBehaviour
{
	public MechSpiderController mechSpiderController;

	private ParticleSystem particles;

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		particles = (ParticleSystem)((Component)this).GetComponent(typeof(ParticleSystem));
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		Vector3 inputVector = mechSpiderController.inputVector;
		float magnitude = inputVector.magnitude;
		float num = Mathf.Clamp(magnitude * 50f, 30f, 50f);
		EmissionModule emission = particles.emission;
		emission.rateOverTime = new MinMaxCurve(num);
		MainModule main = particles.main;
		MainModule main2 = particles.main;
		MinMaxGradient startColor = main2.startColor;
		float r = startColor.color.r;
		main2 = particles.main;
		startColor = main2.startColor;
		float g = startColor.color.g;
		main2 = particles.main;
		startColor = main2.startColor;
		main.startColor = MinMaxGradient.op_Implicit(new Color(r, g, startColor.color.b, Mathf.Clamp(magnitude, 0.4f, 1f)));
	}
}
