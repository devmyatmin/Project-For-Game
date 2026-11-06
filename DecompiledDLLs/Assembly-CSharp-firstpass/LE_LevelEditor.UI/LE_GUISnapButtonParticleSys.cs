using S_SnapTools;
using UnityEngine;

namespace LE_LevelEditor.UI;

public class LE_GUISnapButtonParticleSys : MonoBehaviour
{
	[SerializeField]
	private Material m_openedMaterial;

	[SerializeField]
	private Material m_closedMaterial;

	private S_SnapToObject m_parent;

	private Renderer m_particleSysRenderer;

	private bool m_isClosed = true;

	private void Start()
	{
		m_parent = ((Component)((Component)this).transform).GetComponentInParent<S_SnapToObject>();
		if ((Object)(object)m_parent == (Object)null)
		{
			Debug.LogError((object)"LE_GUISnapButtonParticleSys: could not find S_SnapToObject in parent.");
			Object.Destroy((Object)(object)this);
			return;
		}
		ParticleSystem component = ((Component)this).GetComponent<ParticleSystem>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)"LE_GUISnapButtonParticleSys: could not find ParticleSystem.");
			Object.Destroy((Object)(object)this);
			return;
		}
		m_particleSysRenderer = ((Component)component).GetComponent<Renderer>();
		if ((Object)(object)m_particleSysRenderer == (Object)null)
		{
			Debug.LogError((object)"LE_GUISnapButtonParticleSys: could not find Renderer for ParticleSystem.");
			Object.Destroy((Object)(object)this);
		}
		else
		{
			m_particleSysRenderer.sharedMaterial = m_closedMaterial;
		}
	}

	private void Update()
	{
		if ((Object)(object)m_parent != (Object)null && (Object)(object)m_particleSysRenderer != (Object)null)
		{
			if (m_isClosed && m_parent.IsPrefabSelectionOpen)
			{
				m_isClosed = false;
				m_particleSysRenderer.sharedMaterial = m_openedMaterial;
			}
			else if (!m_isClosed && !m_parent.IsPrefabSelectionOpen)
			{
				m_isClosed = true;
				m_particleSysRenderer.sharedMaterial = m_closedMaterial;
			}
		}
		else
		{
			Object.Destroy((Object)(object)this);
		}
	}
}
