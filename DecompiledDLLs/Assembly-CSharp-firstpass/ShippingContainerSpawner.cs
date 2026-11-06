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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			int num = Singleton<StrandedWorld>.Instance.GenerateLocalSeed(((Component)this).transform.position).Next(_shippingContainers.Length);
			ShippingContainer shippingContainer = GetShippingContainer(num);
			if (BeamTeam.IsNullOrDestroyed(shippingContainer))
			{
				return;
			}
			GameObject val = Object.Instantiate<GameObject>(((Component)shippingContainer).gameObject, ((Component)this).transform.position, ((Component)this).transform.localRotation);
			val.transform.parent = ((Component)this).transform.parent;
			MiniGuid other = MiniGuid.NewFrom(((Component)this).transform.position, MiniGuid.CONTAINER_PREFAB_ID_PLACE_HOLDER, (ushort)num);
			ISaveablePrefab saveablePrefab = default;
			if (val.TryGetComponent<ISaveablePrefab>(ref saveablePrefab))
			{
				MiniGuid miniGuid = (saveablePrefab.ReferenceId = MiniGuid.NewFrom(((Component)this).transform.position, saveablePrefab.PrefabId, (ushort)num));
				other = miniGuid;
			}
			ShippingContainer component = val.GetComponent<ShippingContainer>();
			ISaveablePrefab saveablePrefab2 = default;
			for (int i = 0; i < component.PanelPositions.Length; i++)
			{
				GameObject val2 = Object.Instantiate<GameObject>((i == 0) ? _containerDoor : _containerWall, component.PanelPositions[i].position, Quaternion.identity);
				val2.transform.rotation = Quaternion.LookRotation(component.PanelPositions[i].forward, component.PanelPositions[i].up);
				if (val2.TryGetComponent<ISaveablePrefab>(ref saveablePrefab2))
				{
					saveablePrefab2.ReferenceId = MiniGuid.NewFrom(other, i + 1);
				}
			}
		}
		finally
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
