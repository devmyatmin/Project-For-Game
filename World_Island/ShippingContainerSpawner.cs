using System.Runtime.CompilerServices;
using Beam;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

public class ShippingContainerSpawner : MonoBehaviour, IGenerationObject
{
	[SerializeField]
	private ShippingContainer[] _shippingContainers;

	[SerializeField]
	private GameObject _containerDoor;

	[SerializeField]
	private GameObject _containerWall;

	private void Start()
	{
		GenerateShippingContainer();
	}

	private ShippingContainer GetShippingContainer(int index)
	{
		if (index < 0 || index >= _shippingContainers.Length)
		{
			return null;
		}
		return _shippingContainers[index];
	}

	private void GenerateShippingContainer()
	{
		try
		{
			int num = Singleton<StrandedWorld>.Instance.GenerateLocalSeed(base.transform.position).Next(_shippingContainers.Length);
			ShippingContainer shippingContainer = GetShippingContainer(num);
			if (shippingContainer.IsNullOrDestroyed())
			{
				return;
			}
			GameObject obj = Object.Instantiate(shippingContainer.gameObject, base.transform.position, base.transform.localRotation);
			obj.transform.parent = base.transform.parent;
			MiniGuid other = MiniGuid.NewFrom(base.transform.position, MiniGuid.CONTAINER_PREFAB_ID_PLACE_HOLDER, (ushort)num);
			if (obj.TryGetComponent<ISaveablePrefab>(out var component))
			{
				MiniGuid miniGuid2 = (component.ReferenceId = MiniGuid.NewFrom(base.transform.position, component.PrefabId, (ushort)num));
				other = miniGuid2;
			}
			ShippingContainer component2 = obj.GetComponent<ShippingContainer>();
			for (int i = 0; i < component2.PanelPositions.Length; i++)
			{
				GameObject obj2 = Object.Instantiate((i == 0) ? _containerDoor : _containerWall, component2.PanelPositions[i].position, Quaternion.identity);
				obj2.transform.rotation = Quaternion.LookRotation(component2.PanelPositions[i].forward, component2.PanelPositions[i].up);
				if (obj2.TryGetComponent<ISaveablePrefab>(out var component3))
				{
					component3.ReferenceId = MiniGuid.NewFrom(other, i + 1);
				}
			}
		}
		finally
		{
			Object.Destroy(base.gameObject);
		}
	}

	[SpecialName]
	GameObject IGenerationObject.get_gameObject()
	{
		return base.gameObject;
	}
}
