using System;
using UnityEngine;

namespace Ceto;

[RequireComponent(typeof(Ocean))]
public abstract class OceanComponent : MonoBehaviour
{
	protected Ocean m_ocean;

	public bool WasError { get; protected set; }

	protected virtual void Awake()
	{
		try
		{
			m_ocean = ((Component)this).GetComponent<Ocean>();
			m_ocean.Register(this);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected virtual void OnEnable()
	{
		if (WasError || (Object)(object)m_ocean == (Object)null || m_ocean.WasError)
		{
			((Behaviour)this).enabled = false;
		}
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnDestroy()
	{
		try
		{
			m_ocean.Deregister(this);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	public virtual void OceanOnPreRender(Camera cam, CameraData data)
	{
	}

	public virtual void OceanOnPreCull(Camera cam, CameraData data)
	{
	}

	public virtual void OceanOnPostRender(Camera cam, CameraData data)
	{
	}
}
