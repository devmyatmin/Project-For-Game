using System;
using UnityEngine;

namespace HighlightingSystem;

[Serializable]
public struct HighlightingPreset : IEquatable<HighlightingPreset>
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private int _downsampleFactor;

	[SerializeField]
	private int _iterations;

	[SerializeField]
	private float _blurMinSpread;

	[SerializeField]
	private float _blurSpread;

	[SerializeField]
	private float _blurIntensity;

	[SerializeField]
	private BlurDirections _blurDirections;

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public int downsampleFactor
	{
		get
		{
			return _downsampleFactor;
		}
		set
		{
			_downsampleFactor = value;
		}
	}

	public int iterations
	{
		get
		{
			return _iterations;
		}
		set
		{
			_iterations = value;
		}
	}

	public float blurMinSpread
	{
		get
		{
			return _blurMinSpread;
		}
		set
		{
			_blurMinSpread = value;
		}
	}

	public float blurSpread
	{
		get
		{
			return _blurSpread;
		}
		set
		{
			_blurSpread = value;
		}
	}

	public float blurIntensity
	{
		get
		{
			return _blurIntensity;
		}
		set
		{
			_blurIntensity = value;
		}
	}

	public BlurDirections blurDirections
	{
		get
		{
			return _blurDirections;
		}
		set
		{
			_blurDirections = value;
		}
	}

	bool IEquatable<HighlightingPreset>.Equals(HighlightingPreset other)
	{
		if (_name == other._name && _downsampleFactor == other._downsampleFactor && _iterations == other._iterations && _blurMinSpread == other._blurMinSpread && _blurSpread == other._blurSpread && _blurIntensity == other._blurIntensity)
		{
			return _blurDirections == other._blurDirections;
		}
		return false;
	}
}
