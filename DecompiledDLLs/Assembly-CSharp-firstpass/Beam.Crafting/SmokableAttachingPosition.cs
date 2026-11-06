using System;
using UnityEngine;

namespace Beam.Crafting;

[Serializable]
public class SmokableAttachingPosition
{
	[SerializeField]
	private Transform _attachingPosition;

	public Smoking Smokable;

	public Transform AttachingPosition => _attachingPosition;
}
