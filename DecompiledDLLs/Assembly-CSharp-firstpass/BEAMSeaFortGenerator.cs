using System;
using System.Collections.Generic;
using System.Linq;
using Beam;
using UnityEngine;

public class BEAMSeaFortGenerator : MonoBehaviour, IGenerationObject
{
	[SerializeField]
	private Transform startingPoint;

	public Module[] Modules;

	public Module[] StartModule;

	private static Random _randomSeed;

	[SerializeField]
	private int minIteration = 1;

	[SerializeField]
	private int maxIteration = 4;

	private void Start()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		_randomSeed = Singleton<StrandedWorld>.Instance.GenerateLocalSeed(((Component)this).transform.position);
		int num = _randomSeed.Next(minIteration, maxIteration);
		Module module = Object.Instantiate<Module>(StartModule[_randomSeed.Next(StartModule.Length)], startingPoint.position, ((Component)this).transform.rotation);
		((Component)module).transform.parent = ((Component)this).transform.parent;
		module.SetUpSupport(seaLadderAccess: true);
		List<ModuleConnector> list = new List<ModuleConnector>(module.GetExits());
		for (int i = 0; i < num; i++)
		{
			List<ModuleConnector> list2 = new List<ModuleConnector>();
			foreach (ModuleConnector item in list)
			{
				string random = GetRandom(item.Tags);
				Module module2 = Object.Instantiate<Module>(GetRandomWithTag(Modules, random));
				((Component)module2).transform.parent = ((Component)this).transform.parent;
				bool upSupport = _randomSeed.NextDouble() < 0.20000000298023224;
				module2.SetUpSupport(upSupport);
				ModuleConnector[] exits = module2.GetExits();
				ModuleConnector exitToMatch = exits.FirstOrDefault((ModuleConnector x) => x.IsDefault) ?? GetRandom(exits);
				MatchExits(item, exitToMatch);
				list2.AddRange(exits.Where((ModuleConnector e) => (Object)(object)e != (Object)(object)exitToMatch));
			}
			list = list2;
		}
	}

	private void MatchExits(ModuleConnector oldExit, ModuleConnector newExit)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Component)newExit).transform.parent.parent;
		float num = Azimuth(-((Component)oldExit).transform.forward) - Azimuth(((Component)newExit).transform.forward);
		parent.RotateAround(((Component)newExit).transform.position, Vector3.up, num);
		Vector3 val = ((Component)oldExit).transform.position - ((Component)newExit).transform.position;
		Transform transform = ((Component)parent).transform;
		transform.position += val;
	}

	private static TItem GetRandom<TItem>(TItem[] array)
	{
		return array[_randomSeed.Next(0, array.Length)];
	}

	private static Module GetRandomWithTag(IEnumerable<Module> modules, string tagToMatch)
	{
		return GetRandom(modules.Where((Module m) => Enumerable.Contains(m.Tags, tagToMatch)).ToArray());
	}

	private static float Azimuth(Vector3 vector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
	}
}
