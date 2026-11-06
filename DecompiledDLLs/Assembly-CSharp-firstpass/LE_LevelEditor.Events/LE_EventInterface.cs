using System;

namespace LE_LevelEditor.Events;

public static class LE_EventInterface
{
	public static EventHandler OnLoadedLevelInEditor;

	public static EventHandler OnChangeLevelData;

	public static EventHandler<LE_ObjectPlacedEvent> OnObjectPlaced;

	public static EventHandler<LE_ObjectDragEvent> OnObjectDragged;

	public static EventHandler<LE_TerrainCreatedEvent> OnTerrainCreated;

	public static void UnregisterAll()
	{
		OnLoadedLevelInEditor = null;
		OnChangeLevelData = null;
		OnObjectPlaced = null;
		OnObjectDragged = null;
		OnTerrainCreated = null;
	}

	public static void LevelDataChanged(object p_object)
	{
		if (OnChangeLevelData != null)
		{
			OnChangeLevelData(p_object, EventArgs.Empty);
		}
	}
}
