using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }
    
    private readonly Dictionary<ResourceData, int> _totalProduced = new Dictionary<ResourceData, int>();
    private readonly Dictionary<ResourceData, int> _totalConsumed = new Dictionary<ResourceData, int>();
    
    public event Action<ResourceData, int> OnResourceChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void OnResourceProduced(ResourceData resourceData, int amount)
    {
        if (resourceData == null) return;
        
        if (!_totalProduced.ContainsKey(resourceData))
        {
            _totalProduced[resourceData] = 0;
        }
        
        _totalProduced[resourceData] += amount;
        
        OnResourceChanged?.Invoke(resourceData, GetNetAmount(resourceData));
        
        Debug.Log($"[PlayerResourceManager] Produced {resourceData.resourceName} x{amount}. Total net: {GetNetAmount(resourceData)}");
    }
    
    public void OnResourceConsumed(ResourceData resourceData, int amount)
    {
        if (resourceData == null) return;
        
        if (!_totalConsumed.ContainsKey(resourceData))
        {
            _totalConsumed[resourceData] = 0;
        }
        
        _totalConsumed[resourceData] += amount;
        
        OnResourceChanged?.Invoke(resourceData, GetNetAmount(resourceData));
        
        Debug.Log($"[PlayerResourceManager] Consumed {resourceData.resourceName} x{amount}. Total net: {GetNetAmount(resourceData)}");
    }
    
    public int GetNetAmount(ResourceData resourceData)
    {
        if (resourceData == null) return 0;
        
        int produced = _totalProduced.ContainsKey(resourceData) ? _totalProduced[resourceData] : 0;
        int consumed = _totalConsumed.ContainsKey(resourceData) ? _totalConsumed[resourceData] : 0;
        
        return produced - consumed;
    }
    
    public int GetTotalProduced(ResourceData resourceData)
    {
        if (resourceData == null) return 0;
        return _totalProduced.ContainsKey(resourceData) ? _totalProduced[resourceData] : 0;
    }
    
    public int GetTotalConsumed(ResourceData resourceData)
    {
        if (resourceData == null) return 0;
        return _totalConsumed.ContainsKey(resourceData) ? _totalConsumed[resourceData] : 0;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}