using System.Collections;
using UnityEngine;

namespace Beam;

public class CoroutineHelper : MonoBehaviour
{
	private static CoroutineHelper _Instance;

	protected static CoroutineHelper Instance
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null && !Game.IsQuitting)
			{
				_Instance = new GameObject("~CoroutineHelper").AddComponent<CoroutineHelper>();
				((Component)_Instance).transform.position = Vector3.zero;
				((Component)_Instance).transform.rotation = Quaternion.identity;
				((Component)_Instance).transform.SetAsFirstSibling();
			}
			return _Instance;
		}
	}

	public static GameObject GameObject => ((Component)Instance).gameObject;

	private void OnDestroy()
	{
		_Instance = null;
	}

	public static Coroutine StartCoroutine(IEnumerator routine)
	{
		return ((MonoBehaviour)Instance).StartCoroutine(routine);
	}
}
