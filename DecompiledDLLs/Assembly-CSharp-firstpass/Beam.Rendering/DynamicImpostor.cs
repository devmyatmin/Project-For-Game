using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam.Rendering;

public class DynamicImpostor : ImpostorBase
{
	public const float SQR_MAX_DISTANCE = 62500f;

	public static float ANGLE_BIAS = 1f;

	public static float DISTANCE_BIAS = 1f;

	private Bounds _bounds;

	private ImposterTexture _imposterTexture;

	public int maxTextureSize = 256;

	public float angleTolerance = 2.5f;

	public float distanceTolerance = 15f;

	public float zOffset;

	private int _lastTextureSize;

	private Vector3 _lastCameraVector = Vector3.zero;

	private bool _forceUpdate;

	private float _maxSize;

	private Camera _renderingCamera;

	[Header("Renderables Debug")]
	private List<RenderableProxy> _renderableProxies = new List<RenderableProxy>();

	public Bounds Bounds
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _bounds;
		}
	}

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_renderingCamera = DynamicImposterManager.Instance.imposterRenderingCamera;
		_lastCameraVector = Vector3.zero;
		if (!DynamicImposterManager.Instance.Imposters.Contains(this))
		{
			DynamicImposterManager.Instance.Imposters.Add(this);
		}
	}

	private void OnDestroy()
	{
		Release();
		DynamicImposterManager.Instance.Imposters.Remove(this);
	}

	public override void Release()
	{
		ReleaseProxies();
		ReleaseTexture();
		base.Release();
	}

	public override void Refresh()
	{
		base.Refresh();
		ExtractRenderersAndBounds();
	}

	public override void SetRenderables(IEnumerable<Renderer> renderers)
	{
		foreach (Renderer renderer in renderers)
		{
			CreateRenderableProxy(renderer);
		}
		Refresh();
	}

	private void CreateRenderableProxy(Renderer renderer)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = ((Component)renderer).GetComponent<MeshFilter>();
		Mesh val = ((component != null) ? component.sharedMesh : null);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)"No mesh.");
			return;
		}
		RenderableProxy renderableProxy = ImpostorManager.CreateRenderableProxy(val, renderer);
		((Component)renderableProxy.Renderer).gameObject.layer = DynamicImposterManager.Instance.imposterLayer;
		((Component)renderableProxy).transform.position = ((Component)this).transform.position;
		((Component)renderableProxy).transform.rotation = ((Component)this).transform.rotation;
		_renderableProxies.Add(renderableProxy);
	}

	private void ReleaseProxies()
	{
		using (IEnumerator<RenderableProxy> enumerator = _renderableProxies.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ImpostorManager.ReleaseRenderableProxy(enumerator.Current);
			}
		}
		_renderableProxies.Clear();
	}

	private void ExtractRenderersAndBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		_bounds = default;
		using (IEnumerator<RenderableProxy> enumerator = _renderableProxies.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				RenderableProxy current = enumerator.Current;
				if (_bounds.extents == Vector3.zero)
				{
					_bounds = current.Renderer.bounds;
				}
				else
				{
					_bounds.Encapsulate(current.Renderer.bounds);
				}
			}
		}
		Vector3 extents = _bounds.extents;
		_maxSize = extents.magnitude * 2f;
		((Component)this).transform.position = _bounds.center;
		Transform parent = ((Component)this).transform.parent;
		((Component)this).transform.SetParent((Transform)null);
		((Component)this).transform.localScale = new Vector3(_maxSize, _maxSize, _maxSize);
		((Component)this).transform.SetParent(parent);
	}

	public void ForceUpdate()
	{
		_forceUpdate = true;
	}

	public void UpdateImposter(IImpostorCameraData cameraData)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((Behaviour)this).isActiveAndEnabled && Lod.Renderers[0].enabled;
		if ((!flag && !_forceUpdate) || (_forceUpdate && !flag))
		{
			return;
		}
		Vector3 lastCameraVector = _bounds.center - ((Component)cameraData.Camera).transform.position;
		bool flag2 = NeedsUpdate(cameraData) || _forceUpdate;
		if (cameraData.IsVisible(_bounds))
		{
			if (flag2 || IsTextureInvalid())
			{
				FitTexture();
				Render(cameraData.Camera);
				_lastCameraVector = lastCameraVector;
				_forceUpdate = false;
			}
			return;
		}
		bool flag3 = cameraData.IsVisibleInCache(_bounds);
		if (flag3 && DynamicImposterManager.Instance.cachingBehaviour == DynamicImposterManager.CachingBehaviour.preloadAndCacheInvisibleImposters)
		{
			if ((flag2 || IsTextureInvalid()) && DynamicImposterManager.Instance.GetPreloadLock())
			{
				FitTexture();
				Render(cameraData.Camera);
				_lastCameraVector = lastCameraVector;
				_forceUpdate = false;
			}
		}
		else if ((!flag3 || DynamicImposterManager.Instance.cachingBehaviour == DynamicImposterManager.CachingBehaviour.discardInvisibleImpostors) | flag2)
		{
			ReleaseTexture();
		}
	}

	private bool NeedsUpdate(IImpostorCameraData cameraData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _bounds.center - ((Component)cameraData.Camera).transform.position;
		float num = Vector3.Angle(val, _lastCameraVector);
		float num2 = Mathf.Abs(val.magnitude - _lastCameraVector.magnitude);
		if (!(num > angleTolerance * ANGLE_BIAS))
		{
			return num2 > _lastCameraVector.magnitude * (distanceTolerance * DISTANCE_BIAS / 100f);
		}
		return true;
	}

	public void Render(Camera camera)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		((Component)Beam.Utilities.ExtensionMethods.FirstOrDefault_NonAlloc(Lod.Renderers)).transform.rotation = Quaternion.LookRotation(((Component)this).transform.position - ((Component)camera).transform.position, Vector3.up);
		RenderImposter(camera);
	}

	private void RenderImposter(Camera camera)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (_imposterTexture == null)
		{
			return;
		}
		if ((Object)(object)_renderingCamera == (Object)null)
		{
			_renderingCamera = DynamicImposterManager.Instance.imposterRenderingCamera;
		}
		((Component)_renderingCamera).transform.position = ((Component)camera).transform.position;
		((Component)_renderingCamera).transform.LookAt(_bounds.center);
		_renderingCamera.cullingMask = 1 << DynamicImposterManager.Instance.imposterLayer;
		Vector3 val = ((Component)camera).transform.position - _bounds.center;
		_renderingCamera.orthographic = false;
		Vector3 val2 = ((Component)camera).transform.position - ((Component)this).transform.position;
		float magnitude = val2.magnitude;
		float fieldOfView = 2f * Mathf.Atan(_maxSize / (2f * magnitude)) * (180f / (float)Math.PI);
		_renderingCamera.fieldOfView = fieldOfView;
		_renderingCamera.nearClipPlane = val.magnitude - _maxSize;
		_renderingCamera.farClipPlane = val.magnitude + _maxSize;
		_renderingCamera.targetTexture = _imposterTexture.texture;
		using (IEnumerator<RenderableProxy> enumerator = _renderableProxies.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Renderer.enabled = true;
			}
			_renderingCamera.Render();
			enumerator.Reset();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Renderer.enabled = false;
			}
		}
		_renderingCamera.targetTexture = null;
	}

	private void FitTexture()
	{
		if (IsTextureInvalid() || _lastTextureSize != maxTextureSize)
		{
			AdjustTextureSize(maxTextureSize);
		}
		_lastTextureSize = maxTextureSize;
	}

	private void AdjustTextureSize(int size)
	{
		if (_imposterTexture == null || _imposterTexture.size != size)
		{
			if (_imposterTexture != null)
			{
				DynamicImposterManager.Instance.ReleaseRenderTexture(_imposterTexture);
				_imposterTexture = null;
			}
			_imposterTexture = DynamicImposterManager.Instance.GetRenderTexture(this, size);
			for (int i = 0; i < Lod.Renderers.Length; i++)
			{
				Renderer obj = Lod.Renderers[i];
				obj.material.mainTexture = (Texture)(object)_imposterTexture.texture;
				obj.material.SetFloat("_ZOffset", zOffset);
			}
		}
	}

	private void ReleaseTexture()
	{
		if (!IsTextureInvalid())
		{
			DynamicImposterManager.Instance.ReleaseRenderTexture(_imposterTexture);
			_imposterTexture = null;
		}
	}

	private bool IsTextureInvalid()
	{
		if (_imposterTexture != null)
		{
			return (Object)(object)_imposterTexture.texture == (Object)null;
		}
		return true;
	}

	public DynamicImpostor()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
	}
}
