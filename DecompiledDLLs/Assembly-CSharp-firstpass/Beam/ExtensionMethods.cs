using System;
using System.Collections.Generic;
using Ceto;
using UnityEngine;

namespace Beam;

public static class ExtensionMethods
{
	private static readonly HashSet<Type> _IntegralNumericTypes = new HashSet<Type>
	{
		typeof(sbyte),
		typeof(byte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong)
	};

	private static readonly HashSet<Type> _FloatingPointNumericTypes = new HashSet<Type>
	{
		typeof(float),
		typeof(double),
		typeof(decimal)
	};

	public static void SetFieldOfView(this Camera camera, float fov)
	{
		camera.fieldOfView = fov;
		CameraData cameraData = Ceto.Ocean.Instance.FindCameraData(camera);
		if (cameraData != null && cameraData.mask != null && (Object)(object)cameraData.mask.cam != (Object)null)
		{
			cameraData.mask.cam.fieldOfView = fov;
		}
	}

	public static void SetFieldOfView(this IGameCamera playerCamera, float fov)
	{
		SetFieldOfView(playerCamera.Camera, fov);
	}

	public static void SetFieldOfView(this IList<IGameCamera> playerCameras, float fov)
	{
		for (int i = 0; i < playerCameras.Count; i++)
		{
			playerCameras[i].SetFieldOfView(fov);
		}
	}

	public static void ResetFieldOfView(this Camera camera)
	{
		SetFieldOfView(camera, Cameras.DefaultFOV);
	}

	public static void ResetFieldOfView(this IGameCamera playerCamera)
	{
		ResetFieldOfView(playerCamera.Camera);
	}

	public static void RecalculateTangents(this Mesh mesh)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		int vertexCount = mesh.vertexCount;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector2[] uv = mesh.uv;
		int[] triangles = mesh.triangles;
		int num = triangles.Length / 3;
		Vector4[] array = new Vector4[vertexCount];
		Vector3[] array2 = new Vector3[vertexCount];
		Vector3[] array3 = new Vector3[vertexCount];
		int i = 0;
		int num2 = 0;
		for (; i < num; i++)
		{
			int num3 = triangles[num2];
			int num4 = triangles[num2 + 1];
			int num5 = triangles[num2 + 2];
			Vector3 val = vertices[num3];
			Vector3 val2 = vertices[num4];
			Vector3 val3 = vertices[num5];
			Vector2 val4 = uv[num3];
			Vector2 val5 = uv[num4];
			Vector2 val6 = uv[num5];
			float num6 = val2.x - val.x;
			float num7 = val3.x - val.x;
			float num8 = val2.y - val.y;
			float num9 = val3.y - val.y;
			float num10 = val2.z - val.z;
			float num11 = val3.z - val.z;
			float num12 = val5.x - val4.x;
			float num13 = val6.x - val4.x;
			float num14 = val5.y - val4.y;
			float num15 = val6.y - val4.y;
			float num16 = 1f / (num12 * num15 - num13 * num14);
			Vector3 val7 = new Vector3((num15 * num6 - num14 * num7) * num16, (num15 * num8 - num14 * num9) * num16, (num15 * num10 - num14 * num11) * num16);
			Vector3 val8 = new Vector3((num12 * num7 - num13 * num6) * num16, (num12 * num9 - num13 * num8) * num16, (num12 * num11 - num13 * num10) * num16);
			ref Vector3 reference = ref array2[num3];
			reference += val7;
			ref Vector3 reference2 = ref array2[num4];
			reference2 += val7;
			ref Vector3 reference3 = ref array2[num5];
			reference3 += val7;
			ref Vector3 reference4 = ref array3[num3];
			reference4 += val8;
			ref Vector3 reference5 = ref array3[num4];
			reference5 += val8;
			ref Vector3 reference6 = ref array3[num5];
			reference6 += val8;
			num2 += 3;
		}
		for (int j = 0; j < vertexCount; j++)
		{
			Vector3 val9 = normals[j];
			Vector3 val10 = array2[j];
			Vector3.OrthoNormalize(ref val9, ref val10);
			array[j].x = val10.x;
			array[j].y = val10.y;
			array[j].z = val10.z;
			array[j].w = ((Vector3.Dot(Vector3.Cross(val9, val10), array3[j]) < 0f) ? (-1f) : 1f);
		}
		mesh.tangents = array;
	}

	public static bool HigherThan(this Quality_Preset quality, Quality_Preset value)
	{
		switch (value)
		{
		case Quality_Preset.LAPTOP:
			if (quality != Quality_Preset.LOW && quality != Quality_Preset.MEDIUM && quality != Quality_Preset.HIGH && quality != Quality_Preset.ULTRA)
			{
				return quality == Quality_Preset.CUSTOM;
			}
			return true;
		case Quality_Preset.LOW:
			if (quality != Quality_Preset.MEDIUM && quality != Quality_Preset.HIGH && quality != Quality_Preset.ULTRA)
			{
				return quality == Quality_Preset.CUSTOM;
			}
			return true;
		case Quality_Preset.MEDIUM:
			if (quality != Quality_Preset.HIGH && quality != Quality_Preset.ULTRA)
			{
				return quality == Quality_Preset.CUSTOM;
			}
			return true;
		case Quality_Preset.HIGH:
			if (quality != Quality_Preset.ULTRA)
			{
				return quality == Quality_Preset.CUSTOM;
			}
			return true;
		case Quality_Preset.ULTRA:
		case Quality_Preset.CUSTOM:
			return true;
		default:
			return false;
		}
	}

	public static bool IsIntegralNumeric(this Type type)
	{
		return _IntegralNumericTypes.Contains(type);
	}

	public static bool IsFloatingPointNumeric(this Type type)
	{
		return _FloatingPointNumericTypes.Contains(type);
	}
}
