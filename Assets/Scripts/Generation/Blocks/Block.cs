using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Title("Block Info")]
    [SerializeField, Required] private BlockData blockData;
    [SerializeField, ReadOnly] private Vector2Int gridPosition;
    
    [Title("Visual")]
    [SerializeField] private Transform cellsContainer;
    [SerializeField] private GameObject cellPrefab; //SpriteRenderer
    
    private readonly Dictionary<Vector2Int, GameObject> _cellObjects = new Dictionary<Vector2Int, GameObject>();
    private readonly List<PlacedBuilding> _buildings = new List<PlacedBuilding>();
    
    public BlockData Data => blockData;
    public Vector2Int GridPosition => gridPosition;
    public int BlockSize => blockData != null ? blockData.BlockSize : 16;
    
    public void Initialize(BlockData data, Vector2Int position)
    {
        blockData = data;
        gridPosition = position;
        
        if (cellsContainer == null)
        {
            cellsContainer = new GameObject("Cells").transform;
            cellsContainer.SetParent(transform);
            cellsContainer.localPosition = Vector3.zero;
        }
        
        GenerateVisuals();
    }
    
    [Button("Generate Visuals")]
    public void GenerateVisuals()
    {
        if (blockData == null)
        {
            Debug.LogError("[Block] BlockData is null!");
            return;
        }
        
        ClearVisuals();
        
        for (int x = 0; x < blockData.BlockSize; x++)
        {
            for (int y = 0; y < blockData.BlockSize; y++)
            {
                var cellData = blockData.GetCell(x, y);
                if (cellData == null || cellData.Sprite == null)
                    continue;
                
                CreateCellVisual(x, y, cellData);
            }
        }
        
        Debug.Log($"[Block] Generated {_cellObjects.Count} cell visuals");
    }
    
    private void CreateCellVisual(int x, int y, CellData cellData)
    {
        GameObject cellObj;
        
        if (cellPrefab != null)
        {
            cellObj = Instantiate(cellPrefab, cellsContainer);
        }
        else
        {
            cellObj = new GameObject($"Cell_{x}_{y}");
            cellObj.transform.SetParent(cellsContainer);
            cellObj.AddComponent<SpriteRenderer>();
        }
        
        var spriteRenderer = cellObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = cellData.Sprite;
            spriteRenderer.sortingOrder = -1;
        }
        
        var localPos = new Vector3(x, y, 0);
        cellObj.transform.localPosition = localPos;
        cellObj.name = $"Cell_{x}_{y}_{cellData.Terrain}";
        
        _cellObjects[new Vector2Int(x, y)] = cellObj;
    }
    
    [Button("Clear Visuals")]
    private void ClearVisuals()
    {
        foreach (var cellObj in _cellObjects.Values)
        {
            if (cellObj != null)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(cellObj);
                else
                #endif
                    Destroy(cellObj);
            }
        }
        
        _cellObjects.Clear();
    }
    
    public void ApplyToGrid(GridSystem gridSystem)
    {
        if (gridSystem == null || blockData == null)
            return;
        
        for (int x = 0; x < blockData.BlockSize; x++)
        {
            for (int y = 0; y < blockData.BlockSize; y++)
            {
                var cellData = blockData.GetCell(x, y);
                if (cellData == null)
                    continue;
                
                var worldGridPos = gridPosition + new Vector2Int(x, y);
                var gridCell = gridSystem.GetCell(worldGridPos);
                
                if (gridCell != null)
                {
                    gridCell.SetModifiers(cellData.Modifiers);
                }
            }
        }
    }
    
    public void RegisterBuilding(PlacedBuilding building)
    {
        if (!_buildings.Contains(building))
        {
            _buildings.Add(building);
        }
    }
    
    public void UnregisterBuilding(PlacedBuilding building)
    {
        _buildings.Remove(building);
    }
    
    public List<PlacedBuilding> GetBuildings()
    {
        return new List<PlacedBuilding>(_buildings);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (blockData == null)
            return;
        
        Gizmos.color = Color.yellow;
        var size = blockData.BlockSize;
        var center = transform.position + new Vector3(size * 0.5f, size * 0.5f, 0);
        Gizmos.DrawWireCube(center, new Vector3(size, size, 0.1f));
        
        Gizmos.color = Color.green;
        foreach (var door in blockData.Doors)
        {
            var doorCells = door.GetDoorCells(blockData.BlockSize);
            foreach (var cell in doorCells)
            {
                var pos = transform.position + new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0);
                Gizmos.DrawSphere(pos, 0.3f);
            }
        }
    }
    #endif
    
    private void OnDestroy()
    {
        ClearVisuals();
        _buildings.Clear();
    }
}