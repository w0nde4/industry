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
    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawCell", Transpose = true)]
    [SerializeField, ReadOnly] private CellData[] cells = new CellData[256]; // 16x16 = 256 (одномерный для сериализации!)
    
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
    public List<BlockDoor> Doors => doors;
    public List<BlockType> AllowedNeighbors => allowedNeighbors;
    public bool CanRepeat => canRepeat;
    public int MaxRepeats => maxRepeats;
    public bool IsMandatory => isMandatory;
    
    // Хелпер для конвертации 2D координат в 1D индекс
    private int GetIndex(int x, int y) => y * blockSize + x;
    
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
        cells = new CellData[blockSize * blockSize];
        
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = new CellData();
        }
        
        Debug.Log($"[BlockData] Initialized {blockSize}x{blockSize} cells ({cells.Length} total)");
    }
    
    public CellData GetCell(int x, int y)
    {
        if (x < 0 || x >= blockSize || y < 0 || y >= blockSize)
            return null;
        
        var index = GetIndex(x, y);
        if (index < 0 || index >= cells.Length)
            return null;
        
        return cells[index];
    }
    
    public void SetCell(int x, int y, CellData cellData)
    {
        if (x < 0 || x >= blockSize || y < 0 || y >= blockSize)
            return;
        
        var index = GetIndex(x, y);
        if (index < 0 || index >= cells.Length)
            return;
        
        cells[index] = cellData;
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
    private CellData DrawCell(Rect rect, int index)
    {
        if (cells == null || index < 0 || index >= cells.Length)
        {
            UnityEditor.EditorGUI.DrawRect(rect, Color.black);
            return null;
        }
        
        var value = cells[index];
        if (value == null)
        {
            value = new CellData();
            cells[index] = value;
        }
        
        if (value.Sprite != null)
        {
            DrawSprite(rect, value.Sprite);
        }
        else
        {
            var color = GetTerrainColor(value.Terrain);
            if (color.a > 0)
            {
                UnityEditor.EditorGUI.DrawRect(rect, color);
            }
        }
        
        return value;
    }
    
    private void DrawSprite(Rect rect, Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
            return;

        var texture = sprite.texture;
        var spriteRect = sprite.textureRect;
        
        // Нормализуем UV координаты
        var uvRect = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height
        );
        
        // Отрисовываем с правильными UV
        UnityEngine.GUI.DrawTextureWithTexCoords(rect, texture, uvRect);
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
            _ => new Color(0, 0, 0, 0) // Прозрачный
        };
    }
    #endif
    
    [Button("Validate Doors")]
    public void ValidateDoors()
    {
        Debug.Log($"[BlockData] {blockName} - Validating doors...");
        
        foreach (var door in doors)
        {
            var doorCells = door.GetDoorCells(blockSize);
            Debug.Log($"  Door on {door.Side} at position {door.Position}, width {door.Width}");
            
            foreach (var cell in doorCells)
            {
                var cellData = GetCell(cell.x, cell.y);
                if (cellData != null && !cellData.Modifiers.isWalkable)
                {
                    Debug.LogWarning($"    Cell at {cell} is not walkable!");
                }
            }
        }
    }
    
    [Button("Count Cells With Sprites")]
    private void CountCellsWithSprites()
    {
        if (cells == null)
        {
            Debug.LogWarning("[BlockData] Cells array is null!");
            return;
        }
        
        int count = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] != null && cells[i].Sprite != null)
                count++;
        }
        
        Debug.Log($"[BlockData] {blockName}: {count}/{cells.Length} cells have sprites");
    }
}