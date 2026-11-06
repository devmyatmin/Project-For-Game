using System;
using UnityEngine;

namespace Funlabs;

public abstract class ZenoSync<T> : IBaseSync<T> where T : IEquatable<T>
{
	public float Factor = 15f;

	public T Target;

	public T Current;

	internal abstract T Lerp(T a, T b, float t);

	public virtual void Set(T target)
	{
		Target = target;
	}

	public void SetStart(T start)
	{
		Current = start;
	}

	public virtual T Get()
	{
		return Current = Lerp(Current, Target, Time.unscaledDeltaTime * Factor);
	}
}
