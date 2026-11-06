using LE_LevelEditor.Core;
using UnityEngine;

public class LE_ObjectReference : MonoBehaviour
{
	public LE_Object.Categories Category;

	public string EditorObjectName;

	public int DensityWeight;

	public string IconPath = " ";

	public bool IsPlacementRotationByNormal;

	public bool IsMovable = true;

	public bool IsMovableOnX = true;

	public bool IsMovableOnY = true;

	public bool IsMovableOnZ = true;

	public bool IsRotatable = true;

	public bool IsRotatableAroundX = true;

	public bool IsRotatableAroundY = true;

	public bool IsRotatableAroundZ = true;

	public int MaxInstancesInLevel;

	public LE_Object.ESnapType SnapType;

	public int RootSnapPointIndex;

	public LE_ObjectSnapPoint[] ObjectSnapPoints;

	public bool IsDrawSnapToObjectUI;

	public Vector3 SnapGridOffset;

	public Vector3 SnapGridCellSize;
}
