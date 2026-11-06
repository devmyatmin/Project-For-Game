using System.Collections;
using Beam;
using UnityEngine;

namespace LE_LevelEditor.Logic;

public class LE_ScreenshotCamera : Singleton<LE_ScreenshotCamera>
{
	[SerializeField]
	private Animation _cameraShutter;

	private Transform _camTransform;

	private Camera _cam;

	private int replacementCounter;

	private void Start()
	{
		_cam = Camera.main;
		_camTransform = ((Component)_cam).GetComponent<Transform>();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && !Singleton<LE_LevelEditorMain>.Instance.IsMouseOverGUI())
		{
			RenderLevelPreviewScreenshot();
		}
	}

	private void RenderLevelPreviewScreenshot()
	{
		((Component)this).gameObject.GetComponent<AudioSource>().Play();
		_cameraShutter.Play();
		((MonoBehaviour)this).StartCoroutine(DrawLevelPreview());
	}

	private IEnumerator DrawLevelPreview()
	{
		int imageWidth = 540;
		int imageHeight = 320;
		Singleton<LE_LevelEditorMain>.Instance.Boundary.SetActive(false);
		yield return (object)new WaitForEndOfFrame();
		Texture2D val = new Texture2D(imageWidth, imageHeight, (TextureFormat)3, false, false);
		_ = _cam.pixelRect;
		Rect val2 = new Rect((float)Screen.width * 0.5f - (float)imageWidth * 0.5f, (float)Screen.height * 0.5f - (float)imageHeight * 0.5f, (float)imageWidth, (float)imageHeight);
		_cam.Render();
		val.ReadPixels(val2, 0, 0);
		val.Apply(false);
		if (Singleton<LE_LevelEditorMain>.Instance.MapScreenshots.Count >= 8)
		{
			Singleton<LE_LevelEditorMain>.Instance.MapScreenshots[replacementCounter] = val;
			replacementCounter++;
			if (replacementCounter >= 8)
			{
				replacementCounter = 0;
			}
		}
		else
		{
			Singleton<LE_LevelEditorMain>.Instance.MapScreenshots.Add(val);
		}
		Singleton<LE_LevelEditorMain>.Instance.Boundary.SetActive(true);
	}

	public void TakeManualScreenShots()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		_ = _camTransform.position;
		((Component)_camTransform).transform.position = new Vector3(165f, 0f, 113f);
		((Component)_camTransform).transform.LookAt(Vector3.zero);
		RenderLevelPreviewScreenshot();
		_camTransform.position = new Vector3(-165f, 0f, 113f);
		_camTransform.LookAt(Vector3.zero);
		RenderLevelPreviewScreenshot();
		_camTransform.position = new Vector3(165f, 0f, -113f);
		_camTransform.LookAt(Vector3.zero);
		RenderLevelPreviewScreenshot();
		_camTransform.position = new Vector3(-165f, 0f, -113f);
		_camTransform.LookAt(Vector3.zero);
		RenderLevelPreviewScreenshot();
	}
}
