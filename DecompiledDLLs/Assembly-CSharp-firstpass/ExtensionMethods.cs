using System;
using Beam;
using Beam.Crafting;
using Beam.Language;
using Beam.UI;
using Beam.Utilities;
using Funlabs;
using Photon.Bolt;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.UI;

public static class ExtensionMethods
{
	private class ReplicateCopyProjectilesTo : I_MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab Source;

		[I_Replicate]
		public IBase Destination;

		public override void OnPeer()
		{
			Source.CopyProjectilesTo(Destination);
		}
	}

	private class ReplicateCopyTransformTo : I_MultiplayerMessage
	{
		[Replicate]
		public SaveablePrefab Source;

		[I_Replicate]
		public IBase Destination;

		[Replicate]
		public bool Broad;

		public override void OnPeer()
		{
			Source.CopyTransformsTo(Destination, Broad);
		}
	}

	public static float ToCelsius(this float fahrenheit)
	{
		return (fahrenheit - 32f) * 0.555555f;
	}

	public static void EnableColliders(this Collider[] colliders, bool enabled, bool isTrigger)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = enabled;
			colliders[i].isTrigger = isTrigger;
		}
	}

	public static Transform FindInChildren(this Transform transform, string name)
	{
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (((Object)child).name == name)
			{
				return child;
			}
			Transform val = FindInChildren(child, name);
			if ((Object)(object)val != (Object)null)
			{
				return val;
			}
		}
		return null;
	}

	public static void CopyTransformsToRecursive(this Transform source, Transform destination)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		destination.position = source.position;
		destination.rotation = source.rotation;
		foreach (Transform item in destination)
		{
			Transform val = item;
			Transform val2 = source.Find(((Object)val).name);
			if (Object.op_Implicit((Object)(object)val2))
			{
				CopyTransformsToRecursive(val2, val);
			}
		}
	}

	private static void BroadCopyTransformToRecursive(this Transform source, Transform destination)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in source)
		{
			Transform val = item;
			Transform val2 = FindInChildren(((Component)destination).transform, ((Object)val).name);
			if (!BeamTeam.IsNullOrDestroyed(val2))
			{
				val2.position = val.position;
				val2.rotation = val.rotation;
			}
			val.BroadCopyTransformToRecursive(destination);
		}
	}

	public static void CopyTransformsTo(this SaveablePrefab source, IBase destination, bool broad = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (broad)
		{
			destination.transform.position = ((Component)source).transform.position;
			destination.transform.rotation = ((Component)source).transform.rotation;
			((Component)source).transform.BroadCopyTransformToRecursive(destination.transform);
		}
		else
		{
			CopyTransformsToRecursive(((Component)source).transform, destination.transform);
		}
	}

	public static ProjectileObject[] GetProjectiles(this Transform transform)
	{
		return ((Component)transform).GetComponentsInChildren<ProjectileObject>();
	}

	public static Vector3[] GetPositions(this ProjectileObject[] projectiles, bool local)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[projectiles.Length];
		for (int i = 0; i < projectiles.Length; i++)
		{
			array[i] = (local ? projectiles[i].transform.localPosition : projectiles[i].transform.position);
		}
		return array;
	}

	public static Quaternion[] GetRotations(this ProjectileObject[] projectiles, bool local)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Quaternion[] array = new Quaternion[projectiles.Length];
		for (int i = 0; i < projectiles.Length; i++)
		{
			array[i] = (local ? projectiles[i].transform.localRotation : projectiles[i].transform.rotation);
		}
		return array;
	}

	public static void SetPositions(this ProjectileObject[] projectiles, Vector3[] positions, bool local)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (projectiles.Length != positions.Length)
		{
			throw new Exception("SetPositions projecitles and positions have different lenghts");
		}
		for (int i = 0; i < projectiles.Length; i++)
		{
			if (local)
			{
				projectiles[i].transform.localPosition = positions[i];
			}
			else
			{
				projectiles[i].transform.position = positions[i];
			}
		}
	}

	public static void SetRotation(this ProjectileObject[] projectiles, Quaternion[] rotations, bool local)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (projectiles.Length != rotations.Length)
		{
			throw new Exception("SetPositions projecitles and rotations have different lenghts");
		}
		for (int i = 0; i < projectiles.Length; i++)
		{
			if (local)
			{
				projectiles[i].transform.localRotation = rotations[i];
			}
			else
			{
				projectiles[i].transform.rotation = rotations[i];
			}
		}
	}

	public static void AttachTo(this ProjectileObject[] projectiles, IBase destination)
	{
		foreach (ProjectileObject obj in projectiles)
		{
			obj.transform.parent = null;
			obj.TransformParent = destination.transform;
			obj.Parent();
			obj.ProjectileMovement.AttachedBase = destination;
		}
	}

	public static void CopyProjectilesTo(this SaveablePrefab source, IBase destination)
	{
		GetProjectiles(((Component)source).transform).AttachTo(destination);
	}

	public static void ReplicatedCopyProjectilesTo(this SaveablePrefab source, IBase destination)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateCopyProjectilesTo replicateCopyProjectilesTo = new ReplicateCopyProjectilesTo();
			replicateCopyProjectilesTo.Source = source;
			replicateCopyProjectilesTo.Destination = destination;
			replicateCopyProjectilesTo.Post();
		}
		source.CopyProjectilesTo(destination);
	}

	public static void ReplicatedCopyTransformsTo(this SaveablePrefab source, IBase destination, bool broad = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsMultiplayer(Game.Mode))
		{
			ReplicateCopyTransformTo replicateCopyTransformTo = new ReplicateCopyTransformTo();
			replicateCopyTransformTo.Source = source;
			replicateCopyTransformTo.Destination = destination;
			replicateCopyTransformTo.Broad = broad;
			replicateCopyTransformTo.Post();
		}
		source.CopyTransformsTo(destination, broad);
	}

	public static bool HasConstraint(this UsingConstraint value, UsingConstraint flag)
	{
		return (value & flag) != 0;
	}

	public static string ToPaddedString(this Version version)
	{
		return $"{version.Major}.{version.Minor:00}.{version.Build:00}";
	}

	public static string ToPaddedString2(this Version version)
	{
		string text = $"{version.Major}{version.Minor:000}";
		if (Application.isPlaying && BoltNetwork.IsDebugMode)
		{
			text += "d";
		}
		return text;
	}

	public static bool IsTransient(this int resultCode)
	{
		if (!Enum.IsDefined(typeof(EResultCode), resultCode))
		{
			return false;
		}
		if (resultCode == 48879 || resultCode == 51966 || resultCode == 57005)
		{
			return true;
		}
		return false;
	}

	public static string ToPrettyString(this int resultCode)
	{
		string text = $"hr=0x{resultCode:X8}";
		if (Enum.IsDefined(typeof(EResultCode), resultCode))
		{
			return $"{(EResultCode)resultCode} ({text})";
		}
		return "E_UNHANDLED (" + text + ")";
	}

	public static string GetPrettyResultMsg(this int resultCode, string term)
	{
		string text = Localization.GetLanguageHandler().Localize(term, ToPrettyString(resultCode));
		if (IsTransient(resultCode))
		{
			text = text + "\n" + Localization.GetLanguageHandler().Localize("TRANSIENT_RESULT_CODE_TRY_AGAIN_MSG");
		}
		return text;
	}

	public static void SetNavigation<T>(this T component, NavigationDirection direction, T element) where T : Component
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		object obj = component;
		Selectable val = ((obj != null) ? ((Component)obj).GetComponent<Selectable>() : null);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		object obj2 = element;
		Selectable val2 = ((obj2 != null) ? ((Component)obj2).GetComponent<Selectable>() : null);
		if (!((Object)(object)val2 == (Object)null))
		{
			Navigation navigation = val.navigation;
			navigation.mode = (Mode)4;
			switch (direction)
			{
			case NavigationDirection.Left:
				navigation.selectOnLeft = val2;
				break;
			case NavigationDirection.Right:
				navigation.selectOnRight = val2;
				break;
			case NavigationDirection.Up:
				navigation.selectOnUp = val2;
				break;
			case NavigationDirection.Down:
				navigation.selectOnDown = val2;
				break;
			}
			val.navigation = navigation;
		}
	}

	public static void FocusElement(this IScrollviewViewAdapter scrollview, Transform selectable)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected Obj, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)scrollview.transform;
		RectTransform val2 = (RectTransform)selectable;
		Rect val3 = MathTools.TransformRect(val2.rect, (Transform)(object)val2, (Transform)(object)val);
		Rect rect = val.rect;
		Rect rect2 = val.rect;
		if (!MathTools.RectContains(rect2, val3) && GetOffsetToContainRect(rect2, val3, out var offset))
		{
			RectTransform val4 = (RectTransform)scrollview.Content.Transform;
			Vector2 anchoredPosition = val4.anchoredPosition;
			anchoredPosition.x = Mathf.Clamp(anchoredPosition.x + offset.x, 0f, Mathf.Abs(rect.width - val4.sizeDelta.x));
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y + offset.y, 0f, Mathf.Abs(rect.height - val4.sizeDelta.y));
			val4.anchoredPosition = anchoredPosition;
		}
	}

	public static bool GetOffsetToContainRect(Rect container, Rect child, out Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		offset = Vector2.zero;
		if (container.width < child.width)
		{
			return false;
		}
		if (container.height < child.height)
		{
			return false;
		}
		if (child.xMin < container.xMin)
		{
			offset.x += container.xMin - child.xMin;
		}
		if (child.yMin < container.yMin)
		{
			offset.y += container.yMin - child.yMin;
		}
		if (child.xMax > container.xMax)
		{
			offset.x += container.xMax - child.xMax;
		}
		if (child.yMax > container.yMax)
		{
			offset.y += container.yMax - child.yMax;
		}
		return true;
	}
}
