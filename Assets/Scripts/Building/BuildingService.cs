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
    
    [SerializeField] private BuildingPlacer buildingPlacer;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private BuildingBehaviorManager behaviorManager;
    [SerializeField] private GridSystem gridSystem;
    
    public BuildingPlacer BuildingPlacer => buildingPlacer;
    public BuildingManager BuildingManager => buildingManager;
    public BuildingBehaviorManager BehaviorManager => behaviorManager;
    public GridSystem Grid => gridSystem;
    
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
        
        if (buildingManager == null)
            buildingManager = GetComponent<BuildingManager>();
        
        if (behaviorManager == null)
            behaviorManager = GetComponent<BuildingBehaviorManager>();
        
        if (gridSystem == null)
            gridSystem = GetComponent<GridSystem>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }
    
    public static void StartBuilding(BuildingData data) => Instance.BuildingPlacer.StartBuildMode(data);
    public static void StopBuilding() => Instance.BuildingPlacer.StopBuildMode();
    public static PlacedBuilding GetBuildingAt(Vector2Int gridPos) => Instance.BuildingManager.GetBuildingAtPosition(gridPos);
}