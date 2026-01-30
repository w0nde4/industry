using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class ResourceTester : MonoBehaviour
{
    [Title("Test Resources")]
    [Required]
    [SerializeField] private ResourceData testResource;
    
    [SerializeField] private int spawnAmount = 10;
    
    [Title("Spawn Settings")]
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    [SerializeField] private float spawnRadius = 5f;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnTestResource();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllResources();
        }
    }
    
    [Button("Spawn Test Resource (Space)")]
    private void SpawnTestResource()
    {
        if (testResource == null)
        {
            Debug.LogError("Test resource not assigned!");
            return;
        }
        
        var randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(-spawnRadius, spawnRadius),
            0
        );
        
        var spawnPos = spawnCenter + randomOffset;
        
        var resource = ResourceService.Spawn(testResource, spawnPos, spawnAmount);
        
        if (resource != null)
        {
            Debug.Log($"Spawned {testResource.resourceName} x{spawnAmount} at {spawnPos}");
        }
    }
    
    [Button("Clear All Resources (C)")]
    private void ClearAllResources()
    {
        var resourceService = ResourceService.Instance;
        if (resourceService == null) return;
        
        var allResources = resourceService.AllResources.ToArray();
        
        foreach (var resource in allResources)
        {
            resourceService.DestroyResource(resource);
        }
        
        Debug.Log($"Cleared {allResources.Length} resources");
    }
    
    [Button("Log Resources Info")]
    private void LogInfo()
    {
        var resourceService = ResourceService.Instance;
        Debug.Log($"Active resources: {resourceService.AllResources.Count}");
    }
}