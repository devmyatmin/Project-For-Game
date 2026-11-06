using System;
using System.Collections;
using System.Collections.Generic;
using Beam;
using Beam.Rendering;
using Funlabs;
using UnityEngine;

public class DynamicImposterManager : MonoBehaviour
{
	public enum CachingBehaviour
	{
		discardInvisibleImpostors,
		cacheInvisibleIposters,
		preloadAndCacheInvisibleImposters
	}

	public static DynamicImposterManager Instance;

	public CachingBehaviour cachingBehaviour;

	public int imposterLayer = 30;

	public float preLoadFovFactor = 2f;

	public int preloadRate = 5;

	public List<DynamicImpostor> Imposters = new List<DynamicImpostor>();

	public List<ImposterTexture> imposterTextures = new List<ImposterTexture>();

	public List<ImposterTexture> freeImposterTextures = new List<ImposterTexture>();

	public Camera imposterRenderingCamera;

	private Dictionary<Camera, IImpostorCameraData> _cameraData;

	private List<DynamicImpostor> invalidImposters = new List<DynamicImpostor>();

	private int _preloadCounter;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		SetupCameras();
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
		Singleton<GameTime>.Instance.HourTick += GameTime_HourTick;
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
		if ((Object)(object)Singleton<GameTime>.Instance != (Object)null)
		{
			Singleton<GameTime>.Instance.HourTick -= GameTime_HourTick;
		}
	}

	private void GameTime_HourTick()
	{
		ForceImposterUpdate();
	}

	private void SetupCameras()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_cameraData = new Dictionary<Camera, IImpostorCameraData>();
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			Camera camera = PlayerRegistry.LocalPlayer.PlayerCamera.Camera;
			_cameraData.Add(camera, new ImpostorCameraData(camera, CreateCachingCamera(camera)));
		}
		else
		{
			int index = 0;
			Camera camera2 = Cameras.AllCameras[index].Camera;
			_cameraData.Add(camera2, new ImpostorCameraData(camera2, CreateCachingCamera(camera2)));
		}
	}

	private Camera CreateCachingCamera(Camera camera)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Camera val = new GameObject("Imposter Caching Camera").AddComponent<Camera>();
		val.CopyFrom(camera);
		((Component)val).transform.SetParent(((Component)camera).transform);
		((Component)val).transform.position = ((Component)camera).transform.position;
		((Component)val).transform.rotation = ((Component)camera).transform.rotation;
		((Behaviour)val).enabled = false;
		((Component)val).gameObject.SetActive(false);
		return val;
	}

	public void ForceImposterUpdate()
	{
		ForceImposterUpdate(int.MaxValue);
	}

	public void ForceImposterUpdate(int maxUpdatesPerFrame)
	{
		((MonoBehaviour)this).StartCoroutine(ForceUpdate_Sequence(maxUpdatesPerFrame));
	}

	private IEnumerator ForceUpdate_Sequence(int maxUpdates)
	{
		int i = 0;
		while (i < Imposters.Count)
		{
			int num = 0;
			while (num < maxUpdates && i < Imposters.Count)
			{
				if ((Object)(object)Imposters[i] != (Object)null)
				{
					Imposters[i].ForceUpdate();
					num++;
				}
				i++;
			}
			yield return (object)new WaitForEndOfFrame();
		}
		yield return null;
	}

	private void Camera_OnPreCull(Camera camera)
	{
		if (Imposters.Count == 0 || (Object)(object)camera == (Object)null || (!_cameraData.TryGetValue(camera, out var value) && !Cameras.TryGetCameraData(camera, out value)))
		{
			return;
		}
		_preloadCounter = 0;
		value.Update();
		for (int i = 0; i < Imposters.Count; i++)
		{
			DynamicImpostor dynamicImpostor = Imposters[i];
			if ((Object)(object)dynamicImpostor != (Object)null)
			{
				dynamicImpostor.UpdateImposter(value);
			}
			else
			{
				invalidImposters.Add(dynamicImpostor);
			}
		}
		if (invalidImposters.Count > 0)
		{
			for (int j = 0; j < invalidImposters.Count; j++)
			{
				Imposters.Remove(invalidImposters[j]);
			}
			invalidImposters.Clear();
		}
	}

	public ImposterTexture GetRenderTexture(DynamicImpostor newOwner, int size)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected Obj, but got Unknown
		for (int i = 0; i < freeImposterTextures.Count; i++)
		{
			ImposterTexture imposterTexture = freeImposterTextures[i];
			if (imposterTexture.size == size)
			{
				freeImposterTextures.Remove(imposterTexture);
				imposterTexture.owner = newOwner;
				return imposterTexture;
			}
		}
		RenderTexture val = new RenderTexture(size, size, 16, (RenderTextureFormat)0);
		val.antiAliasing = 1;
		ImposterTexture imposterTexture2 = new ImposterTexture();
		imposterTexture2.owner = newOwner;
		imposterTexture2.size = size;
		imposterTexture2.createdTime = (imposterTexture2.lastUsedTime = Time.frameCount);
		imposterTexture2.texture = val;
		imposterTextures.Add(imposterTexture2);
		return imposterTexture2;
	}

	public void ReleaseRenderTexture(ImposterTexture imposterTexture)
	{
		if (!((Object)(object)imposterTexture.texture == (Object)null))
		{
			freeImposterTextures.Add(imposterTexture);
		}
	}

	public void GarbageCollect()
	{
		for (int i = 0; i < freeImposterTextures.Count; i++)
		{
			ImposterTexture imposterTexture = freeImposterTextures[i];
			imposterTextures.Remove(imposterTexture);
			Object.Destroy((Object)(object)imposterTexture.texture);
		}
		freeImposterTextures.Clear();
	}

	public bool GetPreloadLock()
	{
		return _preloadCounter++ < preloadRate;
	}
}
