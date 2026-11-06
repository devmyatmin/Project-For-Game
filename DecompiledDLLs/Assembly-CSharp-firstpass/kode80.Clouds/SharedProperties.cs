using UnityEngine;

namespace kode80.Clouds;

public class SharedProperties
{
	public float _earthRadius;

	private float _atmosphereStartHeight;

	private float _atmosphereEndHeight;

	public Vector3 cameraPosition;

	private float _maxDistance;

	private float _maxRayDistance;

	public Matrix4x4 jitter;

	public Matrix4x4 previousProjection;

	public Matrix4x4 previousInverseRotation;

	public Matrix4x4 previousRotation;

	public Matrix4x4 projection;

	public Matrix4x4 inverseRotation;

	public Matrix4x4 rotation;

	private int _subFrameNumber;

	private int _downsample;

	private int _subPixelSize;

	private bool _dimensionsChangedSinceLastFrame;

	private int _subFrameWidth;

	private int _subFrameHeight;

	private int _frameWidth;

	private int _frameHeight;

	public bool useFixedDimensions;

	public int fixedWidth;

	public int fixedHeight;

	private int[] _frameNumbers;

	private int _renderCount;

	public float earthRadius
	{
		get
		{
			return _earthRadius;
		}
		set
		{
			_earthRadius = value;
			_maxDistance = CalculateMaxDistance();
			_maxRayDistance = CalculateMaxRayDistance();
		}
	}

	public float atmosphereStartHeight
	{
		get
		{
			return _atmosphereStartHeight;
		}
		set
		{
			_atmosphereStartHeight = value;
			_maxDistance = CalculateMaxDistance();
			_maxRayDistance = CalculateMaxRayDistance();
		}
	}

	public float atmosphereEndHeight
	{
		get
		{
			return _atmosphereEndHeight;
		}
		set
		{
			_atmosphereEndHeight = value;
			_maxDistance = CalculateMaxDistance();
			_maxRayDistance = CalculateMaxRayDistance();
		}
	}

	public float maxDistance => _maxDistance;

	public float maxRayDistance => _maxRayDistance;

	public int subFrameNumber => _subFrameNumber;

	public int downsample
	{
		get
		{
			return _downsample;
		}
		set
		{
			_downsample = value;
		}
	}

	public int subPixelSize
	{
		get
		{
			return _subPixelSize;
		}
		set
		{
			_subPixelSize = value;
			_frameNumbers = CreateFrameNumbers(_subPixelSize);
			_subFrameNumber = 0;
		}
	}

	public bool dimensionsChangedSinceLastFrame => _dimensionsChangedSinceLastFrame;

	public int subFrameWidth => _subFrameWidth;

	public int subFrameHeight => _subFrameHeight;

	public int frameWidth => _frameWidth;

	public int frameHeight => _frameHeight;

	public SharedProperties()
	{
		_renderCount = 0;
		downsample = 2;
		subPixelSize = 2;
	}

	public void BeginFrame(Camera camera)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		UpdateFrameDimensions(camera);
		projection = camera.projectionMatrix;
		rotation = camera.worldToCameraMatrix;
		inverseRotation = camera.cameraToWorldMatrix;
		jitter = CreateJitterMatrix();
	}

	public void EndFrame()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		previousProjection = projection;
		previousRotation = rotation;
		previousInverseRotation = inverseRotation;
		_dimensionsChangedSinceLastFrame = false;
		_renderCount++;
		_subFrameNumber = _frameNumbers[_renderCount % (subPixelSize * subPixelSize)];
	}

	public void ApplyToMaterial(Material material, bool jitterProjection = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 val = projection.inverse;
		if (jitterProjection)
		{
			val *= jitter;
		}
		material.SetFloat("_EarthRadius", earthRadius);
		material.SetFloat("_StartHeight", atmosphereStartHeight);
		material.SetFloat("_EndHeight", atmosphereEndHeight);
		material.SetFloat("_AtmosphereThickness", atmosphereEndHeight - atmosphereStartHeight);
		material.SetVector("_CameraPosition", Vector4.op_Implicit(cameraPosition));
		material.SetFloat("_MaxDistance", maxDistance);
		material.SetMatrix("_PreviousProjection", previousProjection);
		material.SetMatrix("_PreviousInverseProjection", previousProjection.inverse);
		material.SetMatrix("_PreviousRotation", previousRotation);
		material.SetMatrix("_PreviousInverseRotation", previousInverseRotation);
		material.SetMatrix("_Projection", projection);
		material.SetMatrix("_InverseProjection", val);
		material.SetMatrix("_Rotation", rotation);
		material.SetMatrix("_InverseRotation", inverseRotation);
		material.SetFloat("_SubFrameNumber", (float)subFrameNumber);
		material.SetFloat("_SubPixelSize", (float)subPixelSize);
		material.SetVector("_SubFrameSize", Vector4.op_Implicit(new Vector2((float)_subFrameWidth, (float)_subFrameHeight)));
		material.SetVector("_FrameSize", Vector4.op_Implicit(new Vector2((float)_frameWidth, (float)_frameHeight)));
	}

	public Vector3 NormalizedPointToAtmosphere(Vector2 point, Camera theCamera)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		point.x *= theCamera.pixelWidth;
		point.y *= theCamera.pixelHeight;
		return ScreenPointToAtmosphere(point, theCamera);
	}

	private Vector3 ScreenPointToAtmosphere(Vector2 screenPoint, Camera theCamera)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(screenPoint.x / (float)theCamera.pixelWidth, screenPoint.y / (float)theCamera.pixelHeight);
		Vector4 val2 = new Vector4(val.x * 2f - 1f, val.y * 2f - 1f, 1f, 1f);
		Matrix4x4 val3 = theCamera.projectionMatrix;
		val2 = val3.inverse * val2;
		val2.x /= val2.w;
		val2.y /= val2.w;
		val2.z /= val2.w;
		Vector3 val4 = new Vector3(val2.x, val2.y, val2.z);
		val3 = theCamera.cameraToWorldMatrix;
		Vector3 val5 = val3.MultiplyVector(val4);
		val4 = val5.normalized;
		float sphereRadius = earthRadius + atmosphereStartHeight;
		return InternalRaySphereIntersect(sphereRadius, cameraPosition, val4);
	}

	private void UpdateFrameDimensions(Camera camera)
	{
		int i = (useFixedDimensions ? fixedWidth : (camera.pixelWidth / downsample));
		int j = (useFixedDimensions ? fixedHeight : (camera.pixelHeight / downsample));
		for (; i % _subPixelSize != 0; i++)
		{
		}
		for (; j % _subPixelSize != 0; j++)
		{
		}
		int num = i / _subPixelSize;
		int num2 = j / _subPixelSize;
		_dimensionsChangedSinceLastFrame = i != _frameWidth || j != _frameHeight || num != _subFrameWidth || num2 != _subFrameHeight;
		_frameWidth = i;
		_frameHeight = j;
		_subFrameWidth = num;
		_subFrameHeight = num2;
	}

	private int[] CreateFrameNumbers(int subPixelSize)
	{
		int num = subPixelSize * subPixelSize;
		int num2 = 0;
		int[] array = new int[num];
		for (num2 = 0; num2 < num; num2++)
		{
			array[num2] = num2;
		}
		while (num2-- > 0)
		{
			int num3 = array[num2];
			int num4 = (int)(Random.value * 1000f) % num;
			array[num2] = array[num4];
			array[num4] = num3;
		}
		return array;
	}

	private Matrix4x4 CreateJitterMatrix()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		int num = subFrameNumber % subPixelSize;
		int num2 = subFrameNumber / subPixelSize;
		return Matrix4x4.TRS(new Vector3((float)num * 2f / (float)_frameWidth, (float)num2 * 2f / (float)_frameHeight), Quaternion.identity, Vector3.one);
	}

	private float CalculateHorizonDistance(float innerRadius, float outerRadius)
	{
		return Mathf.Sqrt(outerRadius * outerRadius - innerRadius * innerRadius);
	}

	public float CalculatePlanetRadius(float atmosphereHeight, float horizonDistance)
	{
		return (atmosphereHeight * atmosphereHeight + horizonDistance * horizonDistance) / (2f * atmosphereHeight) - atmosphereHeight;
	}

	private float CalculateMaxDistance()
	{
		return CalculateHorizonDistance(earthRadius, earthRadius + atmosphereEndHeight);
	}

	private float CalculateMaxRayDistance()
	{
		float num = CalculateHorizonDistance(earthRadius, earthRadius + atmosphereStartHeight);
		return CalculateHorizonDistance(earthRadius, earthRadius + atmosphereEndHeight) - num;
	}

	private Vector3 InternalRaySphereIntersect(float sphereRadius, Vector3 origin, Vector3 rayDirection)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		float num = sphereRadius * sphereRadius - Vector3.Dot(origin, origin);
		float num2 = Vector3.Dot(origin, rayDirection);
		float num3 = Mathf.Sqrt(num2 * num2 + num) - num2;
		return origin + rayDirection * num3;
	}
}
