
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class CUtlSymbol
{
    public static CUtlSymbolTable s_pSymbolTable = null;
    private static bool s_bAllowStaticSymbolTable = true;
    private UtlSymId_t m_Id;

    public CUtlSymbol()
    {
        m_Id = UtlSymId_t.Invalid;
    }

    public CUtlSymbol(string pStr)
    {
        m_Id = CurrTable().AddString(pStr);
    }

    public string String()
    {
        return CurrTable().String(m_Id);
    }

    public static void DisableStaticSymbolTable()
    {
        s_bAllowStaticSymbolTable = false;
    }

    public static CUtlSymbolTable CurrTable()
    {
        Initialize();
        return s_pSymbolTable;
    }

    public static void Initialize()
    {
        if (!s_bAllowStaticSymbolTable)
        {
            throw new InvalidOperationException("Static symbol table is disallowed.");
        }

        if (s_pSymbolTable == null)
        {
            s_pSymbolTable = new CUtlSymbolTable();
        }
    }

    public static bool operator ==(CUtlSymbol symbol, string pStr)
    {
        if (symbol.m_Id == UtlSymId_t.Invalid)
        {
            return false;
        }
        return symbol.String() == pStr;
    }

    public static bool operator !=(CUtlSymbol symbol, string pStr)
    {
        return !(symbol == pStr);
    }

    public override bool Equals(object obj)
    {
        if (obj is CUtlSymbol)
        {
            return m_Id == ((CUtlSymbol)obj).m_Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return m_Id.GetHashCode();
    }
}

public class CUtlSymbolTable
{
    private List<string> m_StringPools = new List<string>();
    private Dictionary<string, UtlSymId_t> m_Lookup = new Dictionary<string, UtlSymId_t>();
    private ReaderWriterLockSlim m_Lock = new ReaderWriterLockSlim();
    private bool m_bInsensitive;

    public CUtlSymbolTable(bool caseInsensitive = false)
    {
        m_bInsensitive = caseInsensitive;
    }

    public UtlSymId_t AddString(string pString)
    {
        if (string.IsNullOrEmpty(pString))
        {
            return UtlSymId_t.Invalid;
        }

        string key = m_bInsensitive ? pString.ToLower() : pString;

        m_Lock.EnterUpgradeableReadLock();
        try
        {
            if (m_Lookup.TryGetValue(key, out UtlSymId_t existingId))
            {
                return existingId;
            }

            m_Lock.EnterWriteLock();
            try
            {
                m_StringPools.Add(pString);
                UtlSymId_t newId = new UtlSymId_t(m_StringPools.Count - 1);
                m_Lookup[key] = newId;
                return newId;
            }
            finally
            {
                m_Lock.ExitWriteLock();
            }
        }
        finally
        {
            m_Lock.ExitUpgradeableReadLock();
        }
    }

    public string String(UtlSymId_t id)
    {
        if (!id.IsValid() || id.Index >= m_StringPools.Count)
        {
            return "";
        }

        return m_StringPools[id.Index];
    }

    public void RemoveAll()
    {
        m_Lock.EnterWriteLock();
        try
        {
            m_Lookup.Clear();
            m_StringPools.Clear();
        }
        finally
        {
            m_Lock.ExitWriteLock();
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct UtlSymId_t
{
    public int Index;

    public static UtlSymId_t Invalid => new UtlSymId_t { Index = -1 };

    public bool IsValid() => Index != -1;

    public UtlSymId_t(int index)
    {
        Index = index;
    }

    public override bool Equals(object obj)
    {
        if (obj is UtlSymId_t)
        {
            return Index == ((UtlSymId_t)obj).Index;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    public static bool operator ==(UtlSymId_t a, UtlSymId_t b)
    {
        return a.Index == b.Index;
    }

    public static bool operator !=(UtlSymId_t a, UtlSymId_t b)
    {
        return !(a == b);
    }
}

public class CCleanupUtlSymbolTable
{
    ~CCleanupUtlSymbolTable()
    {
        CUtlSymbol.s_pSymbolTable?.RemoveAll();
        CUtlSymbol.s_pSymbolTable = null;
    }
}

// Example usage in Unity
public class TestUtlSymbol : MonoBehaviour
{
    void Start()
    {
        CUtlSymbol.Initialize();
        CUtlSymbol symbol = new CUtlSymbol("example");
        Debug.Log(symbol.String());
    }
}
