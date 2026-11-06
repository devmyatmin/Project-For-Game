using System;
using System.Collections.Generic;
using Ceto.Common.Threading.Scheduling;
using UnityEngine;

namespace Ceto;

public abstract class WaveSpectrumBufferCPU : WaveSpectrumBuffer
{
	public class Buffer
	{
		public IList<Vector4[]> data;

		public Color[] results;

		public Texture2D map;

		public bool disabled;

		public bool doublePacked;
	}

	public const int READ = 1;

	public const int WRITE = 0;

	protected Buffer[] m_buffers;

	private FourierCPU m_fourier;

	private Scheduler m_scheduler;

	private List<FourierTask> m_fourierTasks;

	protected InitSpectrumDisplacementsTask m_initTask;

	public override bool Done => IsDone();

	public override int Size => m_fourier.size;

	public override bool IsGPU => false;

	public Color[] WTable { get; private set; }

	public WaveSpectrumBufferCPU(int size, int numBuffers, Scheduler scheduler)
	{
		m_buffers = new Buffer[numBuffers];
		m_fourier = new FourierCPU(size);
		m_fourierTasks = new List<FourierTask>(3);
		m_fourierTasks.Add(null);
		m_fourierTasks.Add(null);
		m_fourierTasks.Add(null);
		m_scheduler = scheduler;
		for (int i = 0; i < numBuffers; i++)
		{
			m_buffers[i] = CreateBuffer(size);
		}
	}

	private Buffer CreateBuffer(int size)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		Buffer buffer = new Buffer();
		buffer.doublePacked = true;
		buffer.data = new List<Vector4[]>();
		buffer.data.Add(new Vector4[size * size]);
		buffer.data.Add(new Vector4[size * size]);
		buffer.results = new Color[size * size];
		buffer.map = new Texture2D(size, size, (TextureFormat)20, false, true);
		((Texture)buffer.map).wrapMode = (TextureWrapMode)0;
		((Texture)buffer.map).filterMode = (FilterMode)1;
		((Object)buffer.map).hideFlags = (HideFlags)61;
		((Object)buffer.map).name = "Ceto Wave Spectrum CPU Buffer";
		buffer.map.SetPixels(buffer.results);
		buffer.map.Apply();
		return buffer;
	}

	public override void Release()
	{
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			Object.Destroy((Object)(object)m_buffers[i].map);
			m_buffers[i].map = null;
		}
	}

	public override Texture GetTexture(int idx)
	{
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return (Texture)(object)Texture2D.blackTexture;
		}
		if (m_buffers[idx].disabled)
		{
			return (Texture)(object)Texture2D.blackTexture;
		}
		return (Texture)(object)m_buffers[idx].map;
	}

	public Vector4[] GetWriteBuffer(int idx)
	{
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return null;
		}
		if (m_buffers[idx].disabled)
		{
			return null;
		}
		return m_buffers[idx].data[0];
	}

	public Vector4[] GetReadBuffer(int idx)
	{
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return null;
		}
		if (m_buffers[idx].disabled)
		{
			return null;
		}
		return m_buffers[idx].data[1];
	}

	public Buffer GetBuffer(int idx)
	{
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return null;
		}
		if (m_buffers[idx].disabled)
		{
			return null;
		}
		return m_buffers[idx];
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
	}

	public bool IsDone()
	{
		if (m_initTask == null)
		{
			return true;
		}
		if (!m_initTask.Done)
		{
			return false;
		}
		int count = m_fourierTasks.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_fourierTasks[i] != null && !m_fourierTasks[i].Done)
			{
				return false;
			}
		}
		return true;
	}

	public override int EnabledBuffers()
	{
		int num = 0;
		int num2 = m_buffers.Length;
		for (int i = 0; i < num2; i++)
		{
			if (!m_buffers[i].disabled)
			{
				num++;
			}
		}
		return num;
	}

	public override bool IsEnabledBuffer(int idx)
	{
		if (idx < 0 || idx >= m_buffers.Length)
		{
			return false;
		}
		return !m_buffers[idx].disabled;
	}

	public override void Run(WaveSpectrumCondition condition, float time)
	{
		if (!IsDone())
		{
			throw new InvalidOperationException("Can not run when there are tasks that have not finished");
		}
		TimeValue = time;
		HasRun = true;
		BeenSampled = false;
		if (EnabledBuffers() == 0)
		{
			return;
		}
		Initilize(condition, time);
		if (Ocean.DISABLE_FOURIER_MULTITHREADING)
		{
			m_initTask.Start();
			m_initTask.Run();
			m_initTask.End();
		}
		int num = m_buffers.Length;
		for (int i = 0; i < num; i++)
		{
			if (!m_buffers[i].disabled)
			{
				if (m_fourierTasks[i] == null)
				{
					m_fourierTasks[i] = new FourierTask(this, m_fourier, i, m_initTask.NumGrids);
				}
				else
				{
					m_fourierTasks[i].Reset(i, m_initTask.NumGrids);
				}
				FourierTask fourierTask = m_fourierTasks[i];
				if (fourierTask.Done)
				{
					throw new InvalidOperationException("Fourier task should not be done before running");
				}
				if (Ocean.DISABLE_FOURIER_MULTITHREADING)
				{
					fourierTask.Start();
					fourierTask.Run();
					fourierTask.End();
				}
				else
				{
					fourierTask.RunOnStopWaiting = true;
					fourierTask.WaitOn(m_initTask);
					m_scheduler.AddWaiting(fourierTask);
				}
			}
		}
		if (!Ocean.DISABLE_FOURIER_MULTITHREADING)
		{
			m_initTask.NoFinish = true;
			m_scheduler.Run(m_initTask);
		}
	}
}
