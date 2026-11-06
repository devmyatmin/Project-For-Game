using UnityEngine;

namespace RootMotion.FinalIK;

public class VRIKLODController : MonoBehaviour
{
	public Renderer LODRenderer;

	public float LODDistance = 15f;

	public bool allowCulled = true;

	private VRIK ik;

	private void Start()
	{
		ik = ((Component)this).GetComponent<VRIK>();
	}

	private void Update()
	{
		ik.solver.LOD = GetLODLevel();
	}

	private int GetLODLevel()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (allowCulled)
		{
			if ((Object)(object)LODRenderer == (Object)null)
			{
				return 0;
			}
			if (!LODRenderer.isVisible)
			{
				return 2;
			}
		}
		Vector3 val = ((Component)ik).transform.position - ((Component)Camera.main).transform.position;
		if (val.sqrMagnitude > LODDistance * LODDistance)
		{
			return 1;
		}
		return 0;
	}
}
