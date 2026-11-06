using System.Collections.Generic;
using System.Linq;
using Beam.Serialization.Json;
using Beam.Utilities;
using Funlabs;
using UnityEngine;

namespace Beam.Crafting;

public class Construction_STRUCTURE : BaseObject
{
	private List<Constructing> _constructions = new List<Constructing>();

	[PREFAB_PROCESSOR_GetComponent]
	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	[Tooltip("Disable ghost collision when snapping to a connector")]
	private bool _disableGhostCollisionOnSnap;

	public IList<Constructing> Constructions => _constructions;

	public Rigidbody rigidbody => _rigidbody;

	public bool DisableGhostCollisionOnSnap => _disableGhostCollisionOnSnap;

	public override bool CanReorder => false;

	public virtual void AddConstruction(Constructing constructing)
	{
		_constructions.Add(constructing);
	}

	public void RemoveConstruction(Constructing constructing)
	{
		_constructions.Remove(constructing);
		if (_constructions.Count == 0)
		{
			MultiplayerMng.Destroy(this);
		}
	}

	public TConstruction GetConstruction<TConstruction>() where TConstruction : Constructing
	{
		return _constructions.FirstOrDefault((Constructing construction) => ((object)construction).GetType() == typeof(TConstruction)) as TConstruction;
	}

	public virtual void Recenter()
	{
	}

	public override void OnTransformSerialisationIdChanged()
	{
		base.OnTransformSerialisationIdChanged();
		if (BeamTeam.IsNullOrDestroyed(_constructions))
		{
			return;
		}
		foreach (Constructing construction in _constructions)
		{
			construction.OnTransformSerialisationIdChanged();
		}
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		JObject jObject2 = new JObject();
		for (int i = 0; i < _constructions.Count; i++)
		{
			Constructing constructing = _constructions[i];
			jObject2.Add(constructing.ReferenceId.ToString());
		}
		jObject.AddField("Constructions", jObject2);
		return jObject;
	}

	public override void Load(JObject data)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		transform.localScale = Vector3.one;
		JObject constructionsData = data.GetField("Constructions");
		if (constructionsData == null || constructionsData.IsNull())
		{
			return;
		}
		LevelLoader.CurrentLoader.DeferredLoading.Insert(0, () =>
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < constructionsData.Children.Count; i++)
			{
				Constructing constructing = MultiplayerMng.FindObject<Constructing>(Beam.Utilities.ExtensionMethods.ToMiniGuid(constructionsData.Children[i].GetValue<string>()));
				if ((Object)(object)constructing != (Object)null)
				{
					constructing.Structure = this;
					constructing.TransformParent = transform;
					constructing.transform.parent = transform;
					constructing.transform.localPosition = constructing.Pos;
					constructing.transform.localRotation = constructing.Rot;
					constructing.transform.localScale = Vector3.one;
					AddConstruction(constructing);
				}
			}
		});
	}
}
