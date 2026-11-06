using System;
using UnityEngine;

namespace LE_LevelEditor.Events;

public class LE_TerrainCreatedEvent : EventArgs
{
	private readonly GameObject m_terrainGameObject;

	public GameObject TerrainGameObject => m_terrainGameObject;

	public LE_TerrainCreatedEvent(GameObject p_terrainGameObject)
	{
		m_terrainGameObject = p_terrainGameObject;
	}
}
