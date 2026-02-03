using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public static ProjectileSpawner Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public Projectile SpawnProjectile(GameObject prefab, Vector3 position, Enemy target, int damage, float speed)
    {
        if (prefab == null)
        {
            Debug.LogError("[ProjectileSpawner] Prefab is null!");
            return null;
        }
        
        var projectileObj = Instantiate(prefab, position, Quaternion.identity);
        
        var projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile == null)
        {
            projectile = projectileObj.AddComponent<Projectile>();
        }
        
        projectile.Initialize(target, damage, speed);
        
        return projectile;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}