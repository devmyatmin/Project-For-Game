using UnityEngine;

public class FishingRodString : MonoBehaviour
{
	public Animator Animator;

	public Vector3 PositionOffset;

	public void Pullback()
	{
		Animator.Play("BOWSTRING_PULLBACK");
	}

	public void Pullback_Release()
	{
		Animator.Play("BOWSTRING_PULLBACK_RELEASE");
	}

	public void Release()
	{
		Animator.Play("BOWSTRING_RELEASE");
	}
}
