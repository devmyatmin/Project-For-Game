using Beam;
using LE_LevelEditor.Core;
using Rewired;
using UnityEngine;

namespace LE_LevelEditor.LEInput;

public class LE_InputDeviceKeyboard : LE_InputDeviceBase
{
	private const float MOVE_SPEED = 18f;

	private const float MOVE_SPEED_SHIFT_FACTOR = 5f;

	public LE_InputDeviceKeyboard(LE_IInputHandler p_inputHandler)
		: base(p_inputHandler)
	{
	}

	public override void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Player player = ReInput.players.GetPlayer(0);
		val.x += player.GetAxis(43);
		val.y += player.GetAxis(45);
		val.z += player.GetAxis(62);
		if (player.GetButtonDown(50))
		{
			Singleton<LE_LevelEditorMain>.Instance.OnObjectEditModeKey(LE_EObjectEditMode.ROTATE);
		}
		if (player.GetButtonDown(51))
		{
			Singleton<LE_LevelEditorMain>.Instance.OnObjectEditModeKey(LE_EObjectEditMode.MOVE);
		}
		if (player.GetButtonDown(48))
		{
			Singleton<LE_LevelEditorMain>.Instance.OnSelectedObjectDeleteKey();
		}
		if (player.GetButtonDown(49))
		{
			Singleton<LE_LevelEditorMain>.Instance.OnSelectedObjectDuplicateKey();
		}
		if (player.GetButton(52))
		{
			val *= 5f;
		}
		val *= 18f;
		if (!Mathf.Approximately(val.sqrMagnitude, 0f))
		{
			m_inputHandler.MoveCamera(Vector3.zero, val);
		}
	}

	public override void Destroy()
	{
	}
}
