using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beam.Developer.UI;

public class UPrefabListElement : MonoBehaviour
{
	public Text Text;

	public Button Button;

	public GameObject Prefab;

	public event GameObjectEventHandler Click;

	protected virtual void OnClick(GameObject go)
	{
		Click?.Invoke(go);
	}

	private void Awake()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		((UnityEventBase)Button.onClick).RemoveAllListeners();
		((UnityEvent)Button.onClick).AddListener((UnityAction)OnClick);
	}

	private void OnDestroy()
	{
		Click = null;
		((UnityEventBase)Button.onClick).RemoveAllListeners();
		Button.onClick = null;
	}

	private void OnClick()
	{
		OnClick(Prefab);
	}
}
