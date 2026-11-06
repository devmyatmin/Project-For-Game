using UnityEngine;

namespace Beam;

public class Singleton<TBehaviour> : MonoBehaviour where TBehaviour : MonoBehaviour
{
	private static TBehaviour _Instance;

	public static TBehaviour Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_Instance))
			{
				_Instance = Object.FindObjectOfType<TBehaviour>();
				_ = (Object)(object)_Instance == (Object)null;
			}
			return _Instance;
		}
		protected set
		{
			_Instance = value;
		}
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)_Instance == (Object)(object)this)
		{
			_Instance = default;
		}
	}
}
