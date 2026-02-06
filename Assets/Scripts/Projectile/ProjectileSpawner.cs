using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public static ProjectileSpawner Instance { get; private set; }

    private int _activeProjectilesCount;
    
    private const int MAX_PROJECTILES = 50;
    
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
        if (_activeProjectilesCount >= MAX_PROJECTILES)
        {
            Debug.LogError($"[ProjectileSpawner] Max projectiles reached: {MAX_PROJECTILES}");
            return null;
        }
        
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

        _activeProjectilesCount++;
        projectile.OnDestroyed += OnProjectileDestroyed;
        
        Debug.Log($"[ProjectileSpawner] Projectile spawned. Total active: {_activeProjectilesCount}");
        
        return projectile;
    }
    
    private void OnProjectileDestroyed()
    {
        _activeProjectilesCount = Mathf.Max(0, _activeProjectilesCount - 1);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}