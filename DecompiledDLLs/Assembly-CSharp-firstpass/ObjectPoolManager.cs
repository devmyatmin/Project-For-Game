using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
	private static ObjectPoolManager instance;

	public bool debug;

	public Rect debugGuiRect = new Rect(5f, 200f, 160f, 400f);

	private Dictionary<GameObject, ObjectPool> prefab2pool;

	private Dictionary<GameObject, ObjectPool> instance2pool;

	public static ObjectPoolManager Instance
	{
		get
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)instance))
			{
				instance = Object.FindObjectOfType(typeof(ObjectPoolManager)) as ObjectPoolManager;
				if (!Object.op_Implicit((Object)(object)instance))
				{
					GameObject val = new GameObject("~ObjectPoolManager");
					val.transform.position = Vector3.zero;
					val.transform.rotation = Quaternion.identity;
					val.transform.SetAsFirstSibling();
					instance = val.AddComponent<ObjectPoolManager>();
				}
			}
			return instance;
		}
	}

	private void OnApplicationQuit()
	{
		instance = null;
	}

	public static GameObject CreatePooled(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Instance.InternalCreate(prefab, position, rotation);
	}

	public static void DestroyPooled(GameObject obj)
	{
		Instance.InternalDestroy(obj);
	}

	public static void DestroyPooled(GameObject obj, float delay)
	{
		((MonoBehaviour)Instance).StartCoroutine(Instance.InternalDestroy(obj, delay));
	}

	private void Awake()
	{
		prefab2pool = new Dictionary<GameObject, ObjectPool>();
		instance2pool = new Dictionary<GameObject, ObjectPool>();
	}

	private ObjectPool CreatePool(GameObject prefab)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ObjectPool objectPool = new GameObject(((Object)prefab).name + " Pool").AddComponent<ObjectPool>();
		objectPool.Prefab = prefab;
		return objectPool;
	}

	private GameObject InternalCreate(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		ObjectPool objectPool;
		if (!prefab2pool.ContainsKey(prefab))
		{
			objectPool = CreatePool(prefab);
			((Component)objectPool).gameObject.transform.parent = ((Component)this).gameObject.transform;
			prefab2pool[prefab] = objectPool;
		}
		else
		{
			objectPool = prefab2pool[prefab];
		}
		GameObject val = objectPool.Instanciate(position, rotation);
		instance2pool[val] = objectPool;
		return val;
	}

	private void InternalDestroy(GameObject obj)
	{
		if (instance2pool.ContainsKey(obj))
		{
			instance2pool[obj].Recycle(obj);
		}
		else
		{
			Object.Destroy((Object)(object)obj);
		}
	}

	private IEnumerator InternalDestroy(GameObject obj, float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		InternalDestroy(obj);
	}

	private void OnGUI()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!debug)
		{
			return;
		}
		GUILayout.BeginArea(debugGuiRect);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Pools: " + prefab2pool.Count, Array.Empty<GUILayoutOption>());
		foreach (ObjectPool value in prefab2pool.Values)
		{
			GUILayout.Label(((Object)value.Prefab).name + ": " + value.Count, Array.Empty<GUILayoutOption>());
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public ObjectPoolManager()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
	}
}
