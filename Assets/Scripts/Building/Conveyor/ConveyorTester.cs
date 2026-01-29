using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorTester : MonoBehaviour
{
    [Title("Test Setup")]
    [Required]
    [SerializeField] private ConveyorBuilding testConveyor;
    
    [Required]
    [SerializeField] private ResourceData testResource;
    
    [SerializeField] private int spawnAmount = 1;
    
    [Title("Auto Spawn")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float spawnInterval = 2f;
    
    private float _lastSpawnTime;
    
    private void Update()
    {
        if (autoSpawn && Time.time - _lastSpawnTime >= spawnInterval)
        {
            SpawnResourceOnConveyor();
            _lastSpawnTime = Time.time;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnResourceOnConveyor();
        }
    }
    
    [Button("Spawn Resource (R)")]
    private void SpawnResourceOnConveyor()
    {
        if (testConveyor == null)
        {
            Debug.LogError("Test conveyor not assigned!");
            return;
        }
        
        if (testResource == null)
        {
            Debug.LogError("Test resource not assigned!");
            return;
        }
        
        var inputPoint = testConveyor.GetComponentsInChildren<ConnectionPoint>()
            .FirstOrDefault(p => p.Type == ConnectionType.Input);
        
        if (inputPoint == null)
        {
            Debug.LogError("Conveyor has no input point!");
            return;
        }
        
        var resource = ResourceService.SpawnResource(
            testResource,
            inputPoint.WorldPosition,
            spawnAmount
        );
        
        if (!testConveyor.CanAcceptResource(resource))
        {
            Debug.LogWarning("Conveyor cannot accept resource!");
            ResourceService.DestroyResource(resource);
            return;
        }
        
        testConveyor.AcceptResource(resource);
        
        Debug.Log($"Spawned {testResource.resourceName} x{spawnAmount} on conveyor");
    }
    
    [Button("Log Conveyor Info")]
    private void LogInfo()
    {
        if (testConveyor == null)
        {
            Debug.LogError("Test conveyor not assigned!");
            return;
        }
        
        Debug.Log($"=== Conveyor Info ===");
        Debug.Log($"Type: {testConveyor.ConveyorType}");
        Debug.Log($"Resources on conveyor: {testConveyor.ResourceCount}");
        Debug.Log($"Output blocked: {testConveyor.IsOutputBlocked}");
    }
}