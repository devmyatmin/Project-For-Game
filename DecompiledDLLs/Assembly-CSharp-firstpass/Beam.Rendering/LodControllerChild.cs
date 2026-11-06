using UnityEngine;

namespace Beam.Rendering;

public class LodControllerChild : MonoBehaviour
{
	[SerializeField]
	private LodGroup _lodGroup;

	private void Start()
	{
		AdoptParentLodGroup();
	}

	public void AdoptParentLodGroup()
	{
		LodController componentInParent = ((Component)((Component)this).transform).GetComponentInParent<LodController>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			componentInParent.AdoptLodGroup(_lodGroup);
		}
	}
}
