using UnityEngine;

public class ZoneObject
{
	[SerializeField]
	private IGenerationObject _obj;

	[SerializeField]
	private float _rarity = 50f;

	public IGenerationObject Obj => _obj;

	public float Rarity => _rarity;
}
