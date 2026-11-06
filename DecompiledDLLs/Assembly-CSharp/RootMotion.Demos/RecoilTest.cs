using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class RecoilTest : MonoBehaviour
{
	public float magnitude = 1f;

	private Recoil recoil;

	private void Start()
	{
		recoil = ((Component)this).GetComponent<Recoil>();
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)114) || Input.GetMouseButtonDown(0))
		{
			recoil.Fire(magnitude);
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("Press R or LMB for procedural recoil.", Array.Empty<GUILayoutOption>());
	}
}
