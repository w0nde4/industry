using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    private readonly Queue<ResourceInstance> _pool = new Queue<ResourceInstance>();
    private readonly HashSet<ResourceInstance> _activeResources = new HashSet<ResourceInstance>();
    
    private Transform _poolRoot;
    private GameObject _resourcePrefab;
    
    private const int INITIAL_POOL_SIZE = 50;
    private const int MAX_POOL_SIZE = 500;
    
    [Title("Debug Info")]
    [ShowInInspector, ReadOnly]
    public int ActiveCount => _activeResources.Count;
    
    [ShowInInspector, ReadOnly]
    public int PooledCount => _pool.Count;
    
    [ShowInInspector, ReadOnly]
    public int TotalCount => ActiveCount + PooledCount;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Debug.Log("[ResourcePool] Static reset");
    }
    
    public async UniTask Initialize()
    {
        var existingPool = GameObject.Find("ResourcePool_Global");
        if (existingPool != null)
        {
            DestroyImmediate(existingPool);
        }
        
        var poolObj = new GameObject("ResourcePool_Global");
        _poolRoot = poolObj.transform;
        
        _resourcePrefab = new GameObject("ResourcePrefab");
        _resourcePrefab.AddComponent<ResourceInstance>();
        _resourcePrefab.SetActive(false);
        
        await PrewarmPool(INITIAL_POOL_SIZE);
    }
    
    private async UniTask PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var instance = CreateNewInstance();
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_poolRoot);
            _pool.Enqueue(instance);
            
            if(i%10 == 0) await UniTask.Yield();
        }
    }
    
    private ResourceInstance CreateNewInstance()
    {
        var obj = Instantiate(_resourcePrefab, _poolRoot);
        var instance = obj.GetComponent<ResourceInstance>();
        
        if (instance == null)
        {
            instance = obj.AddComponent<ResourceInstance>();
        }
        
        return instance;
    }
    
    public ResourceInstance Get(ResourceData data, int amount = 1)
    {
        ResourceInstance instance = null;
        
        while (_pool.Count > 0)
        {
            instance = _pool.Dequeue();
            
            if (instance != null)
            {
                break;
            }
            
            instance = null;
        }
        
        if (instance == null)
        {
            if (_activeResources.Count + _pool.Count >= MAX_POOL_SIZE)
            {
                Debug.LogWarning($"[ResourcePool] Reached MAX_POOL_SIZE ({MAX_POOL_SIZE}). Consider increasing it.");
            }
        
            instance = CreateNewInstance();
        }
        
        instance.gameObject.SetActive(true);
        instance.transform.SetParent(null);
        instance.Initialize(data, amount);
        
        _activeResources.Add(instance);
        
        return instance;
    }
    
    public void Return(ResourceInstance instance)
    {
        if (instance == null) return;
        
        if (!_activeResources.Contains(instance))
        {
            return;
        }
        
        _activeResources.Remove(instance);
        
        instance.ResetForPool();
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(_poolRoot);
        
        if (_pool.Count < MAX_POOL_SIZE)
        {
            _pool.Enqueue(instance);
        }
        else
        {
            DestroyImmediate(instance.gameObject);
        }
    }
    
    public void ClearAll()
    {
        var temp = new List<ResourceInstance>(_activeResources);
        
        foreach (var instance in temp)
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }
        _activeResources.Clear();
        
        while (_pool.Count > 0)
        {
            var instance = _pool.Dequeue();
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }
        
        if (_poolRoot != null)
        {
            Destroy(_poolRoot.gameObject);
        }
        
        if (_resourcePrefab != null)
        {
            Destroy(_resourcePrefab);
        }
    }
    
    private void OnDestroy()
    {
        ClearAll();
    }
    
    public int GetActiveCount() => _activeResources.Count;
    public int GetPooledCount() => _pool.Count;
}