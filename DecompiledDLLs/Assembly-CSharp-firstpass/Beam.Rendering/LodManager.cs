using System;
using System.Collections.Generic;
using Beam.Events;
using UnityEngine;

namespace Beam.Rendering;

public class LodManager : Singleton<LodManager>
{
	public static float LOD_BIAS = 1f;

	private readonly List<LodController> _lodControllers = new List<LodController>();

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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		Camera.onPreCull = (CameraCallback)Delegate.Remove((Delegate?)(object)Camera.onPreCull, (Delegate?)new CameraCallback(Camera_OnPreCull));
		EventManager.RemoveListener<OptionsAppliedEvent>(EventManager_OptionsApplied);
		base.OnDestroy();
	}

	private void EventManager_OptionsApplied(OptionsAppliedEvent e)
	{
		LoadOptions();
	}

	private void LoadOptions()
	{
		switch (Options.GraphicsSettings.DrawDistance)
		{
		case Quality_DrawDistance.NEAR:
			LOD_BIAS = 0.75f;
			break;
		case Quality_DrawDistance.FAR:
			LOD_BIAS = 0.9f;
			break;
		case Quality_DrawDistance.VERY_FAR:
			LOD_BIAS = 1f;
			break;
		}
	}

	private void Camera_OnPreCull(Camera camera)
	{
		if (!((Object)(object)camera == (Object)null) && (((Component)camera).CompareTag("MainCamera") || ((Component)camera).CompareTag("PlayerCamera")))
		{
			Do(camera);
		}
	}

	public void Add(LodController controller)
	{
		_lodControllers.Add(controller);
	}

	public void Remove(LodController controller)
	{
		_lodControllers.Remove(controller);
	}

	public void Do(Camera camera)
	{
		for (int i = 0; i < _lodControllers.Count; i++)
		{
			_lodControllers[i].UpdateLodGroup(camera);
		}
	}
}
