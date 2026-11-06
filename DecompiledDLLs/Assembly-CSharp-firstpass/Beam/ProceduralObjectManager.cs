using System.Collections.Generic;
using UnityEngine;

namespace Beam;

public class ProceduralObjectManager : MonoBehaviour
{
	public GameObject WolliePrefab;

	[SerializeField]
	private ZoneObjects _sheltersGeneration;

	[SerializeField]
	private List<BiomeCategory> _biomeGeneration;

	[SerializeField]
	private List<ObjectCategory> _objectGeneration;

	public ZoneObjects SheltersGeneration => _sheltersGeneration;

	public IList<BiomeCategory> BiomeGeneration => _biomeGeneration;

	public IList<ObjectCategory> ObjectGeneration => _objectGeneration;
}
