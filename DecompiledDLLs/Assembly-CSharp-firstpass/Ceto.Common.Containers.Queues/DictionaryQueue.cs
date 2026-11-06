using System.Collections.Generic;

namespace Ceto.Common.Containers.Queues;

public class DictionaryQueue<KEY, VALUE>
{
	private Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>> m_dictionary;

	private LinkedList<KeyValuePair<KEY, VALUE>> m_list;

	public VALUE this[KEY key]
	{
		get
		{
			return m_dictionary[key].Value.Value;
		}
		set
		{
			Replace(key, value);
		}
	}

	public int Count => m_dictionary.Count;

	public bool IsEmpty => m_dictionary.Count == 0;

	public DictionaryQueue()
	{
		m_dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>>();
		m_list = new LinkedList<KeyValuePair<KEY, VALUE>>();
	}

	public DictionaryQueue(IEqualityComparer<KEY> comparer)
	{
		m_dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>>(comparer);
		m_list = new LinkedList<KeyValuePair<KEY, VALUE>>();
	}

	public IEnumerator<VALUE> GetEnumerator()
	{
		LinkedList<KeyValuePair<KEY, VALUE>>.Enumerator e = m_list.GetEnumerator();
		while (e.MoveNext())
		{
			yield return e.Current.Value;
		}
	}

	public bool ContainsKey(KEY key)
	{
		return m_dictionary.ContainsKey(key);
	}

	public void Replace(KEY key, VALUE val)
	{
		m_dictionary[key].Value = new KeyValuePair<KEY, VALUE>(key, val);
	}

	public void AddFirst(KEY key, VALUE val)
	{
		m_dictionary.Add(key, m_list.AddFirst(new KeyValuePair<KEY, VALUE>(key, val)));
	}

	public void AddLast(KEY key, VALUE val)
	{
		m_dictionary.Add(key, m_list.AddLast(new KeyValuePair<KEY, VALUE>(key, val)));
	}

	public KeyValuePair<KEY, VALUE> First()
	{
		return m_list.First.Value;
	}

	public KeyValuePair<KEY, VALUE> Last()
	{
		return m_list.Last.Value;
	}

	public VALUE RemoveFirst()
	{
		LinkedListNode<KeyValuePair<KEY, VALUE>> first = m_list.First;
		m_list.RemoveFirst();
		m_dictionary.Remove(first.Value.Key);
		return first.Value.Value;
	}

	public VALUE RemoveLast()
	{
		LinkedListNode<KeyValuePair<KEY, VALUE>> last = m_list.Last;
		m_list.RemoveLast();
		m_dictionary.Remove(last.Value.Key);
		return last.Value.Value;
	}

	public VALUE Remove(KEY key)
	{
		LinkedListNode<KeyValuePair<KEY, VALUE>> linkedListNode = m_dictionary[key];
		m_dictionary.Remove(key);
		m_list.Remove(linkedListNode);
		return linkedListNode.Value.Value;
	}

	public void Clear()
	{
		m_dictionary.Clear();
		m_list.Clear();
	}
}
