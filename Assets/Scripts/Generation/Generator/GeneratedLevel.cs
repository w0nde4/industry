using System.Collections.Generic;
using UnityEngine;

public class GeneratedLevel
{
    private readonly Block[,] _blockGrid;
    private readonly Vector2Int _gridSize;
    private readonly int _seed;
    private readonly Transform _levelRoot;
    
    public Block[,] BlockGrid => _blockGrid;
    public Vector2Int GridSize => _gridSize;
    public int Seed => _seed;
    public Transform LevelRoot => _levelRoot;
    
    public GeneratedLevel(Vector2Int gridSize, int seed, Transform levelRoot)
    {
        _gridSize = gridSize;
        _seed = seed;
        _levelRoot = levelRoot;
        _blockGrid = new Block[gridSize.x, gridSize.y];
    }
    
    public void SetBlock(int x, int y, Block block)
    {
        if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y)
        {
            Debug.LogWarning($"[GeneratedLevel] Position ({x},{y}) out of bounds!");
            return;
        }
        
        _blockGrid[x, y] = block;
    }
    
    public Block GetBlock(int x, int y)
    {
        if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y)
            return null;
        
        return _blockGrid[x, y];
    }
    
    public Block GetBlock(Vector2Int position)
    {
        return GetBlock(position.x, position.y);
    }
    
    public List<Block> GetAllBlocks()
    {
        var blocks = new List<Block>();
        
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                if (_blockGrid[x, y] != null)
                {
                    blocks.Add(_blockGrid[x, y]);
                }
            }
        }
        
        return blocks;
    }
    
    public List<Block> GetBlocksByType(BlockType type)
    {
        var blocks = new List<Block>();
        
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var block = _blockGrid[x, y];
                if (block != null && block.Data.BlockType == type)
                {
                    blocks.Add(block);
                }
            }
        }
        
        return blocks;
    }
    
    public Block GetNeighbor(Vector2Int position, DoorSide direction)
    {
        var offset = direction switch
        {
            DoorSide.North => new Vector2Int(0, 1),
            DoorSide.East => new Vector2Int(1, 0),
            DoorSide.South => new Vector2Int(0, -1),
            DoorSide.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };
        
        return GetBlock(position + offset);
    }
    
    public void Clear()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var block = _blockGrid[x, y];
                if (block != null)
                {
                    Object.Destroy(block.gameObject);
                    _blockGrid[x, y] = null;
                }
            }
        }
    }
    
    public void LogInfo()
    {
        Debug.Log("=== Generated Level Info ===");
        Debug.Log($"Grid Size: {_gridSize.x}x{_gridSize.y}");
        Debug.Log($"Seed: {_seed}");
        
        var typeCount = new Dictionary<BlockType, int>();
        var totalBlocks = 0;
        
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var block = _blockGrid[x, y];
                if (block != null)
                {
                    totalBlocks++;
                    var type = block.Data.BlockType;
                    
                    if (!typeCount.ContainsKey(type))
                        typeCount[type] = 0;
                    
                    typeCount[type]++;
                }
            }
        }
        
        Debug.Log($"Total Blocks: {totalBlocks}/{_gridSize.x * _gridSize.y}");
        
        foreach (var kvp in typeCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
}