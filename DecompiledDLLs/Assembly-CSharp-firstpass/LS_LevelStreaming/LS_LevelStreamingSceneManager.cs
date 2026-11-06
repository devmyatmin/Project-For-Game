using System.Collections.Generic;
using UnityEngine;

namespace LS_LevelStreaming;

public class LS_LevelStreamingSceneManager : MonoBehaviour
{
	private static LS_LevelStreamingSceneManager s_instance;

	private Dictionary<int, LS_ManagedObjectBase> m_managedObjects = new Dictionary<int, LS_ManagedObjectBase>();

	private List<int> m_managedObjectsToRemoveAtFrameEnd = new List<int>();

	public static bool IsInstanceSet => (Object)(object)s_instance != (Object)null;

	public static LS_LevelStreamingSceneManager Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)s_instance == (Object)null)
			{
				s_instance = new GameObject("LS_LevelStreamingSceneManager").AddComponent<LS_LevelStreamingSceneManager>();
			}
			return s_instance;
		}
	}

	public int AddManagedObject(LS_ManagedObjectBase p_managedObj)
	{
		if (p_managedObj != null)
		{
			if (!m_managedObjects.ContainsKey(p_managedObj.ID))
			{
				m_managedObjects.Add(p_managedObj.ID, p_managedObj);
				return p_managedObj.ID;
			}
			Debug.LogError((object)("LS_LevelStreamingSceneManager: AddManagedObject: object with ID(" + p_managedObj.ID + ") is already added!"));
			return -1;
		}
		Debug.LogError((object)"LS_LevelStreamingSceneManager: AddManagedObject: parameter is null!");
		return -1;
	}

	public LS_ManagedObjectBase GetManagedObject(int p_managedObjID)
	{
		m_managedObjects.TryGetValue(p_managedObjID, out var value);
		return value;
	}

	public bool RemoveManagedObject(int p_managedObjID)
	{
		LS_ManagedObjectBase managedObject = GetManagedObject(p_managedObjID);
		if (managedObject != null)
		{
			m_managedObjects.Remove(p_managedObjID);
			managedObject.m_onShow = null;
			managedObject.m_onHide = null;
		}
		return false;
	}

	public void RemoveManagedObjectAtFrameEnd(int p_managedObjID)
	{
		m_managedObjectsToRemoveAtFrameEnd.Add(p_managedObjID);
	}

	public void RemoveAllManagedObjects()
	{
		Dictionary<int, LS_ManagedObjectBase>.KeyCollection keys = m_managedObjects.Keys;
		int[] array = new int[keys.Count];
		keys.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			RemoveManagedObject(array[i]);
		}
	}

	public void ForceShowAllManagedObjects()
	{
		foreach (KeyValuePair<int, LS_ManagedObjectBase> managedObject in m_managedObjects)
		{
			managedObject.Value.ForceShow();
		}
	}

	public void ForceHideAllManagedObjects()
	{
		foreach (KeyValuePair<int, LS_ManagedObjectBase> managedObject in m_managedObjects)
		{
			managedObject.Value.ForceHide();
		}
	}

	private void Update()
	{
		foreach (KeyValuePair<int, LS_ManagedObjectBase> managedObject in m_managedObjects)
		{
			managedObject.Value.Update();
		}
	}

	private void LateUpdate()
	{
		foreach (int item in m_managedObjectsToRemoveAtFrameEnd)
		{
			RemoveManagedObject(item);
		}
		m_managedObjectsToRemoveAtFrameEnd.Clear();
	}
}
