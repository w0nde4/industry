using UnityEngine;

public class AssemblyBehavior : IBuildingBehavior
{
    private AssemblyConfig _config;
    private PlacedBuilding _owner;
    
    public AssemblyBehavior(AssemblyConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        Debug.Log($"[AssemblyBehavior] Initialized");
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