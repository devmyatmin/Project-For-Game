using UnityEngine;

namespace RootMotion;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T sInstance;

	public static T instance => sInstance;

	public static void Clear()
	{
		sInstance = null;
	}

	protected virtual void Awake()
	{
		if ((Object)(object)sInstance != (Object)null)
		{
			Debug.LogError((object)(((Object)this).name + "error: already initialized"), (Object)(object)this);
		}
		sInstance = (T)this;
	}
}
