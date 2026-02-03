using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Title("Spawn Settings")]
    [Required]
    [SerializeField] private EnemyData enemyData;
    
    [Required]
    [SerializeField] private Transform targetPoint;
    
    [MinValue(0.1f)]
    [SerializeField] private float spawnInterval = 3f;
    
    [SerializeField] private bool autoStart = true;
    
    [Title("Debug")]
    [ShowInInspector, ReadOnly]
    private int _spawnedCount;
    
    [ShowInInspector, ReadOnly]
    private bool _isSpawning;
    
    private void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
    }
    
    [Button("Start Spawning")]
    public void StartSpawning()
    {
        if (_isSpawning) return;
        
        _isSpawning = true;
        SpawnLoop().Forget();
    }
    
    [Button("Stop Spawning")]
    public void StopSpawning()
    {
        _isSpawning = false;
    }
    
    private async UniTaskVoid SpawnLoop()
    {
        while (_isSpawning)
        {
            SpawnEnemy();
            
            await UniTask.Delay((int)(spawnInterval * 1000));
        }
    }
    
    private void SpawnEnemy()
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogError("[EnemySpawner] EnemyData or prefab is null!");
            return;
        }
        
        if (targetPoint == null)
        {
            Debug.LogError("[EnemySpawner] Target point is null!");
            return;
        }
        
        var enemyObj = Instantiate(enemyData.prefab, transform.position, Quaternion.identity);
        var enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy == null)
        {
            enemy = enemyObj.AddComponent<Enemy>();
        }
        
        enemy.Initialize(enemyData, transform.position, targetPoint.position);
        
        EnemyManager.Instance?.RegisterEnemy(enemy);
        
        _spawnedCount++;
        
        Debug.Log($"[EnemySpawner] Spawned {enemyData.enemyName}. Total spawned: {_spawnedCount}");
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        if (targetPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPoint.position);
        }
    }
}