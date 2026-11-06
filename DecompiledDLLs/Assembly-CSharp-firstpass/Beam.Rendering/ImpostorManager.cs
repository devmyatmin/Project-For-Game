using System.Collections.Generic;
using Beam.Events;
using Funlabs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beam.Rendering;

public class ImpostorManager : SerializedMonoBehaviour
{
	private const string IMPOSTOR_PREFABS_PATH = "Prefabs/StrandedObjects/Impostors";

	public const string IMPOSTOR_ASSETS_PATH = "Prefabs/Impostor Baking";

	public const string IMPOSTOR_SUFFIX = "_Impostor";

	public const int DYNAMIC_ID = -2;

	public static int BILLBOARD_DENSITY_STEP = 1;

	public static int MIN_BILLBOARD_DENSITY = 25;

	private static ImpostorManager _Instance;

	[SerializeField]
	private Transform _pooledProxyContainer;

	[SerializeField]
	private Transform _activeProxyContainer;

	[SerializeField]
	private Transform _pooledContainer;

	[SerializeField]
	private Transform _activeContainer;

	[SerializeField]
	private RenderableProxy _renderableProxyPrefab;

	[Header("Impostors")]
	[SerializeField]
	private Dictionary<int, ImpostorBase> _impostorPrefabs;

	private Dictionary<int, Stack<ImpostorBase>> _impostorPools = new Dictionary<int, Stack<ImpostorBase>>();

	private Stack<RenderableProxy> _proxyPool = new Stack<RenderableProxy>();

	private void Awake()
	{
		_Instance = this;
	}

	private void Start()
	{
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
	}

	private void OnDestroy()
	{
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)Options.GraphicsSettings.BillboardDensity;
		if (MPExtensions.IsLocalCoop(Game.Mode))
		{
			num = Mathf.Max(num, 1);
		}
		switch ((Quality_LOW_HIGH)num)
		{
		case Quality_LOW_HIGH.HIGH:
			BILLBOARD_DENSITY_STEP = 1;
			break;
		case Quality_LOW_HIGH.MEDIUM:
			BILLBOARD_DENSITY_STEP = 2;
			break;
		case Quality_LOW_HIGH.LOW:
			BILLBOARD_DENSITY_STEP = 3;
			break;
		}
	}

	public static RenderableProxy CreateRenderableProxy(Mesh mesh, Renderer renderer)
	{
		return _Instance?.CreateRenderableProxy_(mesh, renderer);
	}

	public static void ReleaseRenderableProxy(RenderableProxy proxy)
	{
		_Instance?.ReleaseRenderableProxy_(proxy);
	}

	private RenderableProxy CreateRenderableProxy_(Mesh mesh, Renderer renderer)
	{
		RenderableProxy renderableProxy;
		if (_proxyPool.Count > 0)
		{
			renderableProxy = _proxyPool.Pop();
		}
		else
		{
			renderableProxy = Object.Instantiate<RenderableProxy>(_renderableProxyPrefab);
			((Component)renderableProxy).gameObject.layer = DynamicImposterManager.Instance.imposterLayer;
		}
		((Component)renderableProxy).transform.SetParent(_activeProxyContainer, false);
		renderableProxy.MeshFilter.sharedMesh = mesh;
		renderableProxy.Renderer.sharedMaterials = renderer.sharedMaterials;
		return renderableProxy;
	}

	private void ReleaseRenderableProxy_(RenderableProxy proxy)
	{
		((Component)proxy).transform.SetParent(_pooledProxyContainer);
		_proxyPool.Push(proxy);
	}

	public static ImpostorBase Create(int id)
	{
		return _Instance?.Pop(id);
	}

	private ImpostorBase Pop(int id)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (id == -2 && (Options.GraphicsSettings.BillboardDensity == Quality_BillboardDensity.LOW || MPExtensions.IsLocalCoop(Game.Mode)))
		{
			return DummyImpostor.Instance;
		}
		ImpostorBase value2;
		if (_impostorPools.TryGetValue(id, out var value) && value.Count > 0)
		{
			value2 = value.Pop();
		}
		else if (_impostorPrefabs.TryGetValue(id, out value2))
		{
			value2 = Object.Instantiate<ImpostorBase>(value2);
		}
		if ((Object)(object)value2 != (Object)null)
		{
			((Component)value2).transform.SetParent(_activeContainer, false);
			return value2;
		}
		return null;
	}

	public static void Release(ImpostorBase impostor)
	{
		_Instance?.Push(impostor);
	}

	private void Push(ImpostorBase impostor)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		impostor.Release();
		((Component)impostor).transform.SetParent(_pooledContainer, false);
		((Component)impostor).transform.position = Vector3.zero;
		((Component)impostor).transform.rotation = Quaternion.identity;
		impostor.ParentInstanceId = 0;
		if (_impostorPools.TryGetValue(impostor.ParentPrefabId, out var value))
		{
			value.Push(impostor);
			return;
		}
		value = new Stack<ImpostorBase>();
		value.Push(impostor);
		_impostorPools.Add(impostor.ParentPrefabId, value);
	}
}
