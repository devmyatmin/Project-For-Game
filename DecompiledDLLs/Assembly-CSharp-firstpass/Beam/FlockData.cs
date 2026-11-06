using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beam;

[Serializable]
public class FlockData
{
	[Header("Species")]
	[SerializeField]
	private FlockSpecies _species;

	[SerializeField]
	private FlockChild _prefab;

	[Space]
	[SerializeField]
	private int _population;

	[Range(0f, 100f)]
	[SerializeField]
	private int _percentage;

	[Range(0f, 24f)]
	[SerializeField]
	private float _startTime;

	[Range(0f, 24f)]
	[SerializeField]
	private float _endTime;

	[SerializeField]
	private AudioClip _flockAudioClip;

	private bool _active;

	private List<FlockChild> _children = new List<FlockChild>();

	public FlockSpecies Species => _species;

	public FlockChild Prefab => _prefab;

	public int Population => _population;

	public int Percentage => _percentage;

	public float StartTime => _startTime;

	public float EndTime => _endTime;

	public AudioClip FlockAudioClip => _flockAudioClip;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}

	[HideInInspector]
	public bool Migrating { get; set; }

	public IList<FlockChild> Children => _children;

	[HideInInspector]
	public Vector3 Waypoint
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	[HideInInspector]
	public FlockAudio Audio { get; set; }
}
