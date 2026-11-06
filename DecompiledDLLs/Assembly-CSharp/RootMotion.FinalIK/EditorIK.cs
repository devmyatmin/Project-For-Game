using UnityEngine;

namespace RootMotion.FinalIK;

[ExecuteInEditMode]
public class EditorIK : MonoBehaviour
{
	[Tooltip("If slot assigned, will update Animator before IK.")]
	public Animator animator;

	[Tooltip("Create/Final IK/Editor IK Pose")]
	public EditorIKPose defaultPose;

	[HideInInspector]
	public Transform[] bones = new Transform[0];

	public IK ik { get; private set; }

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			if ((Object)(object)ik == (Object)null)
			{
				ik = ((Component)this).GetComponent<IK>();
			}
			if ((Object)(object)ik == (Object)null)
			{
				Debug.LogError((object)"EditorIK needs to have an IK component on the same GameObject.", (Object)(object)((Component)this).transform);
			}
			else if (bones.Length == 0)
			{
				bones = ((Component)((Component)ik).transform).GetComponentsInChildren<Transform>();
			}
		}
	}

	private void OnDisable()
	{
		if (!Application.isPlaying)
		{
			if ((Object)(object)defaultPose != (Object)null && defaultPose.poseStored)
			{
				defaultPose.Restore(bones);
			}
			if ((Object)(object)ik != (Object)null)
			{
				ik.GetIKSolver().executedInEditor = false;
			}
		}
	}

	private void OnDestroy()
	{
		if (!Application.isPlaying && !((Object)(object)ik == (Object)null))
		{
			if (bones.Length == 0)
			{
				bones = ((Component)((Component)ik).transform).GetComponentsInChildren<Transform>();
			}
			if ((Object)(object)defaultPose != (Object)null && defaultPose.poseStored && bones.Length != 0)
			{
				defaultPose.Restore(bones);
			}
			ik.GetIKSolver().executedInEditor = false;
		}
	}

	public void StoreDefaultPose()
	{
		bones = ((Component)((Component)ik).transform).GetComponentsInChildren<Transform>();
		defaultPose.Store(bones);
	}

	public bool Initiate()
	{
		if ((Object)(object)defaultPose == (Object)null)
		{
			return false;
		}
		if (!defaultPose.poseStored)
		{
			return false;
		}
		if (bones.Length == 0)
		{
			return false;
		}
		if ((Object)(object)ik == (Object)null)
		{
			ik = ((Component)this).GetComponent<IK>();
		}
		if ((Object)(object)ik == (Object)null)
		{
			Debug.LogError((object)"EditorIK can not find an IK component.", (Object)(object)((Component)this).transform);
			return false;
		}
		defaultPose.Restore(bones);
		ik.GetIKSolver().executedInEditor = false;
		ik.GetIKSolver().Initiate(((Component)ik).transform);
		ik.GetIKSolver().executedInEditor = true;
		return true;
	}

	public void Update()
	{
		if (Application.isPlaying || (Object)(object)ik == (Object)null || !((Behaviour)ik).enabled || !ik.GetIKSolver().executedInEditor)
		{
			return;
		}
		if (bones.Length == 0)
		{
			bones = ((Component)((Component)ik).transform).GetComponentsInChildren<Transform>();
		}
		if (bones.Length == 0 || !defaultPose.Restore(bones))
		{
			return;
		}
		ik.GetIKSolver().executedInEditor = false;
		if (!ik.GetIKSolver().initiated)
		{
			ik.GetIKSolver().Initiate(((Component)ik).transform);
		}
		if (ik.GetIKSolver().initiated)
		{
			ik.GetIKSolver().executedInEditor = true;
			if ((Object)(object)animator != (Object)null && (Object)(object)animator.runtimeAnimatorController != (Object)null)
			{
				animator.Update(Time.deltaTime);
			}
			ik.GetIKSolver().Update();
		}
	}
}
