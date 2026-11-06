using System.Collections.Generic;
using Beam.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace Beam.Rendering;

public class LodController : MonoBehaviour, IImpostorParent
{
	public const int MAX_LOD_CULLING_DISTANCE = 100;

	[SerializeField]
	private LodGroup _lodGroup = new LodGroup();

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private ImpostorBase _impostor;

	[SerializeField]
	private ImpostorMode _mode;

	[SerializeField]
	private ImposterScope _scope;

	[SerializeField]
	private int _localImpostorCullingDistance = 200;

	[SerializeField]
	private int _id;

	[Tooltip("Will only apply to objects without a terrain impostor lod.")]
	[SerializeField]
	private bool _dither;

	[Tooltip("Length of dither fade. Used by last lod renderer /or impostor.")]
	[SerializeField]
	private float _ditherFadeWidth = 7f;

	public LodGroup LodGroup => _lodGroup;

	public ImpostorBase Impostor => _impostor;

	public ImpostorMode Mode => _mode;

	public ImposterScope Scope => _scope;

	int IImpostorParent.Id => _id;

	Vector3 IImpostorParent.Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
	}

	Quaternion IImpostorParent.Rotation
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.rotation;
		}
	}

	IList<Renderer> IImpostorParent.Renderables => Beam.Utilities.ExtensionMethods.LastOrDefault_NonAlloc(_lodGroup.Lods)?.Renderers;

	private void Start()
	{
		Singleton<LodManager>.Instance?.Add(this);
		if (_scope == ImposterScope.None || _scope.HasFlag(ImposterScope.Manual))
		{
			return;
		}
		if (LevelLoader.IsLoading(((Component)this).gameObject))
		{
			LevelLoader.CurrentLoader.PostLoading.Add(FindImpostor);
			return;
		}
		FindImpostor();
		if ((Object)(object)_impostor == (Object)null)
		{
			CreateImpostor();
		}
	}

	private void OnDestroy()
	{
		Singleton<LodManager>.Instance?.Remove(this);
	}

	public void AdoptLodGroup(LodGroup lodGroup)
	{
		_lodGroup.Lods.Clear();
		LinqExtensions.AddRange<Lod>(_lodGroup.Lods, (IEnumerable<Lod>)lodGroup.Lods);
		if ((Object)(object)_impostor != (Object)null)
		{
			_lodGroup.Lods.Add(_impostor.Lod);
		}
	}

	public void UpdateLodGroup(Camera camera)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)camera).transform.position - ((Component)this).transform.position;
		float magnitude = val.magnitude;
		Lod lod = null;
		for (int i = 0; i < _lodGroup.Lods.Count; i++)
		{
			Lod lod2 = _lodGroup.Lods[i];
			float num = (((float?)lod?.CullingDistance) ?? 0f) * LodManager.LOD_BIAS;
			float num2 = (float)lod2.CullingDistance * LodManager.LOD_BIAS;
			bool flag = magnitude > num;
			bool flag2 = magnitude < num2;
			bool active = flag & flag2;
			lod2.SetActive(active);
			if (_dither && lod2.Active && Beam.Utilities.ExtensionMethods.IsLast(_lodGroup.Lods, i))
			{
				float num3 = Mathf.Min(_ditherFadeWidth, num2);
				float num4 = num2 - num3;
				float value = Mathf.Clamp01((magnitude - num4) / num3);
				lod2.Crossfade(value);
			}
			lod = lod2;
		}
	}

	private void FindImpostor()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_impostor != (Object)null)
		{
			Debug.LogError((object)"We have impostor.", (Object)(object)((Component)this).gameObject);
			return;
		}
		ImpostorBase impostorBase = StrandedWorld.GetZone(((Component)this).transform.position).FindImpostor(((Component)this).gameObject);
		if ((Object)(object)impostorBase != (Object)null)
		{
			AdoptImpostor(impostorBase);
		}
	}

	private void CreateImpostor()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_impostor != (Object)null)
		{
			Debug.LogError((object)"We have impostor.", (Object)(object)((Component)this).gameObject);
			return;
		}
		ImpostorBase impostorBase = StrandedWorld.GetZone(((Component)this).transform.position).CreateImpostor(_id, ((Component)this).transform.position, ((Component)this).transform.rotation);
		if ((Object)(object)impostorBase != (Object)null)
		{
			impostorBase.SetRenderables(((IImpostorParent)this).Renderables);
			AdoptImpostor(impostorBase);
		}
	}

	private void AdoptImpostor(ImpostorBase impostor)
	{
		_impostor = impostor;
		_impostor.Lod.IsImpostor = true;
		_impostor.Lod.CullingDistance = (_scope.HasFlag(ImposterScope.Terrain) ? 1000 : _localImpostorCullingDistance);
		_lodGroup.Lods.Add(_impostor.Lod);
	}

	private bool ValidateLodGroup(LodGroup lodGroup, ref string errorMessage)
	{
		if (_scope.HasFlag(ImposterScope.Manual))
		{
			return true;
		}
		foreach (Lod lod in lodGroup.Lods)
		{
			if (lod.CullingDistance <= 0 || lod.CullingDistance > 100)
			{
				errorMessage = $"Lod culling distances must be between 0-{100} units.";
				return false;
			}
		}
		return true;
	}

	private bool CheckTerrain()
	{
		return _scope.HasFlag(ImposterScope.Terrain);
	}

	private void UpdateMode()
	{
		if (_mode == ImpostorMode.Dynamic)
		{
			_id = -2;
		}
	}

	private void UpdateDitherMode()
	{
		Lod lod = Beam.Utilities.ExtensionMethods.LastOrDefault_NonAlloc(_lodGroup.Lods);
		if (lod == null)
		{
			return;
		}
		for (int i = 0; i < lod.Renderers.Length; i++)
		{
			Material sharedMaterial = lod.Renderers[i].sharedMaterial;
			if (_dither)
			{
				sharedMaterial.EnableKeyword("LOD_FADE_CROSSFADE");
				sharedMaterial.DisableKeyword("_ENABLE_DISTANCE_CROSSFADE");
				if (sharedMaterial.HasProperty("_DitherMode"))
				{
					sharedMaterial.SetInt("_DitherMode", 1);
				}
			}
			else
			{
				sharedMaterial.DisableKeyword("LOD_FADE_CROSSFADE");
				sharedMaterial.DisableKeyword("_ENABLE_DISTANCE_CROSSFADE");
				if (sharedMaterial.HasProperty("_DitherMode"))
				{
					sharedMaterial.SetInt("_DitherMode", 0);
				}
			}
		}
	}
}
