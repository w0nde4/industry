using UnityEngine;

public class TurretBehavior : IBuildingBehavior
{
    private TurretConfig _config;
    private PlacedBuilding _owner;
    
    public TurretBehavior(TurretConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        Debug.Log($"[TurretBehavior] Initialized");
    }
    
    public void OnTick(float deltaTime)
    {
        // Реализация позже (автобаттлер)
    }
    
    public void OnResourceReceived(ConnectionPoint input, ResourceInstance resource)
    {
        // Реализация позже
    }
    
    public void CleanUp()
    {
    }
}