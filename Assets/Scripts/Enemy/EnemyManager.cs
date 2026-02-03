using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    
    public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void RegisterEnemy(Enemy enemy)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            Debug.Log($"[EnemyManager] Registered enemy. Total: {_activeEnemies.Count}");
        }
    }
    
    public void UnregisterEnemy(Enemy enemy)
    {
        if (_activeEnemies.Remove(enemy))
        {
            Debug.Log($"[EnemyManager] Unregistered enemy. Total: {_activeEnemies.Count}");
        }
    }
    
    public List<Enemy> GetEnemiesInRange(Vector3 position, float radius)
    {
        var enemiesInRange = new List<Enemy>();
        var radiusSqr = radius * radius;
        
        foreach (var enemy in _activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            
            var distanceSqr = (enemy.Position - position).sqrMagnitude;
            
            if (distanceSqr <= radiusSqr)
            {
                enemiesInRange.Add(enemy);
            }
        }
        
        return enemiesInRange;
    }
    
    public Enemy GetClosestEnemy(Vector3 position, float maxRange)
    {
        Enemy closest = null;
        var minDistanceSqr = maxRange * maxRange;
        
        foreach (var enemy in _activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            
            var distanceSqr = (enemy.Position - position).sqrMagnitude;
            
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closest = enemy;
            }
        }
        
        return closest;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}