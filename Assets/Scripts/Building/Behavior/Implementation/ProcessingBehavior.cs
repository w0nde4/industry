using System.Collections.Generic;
using UnityEngine;

public class ProcessingBehavior : IBuildingBehavior
{
    private ProcessingConfig _config;
    private PlacedBuilding _owner;
    
    private float _processingTimer;
    private int _inputBuffer;
    private int _outputBuffer;
    
    private ConnectionPoint _inputPoint;
    private ConnectionPoint _outputPoint;
    private bool _isOutputBlocked;
    private bool _isConnectionsInitialized;
    private bool _isProcessing;

    private ConnectionPointSettings _connectionPointSettings;
    private GridSystem _gridSystem;
    private List<ConnectionPoint> _adjacentCache = new List<ConnectionPoint>(20);
    
    public ProcessingBehavior(ProcessingConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        _processingTimer = 0f;
        _inputBuffer = 0;
        _outputBuffer = 0;
        _isOutputBlocked = false;
        _isConnectionsInitialized = false;
        _isProcessing = false;
        
        _connectionPointSettings = Resources.Load<ConnectionPointSettings>("ConnectionPointSettings");
        if (_connectionPointSettings == null)
        {
            Debug.LogError("[ProcessingBehavior] ConnectionPointSettings not found in Resources!");
        }
        
        _gridSystem = GridService.Instance.Grid;
        
        Debug.Log($"[ProcessingBehavior] Initialized for {data.buildingName}");
    }
    
    private void EnsureConnectionsInitialized()
    {
        if (_isConnectionsInitialized) return;
        
        var inputs = _owner.Inputs;
        var outputs = _owner.Outputs;

        if (inputs == null || inputs.Length == 0)
        {
            Debug.LogError($"[ProcessingBehavior] {_owner.Data.buildingName} has no Input ConnectionPoint!");
        }
        else
        {
            _inputPoint = inputs[0];
            Debug.Log($"[ProcessingBehavior] Found input point for {_owner.Data.buildingName}");
        }

        if (outputs == null || outputs.Length == 0)
        {
            Debug.LogError($"[ProcessingBehavior] {_owner.Data.buildingName} has no Output ConnectionPoint!");
        }
        else
        {
            _outputPoint = outputs[0];
            Debug.Log($"[ProcessingBehavior] Found output point for {_owner.Data.buildingName}");
        }
        
        _isConnectionsInitialized = true;
    }

    public void OnTick(float deltaTime)
    {
        EnsureConnectionsInitialized();
        
        if (_outputPoint == null) return;

        if (_outputBuffer > 0)
        {
            TryPushOutputResources();
        }
        
        if (_outputBuffer >= _config.maxOutputBuffer)
        {
            if (_isProcessing)
            {
                Debug.Log($"[ProcessingBehavior] Output buffer full. Pausing processing.");
                _isProcessing = false;
            }
            return;
        }
        
        if (!_isProcessing && _inputBuffer >= _config.inputAmount)
        {
            _isProcessing = true;
            _processingTimer = 0f;
            Debug.Log($"[ProcessingBehavior] Started processing. Input buffer: {_inputBuffer}");
        }
        
        if (_isProcessing)
        {
            _processingTimer += deltaTime * GetProcessingSpeedMultiplier();
            
            if (_processingTimer >= _config.processingTime)
            {
                ProcessResource();
            }
        }
    }

    private void ProcessResource()
    {
        _inputBuffer -= _config.inputAmount;
        
        _outputBuffer += _config.outputAmount;
        
        _processingTimer = 0f;
        _isProcessing = false;
        
        Debug.Log($"[ProcessingBehavior] Processed {_config.inputResource.resourceName} -> {_config.outputResource.resourceName}. " +
                  $"Input buffer: {_inputBuffer}, Output buffer: {_outputBuffer}");
        
        TryPushOutputResources();
    }

    private void TryPushOutputResources()
    {
        if (_outputBuffer == 0) return;
        
        var nextConveyor = FindNextConveyor();

        if (nextConveyor == null)
        {
            if (!_isOutputBlocked)
            {
                _isOutputBlocked = true;
                Debug.Log($"[ProcessingBehavior] No conveyor found. Output buffer: {_outputBuffer}/{_config.maxOutputBuffer}");
            }
            return;
        }

        var resource = ResourceService.Spawn(
            _config.outputResource,
            _outputPoint.WorldPosition,
            1);
        
        if (nextConveyor.CanAcceptResource(resource))
        {
            nextConveyor.AcceptResource(resource);
            
            _outputBuffer--;
            _isOutputBlocked = false;
            
            Debug.Log($"[ProcessingBehavior] Pushed output resource to conveyor. Remaining: {_outputBuffer}");
        }
        else
        {
            ResourceService.Destroy(resource);
            
            if (!_isOutputBlocked)
            {
                _isOutputBlocked = true;
                Debug.Log($"[ProcessingBehavior] Output blocked. Buffer: {_outputBuffer}/{_config.maxOutputBuffer}");
            }
        }
    }

    private ConveyorBuilding FindNextConveyor()
    {
        if (_connectionPointSettings == null) return null;
        
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
            if (point.Type != ConnectionType.Input) continue;
            
            var distance = Vector3.Distance(_outputPoint.WorldPosition, point.WorldPosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestInput = point;
            }
        }
        
        return closestInput?.Owner.GetComponent<ConveyorBuilding>();
    }

    private float GetProcessingSpeedMultiplier()
    {
        if (!_config.useModifiers) return 1f;

        if (_gridSystem == null) return 1f;

        var cell = _gridSystem.GetCell(_owner.GridPosition);

        return cell == null ? 
            1f : cell.Modifiers.productionBonus;
    }

    public void OnResourceReceived(ConnectionPoint input, ResourceInstance resource)
    {
        if (resource.Data != _config.inputResource)
        {
            Debug.LogWarning($"[ProcessingBehavior] Received wrong resource type! Expected {_config.inputResource.resourceName}, got {resource.Data.resourceName}");
            return;
        }
        
        if (_inputBuffer >= _config.maxInputBuffer)
        {
            Debug.LogWarning($"[ProcessingBehavior] Input buffer full! Cannot accept resource.");
            return;
        }
        
        _inputBuffer += resource.Amount;
        
        if (_inputBuffer > _config.maxInputBuffer)
        {
            _inputBuffer = _config.maxInputBuffer;
        }
        
        Debug.Log($"[ProcessingBehavior] Received {resource.Data.resourceName} x{resource.Amount}. Input buffer: {_inputBuffer}/{_config.maxInputBuffer}");
        
        ResourceService.Destroy(resource);
    }
    
    public void CleanUp()
    {
        Debug.Log($"[ProcessingBehavior] Cleanup - Input buffer: {_inputBuffer}, Output buffer: {_outputBuffer}");
    }
}