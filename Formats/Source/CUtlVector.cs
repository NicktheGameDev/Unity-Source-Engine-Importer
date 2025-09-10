
using System;
using System.Collections.Generic;


// Helper class to handle memory management and utilities
public class CUtlMemory<T>
{
    private T[] _elements;
    private int _growSize;
    private int _numAllocated;

    public CUtlMemory(int growSize = 0, int initSize = 0)
    {
        _growSize = growSize > 0 ? growSize : 4;
        _elements = new T[initSize];
        _numAllocated = initSize;
    }

    public void Grow(int num)
    {
        int newSize = _numAllocated + num + _growSize;
        Array.Resize(ref _elements, newSize);
        _numAllocated = newSize;
    }

    public void EnsureCapacity(int num)
    {
        if (num > _numAllocated)
        {
            Grow(num - _numAllocated);
        }
    }

    public int NumAllocated => _numAllocated;

    public T[] Base() => _elements;

    public T this[int index]
    {
        get => _elements[index];
        set => _elements[index] = value;
    }

    public void Purge()
    {
        _elements = new T[0];
        _numAllocated = 0;
    }
}

public class CUtlVector<T>
{
    private CUtlMemory<T> _memory;
    private int _size;

    public CUtlVector(int growSize = 0, int initSize = 0)
    {
        _memory = new CUtlMemory<T>(growSize, initSize);
        _size = 0;
    }

    public int Count => _size;
    public bool IsEmpty => _size == 0;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException();
            return _memory[index];
        }
        set
        {
            if (index < 0 || index >= _size) throw new IndexOutOfRangeException();
            _memory[index] = value;
        }
    }

    public T Element(int index) => this[index];
    public T Head() => this[0];
    public T Tail() => this[_size - 1];

    public void AddToTail(T element)
    {
        EnsureCapacity(_size + 1);
        _memory[_size++] = element;
    }

    public void InsertBefore(int index, T element)
    {
        if (index < 0 || index > _size) throw new IndexOutOfRangeException();
        EnsureCapacity(_size + 1);
        ShiftElementsRight(index);
        _memory[index] = element;
        _size++;
    }

    public void Remove(int index)
    {
        if (index < 0 || index >= _size) throw new IndexOutOfRangeException();
        ShiftElementsLeft(index);
        _size--;
    }

    public void RemoveAll()
    {
        _size = 0;
    }

    public void EnsureCapacity(int num)
    {
        _memory.EnsureCapacity(num);
    }

    private void ShiftElementsRight(int index)
    {
        for (int i = _size; i > index; i--)
        {
            _memory[i] = _memory[i - 1];
        }
    }

    private void ShiftElementsLeft(int index)
    {
        for (int i = index; i < _size - 1; i++)
        {
            _memory[i] = _memory[i + 1];
        }
    }

    public void Sort(Comparison<T> comparison)
    {
        Array.Sort(_memory.Base(), 0, _size, Comparer<T>.Create(comparison));
    }

    public void CopyArray(T[] array)
    {
        _size = array.Length;
        EnsureCapacity(_size);
        Array.Copy(array, _memory.Base(), _size);
    }

    public int Find(T element)
    {
        for (int i = 0; i < _size; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_memory[i], element))
            {
                return i;
            }
        }
        return -1;
    }

    public bool HasElement(T element)
    {
        return Find(element) >= 0;
    }

    public void FastRemove(int index)
    {
        if (index < 0 || index >= _size) throw new IndexOutOfRangeException();
        _memory[index] = _memory[_size - 1];
        _size--;
    }

    public void RemoveMultiple(int index, int num)
    {
        if (index < 0 || index + num > _size) throw new IndexOutOfRangeException();
        for (int i = index; i < index + num; i++)
        {
            Remove(i);
        }
    }

    public void Purge()
    {
        RemoveAll();
        _memory.Purge();
    }

    public void PurgeAndDeleteElements()
    {
        for (int i = 0; i < _size; i++)
        {
            (_memory[i] as IDisposable)?.Dispose();
        }
        Purge();
    }

    public void Compact()
    {
        _memory.Purge();
        EnsureCapacity(_size);
    }

    internal int Count_()
    {
        throw new NotImplementedException();
    }
}



