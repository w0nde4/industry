using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorSettings", menuName = "Level Generation/Generator Settings")]
public class GeneratorSettings : ScriptableObject
{
    [Title("Level Size")]
    [SerializeField, Tooltip("Размер уровня в блоках (например, 3x3 = 9 блоков по 16x16)")]
    private Vector2Int levelGridSize = new Vector2Int(3, 3);
    
    [Title("Block Pool")]
    [SerializeField, Tooltip("Все доступные блоки для генерации")]
    private List<BlockData> availableBlocks = new List<BlockData>();
    
    [Title("Generation Rules")]
    [SerializeField, Tooltip("Стартовый блок (обязательный)")]
    private BlockData starterBlock;
    
    [SerializeField, Tooltip("Позиция стартового блока")]
    private Vector2Int starterPosition = new Vector2Int(0, 0);
    
    [SerializeField, Tooltip("Seed для генерации (0 = случайный)")]
    private int seed = 0;
    
    [SerializeField, Range(0f, 1f), Tooltip("Вероятность размещения блока (0-1)")]
    private float blockPlacementChance = 0.85f;
    
    [Title("Placement Constraints")]
    [SerializeField, Tooltip("Закрытые блоки не могут окружать весь периметр")]
    private bool preventFullPerimeterClosure = true;
    
    [SerializeField, Tooltip("Минимальное количество боевых блоков")]
    private int minCombatBlocks = 1;
    
    [Title("Pathfinding")]
    [SerializeField, Tooltip("Проверять пути между блоками после генерации")]
    private bool validatePaths = true;
    
    [SerializeField, Tooltip("Минимальная ширина коридора между блоками")]
    private int minCorridorWidth = 1;
    
    public Vector2Int LevelGridSize => levelGridSize;
    public List<BlockData> AvailableBlocks => availableBlocks;
    public BlockData StarterBlock => starterBlock;
    public Vector2Int StarterPosition => starterPosition;
    public int Seed => seed;
    public float BlockPlacementChance => blockPlacementChance;
    public bool PreventFullPerimeterClosure => preventFullPerimeterClosure;
    public int MinCombatBlocks => minCombatBlocks;
    public bool ValidatePaths => validatePaths;
    public int MinCorridorWidth => minCorridorWidth;
    
    public int GetActualSeed()
    {
        return seed == 0 ? Random.Range(1, int.MaxValue) : seed;
    }
    
    [Button("Validate Settings")]
    private void ValidateSettings()
    {
        Debug.Log("=== Generator Settings Validation ===");
        
        if (starterBlock == null)
        {
            Debug.LogError("Starter Block is not assigned!");
        }
        else
        {
            Debug.Log($"Starter Block: {starterBlock.BlockName}");
        }
        
        Debug.Log($"Level Size: {levelGridSize.x}x{levelGridSize.y} blocks");
        Debug.Log($"Total Cells: {levelGridSize.x * 16}x{levelGridSize.y * 16}");
        Debug.Log($"Available Blocks: {availableBlocks.Count}");
        
        var mandatoryCount = 0;
        foreach (var block in availableBlocks)
        {
            if (block.IsMandatory)
            {
                mandatoryCount++;
                Debug.Log($"  Mandatory: {block.BlockName} ({block.BlockType})");
            }
        }
        
        Debug.Log($"Mandatory Blocks: {mandatoryCount}");
        
        if (mandatoryCount > levelGridSize.x * levelGridSize.y)
        {
            Debug.LogError($"Too many mandatory blocks! ({mandatoryCount} > {levelGridSize.x * levelGridSize.y})");
        }
    }
    
    [Button("Log Block Statistics")]
    private void LogBlockStatistics()
    {
        Debug.Log("=== Block Pool Statistics ===");
        
        var typeCount = new Dictionary<BlockType, int>();
        
        foreach (var block in availableBlocks)
        {
            if (!typeCount.ContainsKey(block.BlockType))
                typeCount[block.BlockType] = 0;
            
            typeCount[block.BlockType]++;
        }
        
        foreach (var kvp in typeCount)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} blocks");
        }
    }
}