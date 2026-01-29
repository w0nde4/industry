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
                Debug.LogError("BuildingService not found in scene!");
            }
            return _instance;
        }
    }
    
    [SerializeField] private BuildingPlacer placer;
    [SerializeField] private BuildingManager manager;
    [SerializeField] private GridSystem gridSystem;
    
    public BuildingPlacer Placer => placer;
    public BuildingManager Manager => manager;
    public GridSystem Grid => gridSystem;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Auto-find components
        if (placer == null)
            placer = GetComponent<BuildingPlacer>();
        
        if (manager == null)
            manager = GetComponent<BuildingManager>();
        
        if (gridSystem == null)
            gridSystem = GetComponent<GridSystem>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }
    
    // Удобные статические методы
    public static void StartBuilding(BuildingData data) => Instance.Placer.StartBuildMode(data);
    public static void StopBuilding() => Instance.Placer.StopBuildMode();
    public static PlacedBuilding GetBuildingAt(Vector2Int gridPos) => Instance.Manager.GetBuildingAtPosition(gridPos);
}