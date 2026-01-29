using UnityEngine;

public class GridService : MonoBehaviour
{
    private static GridService _instance;
    public static GridService Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            _instance = FindFirstObjectByType<GridService>();
            if (_instance == null)
            {
                Debug.LogError("GridService not found in scene!");
            }
            return _instance;
        }
    }
    
    [SerializeField] private GridSystem gridSystem;
    
    public GridSystem Grid => gridSystem;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        if (gridSystem == null)
        {
            gridSystem = GetComponent<GridSystem>();
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }
    
    // Удобные методы для быстрого доступа
    public static Vector3 SnapToGrid(Vector3 position) => Instance.Grid.SnapToGrid(position);
    public static GridCell GetCell(Vector2Int gridPosition) => Instance.Grid.GetCell(gridPosition);
    public static bool IsAreaAvailable(Vector2Int origin, Vector2Int size) => Instance.Grid.IsAreaAvailable(origin, size);
}