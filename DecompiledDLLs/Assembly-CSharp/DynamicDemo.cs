using UnityEngine;
using ch.sycoforge.Decal;

public class DynamicDemo : MonoBehaviour
{
	public EasyDecal DecalPrefab;

	public StaticProxyCollection ProxyCollection;

	public void Start()
	{
		if ((Object)(object)DecalPrefab == (Object)null)
		{
			Debug.LogError((object)"The DynamicDemo script has no decal prefab attached.");
		}
		EasyDecal.SetStaticProxyCollection(ProxyCollection);
	}

	public void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonUp(0))
		{
			Ray val = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit val2 = default;
			if (Physics.Raycast(val, ref val2, 200f))
			{
				Debug.DrawLine(val.origin, val2.point, Color.red);
				EasyDecal.ProjectAt(((Component)DecalPrefab).gameObject, ((Component)val2.collider).gameObject, val2.point, val2.normal);
			}
		}
	}
}
