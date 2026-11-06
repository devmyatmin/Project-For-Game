using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class VRIKCalibrationController : MonoBehaviour
{
	[Tooltip("Reference to the VRIK component on the avatar.")]
	public VRIK ik;

	[Tooltip("The settings for VRIK calibration.")]
	public VRIKCalibrator.Settings settings;

	[Tooltip("The HMD.")]
	public Transform headTracker;

	[Tooltip("(Optional) A tracker placed anywhere on the body of the player, preferrably close to the pelvis, on the belt area.")]
	public Transform bodyTracker;

	[Tooltip("(Optional) A tracker or hand controller device placed anywhere on or in the player's left hand.")]
	public Transform leftHandTracker;

	[Tooltip("(Optional) A tracker or hand controller device placed anywhere on or in the player's right hand.")]
	public Transform rightHandTracker;

	[Tooltip("(Optional) A tracker placed anywhere on the ankle or toes of the player's left leg.")]
	public Transform leftFootTracker;

	[Tooltip("(Optional) A tracker placed anywhere on the ankle or toes of the player's right leg.")]
	public Transform rightFootTracker;

	[Header("Data stored by Calibration")]
	public VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();

	private void LateUpdate()
	{
		if (Input.GetKeyDown((KeyCode)99))
		{
			data = VRIKCalibrator.Calibrate(ik, settings, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);
		}
		if (Input.GetKeyDown((KeyCode)100))
		{
			if (data.scale == 0f)
			{
				Debug.LogError((object)"No Calibration Data to calibrate to, please calibrate with settings first.");
			}
			else
			{
				VRIKCalibrator.Calibrate(ik, data, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);
			}
		}
		if (Input.GetKeyDown((KeyCode)115))
		{
			if (data.scale == 0f)
			{
				Debug.LogError((object)"Avatar needs to be calibrated before RecalibrateScale is called.");
			}
			VRIKCalibrator.RecalibrateScale(ik, data, settings);
		}
	}
}
