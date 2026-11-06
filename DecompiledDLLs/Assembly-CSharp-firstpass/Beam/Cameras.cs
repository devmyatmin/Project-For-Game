using System;
using System.Collections.Generic;
using System.IO;
using Beam.Events;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Utilities;
using Ceto;
using UnityEngine;

namespace Beam;

public class Cameras : Singleton<Cameras>
{
	private static List<IGameCamera> _AllCameras = new List<IGameCamera>();

	private static Camera _Main;

	private static Camera _Current;

	private static float _DefaultFov;

	private static Plane[] _planes;

	private Camera _postCamera;

	public static IReadOnlyList<IGameCamera> AllCameras => _AllCameras;

	public static Camera Main
	{
		get
		{
			if ((Object)(object)_Main == (Object)null)
			{
				_Main = Camera.main;
			}
			return _Main;
		}
	}

	public static Camera Current
	{
		get
		{
			if ((Object)(object)_Current == (Object)null)
			{
				return Main;
			}
			return _Current;
		}
	}

	public static float DefaultFOV => _DefaultFov;

	public Dictionary<Camera, IImpostorCameraData> CameraData { get; } = new Dictionary<Camera, IImpostorCameraData>();

	public static event Action<Camera> OnPreCull;

	public static Camera GetPostCamera()
	{
		return Singleton<Cameras>.Instance?._postCamera;
	}

	public static bool TryGetCameraData(Camera camera, out IImpostorCameraData cameraData)
	{
		cameraData = null;
		return Singleton<Cameras>.Instance?.CameraData.TryGetValue(camera, out cameraData) ?? false;
	}

	private void Awake()
	{
		_planes = new Plane[6];
		_postCamera = CreatePostCamera();
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
		EventManager.AddListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		LoadOptions();
	}

	protected override void OnDestroy()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected Obj, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected Obj, but got Unknown
		OnPreCull = null;
		_AllCameras.Clear();
		_Main = null;
		_Current = null;
		DestroyCamera(_postCamera);
		_planes = null;
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		base.OnDestroy();
	}

	private Camera CreatePostCamera()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Camera val = Object.Instantiate<Camera>(Resources.Load<Camera>("Prefabs/PostCamera"));
		((Object)((Component)val).gameObject).name = "~PostCamera";
		((Component)val).transform.SetParent(((Component)this).transform);
		((Component)val).transform.position = Vector3.zero;
		((Component)val).transform.rotation = Quaternion.identity;
		return val;
	}

	public Camera CreateCamera(Camera copy)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Camera val = Object.Instantiate<Camera>(copy);
		((Object)((Component)val).gameObject).name = "~Camera";
		((Component)val).transform.SetParent(((Component)this).transform);
		((Component)val).transform.position = Vector3.zero;
		((Component)val).transform.rotation = Quaternion.identity;
		val.rect = new Rect(0f, 0f, 1f, 1f);
		CameraUtils component = ((Component)val).GetComponent<CameraUtils>();
		if (component != null)
		{
			component.RemoveComponents();
			return val;
		}
		return val;
	}

	private void DestroyCamera(Camera camera)
	{
		CameraData.Remove(camera);
		BeamTeam.Destroy<GameObject>(((Component)camera).gameObject);
	}

	public static void AddCamera(IGameCamera playerCamera)
	{
		_AllCameras.Add(playerCamera);
	}

	public static void RemoveCamera(IGameCamera playerCamera)
	{
		_AllCameras.Remove(playerCamera);
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void LoadOptions()
	{
		float num = 60f + Options.GraphicsSettings.FieldOfView * 15f;
		if (num < 60f || num > 75f)
		{
			Debug.LogError((object)"Cameras::LoadOptions:: Something went wrong. Saved field of view value was out of bounds.Field of view is now set to 75.0");
			num = 75f;
		}
		_DefaultFov = num;
		((IList<IGameCamera>)_AllCameras).SetFieldOfView(_DefaultFov);
	}

	public static bool IsUnderwater(Transform t)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Ceto.Ocean.Instance == (Object)null || (Object)(object)t == (Object)null)
		{
			return false;
		}
		return t.position.y < Ceto.Ocean.Instance.QueryWaves(t.position.x, t.position.z);
	}

	public static bool IsVisible(Bounds bounds)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _AllCameras.Count; i++)
		{
			IGameCamera gameCamera = _AllCameras[i];
			BeamGeometryUtility.CalculateFrustumPlanes(_planes, gameCamera.Camera.projectionMatrix * gameCamera.Camera.worldToCameraMatrix);
			if (GeometryUtility.TestPlanesAABB(_planes, bounds))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCulled(Vector3 position, float cullingDistance)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _AllCameras.Count; i++)
		{
			Vector3 val = ((Component)_AllCameras[i].Camera).transform.position - position;
			if (val.sqrMagnitude < BeamMath.Sqr(cullingDistance))
			{
				return false;
			}
		}
		return true;
	}

	private void Camera_OnPreCull(Camera camera)
	{
		if (!((Object)(object)camera == (Object)null) && (((Component)camera).CompareTag("MainCamera") || ((Component)camera).CompareTag("PlayerCamera")))
		{
			_Current = camera;
			OnPreCull?.Invoke(_Current);
			CalculateFrustumCorners(_Current);
		}
	}

	private void CalculateFrustumCorners(Camera camera)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float fieldOfView = camera.fieldOfView;
		float aspect = camera.aspect;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = fieldOfView * 0.5f;
		Vector3 val = ((Component)camera).transform.right * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f)) * aspect;
		Vector3 val2 = ((Component)camera).transform.up * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f));
		Vector3 val3 = ((Component)camera).transform.forward * nearClipPlane - val + val2;
		float num2 = val3.magnitude * farClipPlane / nearClipPlane;
		val3.Normalize();
		val3 *= num2;
		Vector3 val4 = ((Component)camera).transform.forward * nearClipPlane + val + val2;
		val4.Normalize();
		val4 *= num2;
		Vector3 val5 = ((Component)camera).transform.forward * nearClipPlane + val - val2;
		val5.Normalize();
		val5 *= num2;
		Vector3 val6 = ((Component)camera).transform.forward * nearClipPlane - val - val2;
		val6.Normalize();
		val6 *= num2;
		identity.SetRow(0, Vector4.op_Implicit(val3));
		identity.SetRow(1, Vector4.op_Implicit(val4));
		identity.SetRow(2, Vector4.op_Implicit(val5));
		identity.SetRow(3, Vector4.op_Implicit(val6));
		Shader.SetGlobalMatrix("_ShaderFrustumCorners", identity);
	}

	private void DoScreenshot()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 down = Vector3.down;
		Vector3 position = ((Component)StrandedWorld.GetZone(Vector3.zero)).transform.position + Vector3.up * 128f;
		CaptureScreen(position, down, 1920, 1080, "default");
	}

	private void SaveManager_BeforeWrite(JObject rootData)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Zone zone = StrandedWorld.GetZone(((Component)Main).transform.position);
		Vector3 down = Vector3.down;
		Vector3 val = ((Component)zone).transform.position + Vector3.up * 128f;
		string name = (((int)Game.Mode == 0) ? "dynamic" : "dynamic_COOP");
		CaptureScreen(val, down, Screen.width, Screen.height, name);
		JObject jObject = new JObject();
		jObject.AddField("Pos", JSerializer.Serialize((object)val));
		jObject.AddField("Dir", JSerializer.Serialize((object)down));
		rootData.AddField("ScreenCap", jObject);
	}

	private void CaptureScreen(Vector3 position, Vector3 direction, int width, int height, string name)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected Obj, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"Cameras::CaptureScreen:: Capturing Screen...");
		Camera val = CreateCamera(_postCamera);
		val.depth = GetPostCamera().depth + 2f;
		Camera val2 = CreateCamera(PlayerRegistry.AllPlayers[0].PlayerCamera.Camera);
		val2.depth = val.depth - 1f;
		if (!CameraData.ContainsKey(val2))
		{
			CameraData.Add(val2, new AlwaysCameraData(val2));
		}
		Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 24, (RenderTextureFormat)0);
		RenderInto(val2, temporary, position, rotation);
		RenderInto(val, temporary, position, rotation);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D val3 = new Texture2D(((Texture)temporary).width, ((Texture)temporary).height, (TextureFormat)3, false);
		val3.ReadPixels(new Rect(0f, 0f, (float)((Texture)val3).width, (float)((Texture)val3).height), 0, 0);
		val3.Apply(false);
		RenderTexture.active = active;
		temporary.Release();
		DestroyCamera(val2);
		DestroyCamera(val);
		Debug.Log((object)"Cameras::CaptureScreen:: Writing screenshot to file...");
		string path = PathTools.Combine(new string[2]
		{
			SaveManager.GetSaveFolderPath(),
			name + ".bytes"
		});
		try
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				byte[] array = ImageConversion.EncodeToPNG(val3);
				fileStream.Write(array, 0, array.Length);
			}
			Debug.Log((object)"Cameras::CaptureScreen:: Complete.");
		}
		catch (Exception)
		{
			Debug.LogError((object)"Cameras::CaptureScreen:: Error writing screenshot to file: ");
			LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
			localizedNotification.Priority = NotificationPriority.Immediate;
			localizedNotification.Duration = 8f;
			localizedNotification.PlayerId = 0;
			localizedNotification.MessageText.SetTerm("Error writing screenshot to file. Please contact Beam Team on the Steam Forum.");
			localizedNotification.Raise();
		}
	}

	private void RenderInto(Camera camera, RenderTexture target, Vector3 position, Quaternion rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Component)camera).transform.position = position;
		((Component)camera).transform.rotation = rotation;
		camera.targetTexture = target;
		camera.Render();
	}
}
