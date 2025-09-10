using System;
using System.Collections.Generic;

using System.Linq;


public class UtlHash<Data, C, K>
    where C : IEqualityComparer<Data>
   
{
    private Dictionary<uint, List<Data>> buckets;
    private C comparer;
    private K keyFunc;

    public UtlHash(int bucketCount = 16, C compareFunc = default, K keyFunc = default)
    {
        buckets = new Dictionary<uint, List<Data>>(bucketCount);
        comparer = compareFunc;
        this.keyFunc = keyFunc;
    }

    public void Purge()
    {
        buckets.Clear();
    }

    public int Count => GetCount();

    private int GetCount()
    {
        int count = 0;
        foreach (var bucket in buckets.Values)
        {
            count += bucket.Count;
        }
        return count;
    }

    public void Insert(Data item)
    {
        uint key = 0;
        if (!buckets.TryGetValue(key, out List<Data> list))
        {
            list = new List<Data>();
            buckets.Add(key, list);
        }
        if (!list.Contains(item, comparer))
        {
            list.Add(item);
        }
    }

    public bool Remove(Data item)
    {
        uint key = 0;
        if (buckets.TryGetValue(key, out List<Data> list))
        {
            return list.Remove(item);
        }
        return false;
    }

    public Data Find(Data item)
    {
        uint key = 0;
        if (buckets.TryGetValue(key, out List<Data> list))
        {
            int idx = list.FindIndex(x => comparer.Equals(x, item));
            if (idx != -1)
            {
                return list[idx];
            }
        }
        return default;
    }

    public interface IKeyFunc<in T, out TKey>
    {
        TKey GetKey(T item);
    }
}
