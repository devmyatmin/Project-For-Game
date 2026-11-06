using System.IO;
using UnityEngine;

namespace kode80.EditorTools;

[AddComponentMenu("")]
public class RecordVideo : MonoBehaviour
{
	public int captureFramerate = 60;

	public int superSize = 1;

	public string folderPath;

	private bool _isRecording;

	private int _sceneNumber;

	private int _frameNumber;

	public bool isRecording => _isRecording;

	public int sceneNumber => _sceneNumber;

	private void Start()
	{
		Time.captureFramerate = captureFramerate;
	}

	private void Update()
	{
		if (_isRecording)
		{
			ScreenCapture.CaptureScreenshot($"{folderPath}/Scene{_sceneNumber:D03}Frame{_frameNumber:D08}.png", superSize);
			_frameNumber++;
		}
	}

	public bool StartRecording()
	{
		if (!_isRecording && Directory.Exists(folderPath))
		{
			_isRecording = true;
		}
		return _isRecording;
	}

	public void StopRecording()
	{
		if (_isRecording)
		{
			_isRecording = false;
			_sceneNumber++;
			_frameNumber = 0;
		}
	}
}
