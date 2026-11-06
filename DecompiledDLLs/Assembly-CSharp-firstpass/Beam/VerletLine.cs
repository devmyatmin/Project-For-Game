using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class VerletLine : MonoBehaviour
{
	public Transform CableStart;

	public Transform CableEnd;

	public float CableLength = 10f;

	public int CableSegments = 10;

	public int SolverIterations = 1;

	[SerializeField]
	private LineRenderer _lineRenderer;

	private readonly List<VParticle> _particles = new List<VParticle>();

	private float _timeRemainder;

	private float SubstepTime = 0.02f;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = CableEnd.position - CableStart.position;
		_particles.Clear();
		int num = CableSegments + 1;
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i / (float)num;
			VParticle vParticle = new VParticle(((Component)CableStart).transform.position + num2 * val);
			vParticle.Free = i != 0 && i != num - 1;
			_particles.Add(vParticle);
		}
		_lineRenderer.positionCount = _particles.Count;
	}

	private void Update()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		VParticle vParticle = _particles[0];
		vParticle.Position = (vParticle.OldPosition = CableStart.position);
		VParticle vParticle2 = _particles[CableSegments];
		vParticle2.Position = (vParticle2.OldPosition = CableEnd.position);
		Vector3 gravity = Physics.gravity;
		float num = Mathf.Max(SubstepTime, 0.005f);
		_timeRemainder += Time.deltaTime;
		while (_timeRemainder > num)
		{
			PreformSubstep(num, gravity);
			_timeRemainder -= num;
		}
	}

	private void LateUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _particles.Count; i++)
		{
			VParticle vParticle = _particles[i];
			_lineRenderer.SetPosition(i, vParticle.Position);
		}
	}

	private void PreformSubstep(float substepTime, Vector3 gravity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		VerletIntegrate(substepTime, gravity);
		SolveConstraints();
	}

	private void VerletIntegrate(float substepTime, Vector3 gravity)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		int num = CableSegments + 1;
		float num2 = substepTime * substepTime;
		for (int i = 0; i < num; i++)
		{
			VParticle vParticle = _particles[i];
			if (vParticle.Free)
			{
				Vector3 val = vParticle.Position - vParticle.OldPosition;
				Vector3 position = vParticle.Position + val + num2 * gravity;
				vParticle.OldPosition = vParticle.Position;
				vParticle.Position = position;
			}
		}
	}

	private void SolveConstraints()
	{
		float distance = CableLength / (float)CableSegments;
		for (int i = 0; i < SolverIterations; i++)
		{
			for (int j = 0; j < CableSegments; j++)
			{
				VParticle particleA = _particles[j];
				VParticle particleB = _particles[j + 1];
				SolveDistanceConstraint(particleA, particleB, distance);
			}
		}
	}

	private void SolveDistanceConstraint(VParticle particleA, VParticle particleB, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = particleB.Position - particleA.Position;
		float magnitude = val.magnitude;
		float num = (magnitude - distance) / magnitude;
		if (particleA.Free && particleB.Free)
		{
			particleA.Position += num * 0.5f * val;
			particleB.Position -= num * 0.5f * val;
		}
		else if (particleA.Free)
		{
			particleA.Position += num * val;
		}
		else if (particleB.Free)
		{
			particleB.Position -= num * val;
		}
	}
}
