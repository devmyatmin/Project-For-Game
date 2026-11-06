using UnityEngine;

public class TexScroll : MonoBehaviour
{
	[SerializeField]
	private Material _mat;

	private float _offsetX;

	[SerializeField]
	private float _speed;

	private void Update()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		_offsetX += Time.deltaTime * _speed;
		if (_offsetX < -1f)
		{
			_offsetX = 0f;
		}
		Vector2 val = new Vector2(_offsetX, 0f);
		_mat.SetTextureOffset("_MainTex", val);
	}
}
