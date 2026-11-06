using System;
using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
	private void OnEnable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreRender = (CameraCallback)Delegate.Combine((Delegate?)(object)Camera.onPreRender, (Delegate?)new CameraCallback(Camera_OnPreRender));
	}

	private void OnDisable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreRender = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreRender, (Delegate?)new CameraCallback(Camera_OnPreRender));
	}

	private void Camera_OnPreRender(Camera camera)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)camera == (Object)null) && (((Component)camera).CompareTag("MainCamera") || ((Component)camera).CompareTag("PlayerCamera")))
		{
			Vector3 val = ((Component)camera).transform.position - ((Component)this).transform.position;
			val.x = 0f;
			val.z = 0f;
			((Component)this).transform.LookAt(((Component)camera).transform.position - val);
		}
	}
}
