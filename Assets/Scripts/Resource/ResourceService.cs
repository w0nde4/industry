using UnityEngine;

public class ResourceService : MonoBehaviour
{
    private static ResourceService _instance;
    public static ResourceService Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            _instance = FindFirstObjectByType<ResourceService>();
            if (_instance == null)
            {
                Debug.LogError("ResourceService not found in scene!");
            }
            return _instance;
        }
    }
    
    [SerializeField] private ResourceManager manager;
    
    public ResourceManager Manager => manager;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        if (manager == null)
        {
            manager = GetComponent<ResourceManager>();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
    }

    public static ResourceInstance SpawnResource(ResourceData data, Vector3 position, int amount = 1)
    {
        return Instance.Manager.SpawnResource(data, position, amount);
    }

    public static void DestroyResource(ResourceInstance instance)
    {
        Instance.Manager.DestroyResource(instance);
    }
}