using UnityEngine;
using System.Linq;

public class ProductionBehavior : IBuildingBehavior
{
    private ProductionConfig _config;
    private PlacedBuilding _owner;
    
    private float _productionTimer;
    private int _accumulatedResources;
    
    private ConnectionPoint _outputPoint;
    private bool _isOutputBlocked;
    
    public ProductionBehavior(ProductionConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        _productionTimer = 0f;
        _accumulatedResources = 0;
        _isOutputBlocked = false;

        FindOutputPoint();
        
        Debug.Log($"[ProductionBehavior] Initialized for {data.buildingName}");
    }

    private void FindOutputPoint()
    {
        var points = _owner.GetComponentsInChildren<ConnectionPoint>();

        _outputPoint = points.FirstOrDefault(p => p.Type == ConnectionType.Output);

        if (_outputPoint == null)
        {
            Debug.LogError($"[ProductionBehavior] {_owner.Data.buildingName} has no Output ConnectionPoint!");
        }
    }
    
    public void OnTick(float deltaTime)
    {
        if (_outputPoint == null) return;

        if (_accumulatedResources >= _config.maxOutputStack)
        {
            TryPushAccumulatedResources();
            return;
        }
        
        _productionTimer += deltaTime * GetProductionSpeedMultiplier();
        
        if(_productionTimer >= _config.productionInterval)
        {
            _productionTimer = 0f;
            ProduceResource();
        }
    }

    private void ProduceResource()
    {
        _accumulatedResources++;
        
        Debug.Log($"[ProductionBehavior] Produced {_config.outputResource.resourceName}. Accumulated: {_accumulatedResources}/{_config.maxOutputStack}");
        
        TryPushAccumulatedResources();
    }

    private void TryPushAccumulatedResources()
    {
        if (_accumulatedResources == 0) return;
        
        var nextConveyor = FindNextConveyor();

        if (nextConveyor == null)
        {
            if (_isOutputBlocked) return;
            
            _isOutputBlocked = true;
            Debug.Log($"[ProductionBehavior] No conveyor found. Accumulating: {_accumulatedResources}/{_config.maxOutputStack}");
            
            return;
        }

        var resource = ResourceService.SpawnResource(
            _config.outputResource,
            _outputPoint.WorldPosition,
            1);
        
        if (nextConveyor.CanAcceptResource(resource))
        {
            nextConveyor.AcceptResource(resource);
            
            _accumulatedResources--;
            _isOutputBlocked = false;
            
            Debug.Log($"[ProductionBehavior] Pushed resource to conveyor. Remaining: {_accumulatedResources}");
        }
        else
        {
            ResourceService.DestroyResource(resource);
            
            if (_isOutputBlocked) return;
            
            _isOutputBlocked = true;
            Debug.Log($"[ProductionBehavior] Output blocked. Accumulating resources: {_accumulatedResources}/{_config.maxOutputStack}");
        }
    }

    private ConveyorBuilding FindNextConveyor()
    {
        var allBuildings = GameObject.FindObjectsByType<PlacedBuilding>(FindObjectsSortMode.None).ToList();
        
        var adjacentPoints = ConnectionPointHelper.GetAdjacentConnectionPoints(
            _outputPoint,
            allBuildings,
            1.5f
        );
        
        var inputPoints = adjacentPoints
            .Where(p => p.Type == ConnectionType.Input).ToList();
        
        if (inputPoints.Count == 0)
            return null;
        
        ConnectionPoint closestInput = null;
        var minDistance = float.MaxValue;
        
        foreach (var point in inputPoints)
        {
            var distance = Vector3.Distance(_outputPoint.WorldPosition, point.WorldPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestInput = point;
            }
        }
        
        return closestInput?.Owner.GetComponent<ConveyorBuilding>();
    }

    private float GetProductionSpeedMultiplier()
    {
        if (!_config.useModifiers) return 1f;

        var gridSystem = GameObject.FindFirstObjectByType<GridSystem>();
        if (gridSystem == null) return 1f;

        var cell = gridSystem.GetCell(_owner.GridPosition);

        return cell == null ? 
            1f : cell.Modifiers.productionBonus;
    }

    public void OnResourceReceived(ConnectionPoint input, ResourceInstance resource)
    {
        Debug.LogWarning("[ProductionBehavior] Producer should not receive resources!");
    }
    
    public void CleanUp()
    {
        Debug.Log($"[ProductionBehavior] Cleanup - accumulated resources: {_accumulatedResources}");
    }
}