using System;
using UnityEngine;

namespace S_SnapTools;

public class S_SnapToObjectEventArgs : EventArgs
{
	private readonly S_SnapToObject m_source;

	private readonly GameObject m_newInstance;

	public S_SnapToObject Source => m_source;

	public GameObject NewInstance => m_newInstance;

	public S_SnapToObjectEventArgs(S_SnapToObject p_source, GameObject p_newInstance)
	{
		m_source = p_source;
		m_newInstance = p_newInstance;
	}
}
