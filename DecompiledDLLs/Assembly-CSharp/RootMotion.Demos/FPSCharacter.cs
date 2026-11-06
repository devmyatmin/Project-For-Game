using UnityEngine;

namespace RootMotion.Demos;

public class FPSCharacter : MonoBehaviour
{
	[Range(0f, 1f)]
	public float walkSpeed = 0.5f;

	private float sVel;

	private Animator animator;

	private FPSAiming FPSAiming;

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
		FPSAiming = ((Component)this).GetComponent<FPSAiming>();
	}

	private void Update()
	{
		FPSAiming.sightWeight = Mathf.SmoothDamp(FPSAiming.sightWeight, Input.GetMouseButton(1) ? 1f : 0f, ref sVel, 0.1f);
		if (FPSAiming.sightWeight < 0.001f)
		{
			FPSAiming.sightWeight = 0f;
		}
		if (FPSAiming.sightWeight > 0.999f)
		{
			FPSAiming.sightWeight = 1f;
		}
		animator.SetFloat("Speed", walkSpeed);
	}

	private void OnGUI()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		GUI.Label(new Rect((float)(Screen.width - 210), 10f, 200f, 25f), "Hold RMB to aim down the sight");
	}
}
