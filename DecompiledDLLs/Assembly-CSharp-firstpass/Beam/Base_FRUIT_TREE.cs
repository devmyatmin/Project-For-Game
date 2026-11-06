using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

namespace Beam;

public class Base_FRUIT_TREE : BaseObject
{
	[SerializeField]
	private InteractiveObject_FOOD _fruitPrefab;

	[SerializeField]
	private Transform _fruitPosition;

	private bool _fruitCreated;

	protected override void Start()
	{
		base.Start();
		if (!LevelLoader.IsLoading())
		{
			CreateFruit();
		}
	}

	private void CreateFruit()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (_fruitCreated)
		{
			return;
		}
		_fruitCreated = true;
		MiniGuid referenceId = MiniGuid.NewFrom(ReferenceId);
		if (MPExtensions.IsMaster(Game.Mode))
		{
			Setup(MultiplayerMng.Instantiate<InteractiveObject_FOOD>(_fruitPrefab.PrefabId, _fruitPosition.position, _fruitPosition.rotation, referenceId));
			return;
		}
		MultiplayerZoneSync.Instance.LinkData(MultiplayerZoneSync.Usage.Fruit, referenceId, (MultiplayerZoneSync.Command command) =>
		{
			Setup((InteractiveObject_FOOD)command.Instance);
		});
	}

	private void Setup(InteractiveObject_FOOD fruit)
	{
		fruit.Attach(new StaticAttachingAnchor(_fruitPosition));
		fruit.IsGrowing = true;
	}

	public override void OnCustomMapLoaded()
	{
		base.OnCustomMapLoaded();
		CreateFruit();
	}

	public override JObject Save()
	{
		JObject jObject = base.Save();
		jObject.AddField("fruitCreated", _fruitCreated);
		return jObject;
	}

	public override void Load(JObject data)
	{
		base.Load(data);
		JObject field = data.GetField("fruitCreated");
		if (field != null && !field.IsNull())
		{
			_fruitCreated = field.GetValue<bool>();
		}
	}
}
