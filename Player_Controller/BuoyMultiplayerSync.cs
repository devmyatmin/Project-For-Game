using Beam;
using Beam.Rendering;
using Funlabs;
using Photon.Bolt;
using UnityEngine;

public class BuoyMultiplayerSync : MultiplayerBehaviour
{
	[SerializeField]
	private GameObject _buoy;

	public override void Attached()
	{
		base.state.SetTransforms(((IBuoyState)base.state).Transform, _buoy.transform);
	}

	private void Start()
	{
		if (Game.Mode.IsClient())
		{
			_buoy.GetComponent<Rigidbody>().isKinematic = true;
			TryToAttachToBouy();
		}
	}

	public void TryToAttachToBouy()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("Mission_Squid");
		if (gameObject != null)
		{
			base.transform.SetParent(gameObject.transform, worldPositionStays: false);
			base.transform.localPosition = Vector3.zero;
			GetComponent<LodControllerChild>().AdoptParentLodGroup();
		}
	}
}
