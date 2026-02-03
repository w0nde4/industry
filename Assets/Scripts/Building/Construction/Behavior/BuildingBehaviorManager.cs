using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BuildingBehaviorManager : MonoBehaviour
{
    private List<PlacedBuilding> _managedBuildings = new List<PlacedBuilding>();
    
    [ShowInInspector, ReadOnly]
    private int ActiveBehaviorCount => CalculateActiveBehaviorCount();
    
    [ShowInInspector, ReadOnly]
    private int ManagedBuildingsCount => _managedBuildings.Count;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Debug.Log("[BuildingBehaviorManager] Static reset");
    }
    
    public void RegisterBuilding(PlacedBuilding building)
    {
        if (_managedBuildings.Contains(building)) return;
        
        _managedBuildings.Add(building);
        Debug.Log($"[BuildingBehaviorManager] Registered {building.Data.buildingName}");
    }
    
    public void UnregisterBuilding(PlacedBuilding building)
    {
        if (!_managedBuildings.Contains(building)) return;
        
        _managedBuildings.Remove(building);
        Debug.Log($"[BuildingBehaviorManager] Unregistered {building.Data.buildingName}");
    }
    
    private void Update()
    {
        var deltaTime = Time.deltaTime;
    
        for (int i = _managedBuildings.Count - 1; i >= 0; i--)
        {
            var building = _managedBuildings[i];
        
            if (building == null)
            {
                _managedBuildings.RemoveAt(i);
                continue;
            }
        
            if (building.Behaviors == null) continue;
        
            foreach (var behavior in building.Behaviors)
            {
                if (behavior == null) continue;
            
                try
                {
                    behavior.OnTick(deltaTime);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[BuildingBehaviorManager] Exception in {behavior.GetType().Name}.OnTick: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
    
    private int CalculateActiveBehaviorCount()
    {
        var count = 0;
        
        foreach (var building in _managedBuildings)
        {
            if (building != null && building.Behaviors != null)
            {
                count += building.Behaviors.Count;
            }
        }
        
        return count;
    }
    
    [Button("Log Managed Buildings")]
    private void LogManagedBuildings()
    {
        Debug.Log($"=== Managed Buildings: {_managedBuildings.Count} ===");
        foreach (var building in _managedBuildings
                     .Where(building => building != null))
        {
            Debug.Log($"  - {building.Data.buildingName} at {building.GridPosition}, Behaviors: {building.Behaviors?.Count ?? 0}");
        }
    }
    
    private void OnDestroy()
    {
        _managedBuildings.Clear();
    }
}