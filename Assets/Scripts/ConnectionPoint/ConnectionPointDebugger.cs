using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConnectionPointDebugger : MonoBehaviour
{
    [Required]
    [SerializeField] private PlacedBuilding targetBuilding;
    
    [Button("Log Connection Points")]
    private void LogConnectionPoints()
    {
        if (targetBuilding == null)
        {
            Debug.LogError("Target building not assigned!");
            return;
        }
        
        var points = targetBuilding.ConnectionPoints;
        var inputs = targetBuilding.Inputs;
        var outputs = targetBuilding.Outputs;
        
        Debug.Log($"=== {targetBuilding.Data.buildingName} Connection Points ===");
        Debug.Log($"Total: {points.Length}");
        Debug.Log($"Inputs: {inputs.Length}");
        Debug.Log($"Outputs: {outputs.Length}");
        
        foreach (var point in points)
        {
            Debug.Log($"  - {point.Type}: World({point.WorldPosition})");
        }
    }
    
    [Button("Validate Building")]
    private void ValidateBuilding()
    {
        if (targetBuilding == null)
        {
            Debug.LogError("Target building not assigned!");
            return;
        }
        
        var isValid = ConnectionPointValidator.ValidateBuilding(targetBuilding, out string error);
        
        if (isValid)
        {
            Debug.Log($"✓ {targetBuilding.Data.buildingName} is valid!");
        }
        else
        {
            Debug.LogError($"✗ {targetBuilding.Data.buildingName} validation failed: {error}");
        }
    }
    
    [Button("Find Adjacent Points")]
    private void FindAdjacentPoints()
    {
        if (targetBuilding == null)
        {
            Debug.LogError("Target building not assigned!");
            return;
        }
        
        var settings = Resources.Load<ConnectionPointSettings>("ConnectionPointSettings");
        if (settings == null)
        {
            Debug.LogError("ConnectionPointSettings not found in Resources!");
            return;
        }
        
        var allBuildings = new List<PlacedBuilding>(GameObject.FindObjectsByType<PlacedBuilding>(FindObjectsSortMode.None));
        var myPoints = targetBuilding.ConnectionPoints;
        
        var adjacentList = new List<ConnectionPoint>(20);
        
        foreach (var point in myPoints)
        {
            ConnectionPointHelper.GetAdjacentConnectionPoints(point, allBuildings, settings, adjacentList);
            Debug.Log($"{point.Type} at {point.WorldPosition} has {adjacentList.Count} adjacent points");
        }
    }
}