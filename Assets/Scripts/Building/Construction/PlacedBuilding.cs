using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacedBuilding : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;
    
    private Vector2Int _gridPosition;
    private BuildingRotation _currentRotation;
    private readonly List<IBuildingBehavior> _behaviors = new();
    
    private ConnectionPoint[] _connectionPoints;
    private ConnectionPoint[] _inputs;
    private ConnectionPoint[] _outputs;
    
    public BuildingData Data => buildingData;
    public Vector2Int GridPosition => _gridPosition;
    public Vector2Int Size => buildingData.GetRotatedSize(_currentRotation);
    public BuildingRotation Rotation => _currentRotation;
    public List<IBuildingBehavior> Behaviors => _behaviors;
    public ConnectionPoint[] ConnectionPoints => _connectionPoints;
    public ConnectionPoint[] Inputs => _inputs;
    public ConnectionPoint[] Outputs => _outputs;
    
    public event Action<PlacedBuilding> OnBuildingDestroyed;
    
    public void Initialize(BuildingData data, Vector2Int position, BuildingRotation rotation)
    {
        buildingData = data;
        _gridPosition = position;
        _currentRotation = rotation;
        
        ApplyRotation();
        InitializeConnectionPoints();
        
        if (this is ConveyorBuilding conveyor)
        {
            conveyor.InitializeConveyor();
        }
        
        InitializeBehaviors();
    }

    private void InitializeBehaviors()
    {
        if(buildingData.behaviorConfigs == null 
           || buildingData.behaviorConfigs.Count == 0)
        {
            Debug.Log($"[PlacedBuilding] {buildingData.buildingName} has no behaviors");
            return;
        }

        foreach (var conf in buildingData.behaviorConfigs)
        {
            if(conf == null) continue;
            
            var behavior = conf.CreateBehavior();
            behavior.Initialize(this, buildingData);
            _behaviors.Add(behavior);
            
            Debug.Log($"[PlacedBuilding] Added behavior: {behavior.GetType().Name}");
        }

        var service = BuildingService.Instance;
        if (service == null)
        {
            Debug.LogWarning("[PlacedBuilding] BuildingService not found!");
            return;
        }
        
        var manager = service.BehaviorManager;
        if (manager != null)
        {
            manager.RegisterBuilding(this);
        }
        else
        {
            Debug.LogWarning("[PlacedBuilding] BuildingBehaviorManager not assigned to BuildingService!");
        }
    }

    private void InitializeConnectionPoints()
    {
        _connectionPoints = GetComponentsInChildren<ConnectionPoint>();
        
        var inputsList = new List<ConnectionPoint>(_connectionPoints.Length);
        var outputsList = new List<ConnectionPoint>(_connectionPoints.Length);
    
        foreach (var point in _connectionPoints)
        {
            point.Initialize(this);
            point.OnResourceReceived += OnResourceReceivedAtPoint;
            
            if(point.Type == ConnectionType.Input) inputsList.Add(point);
            else if(point.Type == ConnectionType.Output) outputsList.Add(point);
        }
        
        _inputs = inputsList.ToArray();
        _outputs = outputsList.ToArray();
        
        Debug.Log($"[PlacedBuilding] Initialized {_connectionPoints.Length} connection points " +
                  $"({_inputs.Length} inputs, {_outputs.Length} outputs) for {buildingData.buildingName}");
    }

    private void OnResourceReceivedAtPoint(ConnectionPoint point, ResourceInstance resource)
    {
        foreach (var behavior in _behaviors)
        {
            behavior.OnResourceReceived(point, resource);
        }
    }

    public void SetGridPosition(Vector2Int newPosition)
    {
        _gridPosition = newPosition;
    }
    
    public void SetRotation(BuildingRotation rotation)
    {
        _currentRotation = rotation;
        ApplyRotation();
        
        UpdateConnectionPointsWorldPosition();
    }

    private void ApplyRotation()
    {
        if (buildingData.canRotate)
        {
            transform.rotation = Quaternion.Euler(0, 0, -(int)_currentRotation);
        }
    }

    private void UpdateConnectionPointsWorldPosition()
    {
        if(_connectionPoints == null) return;
        
        foreach (var point in _connectionPoints)
        {
            point.UpdateWorldPosition();
        }
    }

    public void Demolish()
    {
        if(_connectionPoints != null)
        {
            foreach (var point in _connectionPoints)
            {
                point.OnResourceReceived -= OnResourceReceivedAtPoint;
            }
        }
        
        var service = BuildingService.Instance;
        if (service != null)
        {
            var manager = service.BehaviorManager;
            if(manager != null)
            {
                manager.UnregisterBuilding(this);
            }
        }
        
        OnBuildingDestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}