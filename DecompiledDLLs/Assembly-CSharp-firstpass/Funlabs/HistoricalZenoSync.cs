using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funlabs;

public abstract class HistoricalZenoSync<T> : ZenoSync<T> where T : IEquatable<T>
{
	protected struct Key(T value, float time)
	{
		public readonly T Value = value;

		public readonly float Time = time;
	}

	protected readonly List<Key> Keys = new List<Key>();

	public float CloseEnough = 0.2f;

	public int MaxSize = 5;

	internal abstract float Distance(T a, T b);

	public override void Set(T target)
	{
		if (target.Equals(Target))
		{
			return;
		}
		Target = target;
		if (Keys.Count == 0 || !(Time.unscaledTime <= Keys[0].Time))
		{
			Keys.Add(new Key(target, Time.unscaledTime));
			Keys.Sort((Key a, Key b) =>
			{
				float time = a.Time;
				return time.CompareTo(b.Time);
			});
			if (Keys.Count == MaxSize)
			{
				Keys.RemoveAt(0);
			}
		}
	}

	public override T Get()
	{
		if (Keys.Count == 0)
		{
			return Current;
		}
		Current = Lerp(Current, Keys[0].Value, Factor * Time.unscaledDeltaTime);
		if (Distance(Current, Keys[0].Value) <= CloseEnough)
		{
			Keys.RemoveAt(0);
		}
		return Current;
	}

	public override string ToString()
	{
		return base.ToString() + "[" + Keys.Count + "]";
	}
}
