using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ConnectionPointHelper
{
    public static ConnectionPoint[] GetAllConnectionPoints(PlacedBuilding building)
    {
        return building?.ConnectionPoints ?? System.Array.Empty<ConnectionPoint>(); 
    }
    
    public static ConnectionPoint[] GetInputs(PlacedBuilding building)
    {
        return building?.Inputs ?? System.Array.Empty<ConnectionPoint>();
    }
    
    public static ConnectionPoint[] GetOutputs(PlacedBuilding building)
    {
        return building?.Outputs ?? System.Array.Empty<ConnectionPoint>();
    }
    
    public static ConnectionPoint GetClosestInput(
        Vector3 position,
        List<PlacedBuilding> buildings,
        ConnectionPointSettings settings)
    {
        ConnectionPoint closest = null;
        var minDistanceSqr = settings.searchRadius * settings.searchRadius;
        
        foreach (var building in buildings)
        {
            var inputs = building.Inputs;
            
            if(inputs == null || inputs.Length == 0) continue;
            
            foreach (var input in inputs)
            {
                var distanceSqr = (position - input.WorldPosition).sqrMagnitude;
                
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    closest = input;
                }
            }
        }
        
        return closest;
    }
    
    public static ConnectionPoint GetClosestOutput(
        Vector3 position, 
        List<PlacedBuilding> buildings, 
        ConnectionPointSettings settings)
    {
        ConnectionPoint closest = null;
        var minDistanceSqr = settings.searchRadius * settings.searchRadius;
        
        foreach (var building in buildings)
        {
            var outputs = building.Outputs;
            
            if(outputs == null || outputs.Length == 0) continue;
            
            foreach (var output in outputs)
            {
                var distanceSqr = (position - output.WorldPosition).sqrMagnitude;
                
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    closest = output;
                }
            }
        }
        
        return closest;
    }
    
    public static void GetAdjacentConnectionPoints(
        ConnectionPoint point, 
        List<PlacedBuilding> buildings, 
        ConnectionPointSettings settings, 
        List<ConnectionPoint> result)
    {
        result.Clear();
        
        var searchRadiusSqr = settings.searchRadius * settings.searchRadius;
        
        foreach (var building in buildings)
        {
            if (building == point.Owner) continue;

            var allPoints = building.ConnectionPoints;
            
            if(allPoints == null || allPoints.Length == 0) continue;
            
            foreach (var otherPoint in allPoints)
            {
                var distanceSqr = (point.WorldPosition - otherPoint.WorldPosition).sqrMagnitude;
                
                if (distanceSqr < searchRadiusSqr)
                {
                    result.Add(otherPoint);
                }
            }
        }
    }
}