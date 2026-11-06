using System.Linq;
using System.Reflection;
using Beam.Utilities;
using Photon.Bolt;
using UnityEngine;

namespace Funlabs;

public abstract class I_MultiplayerMessage : MultiplayerMessage
{
	private FieldInfo _i_field;

	public I_MultiplayerMessage()
	{
		_i_field = MultiplayerMessage.GetFields<I_Replicate>(GetType()).FirstOrDefault();
	}

	protected BoltEntity GetTransportEntity()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (null == _i_field)
		{
			return null;
		}
		object value = _i_field.GetValue(this);
		if (BeamTeam.IsNullOrDestroyed(value))
		{
			return null;
		}
		if (!typeof(Component).IsAssignableFrom(value.GetType()))
		{
			return null;
		}
		return MPExtensions.GetFirstComponentInParent<BoltEntity>(((Component)value).transform);
	}

	public void SetTransportEntity(BoltEntity transportEntity)
	{
		if (!((Object)(object)transportEntity == (Object)null))
		{
			FieldInfo fieldInfo = MultiplayerMessage.GetFields<I_Replicate>(GetType()).FirstOrDefault();
			if (!(fieldInfo == null))
			{
				fieldInfo.SetValue(this, ((Component)((Component)transportEntity).transform).GetComponentInChildren(fieldInfo.FieldType, true));
			}
		}
	}

	public override void Post()
	{
		if (!BeamTeam.IsNullOrDestroyed(MultiplayerMng.Instance))
		{
			_id = MultiplayerMng.GetMessageOutId();
			BoltEntity transportEntity = GetTransportEntity();
			Log($"Send {ToString()}\nTransport entity: {transportEntity}");
			I_MessageGlobalEvent.Post(transportEntity, (IProtocolToken)(object)new MultiplayerTokens.MessageEnvelope(this));
		}
	}

	public override string ToString()
	{
		string text = base.ToString();
		foreach (FieldInfo field in MultiplayerMessage.GetFields<I_Replicate>(GetType()))
		{
			text = text + "\n֎ I:" + field.Name + ":" + (field.GetValue(this)?.ToString() ?? "null");
		}
		return text;
	}
}
