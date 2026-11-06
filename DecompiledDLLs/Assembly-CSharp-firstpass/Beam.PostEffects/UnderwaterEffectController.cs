using Ceto;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Beam.PostEffects;

[RequireComponent(typeof(Camera))]
public class UnderwaterEffectController : MonoBehaviour
{
	private static readonly Vector4[] m_corners = new Vector4[4]
	{
		new Vector4(-1f, -1f, -1f, 1f),
		new Vector4(1f, -1f, -1f, 1f),
		new Vector4(1f, 1f, -1f, 1f),
		new Vector4(-1f, 1f, -1f, 1f)
	};

	[SerializeField]
	private Camera _camera;

	public bool disableOnClip = true;

	public bool controlUnderwaterMode;

	private bool m_underWaterIsVisible;

	private UnderwaterSettings _settings;

	private WaveQuery _query = new WaveQuery();

	private void OnEnable()
	{
		BeamPfxProfile profile = ((Component)_camera).GetComponent<BeamPfxLayer>().Profile;
		if ((Object)(object)profile == (Object)null || !profile.TryGetSettings<UnderwaterSettings>(ref _settings))
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnPreRender()
	{
		m_underWaterIsVisible = false;
		for (int i = 0; i < Cameras.AllCameras.Count; i++)
		{
			IGameCamera gameCamera = Cameras.AllCameras[i];
			if (UnderWaterIsVisible(gameCamera.Camera))
			{
				m_underWaterIsVisible = true;
				break;
			}
		}
		if (controlUnderwaterMode && (Object)(object)Ceto.Ocean.Instance != (Object)null && Object.op_Implicit((Object)(object)Ceto.Ocean.Instance.UnderWater))
		{
			UnderWater underWater = Ceto.Ocean.Instance.UnderWater;
			if (!m_underWaterIsVisible)
			{
				underWater.underwaterMode = UNDERWATER_MODE.ABOVE_ONLY;
			}
			else
			{
				underWater.underwaterMode = UNDERWATER_MODE.ABOVE_AND_BELOW;
			}
		}
		bool flag = ShouldRenderEffect();
		((ParameterOverride<bool>)(object)((PostProcessEffectSettings)_settings).enabled).Override(flag);
	}

	private bool ShouldRenderEffect()
	{
		if (SystemInfo.graphicsShaderLevel < 30)
		{
			return false;
		}
		if ((Object)(object)Ceto.Ocean.Instance == (Object)null || (Object)(object)Ceto.Ocean.Instance.UnderWater == (Object)null || (Object)(object)Ceto.Ocean.Instance.Grid == (Object)null)
		{
			return false;
		}
		if (!((Component)Ceto.Ocean.Instance).gameObject.activeInHierarchy)
		{
			return false;
		}
		if (!((Behaviour)Ceto.Ocean.Instance.UnderWater).enabled || !((Behaviour)Ceto.Ocean.Instance.Grid).enabled)
		{
			return false;
		}
		if (Ceto.Ocean.Instance.UnderWater.underwaterMode == UNDERWATER_MODE.ABOVE_ONLY)
		{
			return false;
		}
		if (!m_underWaterIsVisible)
		{
			return false;
		}
		return true;
	}

	private bool UnderWaterIsVisible(Camera cam)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Ceto.Ocean.Instance == (Object)null)
		{
			return false;
		}
		Vector3 position = ((Component)cam).transform.position;
		if (disableOnClip)
		{
			_query.posX = position.x;
			_query.posZ = position.z;
			_query.mode = QUERY_MODE.CLIP_TEST;
			Ceto.Ocean.Instance.QueryWaves(_query);
			if (_query.result.isClipped)
			{
				return false;
			}
		}
		float num = Ceto.Ocean.Instance.FindMaxDisplacement(inculdeOverlays: true) + Ceto.Ocean.Instance.level;
		if (position.y < num)
		{
			return true;
		}
		Matrix4x4 val = cam.projectionMatrix * cam.worldToCameraMatrix;
		Matrix4x4 inverse = val.inverse;
		for (int i = 0; i < 4; i++)
		{
			Vector4 val2 = inverse * m_corners[i];
			val2.y /= val2.w;
			if (val2.y < num)
			{
				return true;
			}
		}
		return false;
	}

	static UnderwaterEffectController()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
	}
}
