using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private static LevelGenerator _instance;
    public static LevelGenerator Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            _instance = FindFirstObjectByType<LevelGenerator>();
            if (_instance == null)
            {
                Debug.LogError("LevelGenerator not found in scene!");
            }
            return _instance;
        }
    }
    
    [Title("Settings")]
    [SerializeField, Required] private GeneratorSettings settings;
    
    [Title("References")]
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private Transform levelRoot;
    
    [Title("Debug")]
    [SerializeField] private bool generateOnStart = true;
    [ShowInInspector, ReadOnly] private GeneratedLevel _currentLevel;
    [ShowInInspector, ReadOnly] private int _lastUsedSeed;
    
    private System.Random _random;
    private readonly Dictionary<BlockType, int> _blockUsageCount = new Dictionary<BlockType, int>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        if (gridSystem == null)
            gridSystem = FindFirstObjectByType<GridSystem>();
        
        if (levelRoot == null)
        {
            levelRoot = new GameObject("GeneratedLevel").transform;
            levelRoot.SetParent(transform);
        }
    }
    
    private void Start()
    {
        if (generateOnStart)
        {
            GenerateLevel().Forget();
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }
    
    #region Public API
    
    [Button("Generate Level", ButtonSizes.Large)]
    public async UniTask<GeneratedLevel> GenerateLevel()
    {
        if (settings == null)
        {
            Debug.LogError("[LevelGenerator] Settings not assigned!");
            return null;
        }
        
        ClearCurrentLevel();
        
        _lastUsedSeed = settings.GetActualSeed();
        _random = new System.Random(_lastUsedSeed);
        _blockUsageCount.Clear();
        
        Debug.Log($"[LevelGenerator] Starting generation with seed: {_lastUsedSeed}");
        
        var level = new GeneratedLevel(settings.LevelGridSize, _lastUsedSeed, levelRoot);
        
        // Шаг 1: Размещаем стартовый блок
        PlaceStarterBlock(level);
        await UniTask.Yield();
        
        // Шаг 2: Размещаем обязательные блоки
        await PlaceMandatoryBlocks(level);
        await UniTask.Yield();
        
        // Шаг 3: Заполняем остальные позиции
        await FillRemainingPositions(level);
        await UniTask.Yield();
        
        // Шаг 4: Валидация
        if (settings.ValidatePaths)
        {
            ValidateLevel(level);
        }
        
        // Шаг 5: Применяем к GridSystem
        ApplyToGrid(level);
        
        _currentLevel = level;
        
        Debug.Log("[LevelGenerator] Generation complete!");
        level.LogInfo();
        
        return level;
    }
    
    [Button("Clear Level")]
    public void ClearCurrentLevel()
    {
        if (_currentLevel != null)
        {
            _currentLevel.Clear();
            _currentLevel = null;
        }
        
        while (levelRoot.childCount > 0)
        {
            var child = levelRoot.GetChild(0);
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
            #endif
                Destroy(child.gameObject);
        }
    }
    
    #endregion
    
    #region Generation Steps
    
    private void PlaceStarterBlock(GeneratedLevel level)
    {
        if (settings.StarterBlock == null)
        {
            Debug.LogError("[LevelGenerator] Starter block not assigned in GeneratorSettings!");
            return;
        }
        
        Debug.Log($"[LevelGenerator] Placing starter block: {settings.StarterBlock.BlockName} (Type: {settings.StarterBlock.BlockType})");
        
        var position = settings.StarterPosition;
        var block = CreateBlock(settings.StarterBlock, position);
        
        if (block != null)
        {
            level.SetBlock(position.x, position.y, block);
            IncrementBlockUsage(settings.StarterBlock);
            
            Debug.Log($"[LevelGenerator] Placed starter block at {position}");
        }
        else
        {
            Debug.LogError("[LevelGenerator] Failed to create starter block!");
        }
    }
    
    private async UniTask PlaceMandatoryBlocks(GeneratedLevel level)
    {
        var mandatoryBlocks = settings.AvailableBlocks
            .Where(b => b.IsMandatory && b != settings.StarterBlock)
            .ToList();
        
        foreach (var blockData in mandatoryBlocks)
        {
            var placed = TryPlaceBlockRandomly(level, blockData);
            
            if (!placed)
            {
                Debug.LogWarning($"[LevelGenerator] Failed to place mandatory block: {blockData.BlockName}");
            }
            
            await UniTask.Yield();
        }
    }
    
    private async UniTask FillRemainingPositions(GeneratedLevel level)
    {
        var positions = GetEmptyPositions(level);
        
        Shuffle(positions);
        
        foreach (var pos in positions)
        {
            if (_random.NextDouble() > settings.BlockPlacementChance)
            {
                continue;
            }
            
            var blockData = SelectRandomBlock(level, pos);
            
            if (blockData != null)
            {
                var block = CreateBlock(blockData, pos);
                if (block != null)
                {
                    level.SetBlock(pos.x, pos.y, block);
                    IncrementBlockUsage(blockData);
                }
            }
            
            await UniTask.Yield();
        }
        
        await EnsureMinimumCombatBlocks(level);
    }
    
    private async UniTask EnsureMinimumCombatBlocks(GeneratedLevel level)
    {
        var combatBlocks = level.GetBlocksByType(BlockType.Combat);
        var currentCount = combatBlocks.Count;
        
        if (currentCount >= settings.MinCombatBlocks)
            return;
        
        var needed = settings.MinCombatBlocks - currentCount;
        Debug.Log($"[LevelGenerator] Need {needed} more combat blocks");
        
        var combatBlockData = settings.AvailableBlocks
            .Where(b => b.BlockType == BlockType.Combat)
            .ToList();
        
        if (combatBlockData.Count == 0)
        {
            Debug.LogWarning("[LevelGenerator] No combat blocks available!");
            return;
        }
        
        for (int i = 0; i < needed; i++)
        {
            var blockData = combatBlockData[_random.Next(combatBlockData.Count)];
            var placed = TryPlaceBlockRandomly(level, blockData);
            
            if (!placed)
            {
                Debug.LogWarning($"[LevelGenerator] Failed to place combat block {i + 1}/{needed}");
            }
            
            await UniTask.Yield();
        }
    }
    
    #endregion
    
    #region Block Placement
    
    private bool TryPlaceBlockRandomly(GeneratedLevel level, BlockData blockData)
    {
        var emptyPositions = GetEmptyPositions(level);
        
        if (emptyPositions.Count == 0)
            return false;
        
        Shuffle(emptyPositions);
        
        foreach (var pos in emptyPositions)
        {
            if (CanPlaceBlock(level, blockData, pos))
            {
                var block = CreateBlock(blockData, pos);
                if (block != null)
                {
                    level.SetBlock(pos.x, pos.y, block);
                    IncrementBlockUsage(blockData);
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private bool CanPlaceBlock(GeneratedLevel level, BlockData blockData, Vector2Int position)
    {
        if (!blockData.CanRepeat)
        {
            if (GetBlockUsageCount(blockData) >= 1)
                return false;
        }
        else if (GetBlockUsageCount(blockData) >= blockData.MaxRepeats)
        {
            return false;
        }
        
        if (!CheckNeighborConstraints(level, blockData, position))
            return false;
        
        if (settings.PreventFullPerimeterClosure)
        {
            if (!CheckPerimeterConstraint(level, position))
                return false;
        }
        
        return true;
    }
    
    private bool CheckNeighborConstraints(GeneratedLevel level, BlockData blockData, Vector2Int position)
    {
        var directions = new[] { DoorSide.North, DoorSide.East, DoorSide.South, DoorSide.West };
        
        foreach (var dir in directions)
        {
            var neighbor = level.GetNeighbor(position, dir);
            
            if (neighbor == null)
                continue;
            
            if (!blockData.CanBeNeighborWith(neighbor.Data.BlockType))
                return false;
            
            if (!neighbor.Data.CanBeNeighborWith(blockData.BlockType))
                return false;
        }
        
        return true;
    }
    
    private bool CheckPerimeterConstraint(GeneratedLevel level, Vector2Int position)
    {
        // TODO: Реализовать проверку что закрытые блоки не окружают весь периметр
        return true;
    }
    
    private Block CreateBlock(BlockData blockData, Vector2Int gridPosition)
    {
        var blockObj = new GameObject($"Block_{blockData.BlockName}_{gridPosition.x}_{gridPosition.y}");
        blockObj.transform.SetParent(levelRoot);
        
        var worldPos = new Vector3(
            gridPosition.x * blockData.BlockSize,
            gridPosition.y * blockData.BlockSize,
            0
        );
        blockObj.transform.position = worldPos;
        
        var block = blockObj.AddComponent<Block>();
        block.Initialize(blockData, gridPosition * blockData.BlockSize);
        
        return block;
    }
    
    #endregion
    
    #region Validation
    
    private void ValidateLevel(GeneratedLevel level)
    {
        Debug.Log("[LevelGenerator] Validating level...");
        
        for (int x = 0; x < level.GridSize.x; x++)
        {
            for (int y = 0; y < level.GridSize.y; y++)
            {
                var block = level.GetBlock(x, y);
                if (block == null)
                    continue;
                
                ValidateBlockDoors(level, new Vector2Int(x, y));
            }
        }
    }
    
    private void ValidateBlockDoors(GeneratedLevel level, Vector2Int position)
    {
        var block = level.GetBlock(position);
        if (block == null)
            return;
        
        var directions = new[] { DoorSide.North, DoorSide.East, DoorSide.South, DoorSide.West };
        
        foreach (var dir in directions)
        {
            var neighbor = level.GetNeighbor(position, dir);
            if (neighbor == null)
                continue;
            
            var hasDoor = block.Data.HasDoorOn(dir);
            var neighborHasDoor = neighbor.Data.HasDoorOn(BlockDoor.GetOppositeSide(dir));
            
            if (hasDoor != neighborHasDoor)
            {
                Debug.LogWarning($"[LevelGenerator] Door mismatch between {block.Data.BlockName} and {neighbor.Data.BlockName} on {dir} side");
            }
        }
    }
    
    #endregion
    
    #region Grid Integration
    
    private void ApplyToGrid(GeneratedLevel level)
    {
        if (gridSystem == null)
        {
            Debug.LogWarning("[LevelGenerator] GridSystem not assigned!");
            return;
        }
        
        var blocks = level.GetAllBlocks();
        
        foreach (var block in blocks)
        {
            block.ApplyToGrid(gridSystem);
        }
        
        Debug.Log($"[LevelGenerator] Applied {blocks.Count} blocks to GridSystem");
    }
    
    #endregion
    
    #region Helper Methods
    
    private List<Vector2Int> GetEmptyPositions(GeneratedLevel level)
    {
        var positions = new List<Vector2Int>();
        
        for (int x = 0; x < level.GridSize.x; x++)
        {
            for (int y = 0; y < level.GridSize.y; y++)
            {
                if (level.GetBlock(x, y) == null)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return positions;
    }
    
    private BlockData SelectRandomBlock(GeneratedLevel level, Vector2Int position)
    {
        var availableBlocks = settings.AvailableBlocks
            .Where(b => CanPlaceBlock(level, b, position))
            .ToList();
        
        if (availableBlocks.Count == 0)
            return null;
        
        return availableBlocks[_random.Next(availableBlocks.Count)];
    }
    
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    private void IncrementBlockUsage(BlockData blockData)
    {
        if (!_blockUsageCount.ContainsKey(blockData.BlockType))
            _blockUsageCount[blockData.BlockType] = 0;
        
        _blockUsageCount[blockData.BlockType]++;
    }
    
    private int GetBlockUsageCount(BlockData blockData)
    {
        return _blockUsageCount.GetValueOrDefault(blockData.BlockType, 0);
    }
    
    #endregion
}