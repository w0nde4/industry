using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ConveyorBuilding : PlacedBuilding
{
    [Header("Conveyor Settings")]
    [SerializeField] private ConveyorType conveyorType = ConveyorType.Straight;
    [SerializeField] private ConveyorSettings settings;
    
    private readonly List<ResourceInstance> _resourcesOnConveyor = new List<ResourceInstance>();
    private readonly Dictionary<ResourceInstance, Tween> _activeTweens = new Dictionary<ResourceInstance, Tween>();
    
    private ConnectionPoint _input;
    private ConnectionPoint _output;
    
    private bool _isOutputBlocked = false;
    
    private ConnectionPointSettings _connectionPointSettings;
    private List<PlacedBuilding> _buildingsCache = new List<PlacedBuilding>(100);
    private List<ConnectionPoint> _adjacentCache = new List<ConnectionPoint>(20);
    
    public ConveyorType ConveyorType => conveyorType;
    public bool IsOutputBlocked => _isOutputBlocked;
    public int ResourceCount => _resourcesOnConveyor.Count;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Debug.Log("[ConveyorBuilding] Static reset");
    }
    
    private void Awake()
    {
        if (settings == null)
        {
            settings = Resources.Load<ConveyorSettings>("ConveyorSettings");
            
            if (settings == null)
            {
                Debug.LogError("[ConveyorBuilding] ConveyorSettings not found in Resources!");
            }
        }
        
        if (_connectionPointSettings == null)
        {
            _connectionPointSettings = Resources.Load<ConnectionPointSettings>("ConnectionPointSettings");
            
            if (_connectionPointSettings == null)
            {
                Debug.LogError("[ConveyorBuilding] ConnectionPointSettings not found in Resources!");
            }
        }
    }
    
    public void InitializeConveyor()
    {
        var points = ConnectionPoints;
        
        foreach (var point in points)
        {
            if (point.Type == ConnectionType.Input) _input = point;
            else if (point.Type == ConnectionType.Output) _output = point;
        }
        
        if (_input == null || _output == null)
        {
            Debug.LogError($"[ConveyorBuilding] {Data.buildingName} missing input or output point!");
            return;
        }
        
        _input.OnResourceReceived += OnResourceReceived;
        
        Debug.Log($"[ConveyorBuilding] {Data.buildingName} initialized at {GridPosition}");
    }
    
    #region Resource Handling
    
    private void OnResourceReceived(ConnectionPoint point, ResourceInstance resource)
    {
        if (!CanAcceptResource(resource))
        {
            Debug.LogWarning($"[ConveyorBuilding] Cannot accept resource: conveyor full or blocked");
            return;
        }
        
        AcceptResource(resource);
    }
    
    public bool CanAcceptResource(ResourceInstance resource)
    {
        if (resource == null)
        {
            Debug.LogError("[ConveyorBuilding] Resource is null!");
            return false;
        }
    
        if (settings == null)
        {
            Debug.LogError("[ConveyorBuilding] Settings is null!");
            return false;
        }
        
        if (_resourcesOnConveyor.Count >= settings.maxResourcesPerConveyor)
        {
            Debug.LogWarning("[ConveyorBuilding] Conveyor full!");
            return false;
        }

        foreach (var resourceInstance in _resourcesOnConveyor)
        {
            if (resourceInstance.Data == resource.Data)
            {
                Debug.LogWarning("[ConveyorBuilding] Same resource already moving!");
                return false;
            }
        }
        
        return true;
    }
    
    public void AcceptResource(ResourceInstance resource)
    {
        _resourcesOnConveyor.Add(resource);
        
        resource.transform.position = _input.WorldPosition;
        
        MoveResourceToOutput(resource).Forget();
        
        Debug.Log($"[ConveyorBuilding] Accepted {resource.Data.resourceName} x{resource.Amount}");
    }
    
    private async UniTaskVoid MoveResourceToOutput(ResourceInstance resource)
    {
        var distance = Vector3.Distance(_input.WorldPosition, _output.WorldPosition);

        var speed = settings.conveyorSpeed;
        
        var duration = distance / speed;
        
        var tween = resource.transform
            .DOMove(_output.WorldPosition, duration)
            .SetEase(Ease.Linear);
        
        _activeTweens[resource] = tween;
        
        await tween.AsyncWaitForCompletion();
        
        _activeTweens.Remove(resource);
        
        if (resource == null || !_resourcesOnConveyor.Contains(resource))
        {
            return;
        }
        
        TryTransferToNextConveyor(resource);
    }
    
    private void TryTransferToNextConveyor(ResourceInstance resource)
    {
        var nextConveyor = FindNextConveyor();
        
        if (nextConveyor != null && nextConveyor.CanAcceptResource(resource))
        {
            _resourcesOnConveyor.Remove(resource);
            nextConveyor.AcceptResource(resource);
            
            Debug.Log($"[ConveyorBuilding] Transferred {resource.Data.resourceName} to next conveyor");
        }
        else
        {
            _isOutputBlocked = true;
            Debug.Log($"[ConveyorBuilding] Output blocked, resource waiting at output");
            
            CheckOutputUnblock(resource).Forget();
        }
    }
    
    private async UniTaskVoid CheckOutputUnblock(ResourceInstance resource)
    {
        while (_isOutputBlocked)
        {
            await UniTask.Delay(500);
            
            if (resource == null || !_resourcesOnConveyor.Contains(resource))
            {
                _isOutputBlocked = false;
                return;
            }
            
            var nextConveyor = FindNextConveyor();

            if (nextConveyor == null || !nextConveyor.CanAcceptResource(resource)) continue;
            
            _resourcesOnConveyor.Remove(resource);
            nextConveyor.AcceptResource(resource);
                
            _isOutputBlocked = false;
                
            Debug.Log($"[ConveyorBuilding] Output unblocked, transferred resource");
            return;
        }
    }
    
    private ConveyorBuilding FindNextConveyor()
    {
        if (_output == null)
        {
            Debug.LogError("[ConveyorBuilding] _output is null!");
            return null;
        }
    
        if (settings == null)
        {
            Debug.LogError("[ConveyorBuilding] settings is null!");
            return null;
        }
        
        if (_connectionPointSettings == null)
        {
            Debug.LogError("[ConveyorBuilding] _connectionPointSettings is null!");
            return null;
        }
        
        _buildingsCache.Clear();
        var allBuildings = BuildingService.Instance.AllBuildings;
        _buildingsCache.AddRange(allBuildings);
    
        ConnectionPointHelper.GetAdjacentConnectionPoints(
            _output,
            _buildingsCache,
            _connectionPointSettings,
            _adjacentCache
        );
    
        ConnectionPoint closestInput = null;
        var minDistance = float.MaxValue;
    
        foreach (var point in _adjacentCache)
        {
            if(point.Type != ConnectionType.Input) continue;
            
            var distance = Vector3.Distance(_output.WorldPosition, point.WorldPosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestInput = point;
            }
        }

        if (closestInput == null) return null;
        
        var conveyor = closestInput.Owner.GetComponent<ConveyorBuilding>();
        
        if (conveyor != null)
        {
            Debug.Log($"[ConveyorBuilding {GridPosition}] Found next conveyor at {conveyor.GridPosition}");
        }
        
        return conveyor;

    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnResourceReceived -= OnResourceReceived;
        }
        
        foreach (var kvp in _activeTweens)
        {
            if (kvp.Value != null && kvp.Value.IsActive())
            {
                kvp.Value.Kill();
            }
        }
        _activeTweens.Clear();
        
        for (int i = _resourcesOnConveyor.Count - 1; i >= 0; i--)
        {
            var resource = _resourcesOnConveyor[i];
            if (resource != null)
            {
                ResourceService.Destroy(resource);
            }
        }
        _resourcesOnConveyor.Clear();
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmos()
    {
        if (_input == null || _output == null) return;
        
        Gizmos.color = _isOutputBlocked ? Color.red : Color.green;
        Gizmos.DrawLine(_input.WorldPosition, _output.WorldPosition);
        
        Gizmos.color = Color.yellow;
        
        foreach (var resource in _resourcesOnConveyor)
        {
            if (resource != null)
            {
                Gizmos.DrawWireSphere(resource.transform.position, 0.15f);                
            }
        }
    }
    
    #endregion
}