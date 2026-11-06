using LE_LevelEditor.UI;

namespace LE_LevelEditor.Logic;

public class LE_LogicLevel : LE_LogicBase
{
	private LE_GUI3dTerrain m_GUI3dTerrain;

	private LE_GUI3dObject m_GUI3dObject;

	private bool m_isObliqueFocusCentering;

	public LE_LogicLevel(LE_GUI3dTerrain p_GUI3dTerrain, LE_GUI3dObject p_GUI3dObject, bool p_isObliqueFocusCentering)
	{
		m_GUI3dTerrain = p_GUI3dTerrain;
		m_GUI3dObject = p_GUI3dObject;
		m_isObliqueFocusCentering = p_isObliqueFocusCentering;
	}

	public override void Destroy()
	{
	}

	public override void Update()
	{
	}
}
