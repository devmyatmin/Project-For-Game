using System.Collections;
using UnityEngine;

public class DistortionMobile : MonoBehaviour
{
	public float TextureScale = 1f;

	public RenderTextureFormat RenderTextureFormat;

	public FilterMode FilterMode;

	public LayerMask CullingMask = LayerMask.op_Implicit(-17);

	public RenderingPath RenderingPath;

	public int FPSWhenMoveCamera = 40;

	public int FPSWhenStaticCamera = 20;

	private RenderTexture renderTexture;

	private Camera cameraInstance;

	private GameObject goCamera;

	private Vector3 oldPosition;

	private Quaternion oldRotation;

	private Transform instanceCameraTransform;

	private bool canUpdateCamera;

	private bool isStaticUpdate;

	private WaitForSeconds fpsMove;

	private WaitForSeconds fpsStatic;

	private const int dropedFrames = 50;

	private int frameCountWhenCameraIsStatic;

	private bool isInitialized;

	private void OnEnable()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected Obj, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		fpsMove = new WaitForSeconds(1f / (float)FPSWhenMoveCamera);
		fpsStatic = new WaitForSeconds(1f / (float)FPSWhenStaticCamera);
	}

	private void Update()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialized)
		{
			Initialize();
			((MonoBehaviour)this).StartCoroutine(RepeatCameraMove());
			((MonoBehaviour)this).StartCoroutine(RepeatCameraStatic());
		}
		if (Vector3.SqrMagnitude(instanceCameraTransform.position - oldPosition) <= 1E-05f && instanceCameraTransform.rotation == oldRotation)
		{
			frameCountWhenCameraIsStatic++;
			if (frameCountWhenCameraIsStatic >= 50)
			{
				isStaticUpdate = true;
			}
		}
		else
		{
			frameCountWhenCameraIsStatic = 0;
			isStaticUpdate = false;
		}
		oldPosition = instanceCameraTransform.position;
		oldRotation = instanceCameraTransform.rotation;
		if (canUpdateCamera)
		{
			((Behaviour)cameraInstance).enabled = true;
			canUpdateCamera = false;
		}
		else if (((Behaviour)cameraInstance).enabled)
		{
			((Behaviour)cameraInstance).enabled = false;
		}
	}

	private IEnumerator RepeatCameraMove()
	{
		while (true)
		{
			if (!isStaticUpdate)
			{
				canUpdateCamera = true;
			}
			yield return fpsMove;
		}
	}

	private IEnumerator RepeatCameraStatic()
	{
		while (true)
		{
			if (isStaticUpdate)
			{
				canUpdateCamera = true;
			}
			yield return fpsStatic;
		}
	}

	private void OnBecameVisible()
	{
		if ((Object)(object)goCamera != (Object)null)
		{
			goCamera.SetActive(true);
		}
	}

	private void OnBecameInvisible()
	{
		if ((Object)(object)goCamera != (Object)null)
		{
			goCamera.SetActive(false);
		}
	}

	private void Initialize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected Obj, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		goCamera = new GameObject("RenderTextureCamera");
		cameraInstance = goCamera.AddComponent<Camera>();
		Camera main = Camera.main;
		cameraInstance.CopyFrom(main);
		Camera val = cameraInstance;
		float depth = val.depth;
		val.depth = depth + 1f;
		cameraInstance.cullingMask = LayerMask.op_Implicit(CullingMask);
		cameraInstance.renderingPath = RenderingPath;
		goCamera.transform.parent = ((Component)main).transform;
		renderTexture = new RenderTexture(Mathf.RoundToInt((float)Screen.width * TextureScale), Mathf.RoundToInt((float)Screen.height * TextureScale), 16, RenderTextureFormat);
		renderTexture.DiscardContents();
		((Texture)renderTexture).filterMode = FilterMode;
		cameraInstance.targetTexture = renderTexture;
		instanceCameraTransform = ((Component)cameraInstance).transform;
		oldPosition = instanceCameraTransform.position;
		Shader.SetGlobalTexture("_GrabTextureMobile", (Texture)(object)renderTexture);
		isInitialized = true;
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)goCamera))
		{
			Object.DestroyImmediate((Object)(object)goCamera);
			goCamera = null;
		}
		if (Object.op_Implicit((Object)(object)renderTexture))
		{
			Object.DestroyImmediate((Object)(object)renderTexture);
			renderTexture = null;
		}
		isInitialized = false;
	}

	public DistortionMobile()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
	}
}
