using Beam;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class RagdollChild : MonoBehaviour
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private InteractiveObject _interactiveObject;

	private Collider _collider;

	public Rigidbody Rigidbody => _rigidbody;

	public InteractiveObject InteractiveObject => _interactiveObject;

	public Collider Collider
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_collider))
			{
				_collider = ((Component)this).GetComponent<Collider>();
			}
			return _collider;
		}
	}
}
