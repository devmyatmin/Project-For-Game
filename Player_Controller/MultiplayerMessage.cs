using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beam;
using Beam.Crafting;
using Beam.Utilities;
using Photon.Bolt;
using UdpKit;
using UnityEngine;

namespace Funlabs;

public abstract class MultiplayerMessage : IMultiplayerMessage, IProtocolToken
{
	protected ushort _id;

	protected float _receivedTime;

	protected bool _fromSelf;

	protected float _ping;

	public ushort Id => _id;

	public float ReceivedTime
	{
		get
		{
			return _receivedTime;
		}
		set
		{
			_receivedTime = value;
		}
	}

	public float Ping
	{
		get
		{
			return _ping;
		}
		set
		{
			_ping = value;
		}
	}

	protected virtual bool IsLogEnabled()
	{
		return true;
	}

	public void Log(string value, string color = "yellow")
	{
		if (IsLogEnabled())
		{
			MultiplayerMng.Log(value, "messages", color);
		}
	}

	protected static IEnumerable<FieldInfo> GetFields<T>(Type type) where T : Attribute
	{
		if (type == null)
		{
			return Enumerable.Empty<FieldInfo>();
		}
		return (from field in type.GetFields()
			where field.GetCustomAttributes(typeof(T), inherit: true).Length != 0
			select field).Concat(GetFields<T>(type.BaseType));
	}

	protected void Write(Type type, object @object, UdpPacket packet)
	{
		if (type == typeof(bool))
		{
			packet.WriteBool((bool)@object);
			return;
		}
		if (type == typeof(byte))
		{
			packet.WriteByte((byte)@object);
			return;
		}
		if (type == typeof(short))
		{
			packet.WriteShort((short)@object);
			return;
		}
		if (type == typeof(int))
		{
			packet.WriteInt((int)@object);
			return;
		}
		if (type == typeof(uint))
		{
			packet.WriteUInt((uint)@object);
			return;
		}
		if (type == typeof(float))
		{
			packet.WriteFloat((float)@object);
			return;
		}
		if (type == typeof(MiniGuid))
		{
			packet.WriteMiniGuid((MiniGuid)@object);
			return;
		}
		if (type == typeof(Guid))
		{
			packet.WriteGuid((Guid)@object);
			return;
		}
		if (type == typeof(Vector3))
		{
			packet.WriteVector3((Vector3)@object);
			return;
		}
		if (type == typeof(Quaternion))
		{
			packet.WriteQuaternion((Quaternion)@object);
			return;
		}
		if (type == typeof(string))
		{
			packet.WriteString((string)@object);
			return;
		}
		if (typeof(ISaveablePrefab).IsAssignableFrom(type) || typeof(IPickupable).IsAssignableFrom(type) || typeof(IInteractable).IsAssignableFrom(type) || typeof(ISkinnable).IsAssignableFrom(type) || typeof(IUseable).IsAssignableFrom(type) || typeof(IChargeable).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab((ISaveablePrefab)@object);
			return;
		}
		if (typeof(Piscus_Creature).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab(((Piscus_Creature)@object)?.gameObject.GetComponent<ISaveablePrefab>());
			return;
		}
		if (typeof(LandAnimal).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab(((LandAnimal)@object)?.gameObject.GetComponent<ISaveablePrefab>());
			return;
		}
		if (typeof(FlockChild).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab(((FlockChild)@object)?.gameObject.GetComponent<ISaveablePrefab>());
			return;
		}
		if (typeof(VehicleMovementBase).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab(((VehicleMovementBase)@object)?.gameObject.GetComponent<ISaveablePrefab>());
			return;
		}
		if (typeof(BoltEntity).IsAssignableFrom(type))
		{
			packet.WriteULong(((BoltEntity)@object)?.NetworkId.PackedValue ?? 0);
			return;
		}
		if (typeof(IPlayer).IsAssignableFrom(type))
		{
			packet.WriteByte((byte)(((IPlayer)@object)?.Id ?? 255));
			return;
		}
		if (typeof(IMultiplayerStorageReference).IsAssignableFrom(type))
		{
			IMultiplayerStorageReference multiplayerStorageReference = (IMultiplayerStorageReference)@object;
			packet.WriteMiniGuid(multiplayerStorageReference.ReferenceId);
			return;
		}
		if (typeof(Rigidbody).IsAssignableFrom(type))
		{
			Rigidbody rigidbody = (Rigidbody)@object;
			packet.WriteRigidbody(rigidbody);
			return;
		}
		if (typeof(Collider).IsAssignableFrom(type))
		{
			Collider collider = (Collider)@object;
			packet.WriteCollider(collider);
			return;
		}
		if (typeof(ProjectileObject).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab(((ProjectileObject)@object)?.gameObject.GetComponent<ISaveablePrefab>());
			return;
		}
		throw new Exception("Type not supported");
	}

	protected object Read(Type type, UdpPacket packet)
	{
		if (type == typeof(bool))
		{
			return packet.ReadBool();
		}
		if (type == typeof(byte))
		{
			return packet.ReadByte();
		}
		if (type == typeof(short))
		{
			return packet.ReadShort();
		}
		if (type == typeof(int))
		{
			return packet.ReadInt();
		}
		if (type == typeof(uint))
		{
			return packet.ReadUInt();
		}
		if (type == typeof(float))
		{
			return packet.ReadFloat();
		}
		if (type == typeof(MiniGuid))
		{
			return packet.ReadMiniGuid();
		}
		if (type == typeof(Guid))
		{
			return packet.ReadGuid();
		}
		if (type == typeof(Vector3))
		{
			return packet.ReadVector3();
		}
		if (type == typeof(Quaternion))
		{
			return packet.ReadQuaternion();
		}
		if (type == typeof(string))
		{
			return packet.ReadString();
		}
		if (typeof(ISaveablePrefab).IsAssignableFrom(type) || typeof(IPickupable).IsAssignableFrom(type) || typeof(IBase).IsAssignableFrom(type) || typeof(IInteractable).IsAssignableFrom(type) || typeof(ISkinnable).IsAssignableFrom(type) || typeof(IUseable).IsAssignableFrom(type) || typeof(IChargeable).IsAssignableFrom(type))
		{
			return packet.ReadSaveablePrefab<ISaveablePrefab>();
		}
		if (typeof(Piscus_Creature).IsAssignableFrom(type))
		{
			return packet.ReadSaveablePrefab<SaveablePrefab>()?.GetComponent<Piscus_Creature>();
		}
		if (typeof(LandAnimal).IsAssignableFrom(type))
		{
			return packet.ReadSaveablePrefab<SaveablePrefab>()?.GetComponent<LandAnimal>();
		}
		if (typeof(FlockChild).IsAssignableFrom(type))
		{
			return packet.ReadSaveablePrefab<SaveablePrefab>()?.GetComponent<FlockChild>();
		}
		if (typeof(VehicleMovementBase).IsAssignableFrom(type))
		{
			return packet.ReadSaveablePrefab<SaveablePrefab>()?.GetComponent<VehicleMovementBase>();
		}
		if (typeof(BoltEntity).IsAssignableFrom(type))
		{
			return BoltNetwork.FindEntity(new NetworkId(packet.ReadULong()));
		}
		if (typeof(IPlayer).IsAssignableFrom(type))
		{
			return PlayerUtilities.GetPlayerFromId(packet.ReadByte());
		}
		if (typeof(IMultiplayerStorageReference).IsAssignableFrom(type))
		{
			return StorageRegistry.Get(packet.ReadMiniGuid());
		}
		if (typeof(Rigidbody).IsAssignableFrom(type))
		{
			return packet.ReadRigidbody();
		}
		if (typeof(Collider).IsAssignableFrom(type))
		{
			return packet.ReadCollider();
		}
		if (typeof(ProjectileObject).IsAssignableFrom(type))
		{
			return packet.ReadSaveablePrefab<SaveablePrefab>()?.GetComponent<ProjectileObject>();
		}
		throw new Exception("Type not supported");
	}

	protected void Write(FieldInfo field, UdpPacket packet)
	{
		try
		{
			Type fieldType = field.FieldType;
			if (field.FieldType.IsArray)
			{
				Type elementType = fieldType.GetElementType();
				Array array = (Array)field.GetValue(this);
				packet.WriteByte((byte)array.Length);
				{
					foreach (object item in array)
					{
						Write(elementType, item, packet);
					}
					return;
				}
			}
			Write(fieldType, field.GetValue(this), packet);
		}
		catch (Exception ex)
		{
			MultiplayerMng.LogError("Unable to replicate write field " + field.Name + ":" + field.FieldType.Name + " from " + GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	protected void Read(FieldInfo field, UdpPacket packet)
	{
		try
		{
			Type fieldType = field.FieldType;
			if (fieldType.IsArray)
			{
				Type elementType = fieldType.GetElementType();
				int num = packet.ReadByte();
				Array array = Array.CreateInstance(elementType, num);
				for (int i = 0; i < num; i++)
				{
					array.SetValue(Read(elementType, packet), i);
				}
				field.SetValue(this, array);
			}
			else
			{
				field.SetValue(this, Read(field.FieldType, packet));
			}
		}
		catch (Exception ex)
		{
			MultiplayerMng.LogError("Unable to replicate read field " + field.Name + ":" + field.FieldType.Name + " from " + GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public abstract void OnPeer();

	public virtual void Read(UdpPacket packet)
	{
		_id = packet.ReadUShort();
		foreach (FieldInfo field in GetFields<Replicate>(GetType()))
		{
			Read(field, packet);
		}
	}

	public virtual void Write(UdpPacket packet)
	{
		packet.WriteUShort(_id);
		foreach (FieldInfo field in GetFields<Replicate>(GetType()))
		{
			Write(field, packet);
		}
	}

	public virtual void Post()
	{
		if (!MultiplayerMng.Instance.IsNullOrDestroyed())
		{
			_id = MultiplayerMng.GetMessageOutId();
			Log("Send " + ToString());
			MessageGlobalEvent.Post(new MultiplayerTokens.MessageEnvelope(this));
		}
	}

	protected string ToString(FieldInfo field)
	{
		object value = field.GetValue(this);
		if (value == null)
		{
			return "null";
		}
		if (field.FieldType.IsArray)
		{
			Array array = (Array)value;
			string text = $"{array.Length} [";
			foreach (object item in array)
			{
				text = text + (item?.ToString() ?? "null") + ",";
			}
			return text + "]";
		}
		return value.ToString();
	}

	public override string ToString()
	{
		return "";
	}
}
