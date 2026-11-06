using Beam;
using UnityEngine;

public class WorldBarrier : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _barrierPieces;

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("PollBarrierPieces", 10f, 10f);
	}

	private void PollBarrierPieces()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!LevelLoader.IsServerJoinInProgress)
		{
			for (int i = 0; i < _barrierPieces.Length; i++)
			{
				IPlayer player = PlayerUtilities.FindClosestPlayer(_barrierPieces[i].transform.position, 500f);
				_barrierPieces[i].gameObject.SetActive(player != null);
			}
		}
	}
}
