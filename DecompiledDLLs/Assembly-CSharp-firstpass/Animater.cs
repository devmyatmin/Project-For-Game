using System;
using System.Collections;
using System.Collections.Generic;
using Beam;
using Beam.Utilities;
using UnityEngine;

public class Animater : MonoBehaviour, ISerializationCallbackReceiver
{
	[SerializeField]
	private Character _character;

	private Dictionary<AnimationType, AnimationData> _animations = new Dictionary<AnimationType, AnimationData>();

	[SerializeField]
	[HideInInspector]
	private List<AnimationType> _keys;

	[SerializeField]
	[HideInInspector]
	private List<AnimationData> _values;

	private Coroutine _sequence;

	private AnimationType _currentAnimationType;

	public AnimationType CurrentAnimationType => _currentAnimationType;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_keys.Clear();
		_values.Clear();
		if (_animations == null)
		{
			return;
		}
		foreach (KeyValuePair<AnimationType, AnimationData> animation in _animations)
		{
			_keys.Add(animation.Key);
			_values.Add(animation.Value);
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_animations = new Dictionary<AnimationType, AnimationData>();
		for (int i = 0; i < Mathf.Min(_keys.Count, _values.Count); i++)
		{
			_animations.Add(_keys[i], _values[i]);
		}
	}

	public AnimationClip GetAnimation(AnimationType type)
	{
		return _character.AnimatorOverrideController[type.ToString()];
	}

	public void PlayPickupAnimation()
	{
		_character.PlayPickupAnimation();
	}

	public Coroutine Play(AnimationType type, Action callback = null, float scale = 1f, float normalizedTime = 0f, bool useAnimation = true, AnimationBlendMode blend = AnimationBlendMode.Crossfade)
	{
		BeamTeam.SafeStopCoroutine((MonoBehaviour)(object)this, _sequence);
		_currentAnimationType = type;
		_sequence = ((MonoBehaviour)this).StartCoroutine(Play_Sequence(type, callback, scale, normalizedTime, useAnimation, blend));
		return _sequence;
	}

	private IEnumerator Play_Sequence(AnimationType type, Action callback, float scale, float normalizedTime, bool useAnimation, AnimationBlendMode blend)
	{
		_animations.TryGetValue(type, out var value);
		yield return _character.PlayUseAnimation(type, (value?.PlaybackSpeed ?? 1f) * scale, normalizedTime, value?.CallbackDelay ?? 0.3f, useAnimation, callback, blend);
		_currentAnimationType = AnimationType.NONE;
	}
}
