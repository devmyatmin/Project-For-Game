using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Beam.Utilities;
using UnityEngine;

namespace Beam.AccountServices;

public class ModFolderContentProvider : MonoBehaviour, IUserContentProvider
{
	public static ModFolderContentProvider Instance { get; private set; }

	public event Action<LoadedContentArgs> LoadedUserContent;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			BeamTeam.Destroy<GameObject>(((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void OnDestroy()
	{
		LoadedUserContent = null;
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	void IUserContentProvider.RequestUserContent()
	{
		IEnumerable<LoadedContent> loadedContent = LoadMods();
		LoadedUserContent?.Invoke(new LoadedContentArgs
		{
			LoadedContent = loadedContent
		});
	}

	private IEnumerable<LoadedContent> LoadMods()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<LoadedContent> list = new List<LoadedContent>();
		if (Directory.Exists(FilePath.MODS_MAPS_FOLDER))
		{
			string[] directories = Directory.GetDirectories(FilePath.MODS_MAPS_FOLDER);
			foreach (string path in directories)
			{
				list.Add(new LoadedContent
				{
					Path = path,
					Tags = new string[1] { UgcTags.MAPS }
				});
				string relativePath = Path.GetRelativePath(FilePath.MODS_ROOT_FOLDER, path);
				stringBuilder.Append("Loaded mod content: '" + UgcTags.MAPS + "' at path: " + relativePath + "\n");
			}
		}
		if (Directory.Exists(FilePath.MODS_LOCALIZATION_FOLDER))
		{
			string[] directories = Directory.GetDirectories(FilePath.MODS_LOCALIZATION_FOLDER);
			foreach (string path2 in directories)
			{
				list.Add(new LoadedContent
				{
					Path = path2,
					Tags = new string[1] { UgcTags.LOCALIZATION }
				});
				string relativePath2 = Path.GetRelativePath(FilePath.MODS_ROOT_FOLDER, path2);
				stringBuilder.Append("Loaded mod content: '" + UgcTags.LOCALIZATION + "' at path: " + relativePath2 + "\n");
			}
		}
		Debug.Log((object)((stringBuilder.Length == 0) ? ("No mod content found at path: " + FilePath.MODS_ROOT_FOLDER) : ("Loaded mods:\n" + stringBuilder.ToString())));
		return list;
	}
}
