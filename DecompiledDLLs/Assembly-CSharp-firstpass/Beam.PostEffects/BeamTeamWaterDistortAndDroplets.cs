using Beam.Utilities;
using UnityEngine;

namespace Beam.PostEffects;

[RequireComponent(typeof(Camera))]
public class BeamTeamWaterDistortAndDroplets : MonoBehaviour
{
	private Shader _shader;

	private Material _material;

	private void Awake()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		_shader = Shader.Find("Beam Team/Post Effects/Water Distort and Droplets");
		if (!Object.op_Implicit((Object)(object)_shader))
		{
			Debug.LogError((object)"Beam Team Water Distortion:: Error finding water distortion shader.", (Object)(object)this);
			((Behaviour)this).enabled = false;
		}
		else
		{
			_material = new Material(_shader);
		}
	}

	private void OnDestroy()
	{
		BeamTeam.DestroyImmediate<Material>(_material, false);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit((Texture)(object)source, destination, _material);
	}
}
