using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BuildingService : MonoBehaviour
{
    private static BuildingService _instance;
    public static BuildingService Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            _instance = FindFirstObjectByType<BuildingService>();
            if (_instance == null)
            {
                Debug.LogError("BuildingManager not found in scene!");
            }
            return _instance;
        }
    }
    
    [Title("Service References")]
    [SerializeField] private BuildingPlacer buildingPlacer;
    [SerializeField] private BuildingBehaviorManager behaviorManager;
    [SerializeField] private GridSystem gridSystem;
    
    [Title("Hierarchy Organization")]
    [SerializeField] private Transform buildingsRoot;
    
    public BuildingPlacer BuildingPlacer => buildingPlacer;
    public BuildingBehaviorManager BehaviorManager => behaviorManager;
    public GridSystem Grid => gridSystem;
    
    private readonly Dictionary<BuildingCategory, Transform> _categoryRoots = new Dictionary<BuildingCategory, Transform>();
    private readonly Dictionary<BuildingSubType, Transform> _subTypeRoots = new Dictionary<BuildingSubType, Transform>();
    private readonly List<PlacedBuilding> _allBuildings = new List<PlacedBuilding>();
    private readonly List<PlacedBuilding> _tempBuildings = new List<PlacedBuilding>();
    
    public IReadOnlyList<PlacedBuilding> AllBuildings => _allBuildings;
    
    [ShowInInspector, ReadOnly]
    private int TotalBuildings => _allBuildings.Count;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        if (buildingPlacer == null)
            buildingPlacer = GetComponent<BuildingPlacer>();
        
        if (behaviorManager == null)
            behaviorManager = GetComponent<BuildingBehaviorManager>();
        
        if (gridSystem == null)
            gridSystem = GetComponent<GridSystem>();
        
        if (buildingsRoot == null)
        {
            buildingsRoot = new GameObject("Buildings").transform;
            buildingsRoot.SetParent(transform);
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }
    
    #region Static API
    
    public static void StartBuilding(BuildingData data) => Instance.BuildingPlacer.StartBuildMode(data);
    public static void StopBuilding() => Instance.BuildingPlacer.StopBuildMode();
    public static PlacedBuilding GetBuildingAt(Vector2Int gridPos) => Instance.GetBuildingAtPosition(gridPos);
    
    #endregion
    
    #region Building Registration
    
    public void RegisterBuilding(PlacedBuilding building)
    {
        if (_allBuildings.Contains(building)) return;
        
        _allBuildings.Add(building);
        OrganizeInHierarchy(building);
        building.OnBuildingDestroyed += OnBuildingDestroyed;
    }
    
    public void UnregisterBuilding(PlacedBuilding building)
    {
        if (!_allBuildings.Contains(building)) return;
        
        _allBuildings.Remove(building);
        building.OnBuildingDestroyed -= OnBuildingDestroyed;
    }
    
    private void OnBuildingDestroyed(PlacedBuilding building)
    {
        UnregisterBuilding(building);
    }
    
    #endregion
    
    #region Hierarchy Organization
    
    private void OrganizeInHierarchy(PlacedBuilding building)
    {
        var data = building.Data;
        
        if (!_categoryRoots.TryGetValue(data.category, out var categoryRoot))
        {
            var categoryObj = new GameObject(data.category.ToString());
            categoryObj.transform.SetParent(buildingsRoot);
            categoryRoot = categoryObj.transform;
            _categoryRoots[data.category] = categoryRoot;
        }
        
        if (!_subTypeRoots.TryGetValue(data.subType, out var subTypeRoot))
        {
            var subTypeObj = new GameObject(data.subType.ToString() + "s");
            subTypeObj.transform.SetParent(categoryRoot);
            subTypeRoot = subTypeObj.transform;
            _subTypeRoots[data.subType] = subTypeRoot;
        }
        
        building.transform.SetParent(subTypeRoot);
        building.gameObject.name = $"{data.buildingName}_{_allBuildings.Count:000}";
    }
    
    #endregion
    
    #region Query Methods
    
    public void GetBuildingsByCategory(BuildingCategory category, List<PlacedBuilding> result)
    {
        result.Clear();
        
        foreach (var building in _allBuildings)
        {
            if (building.Data.category == category)
            {
                result.Add(building);
            }
        }
    }
    
    public void GetBuildingsBySubType(BuildingSubType subType, List<PlacedBuilding> result)
    {
        result.Clear();
        
        foreach (var building in _allBuildings)
        {
            if (building.Data.subType == subType)
            {
                result.Add(building);
            }
        }
    }
    
    public PlacedBuilding GetBuildingAtPosition(Vector2Int gridPosition)
    {
        foreach (var building in _allBuildings)
        {
            var size = building.Size;
            var pos = building.GridPosition;
            
            if (gridPosition.x >= pos.x && gridPosition.x < pos.x + size.x &&
                gridPosition.y >= pos.y && gridPosition.y < pos.y + size.y)
            {
                return building;
            }
        }
        
        return null;
    }
    
    #endregion
    
    #region Debug/Editor Utilities
    
    [Button("Clear All Buildings")]
    private void ClearAllBuildings()
    {
        _tempBuildings.Clear();
        _tempBuildings.AddRange(_allBuildings);
        
        foreach (var building in _tempBuildings)
        {
            if (building != null)
            {
                building.Demolish();
            }
        }
        
        _allBuildings.Clear();
        _categoryRoots.Clear();
        _subTypeRoots.Clear();
    }
    
    [Button("Log Buildings Info")]
    private void LogBuildingsInfo()
    {
        Debug.Log($"=== Buildings Info ===");
        Debug.Log($"Total Buildings: {_allBuildings.Count}");
        
        var countBySubType = new Dictionary<BuildingSubType, int>();
        
        foreach (var building in _allBuildings)
        {
            var subType = building.Data.subType;
            
            countBySubType.TryAdd(subType, 0);
            
            countBySubType[subType]++;
        }
        
        foreach (var kvp in countBySubType)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
    
    #endregion
}