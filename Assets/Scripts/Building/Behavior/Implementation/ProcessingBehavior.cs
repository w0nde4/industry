using UnityEngine;

public class ProcessingBehavior : IBuildingBehavior
{
    private ProcessingConfig _config;
    private PlacedBuilding _owner;
    
    public ProcessingBehavior(ProcessingConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        Debug.Log($"[ProcessingBehavior] Initialized");
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