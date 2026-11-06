using Beam.Crafting;
using UnityEngine;

public class CreateSavePointHelper : MonoBehaviour
{
	[SerializeField]
	private ConstructionObject_BED _missionBedSavePrefab;

	[SerializeField]
	private Transform _missionBedSavePosition;

	private void Start()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!LevelLoader.IsLoading())
		{
			ConstructionObject_BED constructionObject_BED = Object.Instantiate<ConstructionObject_BED>(_missionBedSavePrefab);
			constructionObject_BED.transform.SetParent(((Component)this).transform.parent, false);
			constructionObject_BED.transform.position = _missionBedSavePosition.position;
			constructionObject_BED.transform.rotation = _missionBedSavePosition.rotation;
			constructionObject_BED.CanDrag = false;
			constructionObject_BED.CanPickUp = false;
			constructionObject_BED.SetLOD(1);
		}
	}
}
