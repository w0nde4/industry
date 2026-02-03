using UnityEngine;

public class StorageBehavior : IBuildingBehavior
{
    private StorageConfig _config;
    private PlacedBuilding _owner;
    
    public StorageBehavior(StorageConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        Debug.Log($"[StorageBehavior] Initialized");
    }
    
    public void OnTick(float deltaTime)
    {
        // Реализация позже
    }
    
    public void OnResourceReceived(ConnectionPoint input, ResourceInstance resource)
    {
        // Реализация позже
    }
    
    public void CleanUp()
    {
    }
}