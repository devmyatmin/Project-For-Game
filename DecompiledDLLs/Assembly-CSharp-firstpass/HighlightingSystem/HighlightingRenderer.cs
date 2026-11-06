using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace HighlightingSystem;

public class HighlightingRenderer : HighlightingBase
{
	public static readonly List<HighlightingPreset> defaultPresets = new List<HighlightingPreset>
	{
		new HighlightingPreset
		{
			name = "Default",
			downsampleFactor = 4,
			iterations = 2,
			blurMinSpread = 0.65f,
			blurSpread = 0.25f,
			blurIntensity = 0.3f,
			blurDirections = BlurDirections.Diagonal
		},
		new HighlightingPreset
		{
			name = "Wide",
			downsampleFactor = 4,
			iterations = 4,
			blurMinSpread = 0.65f,
			blurSpread = 0.25f,
			blurIntensity = 0.3f,
			blurDirections = BlurDirections.Diagonal
		},
		new HighlightingPreset
		{
			name = "Strong",
			downsampleFactor = 4,
			iterations = 2,
			blurMinSpread = 0.5f,
			blurSpread = 0.15f,
			blurIntensity = 0.325f,
			blurDirections = BlurDirections.Diagonal
		},
		new HighlightingPreset
		{
			name = "Speed",
			downsampleFactor = 4,
			iterations = 1,
			blurMinSpread = 0.75f,
			blurSpread = 0f,
			blurIntensity = 0.35f,
			blurDirections = BlurDirections.Diagonal
		},
		new HighlightingPreset
		{
			name = "Quality",
			downsampleFactor = 2,
			iterations = 3,
			blurMinSpread = 0.5f,
			blurSpread = 0.5f,
			blurIntensity = 0.28f,
			blurDirections = BlurDirections.Diagonal
		},
		new HighlightingPreset
		{
			name = "Solid 1px",
			downsampleFactor = 1,
			iterations = 1,
			blurMinSpread = 1f,
			blurSpread = 0f,
			blurIntensity = 1f,
			blurDirections = BlurDirections.All
		},
		new HighlightingPreset
		{
			name = "Solid 2px",
			downsampleFactor = 1,
			iterations = 2,
			blurMinSpread = 1f,
			blurSpread = 0f,
			blurIntensity = 1f,
			blurDirections = BlurDirections.All
		}
	};

	[SerializeField]
	private List<HighlightingPreset> _presets = new List<HighlightingPreset>(defaultPresets);

	private ReadOnlyCollection<HighlightingPreset> _presetsReadonly;

	public ReadOnlyCollection<HighlightingPreset> presets
	{
		get
		{
			if (_presetsReadonly == null)
			{
				_presetsReadonly = _presets.AsReadOnly();
			}
			return _presetsReadonly;
		}
	}

	public bool GetPreset(string name, out HighlightingPreset preset)
	{
		for (int i = 0; i < _presets.Count; i++)
		{
			HighlightingPreset highlightingPreset = _presets[i];
			if (highlightingPreset.name == name)
			{
				preset = highlightingPreset;
				return true;
			}
		}
		preset = default;
		return false;
	}

	public bool AddPreset(HighlightingPreset preset, bool overwrite)
	{
		for (int i = 0; i < _presets.Count; i++)
		{
			if (_presets[i].name == preset.name)
			{
				if (overwrite)
				{
					_presets[i] = preset;
					return true;
				}
				return false;
			}
		}
		_presets.Add(preset);
		return true;
	}

	public bool RemovePreset(string name)
	{
		for (int i = 0; i < _presets.Count; i++)
		{
			if (_presets[i].name == name)
			{
				_presets.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public bool LoadPreset(string name)
	{
		if (GetPreset(name, out var preset))
		{
			ApplyPreset(preset);
		}
		return false;
	}

	public void ApplyPreset(HighlightingPreset preset)
	{
		downsampleFactor = preset.downsampleFactor;
		iterations = preset.iterations;
		blurMinSpread = preset.blurMinSpread;
		blurSpread = preset.blurSpread;
		blurIntensity = preset.blurIntensity;
		blurDirections = preset.blurDirections;
	}

	public void ClearPresets()
	{
		_presets.Clear();
	}
}
