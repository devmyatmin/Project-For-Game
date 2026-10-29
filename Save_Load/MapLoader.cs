using System;
using System.Collections.Generic;
using Beam;
using Beam.Serialization;
using Beam.Serialization.Json;
using LE_LevelEditor;
using LE_LevelEditor.Core;
using MEC;
using UnityEngine;

public class MapLoader : ILoader
{
	private List<Action> _deferredLoading = new List<Action>();

	private List<Action> _postLoading = new List<Action>();

	private List<Action> _physicalLoading = new List<Action>();

	public IList<Action> DeferredLoading => _deferredLoading;

	public IList<Action> PostLoading => _postLoading;

	public IList<Action> PhysicalLoading => _physicalLoading;

	public bool IsLoading { get; set; }

	public CoroutineHandle LoadMap(JObject data)
	{
		return Timing.RunCoroutine(LoadMap_Sequence(data));
	}

	public IEnumerator<float> LoadMap_Sequence(JObject data)
	{
		Singleton<LE_LevelEditorMain>.Instance.InitializeObjects();
		yield return 0f;
		Vector3 position = new Vector3(0f, 5000f, 0f);
		if (data != null && !data.IsNull())
		{
			foreach (JObject child in data.Children)
			{
				JObject field = child.GetField("prefab");
				if (!field)
				{
					Debug.LogError("MapLoader::LoadMap:: Error loading. No prefab id field found for object data.");
				}
				else
				{
					uint num = Prefabs.ConvertLegacyPrefabId(field);
					if (num == 0)
					{
						Debug.LogError("MapLoader::LoadMap:: Error loading instantiated object with empty prefab id.");
					}
					else
					{
						GameObject gameObject = null;
						foreach (LE_Object mapObjectReference in Singleton<LE_LevelEditorMain>.Instance.MapObjectReferences)
						{
							if (mapObjectReference.Id == num)
							{
								gameObject = UnityEngine.Object.Instantiate(mapObjectReference.gameObject);
								gameObject.name = gameObject.name.Replace("(Clone)", "");
								break;
							}
						}
						if (gameObject != null)
						{
							gameObject.transform.position = position;
							gameObject.transform.parent = Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer;
							position += new Vector3(0f, 0f, 50f);
							try
							{
								JSerializer.DeserializeInto(child.GetField("Transform"), gameObject.gameObject.transform);
							}
							catch (Exception ex)
							{
								Debug.LogError("MapLoader::LoadMap:: Error loading editor object. Object has now been destroyed. \n\nName: " + gameObject.name + "\nMessage: " + ex);
							}
						}
						else
						{
							Debug.LogError("mapLoader:: Error loading object " + num + " :: no object to be instantiated!");
						}
					}
				}
			}
			yield break;
		}
		Debug.LogError("mapLoader:: Error loading map data:: no data!");
	}
}
