using System.Collections.Generic;
using Beam;
using Ceto;
using UnityEngine;

public class RagdollHelper : MonoBehaviour
{
	private enum RagdollState
	{
		animated,
		ragdolled,
		blendToAnim
	}

	public class BodyPart
	{
		public Transform transform;

		public Vector3 storedPosition;

		public Quaternion storedRotation;
	}

	private RagdollState state;

	public Transform _renderers;

	public Transform _rig;

	public IPlayer player;

	public float ragdollToMecanimBlendTime = 0.5f;

	private float mecanimToGetUpTransitionTime = 0.05f;

	private float ragdollingEndTime = -100f;

	private Vector3 ragdolledHipPosition;

	private Vector3 ragdolledHeadPosition;

	private Vector3 ragdolledFeetPosition;

	private List<BodyPart> bodyParts = new List<BodyPart>();

	public Animator anim;

	public bool ragdolled
	{
		get
		{
			return state != RagdollState.animated;
		}
		set
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			if (value)
			{
				if (state == RagdollState.animated)
				{
					setKinematic(isKinematic: false);
					((Behaviour)anim).enabled = false;
					state = RagdollState.ragdolled;
				}
			}
			else
			{
				if (state != RagdollState.ragdolled)
				{
					return;
				}
				setKinematic(isKinematic: true);
				ragdollingEndTime = Time.time;
				((Behaviour)anim).enabled = true;
				state = RagdollState.blendToAnim;
				Quaternion rotation = _renderers.rotation;
				Quaternion rotation2 = _rig.rotation;
				((Component)this).transform.rotation = Quaternion.Euler(Vector3.zero);
				_renderers.rotation = rotation;
				_rig.rotation = rotation2;
				foreach (BodyPart bodyPart in bodyParts)
				{
					bodyPart.storedRotation = bodyPart.transform.rotation;
					bodyPart.storedPosition = bodyPart.transform.position;
				}
				ragdolledFeetPosition = 0.5f * (anim.GetBoneTransform((HumanBodyBones)19).position + anim.GetBoneTransform((HumanBodyBones)20).position);
				ragdolledHeadPosition = anim.GetBoneTransform((HumanBodyBones)10).position;
				ragdolledHipPosition = anim.GetBoneTransform((HumanBodyBones)0).position;
				float num = Ocean.Instance.QueryWaves(ragdolledHipPosition.x, ragdolledHipPosition.z);
				if ((ragdolledHipPosition.y < num) ? true : false)
				{
					anim.SetTrigger("ReviveFromWater");
				}
				else if (anim.GetBoneTransform((HumanBodyBones)0).forward.y > 0f)
				{
					anim.SetTrigger("ReviveFromBack");
				}
				else
				{
					anim.SetTrigger("ReviveFromBelly");
				}
			}
		}
	}

	private void setKinematic(bool isKinematic)
	{
		Rigidbody[] componentsInChildren = ((Component)this).GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody obj in componentsInChildren)
		{
			bool flag = isKinematic;
			if ((Object)(object)((Component)obj).gameObject == (Object)(object)((Component)this).gameObject)
			{
				flag |= !player.IsOwner;
			}
			obj.isKinematic = flag;
		}
	}

	private void Start()
	{
		Component[] componentsInChildren = ((Component)this).GetComponentsInChildren(typeof(Transform));
		foreach (Component val in componentsInChildren)
		{
			BodyPart bodyPart = new BodyPart();
			bodyPart.transform = (Transform)(object)((val is Transform) ? val : null);
			bodyParts.Add(bodyPart);
		}
	}

	private void LateUpdate()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		if (state != RagdollState.blendToAnim)
		{
			return;
		}
		if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
		{
			Vector3 val = ragdolledHipPosition - anim.GetBoneTransform((HumanBodyBones)0).position;
			Vector3 val2 = ((Component)this).transform.position + val;
			((Component)this).transform.position = new Vector3(val2.x, player.Movement.GetGroundPosition(val2), val2.z);
			Vector3 val3 = ragdolledHeadPosition - ragdolledFeetPosition;
			val3.y = 0f;
			Vector3 val4 = 0.5f * (anim.GetBoneTransform((HumanBodyBones)5).position + anim.GetBoneTransform((HumanBodyBones)6).position);
			Vector3 val5 = anim.GetBoneTransform((HumanBodyBones)10).position - val4;
			val5.y = 0f;
			Transform transform = ((Component)this).transform;
			transform.rotation *= Quaternion.FromToRotation(val5.normalized, val3.normalized);
		}
		float num = 1f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
		num = Mathf.Clamp01(num);
		foreach (BodyPart bodyPart in bodyParts)
		{
			if ((Object)(object)bodyPart.transform != (Object)(object)((Component)this).transform)
			{
				if ((Object)(object)bodyPart.transform == (Object)(object)anim.GetBoneTransform((HumanBodyBones)0))
				{
					bodyPart.transform.position = Vector3.Lerp(bodyPart.transform.position, bodyPart.storedPosition, num);
				}
				bodyPart.transform.rotation = Quaternion.Slerp(bodyPart.transform.rotation, bodyPart.storedRotation, num);
			}
		}
		if (num == 0f)
		{
			state = RagdollState.animated;
		}
	}
}
