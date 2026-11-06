using System;
using System.Collections.Generic;
using Ceto.Common.Unity.Utility;
using UnityEngine;

namespace Ceto;

public class WaveSpectrumBufferGPU : WaveSpectrumBuffer
{
	private struct Buffer
	{
		public RenderTexture[] data;

		public bool disabled;
	}

	private Buffer[] m_buffers;

	private bool m_samplingEnabled;

	private int m_index = -1;

	private FourierGPU m_fourier;

	private IList<RenderTexture[]> m_enabledData;

	private string m_bufferName;

	private IList<RenderTexture> m_tmpList;

	private RenderBuffer[] m_tmpBuffer2;

	private RenderBuffer[] m_tmpBuffer3;

	private RenderBuffer[] m_tmpBuffer4;

	private Vector4 m_offset;

	public override bool Done => true;

	public override int Size => m_fourier.size;

	public override bool IsGPU => true;

	public WaveSpectrumBufferGPU(int size, Shader fourierSdr, int numBuffers)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (numBuffers < 1 || numBuffers > 4)
		{
			throw new InvalidOperationException("Number of buffers is " + numBuffers + " but must be between (inclusive) 1 and 4");
		}
		m_buffers = new Buffer[numBuffers];
		m_fourier = new FourierGPU(size, fourierSdr);
		m_tmpList = new List<RenderTexture>();
		m_tmpBuffer2 = new RenderBuffer[2];
		m_tmpBuffer3 = new RenderBuffer[3];
		m_tmpBuffer4 = new RenderBuffer[4];
		m_offset = new Vector4(1f + 0.5f / (float)Size, 1f + 0.5f / (float)Size, 0f, 0f);
		for (int i = 0; i < numBuffers; i++)
		{
			m_buffers[i] = CreateBuffer(size);
		}
		m_enabledData = new List<RenderTexture[]>();
		UpdateEnabledData();
		m_bufferName = "Ceto Wave Spectrum GPU Buffer";
	}

	private Buffer CreateBuffer(int size)
	{
		return new Buffer
		{
			data = new RenderTexture[2]
		};
	}

	public override Texture GetTexture(int idx)
	{
		if (m_index == -1)
		{
			return (Texture)(object)Texture2D.blackTexture;
		}
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return (Texture)(object)Texture2D.blackTexture;
		}
		if (m_buffers[idx].disabled)
		{
			return (Texture)(object)Texture2D.blackTexture;
		}
		return (Texture)(object)m_buffers[idx].data[m_index];
	}

	public override void Release()
	{
		m_tmpList.Clear();
		m_fourier.Release();
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			m_buffers[i].data[0] = null;
			m_buffers[i].data[1] = null;
		}
	}

	protected override void Initilize(WaveSpectrumCondition condition, float time)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)InitMaterial == (Object)null)
		{
			throw new InvalidOperationException("GPU buffer has not had its Init material set");
		}
		if (InitPass == -1)
		{
			throw new InvalidOperationException("GPU buffer has not had its Init material pass set");
		}
		InitMaterial.SetTexture("Ceto_Spectrum01", (Texture)(object)(((Object)(object)condition.Spectrum01 != (Object)null) ? condition.Spectrum01 : Texture2D.blackTexture));
		InitMaterial.SetTexture("Ceto_Spectrum23", (Texture)(object)(((Object)(object)condition.Spectrum23 != (Object)null) ? condition.Spectrum23 : Texture2D.blackTexture));
		InitMaterial.SetTexture("Ceto_WTable", (Texture)(object)condition.WTable);
		InitMaterial.SetVector("Ceto_InverseGridSizes", condition.InverseGridSizes());
		InitMaterial.SetVector("Ceto_GridSizes", condition.GridSizes);
		InitMaterial.SetVector("Ceto_Offset", m_offset);
		InitMaterial.SetFloat("Ceto_Time", time);
		m_tmpList.Clear();
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			if (!m_buffers[i].disabled)
			{
				m_tmpList.Add(m_buffers[i].data[1]);
			}
		}
		switch (m_tmpList.Count)
		{
		case 1:
			Graphics.Blit((Texture)null, m_tmpList[0], InitMaterial, InitPass);
			break;
		case 2:
			m_tmpBuffer2[0] = m_tmpList[0].colorBuffer;
			m_tmpBuffer2[1] = m_tmpList[1].colorBuffer;
			RTUtility.MultiTargetBlit(m_tmpBuffer2, m_tmpList[0].depthBuffer, InitMaterial, InitPass);
			break;
		case 3:
			m_tmpBuffer3[0] = m_tmpList[0].colorBuffer;
			m_tmpBuffer3[1] = m_tmpList[1].colorBuffer;
			m_tmpBuffer3[2] = m_tmpList[2].colorBuffer;
			RTUtility.MultiTargetBlit(m_tmpBuffer3, m_tmpList[0].depthBuffer, InitMaterial, InitPass);
			break;
		case 4:
			m_tmpBuffer4[0] = m_tmpList[0].colorBuffer;
			m_tmpBuffer4[1] = m_tmpList[1].colorBuffer;
			m_tmpBuffer4[2] = m_tmpList[2].colorBuffer;
			m_tmpBuffer4[3] = m_tmpList[3].colorBuffer;
			RTUtility.MultiTargetBlit(m_tmpBuffer4, m_tmpList[0].depthBuffer, InitMaterial, InitPass);
			break;
		}
	}

	public void UpdateEnabledData()
	{
		m_enabledData.Clear();
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			if (!m_buffers[i].disabled)
			{
				m_enabledData.Add(m_buffers[i].data);
			}
		}
	}

	public override void EnableBuffer(int idx)
	{
		int num = m_buffers.Length;
		if (idx < -1 || idx >= num)
		{
			return;
		}
		if (idx == -1)
		{
			for (int i = 0; i < num; i++)
			{
				m_buffers[i].disabled = false;
			}
		}
		else
		{
			m_buffers[idx].disabled = false;
		}
		UpdateEnabledData();
	}

	public override void DisableBuffer(int idx)
	{
		int num = m_buffers.Length;
		if (idx < -1 || idx >= num)
		{
			return;
		}
		if (idx == -1)
		{
			for (int i = 0; i < num; i++)
			{
				m_buffers[i].disabled = true;
			}
		}
		else
		{
			m_buffers[idx].disabled = true;
		}
		UpdateEnabledData();
	}

	public override int EnabledBuffers()
	{
		return m_enabledData.Count;
	}

	public override bool IsEnabledBuffer(int idx)
	{
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return false;
		}
		return !m_buffers[idx].disabled;
	}

	private void CreateTextures()
	{
		int count = m_enabledData.Count;
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if ((Object)(object)m_enabledData[i][j] != (Object)null)
				{
					RenderTexture.ReleaseTemporary(m_enabledData[i][j]);
				}
				RenderTexture temporary = RenderTexture.GetTemporary(Size, Size, 0, (RenderTextureFormat)11, (RenderTextureReadWrite)1);
				((Texture)temporary).filterMode = (FilterMode)0;
				((Texture)temporary).wrapMode = (TextureWrapMode)1;
				((Object)temporary).name = m_bufferName;
				((Texture)temporary).anisoLevel = 0;
				temporary.Create();
				m_enabledData[i][j] = temporary;
			}
		}
	}

	public override void Run(WaveSpectrumCondition condition, float time)
	{
		TimeValue = time;
		HasRun = true;
		BeenSampled = false;
		if (m_samplingEnabled)
		{
			throw new InvalidOperationException("Can not run if sampling enabled");
		}
		UpdateEnabledData();
		CreateTextures();
		int count = m_enabledData.Count;
		if (count != 0)
		{
			Initilize(condition, time);
			switch (count)
			{
			case 1:
				m_index = m_fourier.PeformFFT(m_enabledData[0]);
				break;
			case 2:
				m_index = m_fourier.PeformFFT(m_enabledData[0], m_enabledData[1]);
				break;
			case 3:
				m_index = m_fourier.PeformFFT(m_enabledData[0], m_enabledData[1], m_enabledData[2]);
				break;
			case 4:
				m_index = m_fourier.PeformFFT(m_enabledData[0], m_enabledData[1], m_enabledData[2], m_enabledData[3]);
				break;
			}
		}
	}

	public override void EnableSampling()
	{
		if (m_index == -1)
		{
			return;
		}
		m_samplingEnabled = true;
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			if (!((Object)(object)m_buffers[i].data[m_index] == (Object)null))
			{
				((Texture)m_buffers[i].data[m_index]).filterMode = (FilterMode)1;
				((Texture)m_buffers[i].data[m_index]).wrapMode = (TextureWrapMode)0;
			}
		}
	}

	public override void DisableSampling()
	{
		if (m_index == -1)
		{
			return;
		}
		m_samplingEnabled = false;
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			if (!((Object)(object)m_buffers[i].data[m_index] == (Object)null))
			{
				((Texture)m_buffers[i].data[m_index]).filterMode = (FilterMode)0;
				((Texture)m_buffers[i].data[m_index]).wrapMode = (TextureWrapMode)1;
			}
		}
	}
}
