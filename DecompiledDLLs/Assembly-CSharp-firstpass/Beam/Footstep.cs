using System;
using UnityEngine;

namespace Beam;

public class Footstep : MonoBehaviour
{
	[Serializable]
	public class Footsteps
	{
		public string name;

		public AudioClip[] audioClips;
	}

	[SerializeField]
	private LayerMask rayCastMasks;

	[SerializeField]
	private Footsteps[] _foosteps;

	private Movement _playerMovement;

	private IAudioPlayer _audioPlayer;

	private int _clipIndex;

	private int _footstepIndex;

	public Movement PlayerMovement
	{
		get
		{
			return _playerMovement;
		}
		set
		{
			_playerMovement = value;
		}
	}

	private void Awake()
	{
		_audioPlayer = AudioManager.GetAudioPlayer();
	}

	public void PlayFootstep(float intensity)
	{
		if ((Object)(object)_playerMovement == (Object)null)
		{
			return;
		}
		_footstepIndex = 0;
		if (_playerMovement.InWater)
		{
			if (_playerMovement.isFloating)
			{
				_footstepIndex = 10;
			}
			else
			{
				_footstepIndex = 11;
			}
		}
		else if (_playerMovement.ClimbingLadder)
		{
			if (_playerMovement.activeLadder.isTree)
			{
				_footstepIndex = 9;
			}
			else
			{
				_footstepIndex = 8;
			}
		}
		else if (_playerMovement.grounded)
		{
			Footstep_Terrain();
		}
		Play_Audio();
	}

	private void Play_Audio()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (_clipIndex >= _foosteps[_footstepIndex].audioClips.Length)
		{
			_clipIndex = 0;
		}
		Vector3 velocity = _playerMovement.GetVelocity();
		float num = Mathf.Clamp01(Mathf.Abs(velocity.magnitude * 7f));
		if (!(num < 0.05f))
		{
			AudioClip clip = _foosteps[_footstepIndex].audioClips[_clipIndex];
			AudioActorHandle handle = _audioPlayer.Play3D(clip, ((Component)this).transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near);
			_audioPlayer.GetAudioSource(handle).Volume = num;
			_clipIndex++;
		}
	}

	private void Footstep_Terrain()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (!Physics.Raycast(((Component)_playerMovement).transform.position, Vector3.down, ref val, 10f, rayCastMasks.value, (QueryTriggerInteraction)1))
		{
			return;
		}
		if (((Component)val.collider).gameObject.CompareTag("Terrain"))
		{
			float num = ((Component)_playerMovement).transform.position.y - _playerMovement.CharacterController.height / 2f;
			if (num < 0.05f)
			{
				_footstepIndex = 3;
			}
			else if (num < 0.35f)
			{
				_footstepIndex = 4;
			}
			else if (num < 1.2f)
			{
				_footstepIndex = 5;
			}
			else if (num < 2f)
			{
				_footstepIndex = 6;
			}
			else
			{
				_footstepIndex = 7;
			}
		}
		else
		{
			string name = ((Object)val.collider.material).name;
			if (name.Contains("Wood"))
			{
				_footstepIndex = 0;
			}
			else if (name.Contains("Rock"))
			{
				_footstepIndex = 1;
			}
			else if (name.Contains("Metal"))
			{
				_footstepIndex = 2;
			}
			else if (name.Contains("Flesh"))
			{
				_footstepIndex = 3;
			}
		}
	}
}
