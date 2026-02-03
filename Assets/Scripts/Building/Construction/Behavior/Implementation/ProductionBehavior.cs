using System.Collections.Generic;
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
    private bool _isOutputInitialized;

    private ConnectionPointSettings _connectionPointSettings;
    private GridSystem _gridSystem;
    private List<ConnectionPoint> _adjacentCache = new List<ConnectionPoint>(20);
    
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
        _isOutputInitialized = false;
        
        _connectionPointSettings = Resources.Load<ConnectionPointSettings>("ConnectionPointSettings");
        if (_connectionPointSettings == null)
        {
            Debug.LogError("[ProductionBehavior] ConnectionPointSettings not found in Resources!");
        }
        
        _gridSystem = GridService.Instance.Grid;
        
        Debug.Log($"[ProductionBehavior] Initialized for {data.buildingName}");
    }
    
    private void EnsureOutputInitialized()
    {
        if (_isOutputInitialized) return;
        
        var outputs = _owner.Outputs;

        if (outputs == null || outputs.Length == 0)
        {
            Debug.LogError($"[ProductionBehavior] {_owner.Data.buildingName} has no Output ConnectionPoint!");
            _isOutputInitialized = true; 
            return;
        }
        
        _outputPoint = outputs[0];
        _isOutputInitialized = true;
        
        Debug.Log($"[ProductionBehavior] Found output point for {_owner.Data.buildingName}");
    }

    public void OnTick(float deltaTime)
    {
        EnsureOutputInitialized();
        
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

        var resource = ResourceService.Spawn(
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
            ResourceService.Destroy(resource);
            
            if (_isOutputBlocked) return;
            
            _isOutputBlocked = true;
            Debug.Log($"[ProductionBehavior] Output blocked. Accumulating resources: {_accumulatedResources}/{_config.maxOutputStack}");
        }
    }

    private ConveyorBuilding FindNextConveyor()
    {
        if(_connectionPointSettings == null) return null;
        
        var allBuildings = BuildingService.Instance.AllBuildings;
        
        ConnectionPointHelper.GetAdjacentConnectionPoints(
            _outputPoint,
            new List<PlacedBuilding>(allBuildings),
            _connectionPointSettings,
            _adjacentCache);

        ConnectionPoint closestInput = null;
        var minDistance = float.MaxValue;
        
        foreach (var point in _adjacentCache)
        {
            if(point.Type != ConnectionType.Input) continue;
            
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

        if (_gridSystem == null) return 1f;

        var cell = _gridSystem.GetCell(_owner.GridPosition);

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