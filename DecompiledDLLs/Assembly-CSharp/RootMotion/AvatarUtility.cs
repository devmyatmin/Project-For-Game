using System;
using System.Reflection;
using UnityEngine;

namespace RootMotion;

public class AvatarUtility
{
	public static Quaternion GetPostRotation(Avatar avatar, AvatarIKGoal avatarIKGoal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo? method = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		return (Quaternion)method.Invoke(avatar, new object[1] { num });
	}

	public static TQ GetIKGoalTQ(Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, TQ bodyPositionRotation, TQ boneTQ)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetAxisLength method.");
		}
		MethodInfo? method2 = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		Quaternion val = (Quaternion)method2.Invoke(avatar, new object[1] { num });
		TQ tQ = new TQ(boneTQ.t, boneTQ.q * val);
		if ((int)avatarIKGoal == 0 || (int)avatarIKGoal == 1)
		{
			float num2 = (float)method.Invoke(avatar, new object[1] { num });
			Vector3 val2 = new Vector3(num2, 0f, 0f);
			tQ.t += tQ.q * val2;
		}
		Quaternion val3 = Quaternion.Inverse(bodyPositionRotation.q);
		tQ.t = val3 * (tQ.t - bodyPositionRotation.t);
		tQ.q = val3 * tQ.q;
		tQ.t /= humanScale;
		tQ.q = Quaternion.LookRotation(tQ.q * Vector3.forward, tQ.q * Vector3.up);
		return tQ;
	}

	public static TQ WorldSpaceIKGoalToBone(TQ goalTQ, Avatar avatar, AvatarIKGoal avatarIKGoal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
		if (num == 55)
		{
			throw new InvalidOperationException("Invalid human id.");
		}
		MethodInfo method = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException("Cannot find GetAxisLength method.");
		}
		MethodInfo? method2 = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 == null)
		{
			throw new InvalidOperationException("Cannot find GetPostRotation method.");
		}
		Quaternion val = (Quaternion)method2.Invoke(avatar, new object[1] { num });
		if ((int)avatarIKGoal == 0 || (int)avatarIKGoal == 1)
		{
			float num2 = (float)method.Invoke(avatar, new object[1] { num });
			Vector3 val2 = new Vector3(num2, 0f, 0f);
			goalTQ.t -= goalTQ.q * val2;
		}
		return new TQ(goalTQ.t, goalTQ.q * Quaternion.Inverse(val));
	}

	public static TQ GetWorldSpaceIKGoal(BakerHumanoidQT ikQT, BakerHumanoidQT rootQT, float time, float humanScale)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		TQ tQ = ikQT.Evaluate(time);
		TQ tQ2 = rootQT.Evaluate(time);
		tQ.q = tQ2.q * tQ.q;
		tQ.t = tQ2.t + tQ2.q * tQ.t;
		tQ.t *= humanScale;
		return tQ;
	}

	public static HumanBodyBones HumanIDFromAvatarIKGoal(AvatarIKGoal avatarIKGoal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		return (int)avatarIKGoal switch
		{
			0 => (HumanBodyBones)5, 
			1 => (HumanBodyBones)6, 
			2 => (HumanBodyBones)17, 
			3 => (HumanBodyBones)18, 
			_ => (HumanBodyBones)55, 
		};
	}
}
