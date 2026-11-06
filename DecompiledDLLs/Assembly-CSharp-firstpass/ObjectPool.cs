using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	private GameObject prefab;

	private Queue<GameObject> pool;

	public GameObject Prefab
	{
		get
		{
			return prefab;
		}
		set
		{
			prefab = value;
		}
	}

	public int Count => pool.Count;

	public void Awake()
	{
		pool = new Queue<GameObject>();
	}

	public GameObject Instanciate(Vector3 position, Quaternion rotation)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		GameObject val;
		if (pool.Count < 1)
		{
			val = Object.Instantiate<GameObject>(prefab, position, rotation);
		}
		else
		{
			val = pool.Dequeue();
			val.transform.parent = null;
			val.transform.position = position;
			val.transform.rotation = rotation;
			val.SetActive(true);
			val.SendMessage("Start", (SendMessageOptions)1);
		}
		return val;
	}

	public void Recycle(GameObject obj)
	{
		obj.SetActive(false);
		obj.transform.parent = ((Component)this).gameObject.transform;
		pool.Enqueue(obj);
	}
}
