using UnityEngine;
using Sirenix.OdinInspector;

public class GridTester : MonoBehaviour
{
    [Required]
    [SerializeField] private GridSystem gridSystem;
    
    [SerializeField] private GameObject testPrefab;
    
    [Title("Test Parameters")]
    [SerializeField] private Vector2Int testPosition = Vector2Int.zero;
    [SerializeField] private Vector2Int testSize = new Vector2Int(2, 1);
    
    [Button("Test: Place Object")]
    private void TestPlaceObject()
    {
        if (gridSystem.IsAreaAvailable(testPosition, testSize))
        {
            var spawnPos = gridSystem.GetCenterPosition(testPosition, testSize);
            var obj = testPrefab != null ? Instantiate(testPrefab, spawnPos, Quaternion.identity) : new GameObject("TestObject");
            
            if (gridSystem.OccupyArea(testPosition, testSize, obj))
            {
                Debug.Log($"✓ Object placed at {testPosition} (size: {testSize})");
            }
        }
        else
        {
            Debug.LogWarning($"✗ Area not available at {testPosition}");
        }
    }
    
    [Button("Test: Free Area")]
    private void TestFreeArea()
    {
        gridSystem.FreeArea(testPosition, testSize);
        Debug.Log($"Area freed at {testPosition}");
    }
    
    [Button("Test: Check Cell")]
    private void TestCheckCell()
    {
        var cell = gridSystem.GetCell(testPosition);
        if (cell != null)
        {
            Debug.Log($"Cell at {testPosition}: Occupied={cell.IsOccupied}, Spawnable={cell.Modifiers.isSpawnable}");
        }
    }
}