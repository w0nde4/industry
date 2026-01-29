using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Building_", menuName = "Factory/Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Title("Basic Info")]
    public string buildingName = "New Building";
    [TextArea(2, 4)]
    public string description;
    
    [Title("Classification")]
    public BuildingCategory category = BuildingCategory.Production;
    public BuildingSubType subType = BuildingSubType.Producer;
    
    [Title("Grid Properties")]
    [MinValue(1)]
    public Vector2Int size = Vector2Int.one;
    
    [Tooltip("Можно ли поворачивать здание")]
    public bool canRotate = true;
    
    [Title("Prefab")]
    [Required]
    [AssetsOnly]
    public GameObject prefab;
    
    [Title("Building Behaviors")]
    [Tooltip("Список поведений здания")]
    public List<BehaviorConfig> behaviorConfigs = new();
    
    [Title("Building Behavior Settings")]
    [Tooltip("Можно ли апгрейдить здание")]
    public bool canUpgrade = false;
    
    [ShowIf("canUpgrade")]
    public BuildingData upgradeTo;
    
    [Tooltip("Можно ли перемещать после постройки")]
    public bool canMove = true;
    
    [Title("Visual Settings")]
    [PreviewField(100)]
    public Sprite icon;
    
    // Расширяемые настройки для будущего
    [Title("Advanced Settings")]
    [SerializeField] private bool showAdvanced = false;
    
    [ShowIf("showAdvanced")]
    [Tooltip("Кастомные данные в JSON формате")]
    [TextArea(3, 10)]
    public string customData;
    
    public Vector2Int GetRotatedSize(BuildingRotation rotation)
    {
        if (!canRotate || rotation == BuildingRotation.North || rotation == BuildingRotation.South)
        {
            return size;
        }
        
        // При ротации на 90° и 270° меняем X и Y местами
        return new Vector2Int(size.y, size.x);
    }
}