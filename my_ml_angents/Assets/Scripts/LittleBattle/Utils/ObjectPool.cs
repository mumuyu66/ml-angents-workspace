#if UNITY_EDITOR
#define OBJECT_POOL_STATISTICS
#define OBJECT_POOL_DEBUG
#endif

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class ObjectPool<T> where T : class, new()
{
    public static ObjectPool<T> Shared = new ObjectPool<T>();


    public string name { get; private set; }
    private Func<T> factory;
    private Action<T> onClaim;
    private Action<T> onRelease;
    private Stack<T> stack = new Stack<T>();
    private int _capacity = -1; // -1 means no limitation

    public int capacity {
        get { 
            return _capacity; 
        }
        set {
            _capacity = value;
            Clamp(value);
        }
    }

#if OBJECT_POOL_DEBUG
    private HashSet<T> hashset = new HashSet<T>();
#endif

#if OBJECT_POOL_STATISTICS
    public int peakReserved{ get; private set; } // 池中曾经缓存的最大数量
    public int totalClaimed { get; private set; } // 总的 claim 数量
    public int totalInstantiated { get; private set; } // 总的 new 数量
    public int totalReleased { get; private set; } // 总的 release 数量
    public int totalDiscarded { get; private set; } // 由于超出容量限制而被丢弃的实例数量
#endif

    public int reservedCount => stack.Count; // 池中当前缓存数量


    public ObjectPool(string poolName = "", int capacity = -1,  Func<T> factory = null, Action<T> onClaim = null, Action<T> onRelease = null)
    {
        if (string.IsNullOrEmpty(poolName)) {
            this.name = $"ObjectPool<{typeof(T).Name}>";
        } else {
            this.name = poolName;
        }
        this.factory = factory;
        this.onClaim = onClaim;
        this.onRelease = onRelease;
        this._capacity = capacity;
    }

    public T Claim()
    {
        T inst;
        if (stack.Count > 0) {
            inst = stack.Pop();
#if OBJECT_POOL_DEBUG
            hashset.Remove(inst);
#endif
        } else {
            inst = factory == null ? new T() : factory();
#if OBJECT_POOL_STATISTICS
            totalInstantiated++;
#endif
        }
#if OBJECT_POOL_STATISTICS
        totalClaimed++;
#endif
        if (onClaim != null) {
            onClaim(inst);
        }
        return inst;
    }

    public void Release(T inst)
    {
#if OBJECT_POOL_DEBUG
        if (hashset.Contains(inst)) {
            Debug.LogError($"ObjectPool \"{name}\": the object you want to release has been already released.");
            return;
        }
#endif

#if OBJECT_POOL_STATISTICS
        totalReleased++;
#endif
        if (_capacity >= 0 && stack.Count >= _capacity) {
            if (onRelease != null) {
                onRelease(inst);
            }
#if OBJECT_POOL_STATISTICS
            totalDiscarded++;
#endif
            return;
        }
#if OBJECT_POOL_DEBUG
        hashset.Add(inst);
#endif
        if (onRelease != null) {
            onRelease(inst);
        }
        stack.Push(inst);
#if OBJECT_POOL_STATISTICS
        peakReserved = Mathf.Max(peakReserved, stack.Count);
#endif
    }

    public void Warmup(int count)
    {
        if (_capacity >= 0) {
            count = Mathf.Min(count, _capacity);
        }
        if (count <= stack.Count) {
            return;
        }
        for (var i = stack.Count; i < count; i++) {
            var inst = factory == null ? new T() : factory();
            stack.Push(inst);
#if OBJECT_POOL_DEBUG
            hashset.Add(inst);
#endif
#if OBJECT_POOL_STATISTICS
            totalInstantiated++;
#endif
        }
#if OBJECT_POOL_STATISTICS
        peakReserved = Mathf.Max(peakReserved, stack.Count);
#endif
    }

    public void Clamp(int count)
    {
        if (count < 0 || stack.Count <= count) {
            return;
        }
        for (var i = stack.Count; i > count; i--) {
            T inst = stack.Pop();
#if OBJECT_POOL_DEBUG
            hashset.Remove(inst);
#endif
#if OBJECT_POOL_STATISTICS
            totalDiscarded++;
#endif
        }
    }

#if OBJECT_POOL_STATISTICS
    public void ResetStatistics()
    {
        peakReserved = 0;
        totalClaimed = 0;
        totalInstantiated = 0;
        totalReleased = 0;
        totalDiscarded = 0;
    }

    public string GetStatisticsString()
    {
        var sb = new StringBuilder();
        sb.Append($"----- {name} -----\n");
        sb.Append($"peakReserved:        {peakReserved}\n");
        sb.Append($"totalClaimed:        {totalClaimed}\n");
        sb.Append($"totalInstantiated:   {totalInstantiated}\n");
        sb.Append($"totalReleased:       {totalReleased}\n");
        sb.Append($"totalDiscarded:      {totalDiscarded}\n");
        return sb.ToString();
    }
#endif
}



public interface IRecyclable
{
    void OnClaim();
    void OnRelease();
}



public class RecyclablePool<T> where T : class, IRecyclable, new()
{
    public static ObjectPool<T> Shared = new ObjectPool<T>(onClaim: t => t.OnClaim(), onRelease: t => t.OnRelease());
}



public class ListPool<T>
{
    public static ObjectPool<List<T>> Shared = new ObjectPool<List<T>>(onRelease: list => list.Clear());
    public static ObjectPool<List<T>> Large = new ObjectPool<List<T>>(factory: () => new List<T>(2000), onRelease: list => list.Clear(), capacity: 8);
    public static List<T> Get()
    {
        return Shared.Claim();
    }

    public static void Release(List<T> toRelease)
    {
        Shared.Release(toRelease);
    }
}