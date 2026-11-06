using System.Linq;
using Beam.AccountServices;
using Beam.Events;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Construction_STRUCTURE_GYRO : Construction_STRUCTURE, ISelectable, IBase, IHasCraftingType, IHaveAttribute
{
	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Selecting _selecting;

	[SerializeField]
	private GameObject _gyrocopterPrefab;

	[SerializeField]
	private int _piecesRequired;

	[Header("ATTRIBUTE")]
	[SerializeField]
	private IconType _type;

	public Selecting Selecting => _selecting;

	public IconType Type => _type;

	float IHaveAttribute.GetValue()
	{
		return (float)Constructions.Count / (float)_piecesRequired;
	}

	public override void AddConstruction(Constructing constructing)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.AddConstruction(constructing);
		if (Constructions.All((Constructing construction) => construction.MyConnectors.All((Connector connector) => !connector.IsValid)))
		{
			if (MPExtensions.IsMaster(Game.Mode))
			{
				MultiplayerMng.Instantiate<SaveablePrefab>(_gyrocopterPrefab.GetComponent<ISaveablePrefab>().PrefabId, transform.position, transform.rotation, MiniGuid.New());
			}
			EventManager.RaiseEvent(new AchievementUnlockEvent(EAchievement.DA_VINCI));
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
	}

	public override void Recenter()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!LevelLoader.IsLoading())
		{
			Vector3 val = Vector3.zero;
			for (int i = 0; i < Constructions.Count; i++)
			{
				val += Constructions[i].transform.position;
				Constructions[i].transform.parent = null;
			}
			val /= (float)Constructions.Count;
			transform.position = val;
			for (int j = 0; j < Constructions.Count; j++)
			{
				Constructing constructing = Constructions[j];
				constructing.transform.parent = transform;
				constructing.transform.localScale = Vector3.one;
			}
		}
	}
}
