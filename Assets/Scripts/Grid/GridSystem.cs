using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class GridSystem : MonoBehaviour
{
    [Required]
    [SerializeField] private GridSettings settings;
    
    private readonly Dictionary<Vector2Int, GridCell> _cells = new Dictionary<Vector2Int, GridCell>();
    
    private Vector2Int _minBounds;
    private Vector2Int _maxBounds;
    private Vector2Int _tempPos;

    private readonly Vector3 _gridOrigin = Vector3.zero;

    public GridSettings Settings => settings;
    
    #region Debug Info (Odin)
    
    [Title("Debug Info")]
    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    private int TotalCells => _cells.Count;
    
    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    private int OccupiedCells => _cells.Values.Count(c => c.IsOccupied);
    
    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    private int FreeCells => TotalCells - OccupiedCells;
    
    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    private string GridBounds => $"({_minBounds.x},{_minBounds.y}) to ({_maxBounds.x},{_maxBounds.y})";
    
    [ShowInInspector, ReadOnly, PropertyOrder(-1)]
    private Vector2Int GridSize => new Vector2Int(
        _maxBounds.x - _minBounds.x, 
        _maxBounds.y - _minBounds.y
    );
    
    #endregion
    
    private void Awake()
    {
        InitializeAsync().Forget();
    }

    [Button("Reinitialize Grid")]
    private void InitializeGrid()
    {
        InitializeAsync().Forget();
    }

    private async UniTask InitializeAsync()
    {
        _cells.Clear();
        
        var halfWidth = settings.initialGridWidth / 2;
        var halfHeight = settings.initialGridHeight / 2;
        
        _minBounds = new Vector2Int(-halfWidth, -halfHeight);
        _maxBounds = new Vector2Int(halfWidth, halfHeight);
        
        var totalCells = settings.initialGridWidth * settings.initialGridHeight;
        var cellsPerFrame = 1000;
        var cellsCreated = 0;
        
        for (int x = _minBounds.x; x < _maxBounds.x; x++)
        {
            for (int y = _minBounds.y; y < _maxBounds.y; y++)
            {
                var gridPos = new Vector2Int(x, y);
                var worldPos = GridToWorldPosition(gridPos);
                _cells[gridPos] = new GridCell(gridPos, worldPos);

                cellsCreated++;

                if (cellsCreated % cellsPerFrame == 0)
                {
                    await UniTask.Yield();
                }
            }
        }
        
        Debug.Log($"Grid initialized: {_cells.Count} cells ({settings.initialGridWidth}x{settings.initialGridHeight})");
    }
    
    #region Coordinate Conversion   
    
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return _gridOrigin + new Vector3(
            gridPosition.x * settings.cellSize,
            gridPosition.y * settings.cellSize,
            0f
        );
    }
    
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        var offset = worldPosition - _gridOrigin;
        return new Vector2Int(
            Mathf.FloorToInt(offset.x / settings.cellSize),
            Mathf.FloorToInt(offset.y / settings.cellSize)
        );
    }
    
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        var gridPos = WorldToGridPosition(worldPosition);
        return GridToWorldPosition(gridPos);
    }
    
    public Vector3 GetCenterPosition(Vector2Int gridPosition, Vector2Int size)
    {
        var cornerPos = GridToWorldPosition(gridPosition);
        
        var offset = new Vector3(
            size.x * settings.cellSize * 0.5f,
            size.y * settings.cellSize * 0.5f,
            0f
        );
        return cornerPos + offset;
    }
    
    #endregion
    
    #region Cell Access
    
    public GridCell GetCell(Vector2Int gridPosition)
    {
        if (_cells.TryGetValue(gridPosition, out var cell))
        {
            return cell;
        }
        
        if (IsPositionOutOfBounds(gridPosition))
        {
            ExpandGrid(gridPosition);
            return _cells.TryGetValue(gridPosition, out cell) ? cell : null;
        }
        
        return null;
    }
    
    public GridCell GetCellAtWorldPosition(Vector3 worldPosition)
    {
        var gridPos = WorldToGridPosition(worldPosition);
        return GetCell(gridPos);
    }
    
    public void GetCellsInArea(Vector2Int origin, Vector2Int size, System.Action<GridCell> action)
    {
        var endX = origin.x + size.x;
        var endY = origin.y + size.y;
        
        for (int x = origin.x; x < endX; x++)
        {
            for (int y = origin.y; y < endY; y++)
            {
                _tempPos.x = x;
                _tempPos.y = y;
                
                if(_cells.TryGetValue(_tempPos, out var cell))
                {
                    action(cell);
                }
            }
        }
    }
    
    #endregion
    
    #region Cell State Management
    
    public bool IsCellAvailable(Vector2Int gridPosition)
    {
        if (!_cells.TryGetValue(gridPosition, out var cell))
            return false;
            
        return !cell.IsOccupied && cell.Modifiers.isSpawnable;
    }
    
    public bool IsAreaAvailable(Vector2Int origin, Vector2Int size)
    {
        var endX = origin.x + size.x;
        var endY = origin.y + size.y;
        
        for (int x = origin.x; x < endX; x++)
        {
            for (int y = origin.y; y < endY; y++)
            {
                _tempPos.x = x;
                _tempPos.y = y;

                if(!IsSingleCellAvailable(_tempPos)) return false;
            }
        }
        return true;
    }

    private bool IsSingleCellAvailable(Vector2Int position)
    {
        var cell = GetOrCreateCell(position);
        
        if(cell == null) return false;
        
        return !cell.IsOccupied && cell.Modifiers.isSpawnable;
    }

    private GridCell GetOrCreateCell(Vector2Int position)
    {
        if (_cells.TryGetValue(position, out var cell)) return cell;

        if (!IsPositionOutOfBounds(position)) return null;
        
        ExpandGrid(position);
        _cells.TryGetValue(position, out cell);
        return cell;
    }

    public bool OccupyCell(Vector2Int gridPosition, GameObject obj)
    {
        var cell = GetCell(gridPosition);
        if (cell is not { IsOccupied: false }) return false;
        
        cell.Occupy(obj);
        return true;
    }
    
    public bool OccupyArea(Vector2Int origin, Vector2Int size, GameObject obj)
    {
        if (!IsAreaAvailable(origin, size))
        {
            return false;
        }

        GetCellsInArea(origin, size, cell => cell.Occupy(obj));
        return true;
    }
    
    public void FreeCell(Vector2Int gridPosition)
    {
        var cell = GetCell(gridPosition);
        cell?.Free();
    }
    
    public void FreeArea(Vector2Int origin, Vector2Int size)
    {
        GetCellsInArea(origin, size, cell => cell.Free());
    }
    
    #endregion
    
    #region Grid Expansion
    
    private bool IsPositionOutOfBounds(Vector2Int position)
    {
        return position.x < _minBounds.x || position.x >= _maxBounds.x ||
               position.y < _minBounds.y || position.y >= _maxBounds.y;
    }
    
    private void ExpandGrid(Vector2Int targetPosition)
    {
        var currentWidth = _maxBounds.x - _minBounds.x;
        var currentHeight = _maxBounds.y - _minBounds.y;

        if (settings.maxGridSize > 0
            && (currentWidth > settings.maxGridSize
                || currentHeight > settings.maxGridSize))
        {
            Debug.LogWarning($"[GridSystem] Grid expansion blocked: reached max size {settings.maxGridSize}");
            return;
        }
        
        var newMinBounds = _minBounds;
        var newMaxBounds = _maxBounds;
        
        if (targetPosition.x < _minBounds.x)
            newMinBounds.x = Mathf.Max(targetPosition.x, _minBounds.x - settings.expansionStep);
        if (targetPosition.x >= _maxBounds.x)
            newMaxBounds.x = Mathf.Min(targetPosition.x + 1, _maxBounds.x + settings.expansionStep);
        if (targetPosition.y < _minBounds.y)
            newMinBounds.y = Mathf.Max(targetPosition.y, _minBounds.y - settings.expansionStep);
        if (targetPosition.y >= _maxBounds.y)
            newMaxBounds.y = Mathf.Min(targetPosition.y + 1, _maxBounds.y + settings.expansionStep);
        
        var cellsToCreate = (newMaxBounds.x - newMinBounds.x) * (newMaxBounds.y - newMinBounds.y) - _cells.Count;
    
        if (cellsToCreate > 10000)
        {
            Debug.LogError($"[GridSystem] Attempted to create {cellsToCreate} cells at once! Blocked.");
            return;
        }
        
        for (int x = newMinBounds.x; x < newMaxBounds.x; x++)
        {
            for (int y = newMinBounds.y; y < newMaxBounds.y; y++)
            {
                var gridPos = new Vector2Int(x, y);

                if (_cells.ContainsKey(gridPos)) continue;
                
                var worldPos = GridToWorldPosition(gridPos);
                _cells[gridPos] = new GridCell(gridPos, worldPos);
            }
        }
        
        _minBounds = newMinBounds;
        _maxBounds = newMaxBounds;
        
        Debug.Log($"[GridSystem] Grid expanded to {GridSize}");
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmos()
    {
        if (settings == null || !settings.showDebugGizmos || _cells.Count == 0)
            return;

        if (Camera.main == null) return;
        
        var camPos = Camera.main.transform.position;
        var gridCamPos = WorldToGridPosition(camPos);

        var drawRadius = settings.gizmosDrawRadius;
            
        for (int x = gridCamPos.x - drawRadius; x <= gridCamPos.x + drawRadius; x++)
        {
            for (int y = gridCamPos.y - drawRadius; y < gridCamPos.y + drawRadius; y++)
            {
                var gridPos = new Vector2Int(x, y);

                if (!_cells.TryGetValue(gridPos, out var cell)) continue;
                
                Gizmos.color = cell.IsOccupied ? settings.occupiedCellColor : settings.freeCellColor;
                
                var cellCenter = new Vector3(
                    cell.WorldPosition.x + settings.cellSize * 0.5f,
                    cell.WorldPosition.y + settings.cellSize * 0.5f,
                    0f
                );
                
                Gizmos.DrawWireCube(cellCenter, new Vector3(settings.cellSize, settings.cellSize, 0.01f));
            }
        }
    }
    
    [Button("Log Grid Info"), PropertyOrder(100)]
    private void LogGridInfo()
    {
        Debug.Log($"[GridSystem] Total Cells: {TotalCells}");
        Debug.Log($"[GridSystem] Bounds: {GridBounds}");
        Debug.Log($"[GridSystem] Size: {GridSize}");
        Debug.Log($"[GridSystem] Occupied: {OccupiedCells}/{TotalCells}");
        Debug.Log($"[GridSystem] Free: {FreeCells}/{TotalCells}");
    }
    
    #endregion
}