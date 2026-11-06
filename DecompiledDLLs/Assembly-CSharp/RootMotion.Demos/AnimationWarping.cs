using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class AnimationWarping : OffsetModifier
{
	[Serializable]
	public struct Warp
	{
		[Tooltip("Layer of the 'Animation State' in the Animator.")]
		public int animationLayer;

		[Tooltip("Name of the state in the Animator to warp.")]
		public string animationState;

		[Tooltip("Warping weight by normalized time of the animation state.")]
		public AnimationCurve weightCurve;

		[Tooltip("Animated point to warp from. This should be in character space so keep this Transform parented to the root of the character.")]
		public Transform warpFrom;

		[Tooltip("World space point to warp to.")]
		public Transform warpTo;

		[Tooltip("Which FBBIK effector to use?")]
		public FullBodyBipedEffector effector;
	}

	[Serializable]
	public enum EffectorMode
	{
		PositionOffset,
		Position
	}

	[Tooltip("Reference to the Animator component to use")]
	public Animator animator;

	[Tooltip("Using effector.positionOffset or effector.position with effector.positionWeight? The former will enable you to use effector.position for other things, the latter will weigh in the effectors, hence using Reach and Pull in the process.")]
	public EffectorMode effectorMode;

	[Space(10f)]
	[Tooltip("The array of warps, can have multiple simultaneous warps.")]
	public Warp[] warps;

	private EffectorMode lastMode;

	protected override void Start()
	{
		base.Start();
		lastMode = effectorMode;
	}

	public float GetWarpWeight(int warpIndex)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (warpIndex < 0)
		{
			Debug.LogError((object)"Warp index out of range.");
			return 0f;
		}
		if (warpIndex >= warps.Length)
		{
			Debug.LogError((object)"Warp index out of range.");
			return 0f;
		}
		if ((Object)(object)animator == (Object)null)
		{
			Debug.LogError((object)"Animator unassigned in AnimationWarping");
			return 0f;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(warps[warpIndex].animationLayer);
		if (!currentAnimatorStateInfo.IsName(warps[warpIndex].animationState))
		{
			return 0f;
		}
		return warps[warpIndex].weightCurve.Evaluate(currentAnimatorStateInfo.normalizedTime - (float)(int)currentAnimatorStateInfo.normalizedTime);
	}

	protected override void OnModifyOffset()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < warps.Length; i++)
		{
			float warpWeight = GetWarpWeight(i);
			Vector3 val = warps[i].warpTo.position - warps[i].warpFrom.position;
			switch (effectorMode)
			{
			case EffectorMode.PositionOffset:
			{
				IKEffector effector = ik.solver.GetEffector(warps[i].effector);
				effector.positionOffset += val * warpWeight * weight;
				break;
			}
			case EffectorMode.Position:
				ik.solver.GetEffector(warps[i].effector).position = ik.solver.GetEffector(warps[i].effector).bone.position + val;
				ik.solver.GetEffector(warps[i].effector).positionWeight = weight * warpWeight;
				break;
			}
		}
		if (lastMode == EffectorMode.Position && effectorMode == EffectorMode.PositionOffset)
		{
			Warp[] array = warps;
			for (int j = 0; j < array.Length; j++)
			{
				Warp warp = array[j];
				ik.solver.GetEffector(warp.effector).positionWeight = 0f;
			}
		}
		lastMode = effectorMode;
	}

	private void OnDisable()
	{
		if (effectorMode == EffectorMode.Position)
		{
			Warp[] array = warps;
			for (int i = 0; i < array.Length; i++)
			{
				Warp warp = array[i];
				ik.solver.GetEffector(warp.effector).positionWeight = 0f;
			}
		}
	}
}
