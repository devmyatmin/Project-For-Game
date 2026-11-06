using System;
using System.Collections.Generic;
using Beam.Utilities;
using UnityEngine;

namespace Beam;

public static class SpriteDictionary
{
	private sealed class SpriteDictionaryFinalizer
	{
		~SpriteDictionaryFinalizer()
		{
			Unload();
		}
	}

	private class SpriteRef
	{
		public Sprite Sprite;

		public int Refs;
	}

	private static readonly SpriteDictionaryFinalizer _finalizer;

	public const string SPRITE_ASSETS_LIST_FILE_NAME = "Sprites/SpriteAssetList";

	public const string SPRITE_ASSETS_PATH = "Sprites/";

	private static Dictionary<string, SpriteRef> _sprites;

	private static bool _initialzed;

	static SpriteDictionary()
	{
		_finalizer = new SpriteDictionaryFinalizer();
		_sprites = new Dictionary<string, SpriteRef>(StringComparer.CurrentCultureIgnoreCase);
		Load();
	}

	private static void Load()
	{
		if (_initialzed)
		{
			return;
		}
		TextAsset val = Resources.Load<TextAsset>("Sprites/SpriteAssetList");
		string[] array = val.text.Split(',');
		foreach (string key in array)
		{
			if (!_sprites.ContainsKey(key))
			{
				_sprites.Add(key, new SpriteRef());
			}
		}
		Resources.UnloadAsset((Object)(object)val);
		_initialzed = true;
	}

	private static void Unload()
	{
		foreach (KeyValuePair<string, SpriteRef> sprite in _sprites)
		{
			Sprite val = sprite.Value?.Sprite;
			if (!BeamTeam.IsNullOrDestroyed(val))
			{
				Resources.UnloadAsset((Object)(object)val);
			}
		}
		_sprites.Clear();
		_initialzed = false;
		Debug.Log((object)"SpriteDictionary::Unload:: Success.");
	}

	public static Sprite GetSprite(string spriteName)
	{
		_sprites.TryGetValue(spriteName, out var value);
		if (value != null)
		{
			if (!BeamTeam.IsNullOrDestroyed(value.Sprite))
			{
				value.Refs++;
				return value.Sprite;
			}
			Sprite val = Resources.Load<Sprite>("Sprites/" + spriteName);
			if ((Object)(object)val != (Object)null)
			{
				value.Sprite = val;
				value.Refs++;
				return value.Sprite;
			}
		}
		return null;
	}

	public static void ReturnSprite(Sprite sprite)
	{
		if ((Object)(object)sprite == (Object)null)
		{
			return;
		}
		_sprites.TryGetValue(((Object)sprite).name, out var value);
		if (value != null)
		{
			value.Refs--;
			if (value.Refs == 0 && (Object)(object)value.Sprite != (Object)null)
			{
				Resources.UnloadAsset((Object)(object)value.Sprite);
				value.Sprite = null;
			}
		}
	}
}
