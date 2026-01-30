using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ResourceService : MonoBehaviour
{
    private static ResourceService _instance;

    public static ResourceService Instance
    {
        get
        {
            if(_instance != null) return _instance;
            
            _instance = FindFirstObjectByType<ResourceService>();
            if (_instance == null)
            {
                Debug.Log("ResourceService not found in scene!");
            }
            return _instance;
        }
    }
    
    private ResourcePool _pool;
    private readonly List<ResourceInstance> _allResources = new List<ResourceInstance>();
    
    public event Action<ResourceInstance, ResourceData, int> OnResourceCreated;
    public event Action<ResourceInstance> OnResourceDestroyed;

    public IReadOnlyList<ResourceInstance> AllResources => _allResources;

    [Title("Debug Info")]
    [ShowInInspector, ReadOnly]
    private int TotalResourcesCount => _allResources.Count;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Debug.Log("[ResourceManager] Static reset");
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        
        _pool = gameObject.AddComponent<ResourcePool>();
        _pool.Initialize();
    }
    
    #region Static API
    
    public static ResourceInstance Spawn(ResourceData data, Vector3 position, int amount = 1) 
        => Instance.SpawnResource(data, position, amount);
    
    public static void Destroy(ResourceInstance instance) 
        => Instance.DestroyResource(instance);
    
    public static List<ResourceInstance> GetByCategory(ResourceCategory category) 
        => Instance.GetResourcesByCategory(category);
    
    public static List<ResourceInstance> GetInRadius(Vector3 center, float radius) 
        => Instance.GetResourcesInRadius(center, radius);
    
    #endregion

    #region Resource Management
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

    #endregion

    #region Query Methods
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
    
    #endregion
    
    #region Debug/Editor Utilities
    
    [Button("Clear All Resources")]
    private void ClearAllResources()
    {
        var temp = _allResources.ToList();
        
        foreach (var resource in temp)
        {
            if (resource != null)
            {
                DestroyResource(resource);
            }
        }
        
        _allResources.Clear();
    }
    
    [Button("Log Resources Info")]
    private void LogResourcesInfo()
    {
        Debug.Log($"=== Resources Info ===");
        Debug.Log($"Total Resources: {_allResources.Count}");
        
        var countByCategory = new Dictionary<ResourceCategory, int>();
        
        foreach (var resource in _allResources)
        {
            var category = resource.Data.category;
            
            if (!countByCategory.ContainsKey(category))
                countByCategory[category] = 0;
            
            countByCategory[category]++;
        }
        
        foreach (var kvp in countByCategory)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
    
    #endregion
    
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