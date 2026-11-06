using UnityEngine;

public class LTBezierPath
{
	public Vector3[] pts;

	public float length;

	public bool orientToPath;

	public bool orientToPath2d;

	private LTBezier[] beziers;

	private float[] lengthRatio;

	private int currentBezier;

	private int previousBezier;

	public float distance => length;

	public LTBezierPath()
	{
	}

	public LTBezierPath(Vector3[] pts_)
	{
		setPoints(pts_);
	}

	public void setPoints(Vector3[] pts_)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (pts_.Length < 4)
		{
			LeanTween.logError("LeanTween - When passing values for a vector path, you must pass four or more values!");
		}
		if (pts_.Length % 4 != 0)
		{
			LeanTween.logError("LeanTween - When passing values for a vector path, they must be in sets of four: controlPoint1, controlPoint2, endPoint2, controlPoint2, controlPoint2...");
		}
		pts = pts_;
		int num = 0;
		beziers = new LTBezier[pts.Length / 4];
		lengthRatio = new float[beziers.Length];
		length = 0f;
		for (int i = 0; i < pts.Length; i += 4)
		{
			beziers[num] = new LTBezier(pts[i], pts[i + 2], pts[i + 1], pts[i + 3], 0.05f);
			length += beziers[num].length;
			num++;
		}
		for (int i = 0; i < beziers.Length; i++)
		{
			lengthRatio[i] = beziers[i].length / length;
		}
	}

	public Vector3 point(float ratio)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < lengthRatio.Length; i++)
		{
			num += lengthRatio[i];
			if (num >= ratio)
			{
				return beziers[i].point((ratio - (num - lengthRatio[i])) / lengthRatio[i]);
			}
		}
		return beziers[lengthRatio.Length - 1].point(1f);
	}

	public void place2d(Transform transform, float ratio)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		transform.position = point(ratio);
		ratio += 0.001f;
		if (ratio <= 1f)
		{
			Vector3 val = point(ratio) - transform.position;
			float num = Mathf.Atan2(val.y, val.x) * 57.29578f;
			transform.eulerAngles = new Vector3(0f, 0f, num);
		}
	}

	public void placeLocal2d(Transform transform, float ratio)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		transform.localPosition = point(ratio);
		ratio += 0.001f;
		if (ratio <= 1f)
		{
			Vector3 val = transform.parent.TransformPoint(point(ratio)) - transform.localPosition;
			float num = Mathf.Atan2(val.y, val.x) * 57.29578f;
			transform.eulerAngles = new Vector3(0f, 0f, num);
		}
	}

	public void place(Transform transform, float ratio)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		place(transform, ratio, Vector3.up);
	}

	public void place(Transform transform, float ratio, Vector3 worldUp)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		transform.position = point(ratio);
		ratio += 0.001f;
		if (ratio <= 1f)
		{
			transform.LookAt(point(ratio), worldUp);
		}
	}

	public void placeLocal(Transform transform, float ratio)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		placeLocal(transform, ratio, Vector3.up);
	}

	public void placeLocal(Transform transform, float ratio, Vector3 worldUp)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		ratio = getRationInOneRange(ratio);
		transform.localPosition = point(ratio);
		ratio = getRationInOneRange(ratio + 0.001f);
		if (ratio <= 1f)
		{
			transform.LookAt(transform.parent.TransformPoint(point(ratio)), worldUp);
		}
	}

	public float getRationInOneRange(float ratio)
	{
		if (ratio >= 0f && ratio <= 1f)
		{
			return ratio;
		}
		if (ratio < 0f)
		{
			return Mathf.Ceil(ratio) - ratio;
		}
		return ratio - Mathf.Floor(ratio);
	}

	public void gizmoDraw(float t = -1f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point(0f);
		for (int i = 1; i <= 120; i++)
		{
			float ratio = (float)i / 120f;
			Vector3 val2 = point(ratio);
			Gizmos.color = ((previousBezier == currentBezier) ? Color.magenta : Color.grey);
			Gizmos.DrawLine(val2, val);
			val = val2;
			previousBezier = currentBezier;
		}
	}
}
