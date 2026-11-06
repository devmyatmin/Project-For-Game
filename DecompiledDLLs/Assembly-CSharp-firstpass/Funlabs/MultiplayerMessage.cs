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
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Expected Obj, but got Unknown
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Expected Obj, but got Unknown
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Expected Obj, but got Unknown
		if (type == typeof(bool))
		{
			packet.WriteBool((bool)@object);
		}
		else if (type == typeof(byte))
		{
			packet.WriteByte((byte)@object);
		}
		else if (type == typeof(short))
		{
			packet.WriteShort((short)@object);
		}
		else if (type == typeof(int))
		{
			packet.WriteInt((int)@object);
		}
		else if (type == typeof(uint))
		{
			packet.WriteUInt((uint)@object);
		}
		else if (type == typeof(float))
		{
			packet.WriteFloat((float)@object);
		}
		else if (type == typeof(MiniGuid))
		{
			packet.WriteMiniGuid((MiniGuid)@object);
		}
		else if (type == typeof(Guid))
		{
			packet.WriteGuid((Guid)@object);
		}
		else if (type == typeof(Vector3))
		{
			MPExtensions.WriteVector3(packet, (Vector3)@object);
		}
		else if (type == typeof(Quaternion))
		{
			MPExtensions.WriteQuaternion(packet, (Quaternion)@object);
		}
		else if (type == typeof(string))
		{
			packet.WriteString((string)@object);
		}
		else if (typeof(ISaveablePrefab).IsAssignableFrom(type) || typeof(IPickupable).IsAssignableFrom(type) || typeof(IInteractable).IsAssignableFrom(type) || typeof(ISkinnable).IsAssignableFrom(type) || typeof(IUseable).IsAssignableFrom(type) || typeof(IChargeable).IsAssignableFrom(type))
		{
			packet.WriteSaveablePrefab((ISaveablePrefab)@object);
		}
		else if (typeof(Piscus_Creature).IsAssignableFrom(type))
		{
			Piscus_Creature piscus_Creature = (Piscus_Creature)@object;
			packet.WriteSaveablePrefab((piscus_Creature != null) ? ((Component)piscus_Creature).gameObject.GetComponent<ISaveablePrefab>() : null);
		}
		else if (typeof(LandAnimal).IsAssignableFrom(type))
		{
			LandAnimal landAnimal = (LandAnimal)@object;
			packet.WriteSaveablePrefab((landAnimal != null) ? ((Component)landAnimal).gameObject.GetComponent<ISaveablePrefab>() : null);
		}
		else if (typeof(FlockChild).IsAssignableFrom(type))
		{
			FlockChild flockChild = (FlockChild)@object;
			packet.WriteSaveablePrefab((flockChild != null) ? ((Component)flockChild).gameObject.GetComponent<ISaveablePrefab>() : null);
		}
		else if (typeof(VehicleMovementBase).IsAssignableFrom(type))
		{
			VehicleMovementBase vehicleMovementBase = (VehicleMovementBase)@object;
			packet.WriteSaveablePrefab((vehicleMovementBase != null) ? ((Component)vehicleMovementBase).gameObject.GetComponent<ISaveablePrefab>() : null);
		}
		else if (typeof(BoltEntity).IsAssignableFrom(type))
		{
			BoltEntity val = (BoltEntity)@object;
			long num;
			if (val == null)
			{
				num = 0L;
			}
			else
			{
				NetworkId networkId = val.NetworkId;
				num = (long)networkId.PackedValue;
			}
			packet.WriteULong((ulong)num);
		}
		else if (typeof(IPlayer).IsAssignableFrom(type))
		{
			packet.WriteByte((byte)(((IPlayer)@object)?.Id ?? 255));
		}
		else if (typeof(IMultiplayerStorageReference).IsAssignableFrom(type))
		{
			IMultiplayerStorageReference multiplayerStorageReference = (IMultiplayerStorageReference)@object;
			packet.WriteMiniGuid(multiplayerStorageReference.ReferenceId);
		}
		else if (typeof(Rigidbody).IsAssignableFrom(type))
		{
			Rigidbody rigidbody = (Rigidbody)@object;
			MPExtensions.WriteRigidbody(packet, rigidbody);
		}
		else if (typeof(Collider).IsAssignableFrom(type))
		{
			Collider collider = (Collider)@object;
			MPExtensions.WriteCollider(packet, collider);
		}
		else
		{
			if (!typeof(ProjectileObject).IsAssignableFrom(type))
			{
				throw new Exception("Type not supported");
			}
			ProjectileObject projectileObject = (ProjectileObject)@object;
			packet.WriteSaveablePrefab((projectileObject != null) ? ((Component)projectileObject).gameObject.GetComponent<ISaveablePrefab>() : null);
		}
	}

	protected object Read(Type type, UdpPacket packet)
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
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
			return MPExtensions.ReadMiniGuid(packet);
		}
		if (type == typeof(Guid))
		{
			return packet.ReadGuid();
		}
		if (type == typeof(Vector3))
		{
			return MPExtensions.ReadVector3(packet);
		}
		if (type == typeof(Quaternion))
		{
			return MPExtensions.ReadQuaternion(packet);
		}
		if (type == typeof(string))
		{
			return packet.ReadString();
		}
		if (typeof(ISaveablePrefab).IsAssignableFrom(type) || typeof(IPickupable).IsAssignableFrom(type) || typeof(IBase).IsAssignableFrom(type) || typeof(IInteractable).IsAssignableFrom(type) || typeof(ISkinnable).IsAssignableFrom(type) || typeof(IUseable).IsAssignableFrom(type) || typeof(IChargeable).IsAssignableFrom(type))
		{
			return MPExtensions.ReadSaveablePrefab<ISaveablePrefab>(packet);
		}
		if (typeof(Piscus_Creature).IsAssignableFrom(type))
		{
			SaveablePrefab saveablePrefab = MPExtensions.ReadSaveablePrefab<SaveablePrefab>(packet);
			if (saveablePrefab == null)
			{
				return null;
			}
			return ((Component)saveablePrefab).GetComponent<Piscus_Creature>();
		}
		if (typeof(LandAnimal).IsAssignableFrom(type))
		{
			SaveablePrefab saveablePrefab2 = MPExtensions.ReadSaveablePrefab<SaveablePrefab>(packet);
			if (saveablePrefab2 == null)
			{
				return null;
			}
			return ((Component)saveablePrefab2).GetComponent<LandAnimal>();
		}
		if (typeof(FlockChild).IsAssignableFrom(type))
		{
			SaveablePrefab saveablePrefab3 = MPExtensions.ReadSaveablePrefab<SaveablePrefab>(packet);
			if (saveablePrefab3 == null)
			{
				return null;
			}
			return ((Component)saveablePrefab3).GetComponent<FlockChild>();
		}
		if (typeof(VehicleMovementBase).IsAssignableFrom(type))
		{
			SaveablePrefab saveablePrefab4 = MPExtensions.ReadSaveablePrefab<SaveablePrefab>(packet);
			if (saveablePrefab4 == null)
			{
				return null;
			}
			return ((Component)saveablePrefab4).GetComponent<VehicleMovementBase>();
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
			return StorageRegistry.Get(MPExtensions.ReadMiniGuid(packet));
		}
		if (typeof(Rigidbody).IsAssignableFrom(type))
		{
			return MPExtensions.ReadRigidbody(packet);
		}
		if (typeof(Collider).IsAssignableFrom(type))
		{
			return MPExtensions.ReadCollider(packet);
		}
		if (typeof(ProjectileObject).IsAssignableFrom(type))
		{
			SaveablePrefab saveablePrefab5 = MPExtensions.ReadSaveablePrefab<SaveablePrefab>(packet);
			if (saveablePrefab5 == null)
			{
				return null;
			}
			return ((Component)saveablePrefab5).GetComponent<ProjectileObject>();
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
		if (!BeamTeam.IsNullOrDestroyed(MultiplayerMng.Instance))
		{
			_id = MultiplayerMng.GetMessageOutId();
			Log("Send " + ToString());
			MessageGlobalEvent.Post((IProtocolToken)(object)new MultiplayerTokens.MessageEnvelope(this));
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
