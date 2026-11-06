using System;

namespace Funlabs;

public interface IBaseSync<T> where T : IEquatable<T>
{
	void Set(T target);

	void SetStart(T start);

	T Get();
}
