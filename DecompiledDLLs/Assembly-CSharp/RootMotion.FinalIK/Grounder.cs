using UnityEngine;

namespace RootMotion.FinalIK;

public abstract class Grounder : MonoBehaviour
{
	public delegate void GrounderDelegate();

	[Tooltip("The master weight. Use this to fade in/out the grounding effect.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("The Grounding solver. Not to confuse with IK solvers.")]
	public Grounding solver = new Grounding();

	public GrounderDelegate OnPreGrounder;

	public GrounderDelegate OnPostGrounder;

	public GrounderDelegate OnPostIK;

	public bool initiated { get; protected set; }

	public abstract void ResetPosition();

	protected Vector3 GetSpineOffsetTarget()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int i = 0; i < solver.legs.Length; i++)
		{
			val += GetLegSpineBendVector(solver.legs[i]);
		}
		return val;
	}

	protected void LogWarning(string message)
	{
		Warning.Log(message, ((Component)this).transform);
	}

	private Vector3 GetLegSpineBendVector(Grounding.Leg leg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 legSpineTangent = GetLegSpineTangent(leg);
		float num = (Vector3.Dot(solver.root.forward, legSpineTangent.normalized) + 1f) * 0.5f;
		Vector3 val = leg.IKPosition - leg.transform.position;
		float magnitude = val.magnitude;
		return legSpineTangent * magnitude * num;
	}

	private Vector3 GetLegSpineTangent(Grounding.Leg leg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = leg.transform.position - solver.root.position;
		if (!solver.rotateSolver || solver.root.up == Vector3.up)
		{
			return new Vector3(val.x, 0f, val.z);
		}
		Vector3 up = solver.root.up;
		Vector3.OrthoNormalize(ref up, ref val);
		return val;
	}

	protected abstract void OpenUserManual();

	protected abstract void OpenScriptReference();
}
