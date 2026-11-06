using UnityEngine;

namespace RootMotion;

public abstract class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
{
	private static T sInstance;

	public static bool hasInstance => (Object)(object)sInstance != (Object)null;

	public static T instance
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)sInstance == (Object)null)
			{
				sInstance = new GameObject(typeof(T).ToString()).AddComponent<T>();
			}
			return sInstance;
		}
	}

	protected virtual void Awake()
	{
		sInstance = (T)this;
	}
}
