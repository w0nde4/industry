using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private ResourcePool _pool;
    private readonly List<ResourceInstance> _allResources = new List<ResourceInstance>();
    
    public event Action<ResourceInstance, ResourceData, int> OnResourceCreated;
    public event Action<ResourceInstance> OnResourceDestroyed;
    
    [Title("Debug Info")]
    [ShowInInspector, ReadOnly]
    private int TotalResourcesCount => _allResources.Count;
    
    public IReadOnlyList<ResourceInstance> AllResources => _allResources;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Debug.Log("[ResourceManager] Static reset");
    }
    
    private void Awake()
    {
        _pool = gameObject.AddComponent<ResourcePool>();
        _pool.Initialize();
    }
    
    public ResourceInstance SpawnResource(ResourceData data, Vector3 position, int amount = 1)
    {
        if (data == null)
        {
            Debug.LogError("[ResourceManager] ResourceData is null!");
            return null;
        }
        
        var instance = _pool.Get(data, amount);
        instance.transform.position = position;
        
        _allResources.Add(instance);
        instance.OnResourceDepleted += OnInstanceDepleted;
        
        OnResourceCreated?.Invoke(instance, data, amount);
        
        Debug.Log($"[ResourceManager] Spawned: {data.resourceName} x{amount} at {position}");
        
        return instance;
    }
    
    public void DestroyResource(ResourceInstance instance)
    {
        if (instance == null) return;
        
        _allResources.Remove(instance);
        instance.OnResourceDepleted -= OnInstanceDepleted;
        
        OnResourceDestroyed?.Invoke(instance);
        
        _pool.Return(instance);
        
        Debug.Log($"[ResourceManager] Destroyed: {instance.Data?.resourceName}");
    }
    
    private void OnInstanceDepleted(ResourceInstance instance)
    {
        DestroyResource(instance);
    }
    
    public List<ResourceInstance> GetResourcesByCategory(ResourceCategory category)
    {
        return _allResources.Where(r => r.Data.category == category).ToList();
    }
    
    public List<ResourceInstance> GetResourcesInRadius(Vector3 center, float radius)
    {
        return _allResources
            .Where(r => Vector3.Distance(r.transform.position, center) <= radius)
            .ToList();
    }
    
    private void OnDestroy()
    {
        Debug.Log("[ResourceManager] OnDestroy");

        foreach (var instance in _allResources.Where(r=>r!=null))
        {
            instance.OnResourceDepleted -= OnInstanceDepleted;
        }
        
        if (_pool != null)
        {
            _pool.ClearAll();
        }
    }
}