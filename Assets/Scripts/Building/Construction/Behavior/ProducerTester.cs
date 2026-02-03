using Sirenix.OdinInspector;
using UnityEngine;

public class ProducerTester : MonoBehaviour
{
    [Title("Test Setup")]
    [Required]
    [SerializeField] private PlacedBuilding testProducer;
    
    [Button("Log Producer Info")]
    private void LogProducerInfo()
    {
        if (testProducer == null)
        {
            Debug.LogError("Test producer not assigned!");
            return;
        }
        
        Debug.Log($"=== Producer Info ===");
        Debug.Log($"Building: {testProducer.Data.buildingName}");
        Debug.Log($"Position: {testProducer.GridPosition}");
        Debug.Log($"Behaviors: {testProducer.Behaviors.Count}");
        
        foreach (var behavior in testProducer.Behaviors)
        {
            Debug.Log($"  - {behavior.GetType().Name}");
        }
    }
    
    [Button("Find Producers in Scene")]
    private void FindProducers()
    {
        var allBuildings = BuildingService.Instance.AllBuildings;
        var producers = new System.Collections.Generic.List<PlacedBuilding>();
        
        foreach (var building in allBuildings)
        {
            var hasProductionBehavior = false;
            
            foreach (var behavior in building.Behaviors)
            {
                if (behavior is ProductionBehavior)
                {
                    hasProductionBehavior = true;
                    break;
                }
            }
            
            if (hasProductionBehavior)
            {
                producers.Add(building);
            }
        }
        
        Debug.Log($"=== Found {producers.Count} Producers ===");
        foreach (var producer in producers)
        {
            Debug.Log($"  - {producer.Data.buildingName} at {producer.GridPosition}");
        }
    }
}