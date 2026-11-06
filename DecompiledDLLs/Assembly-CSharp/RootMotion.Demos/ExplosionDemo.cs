using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class ExplosionDemo : MonoBehaviour
{
	public SimpleLocomotion character;

	public float forceMlp = 1f;

	public float upForce = 1f;

	public float weightFalloffSpeed = 1f;

	public AnimationCurve weightFalloff;

	public AnimationCurve explosionForceByDistance;

	public AnimationCurve scale;

	private float weight;

	private Vector3 defaultScale = Vector3.one;

	private Rigidbody r;

	private FullBodyBipedIK ik;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		defaultScale = ((Component)this).transform.localScale;
		r = ((Component)character).GetComponent<Rigidbody>();
		ik = ((Component)character).GetComponent<FullBodyBipedIK>();
	}

	private void Update()
	{
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		weight = Mathf.Clamp(weight - Time.deltaTime * weightFalloffSpeed, 0f, 1f);
		if (Input.GetKeyDown((KeyCode)101) && character.isGrounded)
		{
			ik.solver.IKPositionWeight = 1f;
			ik.solver.leftHandEffector.position = ik.solver.leftHandEffector.bone.position;
			ik.solver.rightHandEffector.position = ik.solver.rightHandEffector.bone.position;
			ik.solver.leftFootEffector.position = ik.solver.leftFootEffector.bone.position;
			ik.solver.rightFootEffector.position = ik.solver.rightFootEffector.bone.position;
			weight = 1f;
			Vector3 val = r.position - ((Component)this).transform.position;
			val.y = 0f;
			float num = explosionForceByDistance.Evaluate(val.magnitude);
			r.velocity = (val.normalized + Vector3.up * upForce) * num * forceMlp;
		}
		if (weight < 0.5f && character.isGrounded)
		{
			weight = Mathf.Clamp(weight - Time.deltaTime * 3f, 0f, 1f);
		}
		SetEffectorWeights(weightFalloff.Evaluate(weight));
		((Component)this).transform.localScale = scale.Evaluate(weight) * defaultScale;
	}

	private void SetEffectorWeights(float w)
	{
		ik.solver.leftHandEffector.positionWeight = w;
		ik.solver.rightHandEffector.positionWeight = w;
		ik.solver.leftFootEffector.positionWeight = w;
		ik.solver.rightFootEffector.positionWeight = w;
	}

	public ExplosionDemo()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
	}
}
