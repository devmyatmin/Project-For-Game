using UnityEngine;

namespace Beam.UI;

public class NullTransform : MonoBehaviour
{
	private static Transform _Instance;

	public static Transform Instance
	{
		get
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null)
			{
				_Instance = NullGameObject.Instance.transform;
			}
			((Component)_Instance).transform.position = Vector3.zero;
			((Component)_Instance).transform.rotation = Quaternion.identity;
			Debug.LogError((object)"NullTransform:: ");
			return _Instance;
		}
	}
}
