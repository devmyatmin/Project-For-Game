using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public abstract class IKSolver
{
	[Serializable]
	public class Point
	{
		public Transform transform;

		[Range(0f, 1f)]
		public float weight = 1f;

		public Vector3 solverPosition;

		public Quaternion solverRotation = Quaternion.identity;

		public Vector3 defaultLocalPosition;

		public Quaternion defaultLocalRotation;

		public void StoreDefaultLocalState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			defaultLocalPosition = transform.localPosition;
			defaultLocalRotation = transform.localRotation;
		}

		public void FixTransform()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (transform.localPosition != defaultLocalPosition)
			{
				transform.localPosition = defaultLocalPosition;
			}
			if (transform.localRotation != defaultLocalRotation)
			{
				transform.localRotation = defaultLocalRotation;
			}
		}

		public void UpdateSolverPosition()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			solverPosition = transform.position;
		}

		public void UpdateSolverLocalPosition()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			solverPosition = transform.localPosition;
		}

		public void UpdateSolverState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			solverPosition = transform.position;
			solverRotation = transform.rotation;
		}

		public void UpdateSolverLocalState()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			solverPosition = transform.localPosition;
			solverRotation = transform.localRotation;
		}

		public Point()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Bone : Point
	{
		public float length;

		public float sqrMag;

		public Vector3 axis = -Vector3.right;

		private RotationLimit _rotationLimit;

		private bool isLimited = true;

		public RotationLimit rotationLimit
		{
			get
			{
				if (!isLimited)
				{
					return null;
				}
				if ((Object)(object)_rotationLimit == (Object)null)
				{
					_rotationLimit = ((Component)transform).GetComponent<RotationLimit>();
				}
				isLimited = (Object)(object)_rotationLimit != (Object)null;
				return _rotationLimit;
			}
			set
			{
				_rotationLimit = value;
				isLimited = (Object)(object)value != (Object)null;
			}
		}

		public void Swing(Vector3 swingTarget, float weight = 1f)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				Quaternion val = Quaternion.FromToRotation(transform.rotation * axis, swingTarget - transform.position);
				if (weight >= 1f)
				{
					transform.rotation = val * transform.rotation;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(Quaternion.identity, val, weight) * transform.rotation;
				}
			}
		}

		public static void SolverSwing(Bone[] bones, int index, Vector3 swingTarget, float weight = 1f)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			if (weight <= 0f)
			{
				return;
			}
			Quaternion val = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
			if (weight >= 1f)
			{
				for (int i = index; i < bones.Length; i++)
				{
					bones[i].solverRotation = val * bones[i].solverRotation;
				}
			}
			else
			{
				for (int j = index; j < bones.Length; j++)
				{
					bones[j].solverRotation = Quaternion.Lerp(Quaternion.identity, val, weight) * bones[j].solverRotation;
				}
			}
		}

		public void Swing2D(Vector3 swingTarget, float weight = 1f)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			if (!(weight <= 0f))
			{
				Vector3 val = transform.rotation * axis;
				Vector3 val2 = swingTarget - transform.position;
				float num = Mathf.Atan2(val.x, val.y) * 57.29578f;
				float num2 = Mathf.Atan2(val2.x, val2.y) * 57.29578f;
				transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(num, num2) * weight, Vector3.back) * transform.rotation;
			}
		}

		public void SetToSolverPosition()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			transform.position = solverPosition;
		}

		public Bone()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		}

		public Bone(Transform transform)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			base.transform = transform;
		}

		public Bone(Transform transform, float weight)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			base.transform = transform;
			base.weight = weight;
		}
	}

	[Serializable]
	public class Node : Point
	{
		public float length;

		public float effectorPositionWeight;

		public float effectorRotationWeight;

		public Vector3 offset;

		public Node()
		{
		}

		public Node(Transform transform)
		{
			base.transform = transform;
		}

		public Node(Transform transform, float weight)
		{
			base.transform = transform;
			base.weight = weight;
		}
	}

	public delegate void UpdateDelegate();

	public delegate void IterationDelegate(int i);

	[HideInInspector]
	public bool executedInEditor;

	[HideInInspector]
	public Vector3 IKPosition;

	[Tooltip("The positional or the master weight of the solver.")]
	[Range(0f, 1f)]
	public float IKPositionWeight = 1f;

	public UpdateDelegate OnPreInitiate;

	public UpdateDelegate OnPostInitiate;

	public UpdateDelegate OnPreUpdate;

	public UpdateDelegate OnPostUpdate;

	protected bool firstInitiation = true;

	[SerializeField]
	[HideInInspector]
	protected Transform root;

	public bool initiated { get; private set; }

	public bool IsValid()
	{
		string message = string.Empty;
		return IsValid(ref message);
	}

	public abstract bool IsValid(ref string message);

	public void Initiate(Transform root)
	{
		if (executedInEditor)
		{
			return;
		}
		if (OnPreInitiate != null)
		{
			OnPreInitiate();
		}
		if ((Object)(object)root == (Object)null)
		{
			Debug.LogError((object)"Initiating IKSolver with null root Transform.");
		}
		this.root = root;
		initiated = false;
		string message = string.Empty;
		if (!IsValid(ref message))
		{
			Warning.Log(message, root);
			return;
		}
		OnInitiate();
		StoreDefaultLocalState();
		initiated = true;
		firstInitiation = false;
		if (OnPostInitiate != null)
		{
			OnPostInitiate();
		}
	}

	public void Update()
	{
		if (OnPreUpdate != null)
		{
			OnPreUpdate();
		}
		if (firstInitiation)
		{
			Initiate(root);
		}
		if (initiated)
		{
			OnUpdate();
			if (OnPostUpdate != null)
			{
				OnPostUpdate();
			}
		}
	}

	public virtual Vector3 GetIKPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return IKPosition;
	}

	public void SetIKPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		IKPosition = position;
	}

	public float GetIKPositionWeight()
	{
		return IKPositionWeight;
	}

	public void SetIKPositionWeight(float weight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
	}

	public Transform GetRoot()
	{
		return root;
	}

	public abstract Point[] GetPoints();

	public abstract Point GetPoint(Transform transform);

	public abstract void FixTransforms();

	public abstract void StoreDefaultLocalState();

	protected abstract void OnInitiate();

	protected abstract void OnUpdate();

	protected void LogWarning(string message)
	{
		Warning.Log(message, root, logInEditMode: true);
	}

	public static Transform ContainsDuplicateBone(Bone[] bones)
	{
		for (int i = 0; i < bones.Length; i++)
		{
			for (int j = 0; j < bones.Length; j++)
			{
				if (i != j && (Object)(object)bones[i].transform == (Object)(object)bones[j].transform)
				{
					return bones[i].transform;
				}
			}
		}
		return null;
	}

	public static bool HierarchyIsValid(Bone[] bones)
	{
		for (int i = 1; i < bones.Length; i++)
		{
			if (!Hierarchy.IsAncestor(bones[i].transform, bones[i - 1].transform))
			{
				return false;
			}
		}
		return true;
	}

	protected static float PreSolveBones(ref Bone[] bones)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].solverPosition = bones[i].transform.position;
			bones[i].solverRotation = bones[i].transform.rotation;
		}
		for (int j = 0; j < bones.Length; j++)
		{
			if (j < bones.Length - 1)
			{
				Bone obj = bones[j];
				Vector3 val = bones[j + 1].solverPosition - bones[j].solverPosition;
				obj.sqrMag = val.sqrMagnitude;
				bones[j].length = Mathf.Sqrt(bones[j].sqrMag);
				num += bones[j].length;
				bones[j].axis = Quaternion.Inverse(bones[j].solverRotation) * (bones[j + 1].solverPosition - bones[j].solverPosition);
			}
			else
			{
				bones[j].sqrMag = 0f;
				bones[j].length = 0f;
			}
		}
		return num;
	}
}
