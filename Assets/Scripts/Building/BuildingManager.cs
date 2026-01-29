using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Title("Hierarchy Organization")]
    [SerializeField] private Transform buildingsRoot;
    
    private readonly Dictionary<BuildingCategory, Transform> _categoryRoots = new Dictionary<BuildingCategory, Transform>();
    private readonly Dictionary<BuildingSubType, Transform> _subTypeRoots = new Dictionary<BuildingSubType, Transform>();
    
    private readonly List<PlacedBuilding> _allBuildings = new List<PlacedBuilding>();
    
    public IReadOnlyList<PlacedBuilding> AllBuildings => _allBuildings;
    
    private void Awake()
    {
        if (buildingsRoot == null)
        {
            buildingsRoot = new GameObject("Buildings").transform;
            buildingsRoot.SetParent(transform);
        }
    }
    
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
    
    private void OrganizeInHierarchy(PlacedBuilding building)
    {
        var data = building.Data;
        
        // Получаем или создаем папку категории
        if (!_categoryRoots.TryGetValue(data.category, out Transform categoryRoot))
        {
            var categoryObj = new GameObject(data.category.ToString());
            categoryObj.transform.SetParent(buildingsRoot);
            categoryRoot = categoryObj.transform;
            _categoryRoots[data.category] = categoryRoot;
        }
        
        // Получаем или создаем папку подтипа
        if (!_subTypeRoots.TryGetValue(data.subType, out Transform subTypeRoot))
        {
            var subTypeObj = new GameObject(data.subType.ToString() + "s");
            subTypeObj.transform.SetParent(categoryRoot);
            subTypeRoot = subTypeObj.transform;
            _subTypeRoots[data.subType] = subTypeRoot;
        }
        
        building.transform.SetParent(subTypeRoot);
        building.gameObject.name = $"{data.buildingName}_{_allBuildings.Count:000}";
    }
    
    public List<PlacedBuilding> GetBuildingsByCategory(BuildingCategory category)
    {
        return _allBuildings.Where(b => b.Data.category == category).ToList();
    }
    
    public List<PlacedBuilding> GetBuildingsBySubType(BuildingSubType subType)
    {
        return _allBuildings.Where(b => b.Data.subType == subType).ToList();
    }
    
    public PlacedBuilding GetBuildingAtPosition(Vector2Int gridPosition)
    {
        return _allBuildings.FirstOrDefault(b => 
        {
            var size = b.Size;
            var pos = b.GridPosition;
            
            return gridPosition.x >= pos.x && gridPosition.x < pos.x + size.x &&
                   gridPosition.y >= pos.y && gridPosition.y < pos.y + size.y;
        });
    }
    
    [Button("Clear All Buildings")]
    private void ClearAllBuildings()
    {
        var buildingsToDestroy = _allBuildings.ToList();
        foreach (var building in buildingsToDestroy)
        {
            building.Demolish();
        }
        
        _allBuildings.Clear();
        _categoryRoots.Clear();
        _subTypeRoots.Clear();
    }
    
    [Button("Log Buildings Info")]
    private void LogBuildingsInfo()
    {
        Debug.Log($"Total Buildings: {_allBuildings.Count}");
        
        var grouped = _allBuildings.GroupBy(b => b.Data.subType);
        foreach (var group in grouped)
        {
            Debug.Log($"  {group.Key}: {group.Count()}");
        }
    }
}