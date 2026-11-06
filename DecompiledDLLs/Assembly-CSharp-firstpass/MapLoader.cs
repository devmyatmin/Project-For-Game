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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Timing.RunCoroutine(LoadMap_Sequence(data));
	}

	public IEnumerator<float> LoadMap_Sequence(JObject data)
	{
		Singleton<LE_LevelEditorMain>.Instance.InitializeObjects();
		yield return 0f;
		Vector3 val = new Vector3(0f, 5000f, 0f);
		if (data != null && !data.IsNull())
		{
			foreach (JObject child in data.Children)
			{
				JObject field = child.GetField("prefab");
				if (!field)
				{
					Debug.LogError((object)"MapLoader::LoadMap:: Error loading. No prefab id field found for object data.");
				}
				else
				{
					uint num = Prefabs.ConvertLegacyPrefabId(field);
					if (num == 0)
					{
						Debug.LogError((object)"MapLoader::LoadMap:: Error loading instantiated object with empty prefab id.");
					}
					else
					{
						GameObject val2 = null;
						foreach (LE_Object mapObjectReference in Singleton<LE_LevelEditorMain>.Instance.MapObjectReferences)
						{
							if (mapObjectReference.Id == num)
							{
								val2 = Object.Instantiate<GameObject>(((Component)mapObjectReference).gameObject);
								((Object)val2).name = ((Object)val2).name.Replace("(Clone)", "");
								break;
							}
						}
						if ((Object)(object)val2 != (Object)null)
						{
							val2.transform.position = val;
							val2.transform.parent = Singleton<LE_LevelEditorMain>.Instance.ObjectDumpContainer;
							val += new Vector3(0f, 0f, 50f);
							try
							{
								JSerializer.DeserializeInto(child.GetField("Transform"), (Component)(object)val2.gameObject.transform);
							}
							catch (Exception ex)
							{
								Debug.LogError((object)("MapLoader::LoadMap:: Error loading editor object. Object has now been destroyed. \n\nName: " + ((Object)val2).name + "\nMessage: " + ex));
							}
						}
						else
						{
							Debug.LogError((object)("mapLoader:: Error loading object " + num + " :: no object to be instantiated!"));
						}
					}
				}
			}
			yield break;
		}
		Debug.LogError((object)"mapLoader:: Error loading map data:: no data!");
	}
}
