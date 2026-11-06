using Beam.Utilities;
using UnityEngine;

namespace Beam.AccountServices;

public class EpicSDKManager : MonoBehaviour
{
	private void Awake()
	{
		BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
	}
}
