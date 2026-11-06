using System;
using System.Collections.Generic;
using Ceto.Common.Unity.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ceto;

[AddComponentMenu("Ceto/Components/ProjectedGrid")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Ocean))]
public class ProjectedGrid : OceanComponent
{
	public class Grid
	{
		public int screenWidth;

		public int screenHeight;

		public int resolution;

		public int groups;

		public IList<MeshFilter> topFilters = new List<MeshFilter>();

		public IList<Renderer> topRenderer = new List<Renderer>();

		public IList<GameObject> top = new List<GameObject>();

		public IList<MeshFilter> underFilters = new List<MeshFilter>();

		public IList<Renderer> underRenderer = new List<Renderer>();

		public IList<GameObject> under = new List<GameObject>();
	}

	private static readonly int MAX_SCREEN_WIDTH = 2048;

	private static readonly int MAX_SCREEN_HEIGHT = 2048;

	public MESH_RESOLUTION resolution = MESH_RESOLUTION.HIGH;

	private GRID_GROUPS gridGroups;

	public bool receiveShadows;

	private float borderLength = 200f;

	private ReflectionProbeUsage reflectionProbes;

	public Material oceanTopSideMat;

	public Material oceanUnderSideMat;

	private Dictionary<int, Grid> m_grids = new Dictionary<int, Grid>();

	private void Start()
	{
		try
		{
			if (SystemInfo.graphicsShaderLevel < 30)
			{
				throw new InvalidOperationException("The projected grids needs at least SM3 to render.");
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (WasError)
		{
			return;
		}
		try
		{
			Dictionary<int, Grid>.Enumerator enumerator = m_grids.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Grid value = enumerator.Current.Value;
				Activate(value.top, active: true);
				Activate(value.under, active: true);
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		try
		{
			Dictionary<int, Grid>.Enumerator enumerator = m_grids.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Grid value = enumerator.Current.Value;
				Activate(value.top, active: false);
				Activate(value.under, active: false);
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		try
		{
			Dictionary<int, Grid>.Enumerator enumerator = m_grids.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Grid value = enumerator.Current.Value;
				ClearGrid(value);
			}
			m_grids.Clear();
			m_grids = null;
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void Update()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Shader.SetGlobalFloat("Ceto_GridEdgeBorder", Mathf.Max(0f, borderLength));
			int num = ResolutionToNumber(resolution);
			Vector2 val = new Vector2((float)num / (float)ScreenWidth(), (float)num / (float)ScreenHeight());
			Shader.SetGlobalVector("Ceto_ScreenGridSize", Vector4.op_Implicit(val));
			CreateGrid(resolution);
			Dictionary<int, Grid>.Enumerator enumerator = m_grids.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Grid value = enumerator.Current.Value;
				if (enumerator.Current.Key == (int)resolution)
				{
					UpdateGrid(value);
					Activate(value.top, active: true);
					Activate(value.under, active: true);
				}
				else
				{
					Activate(value.top, active: false);
					Activate(value.under, active: false);
				}
			}
			if (!UnderSideNeeded())
			{
				enumerator = m_grids.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Activate(enumerator.Current.Value.under, active: false);
				}
			}
			if (!TopSideNeeded())
			{
				enumerator = m_grids.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Activate(enumerator.Current.Value.top, active: false);
				}
			}
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	public override void OceanOnPostRender(Camera cam, CameraData data)
	{
		if (((Behaviour)this).enabled && !((Object)(object)cam == (Object)null) && data != null)
		{
			Grid value = null;
			m_grids.TryGetValue((int)resolution, out value);
			if (value != null)
			{
				ResetBounds(value);
			}
		}
	}

	private void Activate(IList<GameObject> list, bool active)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if ((Object)(object)list[i] != (Object)null)
			{
				list[i].SetActive(active);
			}
		}
	}

	private void UpdateGrid(Grid grid)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		ResetBounds(grid);
		int count = grid.topRenderer.Count;
		for (int i = 0; i < count; i++)
		{
			grid.topRenderer[i].receiveShadows = receiveShadows;
			grid.topRenderer[i].reflectionProbeUsage = reflectionProbes;
			if ((Object)(object)oceanTopSideMat != (Object)null)
			{
				grid.topRenderer[i].sharedMaterial = oceanTopSideMat;
			}
		}
		count = grid.underRenderer.Count;
		for (int j = 0; j < count; j++)
		{
			grid.underRenderer[j].receiveShadows = receiveShadows;
			grid.underRenderer[j].reflectionProbeUsage = reflectionProbes;
			if ((Object)(object)oceanUnderSideMat != (Object)null)
			{
				grid.underRenderer[j].sharedMaterial = oceanUnderSideMat;
			}
		}
		count = grid.top.Count;
		for (int k = 0; k < count; k++)
		{
			grid.top[k].transform.localPosition = Vector3.zero;
			grid.top[k].transform.localRotation = Quaternion.identity;
			grid.top[k].transform.localScale = Vector3.one;
		}
		count = grid.under.Count;
		for (int l = 0; l < count; l++)
		{
			grid.under[l].transform.localPosition = Vector3.zero;
			grid.under[l].transform.localRotation = Quaternion.identity;
			grid.under[l].transform.localScale = Vector3.one;
		}
	}

	private void ResetBounds(Grid grid)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		float level = m_ocean.level;
		float num = m_ocean.FindMaxDisplacement(inculdeOverlays: true);
		float num2 = 100000000f;
		int num3 = 0;
		Bounds bounds = new Bounds(Vector3.zero, new Vector3(num2, level + num, num2));
		num3 = grid.topFilters.Count;
		for (int i = 0; i < num3; i++)
		{
			grid.topFilters[i].mesh.bounds = bounds;
		}
		num3 = grid.underFilters.Count;
		for (int j = 0; j < num3; j++)
		{
			grid.underFilters[j].mesh.bounds = bounds;
		}
	}

	private void UpdateBounds(GameObject go, Camera cam)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (!((Object)(object)component == (Object)null))
		{
			Vector3 position = ((Component)cam).transform.position;
			float level = m_ocean.level;
			float num = m_ocean.FindMaxDisplacement(inculdeOverlays: true);
			float num2 = cam.farClipPlane * 2f;
			position.y = level;
			component.mesh.bounds = new Bounds(position, new Vector3(num2, num, num2));
		}
	}

	private bool UnderSideNeeded()
	{
		if (!((Object)(object)m_ocean.UnderWater == (Object)null) && ((Behaviour)m_ocean.UnderWater).enabled)
		{
			return m_ocean.UnderWater.underwaterMode != UNDERWATER_MODE.ABOVE_ONLY;
		}
		return false;
	}

	private bool TopSideNeeded()
	{
		if (!((Object)(object)m_ocean.UnderWater == (Object)null))
		{
			return m_ocean.UnderWater.underwaterMode != UNDERWATER_MODE.BELOW_ONLY;
		}
		return true;
	}

	private int ScreenWidth()
	{
		return Mathf.Min(Screen.width, MAX_SCREEN_WIDTH);
	}

	private int ScreenHeight()
	{
		return Mathf.Min(Screen.height, MAX_SCREEN_HEIGHT);
	}

	private void ApplyProjection(GameObject go)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((Behaviour)this).enabled)
			{
				return;
			}
			Camera current = Camera.current;
			if ((Object)(object)current == (Object)null)
			{
				return;
			}
			CameraData cameraData = m_ocean.FindCameraData(current);
			if (cameraData.projection == null)
			{
				cameraData.projection = new ProjectionData();
			}
			if (!cameraData.projection.IsViewUpdated(current))
			{
				m_ocean.Projection.UpdateProjection(current, cameraData);
				Shader.SetGlobalMatrix("Ceto_Interpolation", cameraData.projection.interpolation);
				Shader.SetGlobalMatrix("Ceto_ProjectorVP", cameraData.projection.projectorVP);
			}
			if (!cameraData.projection.checkedForFlipping)
			{
				int num = 2;
				int num2 = 1;
				if (!Ocean.DISABLE_PROJECTION_FLIPPING)
				{
					bool isFlipped = m_ocean.Projection.IsFlipped;
					if ((Object)(object)oceanTopSideMat != (Object)null)
					{
						oceanTopSideMat.SetInt("_CullFace", isFlipped ? num2 : num);
					}
					if ((Object)(object)oceanUnderSideMat != (Object)null)
					{
						oceanUnderSideMat.SetInt("_CullFace", isFlipped ? num : num2);
					}
				}
				else
				{
					if ((Object)(object)oceanTopSideMat != (Object)null)
					{
						oceanTopSideMat.SetInt("_CullFace", num);
					}
					if ((Object)(object)oceanUnderSideMat != (Object)null)
					{
						oceanUnderSideMat.SetInt("_CullFace", num2);
					}
				}
				cameraData.projection.checkedForFlipping = true;
			}
			UpdateBounds(go, current);
		}
		catch (Exception ex)
		{
			Ocean.LogError(ex.ToString());
			WasError = true;
			((Behaviour)this).enabled = false;
		}
	}

	private void CreateGrid(MESH_RESOLUTION meshRes)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Expected Obj, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		Grid value = null;
		if (!m_grids.TryGetValue((int)meshRes, out value))
		{
			value = new Grid();
			m_grids.Add((int)meshRes, value);
		}
		int num = ScreenWidth();
		int num2 = ScreenHeight();
		int num3 = ResolutionToNumber(meshRes);
		int num4 = ChooseGroupSize(num3, gridGroups, num, num2);
		if (value.screenWidth == num && value.screenHeight == num2 && value.groups == num4)
		{
			return;
		}
		ClearGrid(value);
		value.screenWidth = num;
		value.screenHeight = num2;
		value.resolution = num3;
		value.groups = num4;
		foreach (Mesh item in CreateScreenQuads(num3, num4, num, num2))
		{
			if ((Object)(object)oceanTopSideMat != (Object)null)
			{
				GameObject val = new GameObject("Ceto TopSide Grid LOD: " + meshRes);
				MeshFilter val2 = val.AddComponent<MeshFilter>();
				MeshRenderer val3 = val.AddComponent<MeshRenderer>();
				NotifyOnWillRender notifyOnWillRender = val.AddComponent<NotifyOnWillRender>();
				val2.sharedMesh = item;
				((Renderer)val3).shadowCastingMode = (ShadowCastingMode)0;
				((Renderer)val3).receiveShadows = receiveShadows;
				((Renderer)val3).sharedMaterial = oceanTopSideMat;
				((Renderer)val3).reflectionProbeUsage = reflectionProbes;
				val.layer = LayerMask.NameToLayer(Ocean.OCEAN_LAYER);
				((Object)val).hideFlags = (HideFlags)61;
				notifyOnWillRender.AddAction(m_ocean.RenderReflection);
				notifyOnWillRender.AddAction(ApplyProjection);
				notifyOnWillRender.AddAction(m_ocean.RenderWaveOverlays);
				notifyOnWillRender.AddAction(m_ocean.RenderOceanMask);
				notifyOnWillRender.AddAction(m_ocean.RenderOceanDepth);
				value.top.Add(val);
				value.topRenderer.Add((Renderer)(object)val3);
				value.topFilters.Add(val2);
			}
			if (Object.op_Implicit((Object)(object)oceanUnderSideMat))
			{
				GameObject val4 = new GameObject("Ceto UnderSide Grid LOD: " + meshRes);
				MeshFilter val5 = val4.AddComponent<MeshFilter>();
				MeshRenderer val6 = val4.AddComponent<MeshRenderer>();
				NotifyOnWillRender notifyOnWillRender2 = val4.AddComponent<NotifyOnWillRender>();
				val5.sharedMesh = item;
				((Renderer)val6).shadowCastingMode = (ShadowCastingMode)0;
				((Renderer)val6).receiveShadows = receiveShadows;
				((Renderer)val6).reflectionProbeUsage = reflectionProbes;
				((Renderer)val6).sharedMaterial = oceanUnderSideMat;
				val4.layer = LayerMask.NameToLayer(Ocean.OCEAN_LAYER);
				((Object)val4).hideFlags = (HideFlags)61;
				notifyOnWillRender2.AddAction(ApplyProjection);
				notifyOnWillRender2.AddAction(m_ocean.RenderWaveOverlays);
				notifyOnWillRender2.AddAction(m_ocean.RenderOceanMask);
				notifyOnWillRender2.AddAction(m_ocean.RenderOceanDepth);
				value.under.Add(val4);
				value.underRenderer.Add((Renderer)(object)val6);
				value.underFilters.Add(val5);
			}
			Object.Destroy((Object)(object)item);
		}
	}

	private void ClearGrid(Grid grid)
	{
		if (grid == null)
		{
			return;
		}
		grid.screenWidth = 0;
		grid.screenHeight = 0;
		grid.resolution = 0;
		grid.groups = 0;
		if (grid.top != null)
		{
			int count = grid.top.Count;
			for (int i = 0; i < count; i++)
			{
				if (!((Object)(object)grid.top[i] == (Object)null))
				{
					Object.Destroy((Object)(object)grid.top[i]);
				}
			}
			grid.top.Clear();
		}
		if (grid.topFilters != null)
		{
			int count2 = grid.topFilters.Count;
			for (int j = 0; j < count2; j++)
			{
				if (!((Object)(object)grid.topFilters[j] == (Object)null))
				{
					Object.Destroy((Object)(object)grid.topFilters[j].mesh);
				}
			}
			grid.topFilters.Clear();
		}
		if (grid.under != null)
		{
			int count3 = grid.under.Count;
			for (int k = 0; k < count3; k++)
			{
				if (!((Object)(object)grid.under[k] == (Object)null))
				{
					Object.Destroy((Object)(object)grid.under[k]);
				}
			}
			grid.under.Clear();
		}
		if (grid.underFilters != null)
		{
			int count4 = grid.underFilters.Count;
			for (int l = 0; l < count4; l++)
			{
				if (!((Object)(object)grid.underFilters[l] == (Object)null))
				{
					Object.Destroy((Object)(object)grid.underFilters[l].mesh);
				}
			}
			grid.underFilters.Clear();
		}
		if (grid.topRenderer != null)
		{
			grid.topRenderer.Clear();
		}
		if (grid.underRenderer != null)
		{
			grid.underRenderer.Clear();
		}
	}

	private int ResolutionToNumber(MESH_RESOLUTION resolution)
	{
		return resolution switch
		{
			MESH_RESOLUTION.EXTREME => 1, 
			MESH_RESOLUTION.ULTRA => 2, 
			MESH_RESOLUTION.HIGH => 4, 
			MESH_RESOLUTION.MEDIUM => 8, 
			MESH_RESOLUTION.LOW => 16, 
			_ => 16, 
		};
	}

	private int GroupToNumber(GRID_GROUPS groups)
	{
		return groups switch
		{
			GRID_GROUPS.EXTREME => 128, 
			GRID_GROUPS.HIGH => 196, 
			GRID_GROUPS.MEDIUM => 256, 
			GRID_GROUPS.LOW => 512, 
			GRID_GROUPS.SINGLE => -1, 
			_ => 128, 
		};
	}

	private int ChooseGroupSize(int resolution, GRID_GROUPS groups, int width, int height)
	{
		int num = 0;
		int num2 = 0;
		int num3 = GroupToNumber(groups);
		if (num3 == -1)
		{
			num = width / resolution;
			num2 = height / resolution;
		}
		else
		{
			num = num3 / resolution;
			num2 = num3 / resolution;
		}
		while (num * num2 > 65000)
		{
			if (groups == GRID_GROUPS.EXTREME)
			{
				throw new InvalidOperationException("Can not increase group size");
			}
			groups++;
			num3 = GroupToNumber(groups);
			num = num3 / resolution;
			num2 = num3 / resolution;
		}
		return num3;
	}

	private IList<Mesh> CreateScreenQuads(int resolution, int groupSize, int width, int height)
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		if (groupSize != -1)
		{
			while (width % groupSize != 0)
			{
				width++;
			}
			while (height % groupSize != 0)
			{
				height++;
			}
			num3 = groupSize / resolution;
			num4 = groupSize / resolution;
			num5 = width / groupSize;
			num6 = height / groupSize;
			num = (float)groupSize / (float)width;
			num2 = (float)groupSize / (float)height;
		}
		else
		{
			num3 = width / resolution;
			num4 = height / resolution;
			num5 = 1;
			num6 = 1;
			num = 1f;
			num2 = 1f;
		}
		List<Mesh> list = new List<Mesh>();
		for (int i = 0; i < num5; i++)
		{
			for (int j = 0; j < num6; j++)
			{
				float ux = (float)i * num;
				float uy = (float)j * num2;
				Mesh item = CreateQuad(num3, num4, ux, uy, num, num2);
				list.Add(item);
			}
		}
		return list;
	}

	public Mesh CreateQuad(int numVertsX, int numVertsY, float ux, float uy, float w, float h)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Expected Obj, but got Unknown
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[numVertsX * numVertsY];
		Vector2[] array2 = new Vector2[numVertsX * numVertsY];
		int[] array3 = new int[numVertsX * numVertsY * 6];
		float num = 0.1f;
		for (int i = 0; i < numVertsX; i++)
		{
			for (int j = 0; j < numVertsY; j++)
			{
				Vector2 val = Vector2.op_Implicit(new Vector3((float)i / (float)(numVertsX - 1), (float)j / (float)(numVertsY - 1)));
				val.x *= w;
				val.x += ux;
				val.y *= h;
				val.y += uy;
				if (!Ocean.DISABLE_PROJECTED_GRID_BORDER)
				{
					val.x = val.x * (1f + num * 2f) - num;
					val.y = val.y * (1f + num * 2f) - num;
					Vector2 val2 = val;
					val2.x = Mathf.Clamp01(val2.x);
					val2.y = Mathf.Clamp01(val2.y);
					Vector2 val3 = val;
					if (val3.x < 0f)
					{
						val3.x = Mathf.Abs(val3.x) / num;
					}
					else if (val3.x > 1f)
					{
						val3.x = Mathf.Max(0f, val3.x - 1f) / num;
					}
					else
					{
						val3.x = 0f;
					}
					if (val3.y < 0f)
					{
						val3.y = Mathf.Abs(val3.y) / num;
					}
					else if (val3.y > 1f)
					{
						val3.y = Mathf.Max(0f, val3.y - 1f) / num;
					}
					else
					{
						val3.y = 0f;
					}
					val3.x = Mathf.Pow(val3.x, 2f);
					val3.y = Mathf.Pow(val3.y, 2f);
					array2[i + j * numVertsX] = val3;
					array[i + j * numVertsX] = new Vector3(val2.x, val2.y, 0f);
				}
				else
				{
					array2[i + j * numVertsX] = new Vector2(0f, 0f);
					array[i + j * numVertsX] = new Vector3(val.x, val.y, 0f);
				}
			}
		}
		int num2 = 0;
		for (int k = 0; k < numVertsX - 1; k++)
		{
			for (int l = 0; l < numVertsY - 1; l++)
			{
				array3[num2++] = k + l * numVertsX;
				array3[num2++] = k + (l + 1) * numVertsX;
				array3[num2++] = k + 1 + l * numVertsX;
				array3[num2++] = k + (l + 1) * numVertsX;
				array3[num2++] = k + 1 + (l + 1) * numVertsX;
				array3[num2++] = k + 1 + l * numVertsX;
			}
		}
		return new Mesh
		{
			vertices = array,
			uv = array2,
			triangles = array3,
			name = "Ceto Projected Grid Mesh",
			hideFlags = (HideFlags)61
		};
	}
}
