using UnityEngine;

namespace Beam.UI;

public class NullGameObject
{
	private static GameObject _Instance;

	public static GameObject Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected Obj, but got Unknown
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_Instance == (Object)null)
			{
				_Instance = new GameObject("~NullGameObject");
				_Instance.transform.SetAsFirstSibling();
				_Instance.transform.position = Vector3.zero;
				_Instance.transform.rotation = Quaternion.identity;
			}
			Debug.LogError((object)"NullGameObject:: ");
			return _Instance;
		}
	}
}
