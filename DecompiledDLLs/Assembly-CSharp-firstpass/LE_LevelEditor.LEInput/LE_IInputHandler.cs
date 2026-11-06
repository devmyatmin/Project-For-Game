using UnityEngine;

namespace LE_LevelEditor.LEInput;

public interface LE_IInputHandler
{
	void SetCursorPosition(Vector3 p_cursorScreenCoords);

	void SetIsCursorAction(bool p_isCursorAction);

	void MoveCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords, bool scrolling = false);

	void RotateCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords);

	void RotateCameraAroundPivot(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords);
}
