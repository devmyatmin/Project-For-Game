using UnityEngine;

public class ZoneNML : Zone
{
	private void Awake()
	{
	}

	public override bool InSaveBounds(Vector3 position)
	{
		return true;
	}

	public override void Enter()
	{
	}
}
