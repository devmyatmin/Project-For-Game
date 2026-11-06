using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beam;

public class ProjectileAudio : MonoBehaviour
{
	[SerializeField]
	[PREFAB_PROCESSOR_GetComponent]
	private ProjectileMovement _projectile;

	[SerializeField]
	private PhysicMaterialAudio[] _stuckMaterialAudioClips;

	[SerializeField]
	private PhysicMaterialAudio[] _brokenMaterialAudioClips;

	private void Start()
	{
		if ((Object)(object)_projectile != (Object)null)
		{
			_projectile.Stuck += Projectile_Stuck;
			_projectile.Broken += Projectile_Broken;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_projectile != (Object)null)
		{
			_projectile.Stuck -= Projectile_Stuck;
			_projectile.Broken -= Projectile_Broken;
		}
	}

	private void Projectile_Stuck(ProjectileMovement sender, IProjectileEventData data)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		AudioClip materialAudioClip = GetMaterialAudioClip(_stuckMaterialAudioClips, data.Material);
		AudioManager.GetAudioPlayer().Play3D(materialAudioClip, data.Position, AudioMixerChannels.FX);
	}

	private void Projectile_Broken(ProjectileMovement sender, IProjectileEventData data)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		AudioClip materialAudioClip = GetMaterialAudioClip(_brokenMaterialAudioClips, data.Material);
		AudioManager.GetAudioPlayer().Play3D(materialAudioClip, data.Position, AudioMixerChannels.FX);
	}

	private AudioClip GetMaterialAudioClip(IEnumerable<PhysicMaterialAudio> materialClips, PhysicMaterial material)
	{
		PhysicMaterialAudio physicMaterialAudio = materialClips.FirstOrDefault((PhysicMaterialAudio p) => p.Material == material);
		if (physicMaterialAudio?.Clips != null && physicMaterialAudio.Clips.Length != 0)
		{
			return physicMaterialAudio.Clips[Random.Range(0, physicMaterialAudio.Clips.Length)];
		}
		return null;
	}
}
