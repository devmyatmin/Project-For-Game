using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace uMyGUI;

public class uMyGUI_TexturePicker : MonoBehaviour
{
	[SerializeField]
	private GameObject m_texturePrefab;

	[SerializeField]
	private GameObject m_selectionPrefab;

	[SerializeField]
	private float m_offsetStart;

	[SerializeField]
	private float m_offsetEnd;

	[SerializeField]
	private float m_padding = 4f;

	[SerializeField]
	private Action<int> m_buttonCallback;

	private Texture2D[] m_textures = new Texture2D[0];

	private RectTransform m_rectTransform;

	private float m_elementSize = 1f;

	private GameObject m_selectionInstance;

	private GameObject[] m_instances = new GameObject[0];

	public GameObject TexturePrefab
	{
		get
		{
			return m_texturePrefab;
		}
		set
		{
			m_texturePrefab = value;
		}
	}

	public GameObject SelectionPrefab
	{
		get
		{
			return m_selectionPrefab;
		}
		set
		{
			m_selectionPrefab = value;
		}
	}

	public float OffsetStart
	{
		get
		{
			return m_offsetStart;
		}
		set
		{
			m_offsetStart = value;
		}
	}

	public float OffsetEnd
	{
		get
		{
			return m_offsetEnd;
		}
		set
		{
			m_offsetEnd = value;
		}
	}

	public float Padding
	{
		get
		{
			return m_padding;
		}
		set
		{
			m_padding = value;
		}
	}

	public Action<int> ButtonCallback
	{
		get
		{
			return m_buttonCallback;
		}
		set
		{
			m_buttonCallback = value;
		}
	}

	public Texture2D[] Textures
	{
		get
		{
			return m_textures;
		}
		set
		{
			m_textures = value;
		}
	}

	private RectTransform RTransform
	{
		get
		{
			if (!((Object)(object)m_rectTransform != (Object)null))
			{
				return m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public void SetSelection(int p_selectionIndex)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_selectionPrefab != (Object)null)
		{
			if (p_selectionIndex < 0 || p_selectionIndex >= m_instances.Length)
			{
				Object.Destroy((Object)(object)m_selectionInstance);
				m_selectionInstance = null;
				return;
			}
			if ((Object)(object)m_selectionInstance == (Object)null)
			{
				m_selectionInstance = Object.Instantiate<GameObject>(m_selectionPrefab);
			}
			else
			{
				RectTransform component = m_selectionInstance.GetComponent<RectTransform>();
				Vector2 anchoredPosition = component.anchoredPosition;
				anchoredPosition.x = m_selectionPrefab.GetComponent<RectTransform>().anchoredPosition.x;
				component.anchoredPosition = anchoredPosition;
			}
			SetRectTransformPosition(m_selectionInstance.GetComponent<RectTransform>(), p_selectionIndex, m_elementSize);
		}
		else
		{
			Debug.LogError((object)("uMyGUI_TexturePicker: SetSelection: you have passed a non negative selection index '" + p_selectionIndex + "', but the SelectionPrefab was not provided in the inspector or via script!"));
		}
	}

	public void SetTextures(Texture2D[] p_textures, int p_selectedIndex)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected Obj, but got Unknown
		if ((Object)(object)m_texturePrefab != (Object)null)
		{
			m_textures = p_textures;
			Object.Destroy((Object)(object)m_selectionInstance);
			for (int i = 0; i < m_instances.Length; i++)
			{
				Object.Destroy((Object)(object)m_instances[i]);
			}
			m_instances = new GameObject[p_textures.Length];
			float num = 0f;
			Rect rect;
			for (int j = 0; j < p_textures.Length; j++)
			{
				m_instances[j] = Object.Instantiate<GameObject>(m_texturePrefab);
				RectTransform component = m_instances[j].GetComponent<RectTransform>();
				rect = component.rect;
				m_elementSize = rect.width;
				SetRectTransformPosition(component, j, m_elementSize);
				RawImage val = TryFindComponent<RawImage>(m_instances[j]);
				if ((Object)(object)val != (Object)null)
				{
					val.texture = (Texture)(object)p_textures[j];
				}
				else
				{
					Debug.LogError((object)"uMyGUI_TexturePicker: SetTextures: TexturePrefab must have a RawImage component attached (can be in children).");
				}
				if (m_buttonCallback != null)
				{
					Button val2 = TryFindComponent<Button>(m_instances[j]);
					if ((Object)(object)val2 != (Object)null)
					{
						int indexCopy = j;
						((UnityEvent)val2.onClick).AddListener((UnityAction)(() =>
						{
							m_buttonCallback(indexCopy);
						}));
					}
				}
				num = component.anchoredPosition.x + m_elementSize;
				if (j == p_selectedIndex)
				{
					if ((Object)(object)m_selectionPrefab != (Object)null)
					{
						m_selectionInstance = Object.Instantiate<GameObject>(m_selectionPrefab);
						SetRectTransformPosition(m_selectionInstance.GetComponent<RectTransform>(), j, m_elementSize);
					}
					else
					{
						Debug.LogError((object)("uMyGUI_TexturePicker: SetTextures: you have passed a non negative selection index '" + p_selectedIndex + "', but the SelectionPrefab was not provided in the inspector or via script!"));
					}
				}
			}
			RectTransform rTransform = RTransform;
			float num2 = num;
			rect = RTransform.rect;
			rTransform.SetSizeWithCurrentAnchors((Axis)0, num2 - rect.xMin + m_offsetEnd);
		}
		else
		{
			Debug.LogError((object)"uMyGUI_TexturePicker: SetTextures: you must provide the TexturePrefab in the inspector or via script!");
		}
	}

	private void OnDestroy()
	{
		m_buttonCallback = null;
	}

	private void SetRectTransformPosition(RectTransform p_transform, int p_positionIndex, float p_size)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((Transform)p_transform).SetParent((Transform)(object)RTransform, false);
		Vector2 anchoredPosition = p_transform.anchoredPosition;
		anchoredPosition.x += m_offsetStart + (float)p_positionIndex * (p_size + m_padding);
		p_transform.anchoredPosition = anchoredPosition;
	}

	private T TryFindComponent<T>(GameObject p_object) where T : Component
	{
		T val = p_object.GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			T[] componentsInChildren = p_object.GetComponentsInChildren<T>(true);
			if (componentsInChildren.Length != 0)
			{
				val = componentsInChildren[0];
			}
		}
		return val;
	}
}
