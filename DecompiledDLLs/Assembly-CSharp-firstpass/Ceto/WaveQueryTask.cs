using System;
using System.Collections.Generic;
using Ceto.Common.Containers.Interpolation;
using Ceto.Common.Threading.Tasks;
using UnityEngine;

namespace Ceto;

public abstract class WaveQueryTask : ThreadedTask
{
	protected IList<InterpolatedArray2f> Displacements;

	public abstract bool SupportsOverlays { get; }

	public IOverlaySampler OverlaySampler { get; set; }

	public bool IsScheduled { get; set; }

	public Action<IEnumerable<WaveQuery>> CallBack { get; private set; }

	public IEnumerable<WaveQuery> Querys { get; private set; }

	protected float OceanLevel { get; private set; }

	protected int EnabledBuffers { get; private set; }

	protected QueryGridScaling Scaling { get; private set; }

	protected int BufferSize { get; private set; }

	public WaveQueryTask(IEnumerable<WaveQuery> querys, Action<IEnumerable<WaveQuery>> callback, bool isThreaded)
		: base(isThreaded)
	{
		Querys = querys;
		CallBack = callback;
		Scaling = new QueryGridScaling();
	}

	public void Reset(WaveSpectrum spectrum, Vector3 offset, float level)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		base.Reset();
		IsScheduled = false;
		if ((Object)(object)spectrum != (Object)null && spectrum.DisplacementBuffer != null)
		{
			IDisplacementBuffer displacementBuffer = spectrum.DisplacementBuffer;
			if (Displacements == null || BufferSize != displacementBuffer.Size)
			{
				displacementBuffer.CopyAndCreateDisplacements(out Displacements);
				BufferSize = displacementBuffer.Size;
			}
			else
			{
				displacementBuffer.CopyDisplacements(Displacements);
			}
			EnabledBuffers = displacementBuffer.EnabledBuffers();
			Vector4 invGridSizes = new Vector4
			{
				x = 1f / (spectrum.GridSizes.x * spectrum.gridScale),
				y = 1f / (spectrum.GridSizes.y * spectrum.gridScale),
				z = 1f / (spectrum.GridSizes.z * spectrum.gridScale),
				w = 1f / (spectrum.GridSizes.w * spectrum.gridScale)
			};
			Scaling.invGridSizes = invGridSizes;
			Scaling.choppyness = spectrum.Choppyness * spectrum.gridScale;
			Scaling.scaleY = spectrum.gridScale;
			Scaling.offset = offset;
			Scaling.numGrids = spectrum.numberOfGrids;
		}
	}

	public override void End()
	{
		base.End();
		CallBack(Querys);
	}
}
