using System;
using UnityEngine;

namespace Beam;

[Serializable]
public class Track
{
	public TrackType Type;

	public AudioClip Clip;

	public bool Loop = true;
}
