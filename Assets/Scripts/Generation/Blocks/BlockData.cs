using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Level Generation/Block Data")]
public class BlockData : ScriptableObject
{
    [Title("Block Info")]
    [SerializeField] private string blockName;
    [SerializeField] private BlockType blockType;
    [SerializeField, ReadOnly] private int blockSize = 16;
    
    [Title("Visual & Logic")]
    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawCell")]
    private CellData[,] cells = new CellData[16, 16];
    
    [Title("Doors")]
    [SerializeField] private List<BlockDoor> doors = new List<BlockDoor>();
    
    [Title("Rules")]
    [SerializeField, Tooltip("Типы блоков, которые могут быть соседями")]
    private List<BlockType> allowedNeighbors = new List<BlockType>();
    
    [SerializeField, Tooltip("Может ли этот блок повторяться в генерации")]
    private bool canRepeat = true;
    
    [SerializeField, Tooltip("Максимальное количество повторений (если canRepeat = true)")]
    private int maxRepeats = 1;
    
    [SerializeField, Tooltip("Является ли обязательным в генерации")]
    private bool isMandatory = false;
    
    public string BlockName => blockName;
    public BlockType BlockType => blockType;
    public int BlockSize => blockSize;
    public CellData[,] Cells => cells;
    public List<BlockDoor> Doors => doors;
    public List<BlockType> AllowedNeighbors => allowedNeighbors;
    public bool CanRepeat => canRepeat;
    public int MaxRepeats => maxRepeats;
    public bool IsMandatory => isMandatory;
    
    private void OnEnable()
    {
        if (cells == null || cells.Length != blockSize * blockSize)
        {
            InitializeCells();
        }
    }
    
    [Button("Initialize Cells")]
    public void InitializeCells()
    {
        cells = new CellData[blockSize, blockSize];
        
        for (int x = 0; x < blockSize; x++)
        {
            for (int y = 0; y < blockSize; y++)
            {
                cells[x, y] = new CellData();
            }
        }
        
        Debug.Log($"[BlockData] Initialized {blockSize}x{blockSize} cells");
    }
    
    public CellData GetCell(int x, int y)
    {
        if (x < 0 || x >= blockSize || y < 0 || y >= blockSize)
            return null;
        
        return cells[x, y];
    }
    
    public void SetCell(int x, int y, CellData cellData)
    {
        if (x < 0 || x >= blockSize || y < 0 || y >= blockSize)
            return;
        
        cells[x, y] = cellData;
    }
    
    public bool HasDoorOn(DoorSide side)
    {
        return doors.Exists(d => d.Side == side);
    }
    
    public List<BlockDoor> GetDoorsOn(DoorSide side)
    {
        return doors.FindAll(d => d.Side == side);
    }
    
    public bool CanBeNeighborWith(BlockType otherType)
    {
        if (allowedNeighbors.Count == 0)
            return true;
        
        return allowedNeighbors.Contains(otherType);
    }
    
    #if UNITY_EDITOR
    private CellData DrawCell(Rect rect, CellData value)
    {
        if (value == null)
            value = new CellData();
        
        if (value.Sprite != null)
        {
            UnityEditor.EditorGUI.DrawPreviewTexture(rect, value.Sprite.texture);
        }
        else
        {
            UnityEditor.EditorGUI.DrawRect(rect, GetTerrainColor(value.Terrain));
        }
        
        return value;
    }
    
    private Color GetTerrainColor(TerrainType type)
    {
        return type switch
        {
            TerrainType.Grass => new Color(0.2f, 0.8f, 0.2f),
            TerrainType.Stone => new Color(0.5f, 0.5f, 0.5f),
            TerrainType.Sand => new Color(0.9f, 0.8f, 0.5f),
            TerrainType.Water => new Color(0.2f, 0.4f, 0.8f),
            TerrainType.Obstacle => new Color(0.3f, 0.2f, 0.1f),
            TerrainType.Destructible => new Color(0.8f, 0.4f, 0.2f),
            _ => Color.white
        };
    }
    #endif
    
    [Button("Validate Doors")]
    public void ValidateDoors()
    {
        Debug.Log($"[BlockData] {blockName} - Validating doors...");
        
        foreach (var door in doors)
        {
            var cells = door.GetDoorCells(blockSize);
            Debug.Log($"  Door on {door.Side} at position {door.Position}, width {door.Width}");
            
            foreach (var cell in cells)
            {
                var cellData = GetCell(cell.x, cell.y);
                if (cellData != null && !cellData.Modifiers.isWalkable)
                {
                    Debug.LogWarning($"    Cell at {cell} is not walkable!");
                }
            }
        }
    }
}