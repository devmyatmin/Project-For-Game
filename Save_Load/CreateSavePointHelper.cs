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
		if (!LevelLoader.IsLoading())
		{
			ConstructionObject_BED constructionObject_BED = Object.Instantiate(_missionBedSavePrefab);
			constructionObject_BED.transform.SetParent(base.transform.parent, worldPositionStays: false);
			constructionObject_BED.transform.position = _missionBedSavePosition.position;
			constructionObject_BED.transform.rotation = _missionBedSavePosition.rotation;
			constructionObject_BED.CanDrag = false;
			constructionObject_BED.CanPickUp = false;
			constructionObject_BED.SetLOD(1);
		}
	}
}
