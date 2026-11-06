using System;
using UnityEngine;

namespace Beam.Utilities;

public static class Debug3DText
{
	public static void Update(Transform parent, ref TextMesh textMesh, string value, float localOffset = 1.67f, bool attach = true, float localScale = 0.1f)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (BeamTeam.IsNullOrDestroyed(textMesh))
		{
			GameObject val = new GameObject("DebugText", new Type[1] { typeof(TextMesh) })
			{
				layer = Layers.PLAYER
			};
			textMesh = val.GetComponent<TextMesh>();
			textMesh.alignment = (TextAlignment)1;
			textMesh.anchor = (TextAnchor)4;
		}
		GameObject gameObject = ((Component)textMesh).gameObject;
		if (attach)
		{
			gameObject.transform.parent = parent;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = new Vector3(localScale, localScale, localScale);
			gameObject.transform.localPosition = new Vector3(0f, localOffset, 0f);
		}
		else
		{
			gameObject.transform.position = parent.position;
			gameObject.transform.rotation = parent.rotation;
			gameObject.transform.localScale = new Vector3(localScale, localScale, localScale);
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = Vector3.zero;
		}
		textMesh.text = value;
	}
}
