using System;
using System.Collections;
using System.Collections.Generic;
namespace Utilities.Collections
{
    public class OrderedSet<T>:ISet<T>,IReadOnlyList<T>
    {
        readonly List<T> items=new();
        readonly Dictionary<T,int> map=new();
        public int Count=>items.Count;
        public bool IsReadOnly=>false;
        public T this[int i]=>items[i];
        public bool Add(T item){
            if(map.ContainsKey(item)) return false;
            map[item]=items.Count; items.Add(item); return true;
        }
        void ICollection<T>.Add(T item)=>Add(item);
        public void Clear(){items.Clear();map.Clear();}
        public bool Contains(T item)=>map.ContainsKey(item);
        public void CopyTo(T[] array,int arrayIndex)=>items.CopyTo(array,arrayIndex);
        public void ExceptWith(IEnumerable<T> other){foreach(var t in other) Remove(t);}
        public IEnumerator<T> GetEnumerator()=>items.GetEnumerator();
        public void IntersectWith(IEnumerable<T> other){
            var set=new HashSet<T>(other);
            items.RemoveAll(x=>!set.Contains(x)); map.Clear(); for(int i=0;i<items.Count;i++) map[items[i]]=i;
        }
        public bool IsProperSubsetOf(IEnumerable<T> other)=>new HashSet<T>(items).IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other)=>new HashSet<T>(items).IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other)=>new HashSet<T>(items).IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other)=>new HashSet<T>(items).IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other)=>new HashSet<T>(items).Overlaps(other);
        public bool SetEquals(IEnumerable<T> other)=>new HashSet<T>(items).SetEquals(other);
        public bool Remove(T item){
            if(!map.TryGetValue(item,out var idx)) return false;
            items.RemoveAt(idx); map.Remove(item);
            for(int i=idx;i<items.Count;i++) map[items[i]]=i;
            return true;
        }
        public void SymmetricExceptWith(IEnumerable<T> other){
            foreach(var t in other){
                if(!Remove(t)) Add(t);
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }
}
