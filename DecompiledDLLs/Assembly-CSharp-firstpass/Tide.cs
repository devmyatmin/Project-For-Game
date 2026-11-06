using Beam;
using Ceto;
using UnityEngine;

public class Tide : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve _tide;

	[SerializeField]
	private float _lowTideHeight;

	[SerializeField]
	private float _highTideHeight;

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)Ocean.Instance))
		{
			Ocean.Instance.level = Mathf.Lerp(_lowTideHeight, _highTideHeight, _tide.Evaluate(Singleton<GameTime>.Instance.NormalizedTime));
		}
	}
}
