using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a generic hash table with customizable compare and key functions.
/// </summary>
/// <typeparam name="Data">The type of data stored in the hash table.</typeparam>
public class CUtlHash<Data>
{
    /// <summary>
    /// Struct representing a hash handle, combining bucket and key indices.
    /// </summary>
     [StructLayout(LayoutKind.Sequential, Pack = 1)]
         
        public  struct UtlHashHandle_t
    {
        public int Value;

        public UtlHashHandle_t(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is UtlHashHandle_t)
                return Value == ((UtlHashHandle_t)obj).Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(UtlHashHandle_t a, UtlHashHandle_t b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(UtlHashHandle_t a, UtlHashHandle_t b)
        {
            return a.Value != b.Value;
        }
    }

    // Delegates for comparison and key functions
    private Func<Data, Data, bool> m_CompareFunc;
    private Func<Data, uint> m_KeyFunc;

    // Buckets: Each bucket contains a list of Data elements
    private List<List<Data>> m_Buckets;

    // Optimization flags
    private bool m_bPowerOfTwo;
    private uint m_ModMask;

    /// <summary>
    /// Initializes a new instance of the CUtlHash class.
    /// </summary>
    /// <param name="bucketCount">Initial number of buckets.</param>
    /// <param name="growCount">Grow size for each bucket (not directly used in C#).</param>
    /// <param name="initCount">Initial capacity for each bucket.</param>
    /// <param name="compareFunc">Function to compare two Data elements.</param>
    /// <param name="keyFunc">Function to generate a key from a Data element.</param>
    public CUtlHash(int bucketCount = 0, int growCount = 0, int initCount = 0,
                   Func<Data, Data, bool> compareFunc = null,
                   Func<Data, uint> keyFunc = null)
    {
        m_CompareFunc = compareFunc;
        m_KeyFunc = keyFunc;

        m_Buckets = new List<List<Data>>(bucketCount);
        for (int i = 0; i < bucketCount; i++)
        {
            var bucket = new List<Data>(initCount);
            // growCount is not directly applicable in C#, List<T> automatically grows
            m_Buckets.Add(bucket);
        }

        // Check if bucket count is a power of two
        m_bPowerOfTwo = IsPowerOfTwo(bucketCount);
        m_ModMask = m_bPowerOfTwo ? (uint)(bucketCount - 1) : 0;
    }

    /// <summary>
    /// Destructor to purge the hash table.
    /// </summary>
    ~CUtlHash()
    {
        Purge();
    }

    /// <summary>
    /// Returns an invalid handle.
    /// </summary>
    /// <returns>An invalid UtlHashHandle_t.</returns>
    public static UtlHashHandle_t InvalidHandle()
    {
        return new UtlHashHandle_t(~0);
    }

    /// <summary>
    /// Checks if a handle is valid.
    /// </summary>
    /// <param name="handle">The handle to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public bool IsValidHandle(UtlHashHandle_t handle)
    {
        int ndxBucket = GetBucketIndex(handle);
        int ndxKeyData = GetKeyDataIndex(handle);

        if (ndxBucket < m_Buckets.Count)
        {
            if (ndxKeyData < m_Buckets[ndxBucket].Count)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the total count of elements in the hash table.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count()
    {
        int count = 0;
        foreach (var bucket in m_Buckets)
        {
            count += bucket.Count;
        }
        return count;
    }

    /// <summary>
    /// Removes all elements from the hash table.
    /// </summary>
    public void Purge()
    {
        foreach (var bucket in m_Buckets)
        {
            bucket.Clear();
        }
    }

    /// <summary>
    /// Inserts an element into the hash table.
    /// </summary>
    /// <param name="src">The data to insert.</param>
    /// <returns>The handle to the inserted or existing element.</returns>
    public UtlHashHandle_t Insert(Data src)
    {
        uint ndxBucket;
        int ndxKeyData;
        if (DoFind(src, out ndxBucket, out ndxKeyData))
        {
            return BuildHandle((int)ndxBucket, ndxKeyData);
        }

        // Insert into the bucket
        m_Buckets[(int)ndxBucket].Add(src);
        ndxKeyData = m_Buckets[(int)ndxBucket].Count - 1;

        return BuildHandle((int)ndxBucket, ndxKeyData);
    }

    /// <summary>
    /// Inserts an element into the hash table with an output indicating if insertion occurred.
    /// </summary>
    /// <param name="src">The data to insert.</param>
    /// <param name="didInsert">Output indicating if a new insertion occurred.</param>
    /// <returns>The handle to the inserted or existing element.</returns>
    public UtlHashHandle_t Insert(Data src, out bool didInsert)
    {
        uint ndxBucket;
        int ndxKeyData;
        if (DoFind(src, out ndxBucket, out ndxKeyData))
        {
            didInsert = false;
            return BuildHandle((int)ndxBucket, ndxKeyData);
        }

        // Insert into the bucket
        m_Buckets[(int)ndxBucket].Add(src);
        ndxKeyData = m_Buckets[(int)ndxBucket].Count - 1;

        didInsert = true;
        return BuildHandle((int)ndxBucket, ndxKeyData);
    }

    /// <summary>
    /// Allocates an entry from the key without initializing the data.
    /// </summary>
    /// <param name="src">The data to allocate.</param>
    /// <returns>The handle to the allocated entry.</returns>
    public UtlHashHandle_t AllocEntryFromKey(Data src)
    {
        uint ndxBucket;
        int ndxKeyData;
        if (DoFind(src, out ndxBucket, out ndxKeyData))
        {
            return BuildHandle((int)ndxBucket, ndxKeyData);
        }

        // Allocate entry without initializing
        m_Buckets[(int)ndxBucket].Add(default(Data));
        ndxKeyData = m_Buckets[(int)ndxBucket].Count - 1;

        return BuildHandle((int)ndxBucket, ndxKeyData);
    }

    /// <summary>
    /// Removes an element from the hash table using its handle.
    /// </summary>
    /// <param name="handle">The handle of the element to remove.</param>
    public void Remove(UtlHashHandle_t handle)
    {
        Debug.Assert(IsValidHandle(handle), "Invalid handle");

        int ndxBucket = GetBucketIndex(handle);
        int ndxKeyData = GetKeyDataIndex(handle);

        if (ndxBucket < m_Buckets.Count)
        {
            var bucket = m_Buckets[ndxBucket];
            if (ndxKeyData < bucket.Count)
            {
                // Fast remove: replace with last element
                int lastIndex = bucket.Count - 1;
                if (ndxKeyData != lastIndex)
                {
                    bucket[ndxKeyData] = bucket[lastIndex];
                }
                bucket.RemoveAt(lastIndex);
            }
        }
    }

    /// <summary>
    /// Removes all elements from the hash table.
    /// </summary>
    public void RemoveAll()
    {
        Purge();
    }

    /// <summary>
    /// Finds an element in the hash table.
    /// </summary>
    /// <param name="src">The data to find.</param>
    /// <returns>The handle to the found element or an invalid handle.</returns>
    public UtlHashHandle_t Find(Data src)
    {
        uint ndxBucket;
        int ndxKeyData;
        if (DoFind(src, out ndxBucket, out ndxKeyData))
        {
            return BuildHandle((int)ndxBucket, ndxKeyData);
        }
        return InvalidHandle();
    }

    /// <summary>
    /// Retrieves an element by its handle.
    /// </summary>
    /// <param name="handle">The handle of the element.</param>
    /// <returns>The data element.</returns>
    public Data Element(UtlHashHandle_t handle)
    {
        int ndxBucket = GetBucketIndex(handle);
        int ndxKeyData = GetKeyDataIndex(handle);

        return m_Buckets[ndxBucket][ndxKeyData];
    }

    /// <summary>
    /// Retrieves an element by its handle (read-only).
    /// </summary>
    /// <param name="handle">The handle of the element.</param>
    /// <returns>The data element.</returns>


    /// <summary>
    /// Indexer to get or set elements by handle.
    /// </summary>
    /// <param name="handle">The handle of the element.</param>
    /// <returns>The data element.</returns>
    public Data this[UtlHashHandle_t handle]
    {
        get
        {
            int ndxBucket = GetBucketIndex(handle);
            int ndxKeyData = GetKeyDataIndex(handle);

            return m_Buckets[ndxBucket][ndxKeyData];
        }
        set
        {
            int ndxBucket = GetBucketIndex(handle);
            int ndxKeyData = GetKeyDataIndex(handle);

            m_Buckets[ndxBucket][ndxKeyData] = value;
        }
    }

    /// <summary>
    /// Gets the first valid handle in the hash table.
    /// </summary>
    /// <returns>The first handle or an invalid handle if empty.</returns>
    public UtlHashHandle_t GetFirstHandle()
    {
        return GetNextHandle(new UtlHashHandle_t(-1));
    }

    /// <summary>
    /// Gets the next valid handle following the given handle.
    /// </summary>
    /// <param name="handle">The current handle.</param>
    /// <returns>The next handle or an invalid handle if none.</returns>
    public UtlHashHandle_t GetNextHandle(UtlHashHandle_t handle)
    {
        int handleValue = handle.Value + 1;

        while (handleValue < 0xFFFFFFFF)
        {
            int bi = GetBucketIndex(new UtlHashHandle_t(handleValue));
            int ki = GetKeyDataIndex(new UtlHashHandle_t(handleValue));

            if (bi >= m_Buckets.Count)
                break;

            if (ki < m_Buckets[bi].Count)
                return BuildHandle(bi, ki);

            handleValue = (bi + 1) << 16; // Move to the next bucket
        }

        return InvalidHandle();
    }

    /// <summary>
    /// Logs the hash table statistics to a file.
    /// </summary>
    /// <param name="filename">The path to the log file.</param>
    public void Log(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            int maxBucketSize = 0;
            int numBucketsEmpty = 0;

            int bucketCount = m_Buckets.Count;
            writer.WriteLine("\n{0} Buckets\n", bucketCount);

            for (int ndxBucket = 0; ndxBucket < bucketCount; ndxBucket++)
            {
                int count = m_Buckets[ndxBucket].Count;

                if (count > maxBucketSize) { maxBucketSize = count; }
                if (count == 0)
                    numBucketsEmpty++;

                writer.WriteLine("Bucket {0}: {1}", ndxBucket, count);
            }

            writer.WriteLine("\nBucketHeads Used: {0}", bucketCount - numBucketsEmpty);
            writer.WriteLine("Max Bucket Size: {0}", maxBucketSize);
        }
    }

    // Protected helper methods

    /// <summary>
    /// Calculates the bucket index from a handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The bucket index.</returns>
    private int GetBucketIndex(UtlHashHandle_t handle)
    {
        return (handle.Value >> 16) & 0x0000FFFF;
    }

    /// <summary>
    /// Calculates the key data index from a handle.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The key data index.</returns>
    private int GetKeyDataIndex(UtlHashHandle_t handle)
    {
        return handle.Value & 0x0000FFFF;
    }

    /// <summary>
    /// Builds a handle from bucket and key data indices.
    /// </summary>
    /// <param name="ndxBucket">The bucket index.</param>
    /// <param name="ndxKeyData">The key data index.</param>
    /// <returns>The constructed handle.</returns>
    private UtlHashHandle_t BuildHandle(int ndxBucket, int ndxKeyData)
    {
        Debug.Assert((ndxBucket >= 0) && (ndxBucket < 65536), "Bucket index out of range");
        Debug.Assert((ndxKeyData >= 0) && (ndxKeyData < 65536), "KeyData index out of range");

        int handle = (ndxBucket << 16) | (ndxKeyData & 0xFFFF);
        return new UtlHashHandle_t(handle);
    }

    /// <summary>
    /// Finds the bucket and index of a given data element.
    /// </summary>
    /// <param name="src">The data to find.</param>
    /// <param name="pBucket">Output bucket index.</param>
    /// <param name="pIndex">Output key data index.</param>
    /// <returns>True if found; otherwise, false.</returns>
    private bool DoFind(Data src, out uint pBucket, out int pIndex)
    {
        // Generate the key
        uint key = m_KeyFunc != null ? m_KeyFunc(src) : 0;

        // Hash to find bucket
        int ndxBucket;
        if (m_bPowerOfTwo)
        {
            ndxBucket = (int)(key & m_ModMask);
            pBucket = (uint)ndxBucket;
        }
        else
        {
            if (m_Buckets.Count == 0)
            {
                pBucket = 0;
                pIndex = -1;
                return false;
            }
            ndxBucket = (int)(key % m_Buckets.Count);
            pBucket = (uint)ndxBucket;
        }

        // Search the bucket
        var bucket = m_Buckets[ndxBucket];
        for (int i = 0; i < bucket.Count; i++)
        {
            if (m_CompareFunc != null)
            {
                if (m_CompareFunc(bucket[i], src))
                {
                    pIndex = i;
                    return true;
                }
            }
            else
            {
                // If no compare function provided, use default equality
                if (EqualityComparer<Data>.Default.Equals(bucket[i], src))
                {
                    pIndex = i;
                    return true;
                }
            }
        }

        pIndex = -1;
        return false;
    }

    /// <summary>
    /// Checks if a number is a power of two.
    /// </summary>
    /// <param name="x">The number to check.</param>
    /// <returns>True if power of two; otherwise, false.</returns>
    private bool IsPowerOfTwo(int x)
    {
        return (x > 0) && ((x & (x - 1)) == 0);
    }
}

/// <summary>
/// Represents a fast hash table optimized for performance with power-of-two bucket counts.
/// </summary>
/// <typeparam name="Data">The type of data stored in the hash table.</typeparam>
/// <typeparam name="HashFuncs">The hashing function class.</typeparam>
public class CUtlHashFast<Data, HashFuncs> where HashFuncs : IHashFunction, new()
{
    /// <summary>
    /// Struct representing a fast hash handle.
    /// </summary>
     [StructLayout(LayoutKind.Sequential, Pack = 1)]
   
        public  struct UtlHashFastHandle_t
    {
        public int Value;

        public UtlHashFastHandle_t(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is UtlHashFastHandle_t)
                return Value == ((UtlHashFastHandle_t)obj).Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(UtlHashFastHandle_t a, UtlHashFastHandle_t b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(UtlHashFastHandle_t a, UtlHashFastHandle_t b)
        {
            return a.Value != b.Value;
        }
    }

    // Internal data structure for hash entries
    private class HashFastData_t
    {
        public uint m_uiKey;
        public Data m_Data;

        public HashFastData_t(uint key, Data data)
        {
            m_uiKey = key;
            m_Data = data;
        }
    }

    private uint m_uiBucketMask;
    private List<UtlHashFastHandle_t> m_aBuckets;
    private List<HashFastData_t> m_aDataPool;
    private HashFuncs m_HashFuncs;

    /// <summary>
    /// Initializes a new instance of the CUtlHashFast class.
    /// </summary>
    public CUtlHashFast()
    {
        m_aBuckets = new List<UtlHashFastHandle_t>();
        m_aDataPool = new List<HashFastData_t>();
        m_HashFuncs = new HashFuncs();
    }

    /// <summary>
    /// Destructor to purge the hash table.
    /// </summary>
    ~CUtlHashFast()
    {
        Purge();
    }

    /// <summary>
    /// Removes all elements from the hash table.
    /// </summary>
    public void Purge()
    {
        m_aBuckets.Clear();
        m_aDataPool.Clear();
    }

    /// <summary>
    /// Returns an invalid handle.
    /// </summary>
    /// <returns>An invalid UtlHashFastHandle_t.</returns>
    public static UtlHashFastHandle_t InvalidHandle()
    {
        return new UtlHashFastHandle_t(~0);
    }

    /// <summary>
    /// Initializes the hash table with a specified number of buckets.
    /// </summary>
    /// <param name="nBucketCount">The number of buckets (must be a power of two).</param>
    /// <returns>True if initialized successfully; otherwise, false.</returns>
    public bool Init(int nBucketCount)
    {
        // Verify the bucket count is power of 2.
        if (!IsPowerOfTwo(nBucketCount))
            return false;

        // Set the bucket size.
        m_aBuckets.Clear();
        for (int i = 0; i < nBucketCount; i++)
        {
            m_aBuckets.Add(InvalidHandle());
        }

        // Set the mod mask.
        m_uiBucketMask = (uint)(nBucketCount - 1);

        return true;
    }

    /// <summary>
    /// Gets the total count of elements in the hash table.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count()
    {
        return m_aDataPool.Count;
    }

    /// <summary>
    /// Inserts an element into the hash table with a key.
    /// </summary>
    /// <param name="uiKey">The key associated with the data.</param>
    /// <param name="data">The data to insert.</param>
    /// <returns>The handle to the inserted or existing element.</returns>
    public UtlHashFastHandle_t Insert(uint uiKey, Data data)
    {
        // Check if the key already exists
        UtlHashFastHandle_t existingHandle = Find(uiKey);
        if (existingHandle != InvalidHandle())
            return existingHandle;

        return FastInsert(uiKey, data);
    }

    /// <summary>
    /// Inserts an element into the hash table with a key without checking for duplicates.
    /// </summary>
    /// <param name="uiKey">The key associated with the data.</param>
    /// <param name="data">The data to insert.</param>
    /// <returns>The handle to the inserted element.</returns>
    public UtlHashFastHandle_t FastInsert(uint uiKey, Data data)
    {
        // Add new data to the pool
        HashFastData_t newData = new HashFastData_t(uiKey, data);
        m_aDataPool.Add(newData);
        int iHashData = m_aDataPool.Count - 1;

        // Link element to the appropriate bucket
        int iBucket = m_HashFuncs.Hash((int)uiKey, (int)m_uiBucketMask);
        UtlHashFastHandle_t currentHead = m_aBuckets[iBucket];
        m_aBuckets[iBucket] = new UtlHashFastHandle_t(iHashData);

        return new UtlHashFastHandle_t(iHashData);
    }

    /// <summary>
    /// Removes an element from the hash table using its handle.
    /// </summary>
    /// <param name="hHash">The handle of the element to remove.</param>
    public void Remove(UtlHashFastHandle_t hHash)
    {
        if (hHash == InvalidHandle())
            return;

        HashFastData_t hashData = m_aDataPool[hHash.Value];
        int iBucket = m_HashFuncs.Hash((int)hashData.m_uiKey, (int)m_uiBucketMask);

        if (m_aBuckets[iBucket].Value == hHash.Value)
        {
            // It is a bucket head.
            m_aBuckets[iBucket] = InvalidHandle();
        }

        // Remove the element from the pool
        m_aDataPool.RemoveAt(hHash.Value);
    }

    /// <summary>
    /// Removes all elements from the hash table.
    /// </summary>
    public void RemoveAll()
    {
        m_aBuckets.Clear();
        m_aDataPool.Clear();
    }

    /// <summary>
    /// Finds an element in the hash table by key.
    /// </summary>
    /// <param name="uiKey">The key to find.</param>
    /// <returns>The handle to the found element or an invalid handle.</returns>
    public UtlHashFastHandle_t Find(uint uiKey)
    {
        int iBucket = m_HashFuncs.Hash((int)uiKey, (int)m_uiBucketMask);
        UtlHashFastHandle_t currentHandle = m_aBuckets[iBucket];

        while (currentHandle != InvalidHandle())
        {
            HashFastData_t hashData = m_aDataPool[currentHandle.Value];
            if (hashData.m_uiKey == uiKey)
                return currentHandle;

            currentHandle = InvalidHandle(); // Simplification: Not maintaining linked list
        }

        return InvalidHandle();
    }

    /// <summary>
    /// Retrieves an element by its handle.
    /// </summary>
    /// <param name="hHash">The handle of the element.</param>
    /// <returns>The data element.</returns>
    public Data Element(UtlHashFastHandle_t hHash)
    {
        return m_aDataPool[hHash.Value].m_Data;
    }

    /// <summary>
    /// Retrieves an element by its handle (read-only).
    /// </summary>
    /// <param name="hHash">The handle of the element.</param>
    /// <returns>The data element.</returns>

    /// <summary>
    /// Indexer to get or set elements by handle.
    /// </summary>
    /// <param name="hHash">The handle of the element.</param>
    /// <returns>The data element.</returns>
    public Data this[UtlHashFastHandle_t hHash]
    {
        get
        {
            return m_aDataPool[hHash.Value].m_Data;
        }
        set
        {
            m_aDataPool[hHash.Value].m_Data = value;
        }
    }

    // Helper methods

    /// <summary>
    /// Checks if a number is a power of two.
    /// </summary>
    /// <param name="x">The number to check.</param>
    /// <returns>True if power of two; otherwise, false.</returns>
    private bool IsPowerOfTwo(int x)
    {
        return (x > 0) && ((x & (x - 1)) == 0);
    }
}

/// <summary>
/// Interface for hash functions used in CUtlHashFast.
/// </summary>
public interface IHashFunction
{
    int Hash(int key, int bucketMask);
}

/// <summary>
/// Default hash function that does not perform any hashing.
/// </summary>
public class CUtlHashFastNoHash : IHashFunction
{
    public int Hash(int key, int bucketMask)
    {
        return key & bucketMask;
    }
}

/// <summary>
/// Generic hash function using conventional integer hashing.
/// </summary>
public class CUtlHashFastGenericHash : IHashFunction
{
    public int Hash(int key, int bucketMask)
    {
        return HashIntConventional(key) & bucketMask;
    }

    private int HashIntConventional(int key)
    {
        // Simple integer hash (can be replaced with a better hash function)
        return key ^ (key >> 16);
    }
}

/// <summary>
/// Represents a fixed-size hash table with a predefined number of buckets.
/// </summary>
/// <typeparam name="Data">The type of data stored in the hash table.</typeparam>
/// <typeparam name="HashFuncs">The hashing function class.</typeparam>
/// <param name="NUM_BUCKETS">The number of buckets (must be a power of two).</param>
public class CUtlHashFixed<Data, HashFuncs> where HashFuncs : IHashFunction, new()
{
    /// <summary>
    /// Struct representing a fixed hash handle.
    /// </summary>
     [StructLayout(LayoutKind.Sequential, Pack = 1)]
  
        public  struct UtlHashFixedHandle_t
    {
        public int Value;

        public UtlHashFixedHandle_t(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is UtlHashFixedHandle_t)
                return Value == ((UtlHashFixedHandle_t)obj).Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(UtlHashFixedHandle_t a, UtlHashFixedHandle_t b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(UtlHashFixedHandle_t a, UtlHashFixedHandle_t b)
        {
            return a.Value != b.Value;
        }
    }

    // Internal data structure for fixed hash entries
    private class HashFixedData_t
    {
        public uint m_uiKey;
        public Data m_Data;

        public HashFixedData_t(uint key, Data data)
        {
            m_uiKey = key;
            m_Data = data;
        }
    }

    private List<HashFixedData_t>[] m_aBuckets;
    private int m_nElements;
    private HashFuncs m_HashFuncs;

    /// <summary>
    /// Initializes a new instance of the CUtlHashFixed class.
    /// </summary>
    public CUtlHashFixed()
    {
        m_nElements = 0;
        m_HashFuncs = new HashFuncs();
    }

    /// <summary>
    /// Destructor to purge the hash table.
    /// </summary>
    ~CUtlHashFixed()
    {
        Purge();
    }

    /// <summary>
    /// Removes all elements from the hash table.
    /// </summary>
    public void Purge()
    {
        RemoveAll();
    }

    /// <summary>
    /// Returns an invalid handle.
    /// </summary>
    /// <returns>An invalid UtlHashFixedHandle_t.</returns>
    public static UtlHashFixedHandle_t InvalidHandle()
    {
        return new UtlHashFixedHandle_t(~0);
    }

    /// <summary>
    /// Gets the total count of elements in the hash table.
    /// </summary>
    /// <returns>The number of elements.</returns>
    public int Count()
    {
        return m_nElements;
    }

    /// <summary>
    /// Inserts an element into the hash table with a key.
    /// </summary>
    /// <param name="uiKey">The key associated with the data.</param>
    /// <param name="data">The data to insert.</param>
    /// <returns>The handle to the inserted or existing element.</returns>
    public UtlHashFixedHandle_t Insert(uint uiKey, Data data)
    {
        // Check if the key already exists
        UtlHashFixedHandle_t existingHandle = Find(uiKey);
        if (existingHandle != InvalidHandle())
            return existingHandle;

        return FastInsert(uiKey, data);
    }

    /// <summary>
    /// Inserts an element into the hash table with a key without checking for duplicates.
    /// </summary>
    /// <param name="uiKey">The key associated with the data.</param>
    /// <param name="data">The data to insert.</param>
    /// <returns>The handle to the inserted element.</returns>
    public UtlHashFixedHandle_t FastInsert(uint uiKey, Data data)
    {
        int iBucket = m_HashFuncs.Hash((int)uiKey, (int)(NUM_BUCKETS - 1));
        var newData = new HashFixedData_t(uiKey, data);

        if (m_aBuckets[iBucket] == null)
            m_aBuckets[iBucket] = new List<HashFixedData_t>();

        m_aBuckets[iBucket].Add(newData);
        m_nElements++;

        // Return handle as the index within the bucket
        return new UtlHashFixedHandle_t(m_aBuckets[iBucket].Count - 1);
    }

    /// <summary>
    /// Removes an element from the hash table using its handle.
    /// </summary>
    /// <param name="hHash">The handle of the element to remove.</param>
    public void Remove(UtlHashFixedHandle_t hHash)
    {
        if (hHash == InvalidHandle())
            return;

        foreach (var bucket in m_aBuckets)
        {
            if (bucket != null && hHash.Value < bucket.Count)
            {
                bucket.RemoveAt(hHash.Value);
                m_nElements--;
                break;
            }
        }
    }

    /// <summary>
    /// Removes all elements from the hash table.
    /// </summary>
    public void RemoveAll()
    {
        for (int i = 0; i < NUM_BUCKETS; i++)
        {
            if (m_aBuckets[i] != null)
                m_aBuckets[i].Clear();
        }
        m_nElements = 0;
    }

    /// <summary>
    /// Finds an element in the hash table by key.
    /// </summary>
    /// <param name="uiKey">The key to find.</param>
    /// <returns>The handle to the found element or an invalid handle.</returns>
    public UtlHashFixedHandle_t Find(uint uiKey)
    {
        int iBucket = m_HashFuncs.Hash((int)uiKey, (int)(NUM_BUCKETS - 1));
        var bucket = m_aBuckets[iBucket];

        if (bucket == null)
            return InvalidHandle();

        for (int i = 0; i < bucket.Count; i++)
        {
            if (bucket[i].m_uiKey == uiKey)
                return new UtlHashFixedHandle_t(i);
        }

        return InvalidHandle();
    }

    /// <summary>
    /// Retrieves an element by its handle.
    /// </summary>
    /// <param name="hHash">The handle of the element.</param>
    /// <returns>The data element.</returns>
    public Data Element(UtlHashFixedHandle_t hHash)
    {
        foreach (var bucket in m_aBuckets)
        {
            if (bucket != null && hHash.Value < bucket.Count)
            {
                return bucket[hHash.Value].m_Data;
            }
        }
        throw new ArgumentException("Invalid handle");
    }

    /// <summary>
    /// Retrieves an element by its handle (read-only).
    /// </summary>
    /// <param name="hHash">The handle of the element.</param>
    /// <returns>The data element.</returns>
   

    /// <summary>
    /// Indexer to get or set elements by handle.
    /// </summary>
    /// <param name="hHash">The handle of the element.</param>
    /// <returns>The data element.</returns>
    public Data this[UtlHashFixedHandle_t hHash]
    {
        get
        {
            foreach (var bucket in m_aBuckets)
            {
                if (bucket != null && hHash.Value < bucket.Count)
                {
                    return bucket[hHash.Value].m_Data;
                }
            }
            throw new ArgumentException("Invalid handle");
        }
        set
        {
            foreach (var bucket in m_aBuckets)
            {
                if (bucket != null && hHash.Value < bucket.Count)
                {
                    bucket[hHash.Value].m_Data = value;
                    return;
                }
            }
            throw new ArgumentException("Invalid handle");
        }
    }

    // Constants
    private const int NUM_BUCKETS = 65536; // Example fixed number of buckets

    /// <summary>
    /// Initializes the fixed hash table with the specified number of buckets.
    /// </summary>
    public void Initialize()
    {
        m_aBuckets = new List<HashFixedData_t>[NUM_BUCKETS];
        for (int i = 0; i < NUM_BUCKETS; i++)
        {
            m_aBuckets[i] = new List<HashFixedData_t>();
        }
    }
}

/// <summary>
/// <summary>
/// A generic fixed hash function with a specified number of buckets.
/// </summary>
public class CUtlHashFixedGenericHash : IHashFunction
{
    public int Hash(int key, int bucketMask)
    {
        int hash = HashIntConventional(key);
        if (NUM_BUCKETS <= ushort.MaxValue)
        {
            hash ^= (hash >> 16);
        }
        if (NUM_BUCKETS <= byte.MaxValue)
        {
            hash ^= (hash >> 8);
        }
        return hash & bucketMask;
    }

    private int HashIntConventional(int key)
    {
        // Replace with a better hash function if needed
        return key ^ (key >> 16);
    }

    private const int NUM_BUCKETS = 65536; // Example fixed number of buckets
}
