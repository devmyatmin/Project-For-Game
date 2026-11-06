using System.Collections.Generic;
using UnityEngine;

namespace Ceto;

public class WaveQuery
{
	public struct WaveQueryResult
	{
		public float height;

		public float displacementX;

		public float displacementZ;

		public float overlayHeight;

		public Vector3[] displacement;

		public int iterations;

		public float error;

		public bool isClipped;

		public IEnumerable<QueryableOverlayResult> overlays;

		public void Clear()
		{
			height = 0f;
			overlayHeight = 0f;
			displacementX = 0f;
			displacementZ = 0f;
			iterations = 0;
			error = 0f;
			isClipped = false;
			overlays = null;
			displacement[0].x = 0f;
			displacement[0].y = 0f;
			displacement[0].z = 0f;
			displacement[1].x = 0f;
			displacement[1].y = 0f;
			displacement[1].z = 0f;
			displacement[2].x = 0f;
			displacement[2].y = 0f;
			displacement[2].z = 0f;
			displacement[3].x = 0f;
			displacement[3].y = 0f;
			displacement[3].z = 0f;
		}
	}

	public static readonly float MIN_ERROR = 0.01f;

	public static readonly int MAX_ITERATIONS = 20;

	public float minError;

	public float posX;

	public float posZ;

	public readonly bool[] sampleSpectrum;

	public bool sampleOverlay;

	public bool overrideIgnoreQuerys;

	public QUERY_MODE mode;

	public int tag;

	public WaveQueryResult result;

	public WaveQuery()
	{
		posX = 0f;
		posZ = 0f;
		minError = 0.1f;
		sampleSpectrum = new bool[4] { true, true, false, false };
		sampleOverlay = true;
		mode = QUERY_MODE.POSITION;
		result.displacement = new Vector3[4];
	}

	public WaveQuery(Vector3 worldPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		posX = worldPos.x;
		posZ = worldPos.z;
		minError = 0.1f;
		sampleSpectrum = new bool[4] { true, true, false, false };
		sampleOverlay = true;
		mode = QUERY_MODE.POSITION;
		result.displacement = new Vector3[4];
	}

	public WaveQuery(float x, float z)
	{
		posX = x;
		posZ = z;
		minError = 0.1f;
		sampleSpectrum = new bool[4] { true, true, false, false };
		sampleOverlay = true;
		mode = QUERY_MODE.POSITION;
		result.displacement = new Vector3[4];
	}
}
