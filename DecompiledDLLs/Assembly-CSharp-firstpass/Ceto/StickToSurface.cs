using UnityEngine;

namespace Ceto;

[AddComponentMenu("Ceto/Buoyancy/StickToSurface")]
public class StickToSurface : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Ocean.Instance == (Object)null))
		{
			Vector3 position = ((Component)this).transform.position;
			position.y = Ocean.Instance.QueryWaves(position.x, position.z);
			((Component)this).transform.position = position;
		}
	}
}
