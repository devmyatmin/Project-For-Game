using System;
using System.Globalization;
using Beam;
using Beam.Serialization.Json;
using UnityEngine;

namespace Funlabs;

public struct MiniGuid : IEquatable<MiniGuid>
{
	public byte _d0;

	public byte _d1;

	public byte _d2;

	public byte _d3;

	public byte _d4;

	public byte _d5;

	public byte _d6;

	public byte _d7;

	private static int[] _wordLengths = new int[5] { 8, 4, 4, 4, 12 };

	public static MiniGuid VEHICLE_LIFERAFT_STORAGE = new MiniGuid(14627333964952502273uL);

	public static MiniGuid MISSIONSHIP_STORAGE = new MiniGuid(14627333964952502274uL);

	public static MiniGuid PADDLE_RAFT = new MiniGuid(14627333964952502275uL);

	public static MiniGuid LIFERAFT = new MiniGuid(14627333964952502276uL);

	public static MiniGuid INTERACTIVE_AIRCRAFT = new MiniGuid(11622747163053785089uL);

	public static MiniGuid INTERACTIVE_AIRCRAFT_COCKPIT = new MiniGuid(11622747163053785090uL);

	public static MiniGuid INTERACTIVE_AIRCRAFT_VFX = new MiniGuid(11622747163053785091uL);

	public static MiniGuid INTERACTIVE_AIRCRAFT_DOOR = new MiniGuid(11622747163053785092uL);

	public static uint CONTAINER_PREFAB_ID_PLACE_HOLDER = 1044942u;

	public MiniGuid(Guid value)
	{
		byte[] array = value.ToByteArray();
		_d0 = array[0];
		_d1 = array[1];
		_d2 = array[2];
		_d3 = array[3];
		_d4 = array[4];
		_d5 = array[5];
		_d6 = array[6];
		_d7 = array[7];
	}

	public MiniGuid(ulong value)
	{
		_d0 = (byte)(value & 0xFF);
		_d1 = (byte)((value >> 8) & 0xFF);
		_d2 = (byte)((value >> 16) & 0xFF);
		_d3 = (byte)((value >> 24) & 0xFF);
		_d4 = (byte)((value >> 32) & 0xFF);
		_d5 = (byte)((value >> 40) & 0xFF);
		_d6 = (byte)((value >> 48) & 0xFF);
		_d7 = (byte)(value >> 56);
	}

	public MiniGuid(int low, int high)
	{
		_d0 = (byte)(low & 0xFF);
		_d1 = (byte)((low >> 8) & 0xFF);
		_d2 = (byte)((low >> 16) & 0xFF);
		_d3 = (byte)(low >> 24);
		_d4 = (byte)(high & 0xFF);
		_d5 = (byte)((high >> 8) & 0xFF);
		_d6 = (byte)((high >> 16) & 0xFF);
		_d7 = (byte)(high >> 24);
	}

	public MiniGuid(byte d0, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7)
	{
		_d0 = d0;
		_d1 = d1;
		_d2 = d2;
		_d3 = d3;
		_d4 = d4;
		_d5 = d5;
		_d6 = d6;
		_d7 = d7;
	}

	public bool IsDefault()
	{
		return Equals(default);
	}

	public bool Equals(MiniGuid other)
	{
		if (_d0 == other._d0 && _d1 == other._d1 && _d2 == other._d2 && _d3 == other._d3 && _d4 == other._d4 && _d5 == other._d5 && _d6 == other._d6)
		{
			return _d7 == other._d7;
		}
		return false;
	}

	public override string ToString()
	{
		return new Guid(new byte[16]
		{
			_d0, _d1, _d2, _d3, _d4, _d5, _d6, _d7, 0, 0,
			0, 0, 0, 0, 0, 0
		}).ToString();
	}

	public ulong ToUlong()
	{
		return _d0 | ((ulong)_d1 << 8) | ((ulong)_d2 << 16) | ((ulong)_d3 << 24) | ((ulong)_d4 << 32) | ((ulong)_d5 << 40) | ((ulong)_d6 << 48) | ((ulong)_d7 << 56);
	}

	public int ToIntLow()
	{
		return _d0 | (_d1 << 8) | (_d2 << 16) | (_d3 << 24);
	}

	public int ToIntHigh()
	{
		return _d4 | (_d5 << 8) | (_d6 << 16) | (_d7 << 24);
	}

	public static bool IsMiniGuid(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (value.Length != 36)
		{
			return false;
		}
		string[] array = value.Split('-');
		if (array.Length != 5)
		{
			return false;
		}
		for (int i = 0; i < 5; i++)
		{
			if (array[i].Length != _wordLengths[i])
			{
				return false;
			}
			if (!long.TryParse(array[i], NumberStyles.HexNumber, Beam.Serialization.Json.ExtensionMethods.EnglishCulture, out var _))
			{
				return false;
			}
		}
		return true;
	}

	public static MiniGuid New()
	{
		return new MiniGuid(Guid.NewGuid());
	}

	public static MiniGuid NewFrom(Vector3 position, uint prefabId, ushort variation = 48879)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		byte d = (byte)(prefabId & 0xFF);
		byte d2 = (byte)(prefabId >> 8);
		byte[] bytes = BitConverter.GetBytes(Vector3Tools.Hash(position));
		return new MiniGuid(d, d2, bytes[0], bytes[1], bytes[2], bytes[3], (byte)(variation & 0xFF), (byte)(variation >> 8));
	}

	public static MiniGuid NewFrom(MiniGuid other, int shift = 1)
	{
		ulong num = other.ToUlong();
		num = (num << shift) | (num >> 64 - shift);
		return new MiniGuid(num);
	}

	public static MiniGuid PLAYER_STORAGE(IPlayer player)
	{
		return new MiniGuid((ulong)(-2401053092612145152L + player.Id));
	}

	public static MiniGuid PLAYER_RAGDOLL(IPlayer player)
	{
		return new MiniGuid((ulong)(-2401053363754123601L + player.Id));
	}

	public static int PLAYER_ID_FROM_RAGDOLL(MiniGuid refId)
	{
		return (int)((long)refId.ToUlong() - -2401053363754123601L);
	}

	public static MiniGuid AIRCRAFT_STATION(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return NewFrom(pos, uint.MaxValue, 48879);
	}
}
