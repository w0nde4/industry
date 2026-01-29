using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BuildingBehaviorManager : MonoBehaviour
{
    private List<PlacedBuilding> _managedBuildings = new List<PlacedBuilding>();
    
    [ShowInInspector, ReadOnly]
    private int _activeBehaviorCount = 0;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Debug.Log("[BuildingBehaviorManager] Static reset");
    }
    
    public void RegisterBuilding(PlacedBuilding building)
    {
        if (_managedBuildings.Contains(building)) return;
        
        _managedBuildings.Add(building);
        UpdateBehaviorCount();
        Debug.Log($"[BuildingBehaviorManager] Registered {building.Data.buildingName}");
    }
    
    public void UnregisterBuilding(PlacedBuilding building)
    {
        if (!_managedBuildings.Contains(building)) return;
        
        _managedBuildings.Remove(building);
        UpdateBehaviorCount();
        Debug.Log($"[BuildingBehaviorManager] Unregistered {building.Data.buildingName}");
    }
    
    private void Update()
    {
        var deltaTime = Time.deltaTime;
        
        foreach (var building in _managedBuildings)
        {
            if (building != null && building.Behaviors != null)
            {
                foreach (var behavior in building.Behaviors)
                {
                    behavior?.OnTick(deltaTime);
                }
            }
        }
    }
    
    private void UpdateBehaviorCount()
    {
        _activeBehaviorCount = 0;
        foreach (var building in _managedBuildings
                     .Where(building => building != null 
                                        && building.Behaviors != null))
        {
            _activeBehaviorCount += building.Behaviors.Count;
        }
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