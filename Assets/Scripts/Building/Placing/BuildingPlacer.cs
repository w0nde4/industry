using System;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [SerializeField] private BuildingSettings settings;
    
    private BuildingPreview _preview;
    private BuildingObjectPool _objectPool;
    
    private PlacedBuilding _movingBuilding;
    private Vector2Int _originalMovePosition;
    
    public event Action<PlacedBuilding> OnBuildingPlaced;
    public event Action<PlacedBuilding> OnBuildingDemolished;
    public event Action<PlacedBuilding, Vector2Int> OnBuildingMoved;
    
    public bool IsInMoveMode => _movingBuilding != null;
    public bool IsInBuildMode => _preview.IsActive && !IsInMoveMode;
    
    private void Awake()
    {
        _objectPool = gameObject.AddComponent<BuildingObjectPool>();
        _objectPool.Initialize(settings);
        
        _preview = gameObject.AddComponent<BuildingPreview>();
        _preview.Initialize(settings, GridService.Instance.Grid, _objectPool);
    }
    
    public void StartBuildMode(BuildingData data)
    {
        if (IsInMoveMode)
        {
            CancelMove();
        }
        
        _preview.StartPreview(data);
    }
    
    public void StopBuildMode()
    {
        _preview.StopPreview();
    }
    
    public void UpdatePreview(Vector3 worldPosition)
    {
        if (IsInMoveMode || _preview.IsActive)
        {
            _preview.UpdatePreview(worldPosition);
        }
    }
    
    public void RotatePreview(int direction)
    {
        _preview.Rotate(direction);
    }
    
    public bool TryPlaceBuilding()
    {
        if (IsInMoveMode)
        {
            return ConfirmMove();
        }
        
        if (!_preview.CanPlace())
        {
            Debug.LogWarning("Cannot place building here!");
            return false;
        }
        
        var data = _preview.CurrentData;
        var gridPos = _preview.GetCurrentGridPosition();
        var rotation = _preview.CurrentRotation;
        var size = data.GetRotatedSize(rotation);
        var grid = GridService.Instance.Grid;
        
        if (!grid.OccupyArea(gridPos, size, null))
        {
            return false;
        }
        
        var worldPos = grid.GetCenterPosition(gridPos, size);
        var buildingObj = Instantiate(data.prefab, worldPos, Quaternion.identity);
        
        var placedBuilding = buildingObj.GetComponent<PlacedBuilding>();
        if (placedBuilding == null)
        {
            placedBuilding = buildingObj.AddComponent<PlacedBuilding>();
        }
        
        placedBuilding.Initialize(data, gridPos, rotation);
        UpdateGridReference(gridPos, size, buildingObj);
        BuildingService.Instance.RegisterBuilding(placedBuilding);
        
        OnBuildingPlaced?.Invoke(placedBuilding);
        
        Debug.Log($"Building placed: {data.buildingName} at {gridPos}");
        
        return true;
    }
    
    public void StartMove(PlacedBuilding building)
    {
        if (!building.Data.canMove)
        {
            Debug.LogWarning("This building cannot be moved!");
            return;
        }
        
        _movingBuilding = building;
        _originalMovePosition = building.GridPosition;
        
        GridService.Instance.Grid.FreeArea(building.GridPosition, building.Size);
        
        _preview.StartPreview(building.Data);
        
        Debug.Log($"Started moving: {building.Data.buildingName}");
    }
    
    public bool ConfirmMove()
    {
        if (!IsInMoveMode || !_preview.CanPlace())
        {
            return false;
        }
        
        var newGridPos = _preview.GetCurrentGridPosition();
        var size = _movingBuilding.Size;
        var grid = GridService.Instance.Grid;
        
        if (!grid.OccupyArea(newGridPos, size, _movingBuilding.gameObject))
        {
            return false;
        }
        
        var newWorldPos = grid.GetCenterPosition(newGridPos, size);
        _movingBuilding.transform.position = newWorldPos;
        _movingBuilding.SetGridPosition(newGridPos);
        
        UpdateGridReference(newGridPos, size, _movingBuilding.gameObject);
        
        OnBuildingMoved?.Invoke(_movingBuilding, _originalMovePosition);
        
        Debug.Log($"Building moved from {_originalMovePosition} to {newGridPos}");
        
        _movingBuilding = null;
        _preview.StopPreview();
        
        return true;
    }
    
    public void CancelMove()
    {
        if (!IsInMoveMode)
            return;
        
        GridService.Instance.Grid.OccupyArea(_originalMovePosition, _movingBuilding.Size, _movingBuilding.gameObject);
        UpdateGridReference(_originalMovePosition, _movingBuilding.Size, _movingBuilding.gameObject);
        
        Debug.Log("Move cancelled");
        
        _movingBuilding = null;
        _preview.StopPreview();
    }
    
    public bool TryDemolish(Vector3 worldPosition)
    {
        var gridPos = GridService.Instance.Grid.WorldToGridPosition(worldPosition);
        var building = BuildingService.Instance.GetBuildingAtPosition(gridPos);

        if (building != null) return DemolishBuilding(building);
        
        Debug.LogWarning("No building at this position!");
        return false;

    }
    
    public bool DemolishBuilding(PlacedBuilding building)
    {
        var gridPos = building.GridPosition;
        var size = building.Size;
        
        GridService.Instance.Grid.FreeArea(gridPos, size);
        
        OnBuildingDemolished?.Invoke(building);
        
        BuildingService.Instance.UnregisterBuilding(building);
        building.Demolish();
        
        Debug.Log($"Building demolished at {gridPos}");
        
        return true;
    }
    
    private void UpdateGridReference(Vector2Int origin, Vector2Int size, GameObject building)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var pos = origin + new Vector2Int(x, y);
                var cell = GridService.Instance.Grid.GetCell(pos);
                cell?.Occupy(building);
            }
        }
    }

    private void OnDestroy()
    {
        if(_preview != null) _preview.StopPreview();
        if(_objectPool != null) _objectPool.ClearAll();
    }
}