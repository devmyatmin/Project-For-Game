using System;
using UnityEngine;

namespace RootMotion;

[Serializable]
public class BakerHumanoidQT
{
	private Transform transform;

	private string Qx;

	private string Qy;

	private string Qz;

	private string Qw;

	private string Tx;

	private string Ty;

	private string Tz;

	public AnimationCurve rotX;

	public AnimationCurve rotY;

	public AnimationCurve rotZ;

	public AnimationCurve rotW;

	public AnimationCurve posX;

	public AnimationCurve posY;

	public AnimationCurve posZ;

	private AvatarIKGoal goal;

	private Quaternion lastQ;

	private bool lastQSet;

	public BakerHumanoidQT(string name)
	{
		Qx = name + "Q.x";
		Qy = name + "Q.y";
		Qz = name + "Q.z";
		Qw = name + "Q.w";
		Tx = name + "T.x";
		Ty = name + "T.y";
		Tz = name + "T.z";
		Reset();
	}

	public BakerHumanoidQT(Transform transform, AvatarIKGoal goal, string name)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		this.transform = transform;
		this.goal = goal;
		Qx = name + "Q.x";
		Qy = name + "Q.y";
		Qz = name + "Q.z";
		Qw = name + "Q.w";
		Tx = name + "T.x";
		Ty = name + "T.y";
		Tz = name + "T.z";
		Reset();
	}

	public Quaternion EvaluateRotation(float time)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new Quaternion(rotX.Evaluate(time), rotY.Evaluate(time), rotZ.Evaluate(time), rotW.Evaluate(time));
	}

	public Vector3 EvaluatePosition(float time)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(posX.Evaluate(time), posY.Evaluate(time), posZ.Evaluate(time));
	}

	public TQ Evaluate(float time)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return new TQ(EvaluatePosition(time), EvaluateRotation(time));
	}

	public void GetCurvesFromClip(AnimationClip clip, Animator animator)
	{
	}

	public void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected Obj, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected Obj, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		rotX = new AnimationCurve();
		rotY = new AnimationCurve();
		rotZ = new AnimationCurve();
		rotW = new AnimationCurve();
		posX = new AnimationCurve();
		posY = new AnimationCurve();
		posZ = new AnimationCurve();
		lastQ = Quaternion.identity;
		lastQSet = false;
	}

	public void SetIKKeyframes(float time, Avatar avatar, Transform root, float humanScale, Vector3 bodyPosition, Quaternion bodyRotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = transform.position;
		Quaternion val2 = transform.rotation;
		if ((Object)(object)root.parent != (Object)null)
		{
			val = root.parent.InverseTransformPoint(val);
			val2 = Quaternion.Inverse(root.parent.rotation) * val2;
		}
		TQ iKGoalTQ = AvatarUtility.GetIKGoalTQ(avatar, humanScale, goal, new TQ(bodyPosition, bodyRotation), new TQ(val, val2));
		Quaternion val3 = iKGoalTQ.q;
		if (lastQSet)
		{
			val3 = BakerUtilities.EnsureQuaternionContinuity(lastQ, iKGoalTQ.q);
		}
		lastQ = val3;
		lastQSet = true;
		rotX.AddKey(time, val3.x);
		rotY.AddKey(time, val3.y);
		rotZ.AddKey(time, val3.z);
		rotW.AddKey(time, val3.w);
		Vector3 t = iKGoalTQ.t;
		posX.AddKey(time, t.x);
		posY.AddKey(time, t.y);
		posZ.AddKey(time, t.z);
	}

	public void SetKeyframes(float time, Vector3 pos, Quaternion rot)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		rotX.AddKey(time, rot.x);
		rotY.AddKey(time, rot.y);
		rotZ.AddKey(time, rot.z);
		rotW.AddKey(time, rot.w);
		posX.AddKey(time, pos.x);
		posY.AddKey(time, pos.y);
		posZ.AddKey(time, pos.z);
	}

	public void MoveLastKeyframes(float time)
	{
		MoveLastKeyframe(time, rotX);
		MoveLastKeyframe(time, rotY);
		MoveLastKeyframe(time, rotZ);
		MoveLastKeyframe(time, rotW);
		MoveLastKeyframe(time, posX);
		MoveLastKeyframe(time, posY);
		MoveLastKeyframe(time, posZ);
	}

	public void SetLoopFrame(float time)
	{
		BakerUtilities.SetLoopFrame(time, rotX);
		BakerUtilities.SetLoopFrame(time, rotY);
		BakerUtilities.SetLoopFrame(time, rotZ);
		BakerUtilities.SetLoopFrame(time, rotW);
		BakerUtilities.SetLoopFrame(time, posX);
		BakerUtilities.SetLoopFrame(time, posY);
		BakerUtilities.SetLoopFrame(time, posZ);
	}

	public void SetRootLoopFrame(float time)
	{
	}

	private void MoveLastKeyframe(float time, AnimationCurve curve)
	{
		Keyframe[] keys = curve.keys;
		keys[^1].time = time;
		curve.keys = keys;
	}

	public void MultiplyLength(AnimationCurve curve, float mlp)
	{
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			ref Keyframe reference = ref keys[i];
			reference.time *= mlp;
		}
		curve.keys = keys;
	}

	public void SetCurves(ref AnimationClip clip, float maxError, float lengthMlp)
	{
		MultiplyLength(rotX, lengthMlp);
		MultiplyLength(rotY, lengthMlp);
		MultiplyLength(rotZ, lengthMlp);
		MultiplyLength(rotW, lengthMlp);
		MultiplyLength(posX, lengthMlp);
		MultiplyLength(posY, lengthMlp);
		MultiplyLength(posZ, lengthMlp);
		BakerUtilities.ReduceKeyframes(rotX, maxError);
		BakerUtilities.ReduceKeyframes(rotY, maxError);
		BakerUtilities.ReduceKeyframes(rotZ, maxError);
		BakerUtilities.ReduceKeyframes(rotW, maxError);
		BakerUtilities.ReduceKeyframes(posX, maxError);
		BakerUtilities.ReduceKeyframes(posY, maxError);
		BakerUtilities.ReduceKeyframes(posZ, maxError);
		BakerUtilities.SetTangentMode(rotX);
		BakerUtilities.SetTangentMode(rotY);
		BakerUtilities.SetTangentMode(rotZ);
		BakerUtilities.SetTangentMode(rotW);
		clip.SetCurve(string.Empty, typeof(Animator), Qx, rotX);
		clip.SetCurve(string.Empty, typeof(Animator), Qy, rotY);
		clip.SetCurve(string.Empty, typeof(Animator), Qz, rotZ);
		clip.SetCurve(string.Empty, typeof(Animator), Qw, rotW);
		clip.SetCurve(string.Empty, typeof(Animator), Tx, posX);
		clip.SetCurve(string.Empty, typeof(Animator), Ty, posY);
		clip.SetCurve(string.Empty, typeof(Animator), Tz, posZ);
	}
}
