using System.Linq;
using UnityEngine;

public static class ConnectionPointValidator
{
    public static bool ValidateBuilding(PlacedBuilding building, out string error)
    {
        error = "";
        
        var points = building.ConnectionPoints;
        
        if (points == null || points.Length == 0)
        {
            error = "Building has no connection points";
            return false;
        }
        
        var inputs = 0;
        var outputs = 0;
        
        foreach (var point in points)
        {
            if (point.Owner == null)
            {
                error = $"Connection point {point.name} has no owner assigned";
                return false;
            }
            if(point.Type == ConnectionType.Input) inputs++;
            else if(point.Type == ConnectionType.Output) outputs++;
        }

        if (inputs == 0 || outputs == 0)
        {
            error = "Building has connection points but no inputs or outputs defined";
            return false;
        }
        
        return true;
    }
    
    public static bool CanConnect(
        ConnectionPoint output, 
        ConnectionPoint input,
        ConnectionPointSettings settings,
        out string reason)
    {
        reason = "";
        
        if (output.Type != ConnectionType.Output)
        {
            reason = "First point must be Output";
            return false;
        }
        
        if (input.Type != ConnectionType.Input)
        {
            reason = "Second point must be Input";
            return false;
        }
        
        if (output.Owner == input.Owner)
        {
            reason = "Cannot connect building to itself";
            return false;
        }
        
        var distance = Vector3.Distance(output.WorldPosition, input.WorldPosition);
        if (distance > settings.maxConnectionDistance)
        {
            reason = $"Distance too large: {distance:F2} (max: {settings.maxConnectionDistance:F2}))";
            return false;
        }
        
        return true;
    }
}