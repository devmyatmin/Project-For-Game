using Beam;
using LE_LevelEditor.Core;
using UnityEngine;

namespace LE_LevelEditor;

public class LE_DensityBounds : Singleton<LE_DensityBounds>
{
	private class DensityBound
	{
		public Bounds Bound;

		public int Density;

		public Material densityMat;

		public DensityBound(Bounds bound, int density)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Bound = bound;
			Density = density;
		}
	}

	[SerializeField]
	private Transform densityGridParent;

	[SerializeField]
	private GameObject densityProjector;

	private DensityBound[,] bounds;

	private int numberOfBounds = 20;

	private int boundSize = 25;

	private int zoneSize = 500;

	private void Start()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected Obj, but got Unknown
		boundSize = zoneSize / numberOfBounds;
		bounds = new DensityBound[numberOfBounds, numberOfBounds];
		for (int i = 0; i < numberOfBounds; i++)
		{
			for (int j = 0; j < numberOfBounds; j++)
			{
				bounds[i, j] = new DensityBound(default, 0);
				bounds[i, j].Bound.size = new Vector3((float)boundSize, 200f, (float)boundSize);
				bounds[i, j].Bound.center = new Vector3(250f, 0f, 250f) - new Vector3((float)(i * boundSize), 0f, (float)(j * boundSize));
				GameObject val = Object.Instantiate<GameObject>(densityProjector, new Vector3(250f, 150f, 250f) - new Vector3((float)(i * boundSize), 0f, (float)(j * boundSize)), Quaternion.identity);
				val.transform.parent = densityGridParent;
				val.transform.rotation = Quaternion.LookRotation(Vector3.down);
				bounds[i, j].densityMat = new Material(val.GetComponent<Projector>().material);
				val.GetComponent<Projector>().material = bounds[i, j].densityMat;
			}
		}
		((MonoBehaviour)this).InvokeRepeating("UpdateVisual", 1f, 1f);
	}

	private void UpdateVisual()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		CheckDensity();
		for (int i = 0; i < numberOfBounds; i++)
		{
			for (int j = 0; j < numberOfBounds; j++)
			{
				float num = (float)bounds[i, j].Density / 100f;
				Color color = Color.Lerp(Color.green, Color.red, num);
				bounds[i, j].densityMat.color = color;
			}
		}
	}

	public bool CheckObjectDensity(LE_Object leObj, Vector3 position)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < numberOfBounds; i++)
		{
			for (int j = 0; j < numberOfBounds; j++)
			{
				if (bounds[i, j].Bound.Contains(position) && bounds[i, j].Density + leObj.DensityWeight > 100)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void CheckDensity()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		LE_Object[] array = Object.FindObjectsOfType<LE_Object>();
		for (int i = 0; i < numberOfBounds; i++)
		{
			for (int j = 0; j < numberOfBounds; j++)
			{
				bounds[i, j].Density = 0;
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			for (int l = 0; l < numberOfBounds; l++)
			{
				for (int m = 0; m < numberOfBounds; m++)
				{
					if (bounds[l, m].Bound.Contains(((Component)array[k]).transform.position))
					{
						bounds[l, m].Density += array[k].DensityWeight;
					}
				}
			}
		}
	}
}
