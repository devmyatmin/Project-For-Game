using System.Collections.Generic;

namespace Ceto.Common.Containers.Queues;

public class SetQueue<VALUE>
{
	private Dictionary<VALUE, LinkedListNode<VALUE>> m_dictionary;

	private LinkedList<VALUE> m_list;

	public VALUE this[VALUE key]
	{
		get
		{
			return m_dictionary[key].Value;
		}
		set
		{
			Replace(key, value);
		}
	}

	public int Count => m_dictionary.Count;

	public bool IsEmpty => m_dictionary.Count == 0;

	public SetQueue()
	{
		m_dictionary = new Dictionary<VALUE, LinkedListNode<VALUE>>();
		m_list = new LinkedList<VALUE>();
	}

	public SetQueue(IEqualityComparer<VALUE> comparer)
	{
		m_dictionary = new Dictionary<VALUE, LinkedListNode<VALUE>>(comparer);
		m_list = new LinkedList<VALUE>();
	}

	public IEnumerator<VALUE> GetEnumerator()
	{
		LinkedList<VALUE>.Enumerator e = m_list.GetEnumerator();
		while (e.MoveNext())
		{
			yield return e.Current;
		}
	}

	public bool Contains(VALUE val)
	{
		return m_dictionary.ContainsKey(val);
	}

	public void Replace(VALUE key, VALUE val)
	{
		LinkedListNode<VALUE> linkedListNode = m_dictionary[key];
		linkedListNode.Value = val;
		m_dictionary.Remove(key);
		m_dictionary.Add(val, linkedListNode);
	}

	public void AddFirst(VALUE val)
	{
		m_dictionary.Add(val, m_list.AddFirst(val));
	}

	public void AddLast(VALUE val)
	{
		m_dictionary.Add(val, m_list.AddLast(val));
	}

	public VALUE First()
	{
		return m_list.First.Value;
	}

	public VALUE Last()
	{
		return m_list.Last.Value;
	}

	public VALUE RemoveFirst()
	{
		LinkedListNode<VALUE> first = m_list.First;
		m_list.RemoveFirst();
		m_dictionary.Remove(first.Value);
		return first.Value;
	}

	public VALUE RemoveLast()
	{
		LinkedListNode<VALUE> last = m_list.Last;
		m_list.RemoveLast();
		m_dictionary.Remove(last.Value);
		return last.Value;
	}

	public void Remove(VALUE val)
	{
		LinkedListNode<VALUE> node = m_dictionary[val];
		m_dictionary.Remove(val);
		m_list.Remove(node);
	}

	public void Clear()
	{
		m_dictionary.Clear();
		m_list.Clear();
	}
}
