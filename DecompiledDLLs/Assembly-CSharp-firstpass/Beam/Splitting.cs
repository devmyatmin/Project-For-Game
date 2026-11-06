using System;
using System.Linq;
using Beam.Utilities;
using Funlabs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beam;

public class Splitting : MonoBehaviour
{
	private enum SplittingType
	{
		Radial,
		Stack
	}

	private enum SplittingMode
	{
		Manual,
		Force,
		Any
	}

	private enum SplittingAxisSpace
	{
		Local,
		World
	}

	private enum SplittingPrefabForward
	{
		Parent,
		Radius
	}

	[Serializable]
	public sealed class SplittingPrefab
	{
		public int Quantity;

		public InteractiveObject Prefab;
	}

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[FormerlySerializedAs("_splittingRadius")]
	[SerializeField]
	private float _splittingDistance = 0.5f;

	[SerializeField]
	private Vector3 _splittingAxis = new Vector3(0f, 1f, 0f);

	[SerializeField]
	private SplittingAxisSpace _splittingAxisSpace;

	[SerializeField]
	private SplittingPrefabForward _splittingPrefabForward;

	[SerializeField]
	private SplittingMode _splittingMode;

	[SerializeField]
	private float _splittingImpactForce;

	[SerializeField]
	private bool _splittingExplosion = true;

	[SerializeField]
	private bool _logAxisFix;

	[SerializeField]
	[Tooltip("Find a safe position for prefabs that would be spawned inside other objects (uses interactive_objects layer mask).")]
	private bool _handleOriginCollisions;

	[SerializeField]
	private SplittingPrefab[] _splittingPrefabs;

	private float _explosionRadius = 0.5f;

	private float _impulse;

	public BaseActionUnityEvent Splat;

	public bool CanSplit { get; private set; } = true;

	private void OnCollisionEnter(Collision collision)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (_splittingMode != SplittingMode.Manual && CanSplit && !LevelLoader.IsLoading())
		{
			Vector3 relativeVelocity = collision.relativeVelocity;
			_impulse = relativeVelocity.magnitude;
			if (_impulse > _splittingImpactForce)
			{
				Split(Mathf.Sqrt(_impulse));
			}
		}
	}

	public bool Split()
	{
		if (_splittingMode == SplittingMode.Force || !CanSplit)
		{
			return false;
		}
		Split(3f);
		return true;
	}

	private void Split(float impulse)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		CanSplit = false;
		if (_splittingPrefabs == null)
		{
			return;
		}
		Beam.Utilities.ExtensionMethods.CheckForProjectilesInChildren(((Component)this).transform);
		if (MPExtensions.IsMaster(Game.Mode))
		{
			int num = _splittingPrefabs.Sum((SplittingPrefab prefab) => prefab.Quantity);
			Quaternion val = ((_splittingAxisSpace == SplittingAxisSpace.Local) ? ((Component)this).transform.rotation : Quaternion.identity);
			Vector3 val2 = new Vector3(1f - Mathf.Clamp01(_splittingAxis.x), 1f - Mathf.Clamp01(_splittingAxis.y), 1f - Mathf.Clamp01(_splittingAxis.z));
			int num2 = 0;
			for (int num3 = 0; num3 < _splittingPrefabs.Length; num3++)
			{
				for (int num4 = 0; num4 < _splittingPrefabs[num3].Quantity; num4++)
				{
					float num5 = 360f / (float)num * (float)num2++;
					Vector3 val3 = val * (Quaternion.AngleAxis(num5, _splittingAxis) * val2 * _splittingDistance);
					Vector3 val4 = ((Component)this).transform.position + val3;
					if (_handleOriginCollisions)
					{
						int num6 = 5;
						float num7 = 0.5f;
						LayerMask physicsLayerMask = GetPhysicsLayerMask(Layers.INTERACTIVE_OBJECTS);
						while (Physics.CheckSphere(val4, num7, LayerMask.op_Implicit(physicsLayerMask)) && num6 > 0)
						{
							num6--;
							val4 += Vector3.up;
							if (Physics.CheckSphere(val4, num7, LayerMask.op_Implicit(physicsLayerMask)))
							{
								val4 += Vector3.right;
							}
						}
					}
					InteractiveObject interactiveObject = MultiplayerMng.Instantiate<InteractiveObject>(_splittingPrefabs[num3].Prefab.PrefabId, val4, Quaternion.identity, MiniGuid.New());
					Transform transform = interactiveObject.transform;
					Vector3 val5;
					Vector3 forward;
					if (_splittingPrefabForward != SplittingPrefabForward.Parent)
					{
						val5 = ((Component)this).transform.position - val4;
						forward = val5.normalized;
					}
					else
					{
						forward = ((Component)this).transform.forward;
					}
					transform.forward = forward;
					if (_logAxisFix)
					{
						interactiveObject.transform.forward = Quaternion.FromToRotation(((Component)this).transform.forward, ((Component)this).transform.up) * ((Component)this).transform.forward;
					}
					if (!((Object)(object)interactiveObject.rigidbody != (Object)null))
					{
						continue;
					}
					interactiveObject.rigidbody.velocity = Vector3.zero;
					if (!_splittingExplosion)
					{
						continue;
					}
					if (num > 1)
					{
						Vector3 val6 = interactiveObject.transform.position - ((Component)this).transform.position;
						val5 = Vector3.up + val6;
						Vector3 normalized = val5.normalized;
						float magnitude = val6.magnitude;
						if (magnitude < _explosionRadius)
						{
							interactiveObject.rigidbody.AddForce(normalized * impulse * (1f - magnitude / _explosionRadius), (ForceMode)1);
						}
					}
					else
					{
						interactiveObject.rigidbody.AddForce(Vector3.up * impulse, (ForceMode)1);
					}
				}
			}
		}
		((UnityEvent<IBase, IBaseActionEventData>)Splat)?.Invoke((IBase)null, (IBaseActionEventData)BaseActionEventData.Empty);
		MultiplayerMng.Destroy(((Component)this).gameObject.GetComponent<SaveablePrefab>());
	}

	private LayerMask GetPhysicsLayerMask(int currentLayer)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(currentLayer, i))
			{
				num |= 1 << i;
			}
		}
		return LayerMask.op_Implicit(num);
	}

	public Splitting()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
	}
}
