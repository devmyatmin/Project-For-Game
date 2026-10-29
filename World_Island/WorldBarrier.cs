using Beam;
using UnityEngine;

public class WorldBarrier : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _barrierPieces;

	private void Start()
	{
		InvokeRepeating("PollBarrierPieces", 10f, 10f);
	}

	private void PollBarrierPieces()
	{
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
