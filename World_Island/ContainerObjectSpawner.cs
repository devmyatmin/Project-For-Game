using System;
using UnityEngine;

public class ContainerObjectSpawner : MonoBehaviour
{
	[Serializable]
	public class Slot
	{
		public Transform slot;

		public SlotItem[] items;

		[HideInInspector]
		public bool occupied;
	}

	[Serializable]
	public class SlotItem
	{
		public GameObject prefab;

		public float rarity;
	}

	[SerializeField]
	private Slot[] _objectSlots;

	public Slot[] objectSlots => _objectSlots;
}
