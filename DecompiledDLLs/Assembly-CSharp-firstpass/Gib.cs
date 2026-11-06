using Beam.Utilities;
using UnityEngine;

public class Gib : MonoBehaviour
{
	[SerializeField]
	private Rigidbody[] _gibs;

	[SerializeField]
	private float _explosionForce = 1f;

	private float _timer;

	private void Start()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody[] gibs = _gibs;
		for (int i = 0; i < gibs.Length; i++)
		{
			gibs[i].AddExplosionForce(_explosionForce, ((Component)this).transform.position, 0f, 0f, (ForceMode)2);
		}
	}

	private void Update()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		_timer += Time.deltaTime;
		if (!(_timer > 7f))
		{
			return;
		}
		Rigidbody[] gibs = _gibs;
		foreach (Rigidbody val in gibs)
		{
			if (!val.isKinematic)
			{
				val.isKinematic = true;
				((Component)val).GetComponent<Collider>().enabled = false;
			}
			Transform transform = ((Component)val).transform;
			transform.position += Vector3.down * Time.deltaTime * 0.15f;
		}
		if (_timer > 10f)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	public void Reset()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody[] gibs = _gibs;
		foreach (Rigidbody obj in gibs)
		{
			((Component)obj).transform.localPosition = Vector3.zero;
			((Component)obj).GetComponent<Collider>().enabled = true;
			obj.isKinematic = false;
		}
		((Component)this).transform.position = Vector3.zero;
		_timer = 0f;
	}
}
