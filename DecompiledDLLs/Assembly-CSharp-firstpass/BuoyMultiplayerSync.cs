using Beam;
using Beam.Rendering;
using Funlabs;
using Photon.Bolt;
using Photon.Bolt.Internal;
using UnityEngine;

public class BuoyMultiplayerSync : MultiplayerBehaviour
{
	[SerializeField]
	private GameObject _buoy;

	public override void Attached()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((EntityEventListenerBase<IState>)(object)this).state.SetTransforms(((IBuoyState)((EntityEventListenerBase<IState>)(object)this).state).Transform, _buoy.transform);
	}

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsClient(Game.Mode))
		{
			_buoy.GetComponent<Rigidbody>().isKinematic = true;
			TryToAttachToBouy();
		}
	}

	public void TryToAttachToBouy()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameObject.FindGameObjectWithTag("Mission_Squid");
		if ((Object)(object)val != (Object)null)
		{
			((Component)this).transform.SetParent(val.transform, false);
			((Component)this).transform.localPosition = Vector3.zero;
			((Component)this).GetComponent<LodControllerChild>().AdoptParentLodGroup();
		}
	}
}
