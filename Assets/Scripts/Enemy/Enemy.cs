using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyData _data;
    private int _currentHP;
    private List<Vector3> _worldPath;
    private int _currentPathIndex;
    private bool _hasReachedTarget;
    
    private SpriteRenderer _spriteRenderer;
    private PathfindingSystem _pathfinding;
    private GridSystem _grid;
    
    public EnemyData Data => _data;
    public bool IsAlive => _currentHP > 0;
    public Vector3 Position => transform.position;
    
    public void Initialize(EnemyData data, Vector3 startPosition, Vector3 targetPosition)
    {
        _data = data;
        _currentHP = data.maxHP;
        _hasReachedTarget = false;
        
        transform.position = startPosition;
        
        SetupVisuals();
        
        _pathfinding = FindObjectOfType<PathfindingSystem>();
        _grid = GridService.Instance.Grid;
        
        CalculatePath(startPosition, targetPosition);
    }
    
    private void SetupVisuals()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (_data.sprite != null)
        {
            _spriteRenderer.sprite = _data.sprite;
            _spriteRenderer.color = _data.enemyColor;
        }
    }
    
    private void CalculatePath(Vector3 startWorld, Vector3 targetWorld)
    {
        var startGrid = _grid.WorldToGridPosition(startWorld);
        var endGrid = _grid.WorldToGridPosition(targetWorld);
        
        var gridPath = _pathfinding.FindPath(startGrid, endGrid);
        
        if (gridPath == null || gridPath.Count == 0)
        {
            Debug.LogWarning($"[Enemy] No path found from {startGrid} to {endGrid}");
            _hasReachedTarget = true;
            return;
        }
        
        _worldPath = new List<Vector3>(gridPath.Count);
        
        foreach (var gridPos in gridPath)
        {
            var worldPos = _grid.GridToWorldPosition(gridPos);
            worldPos += new Vector3(_grid.Settings.cellSize * 0.5f, _grid.Settings.cellSize * 0.5f, 0);
            _worldPath.Add(worldPos);
        }
        
        _currentPathIndex = 0;
        
        Debug.Log($"[Enemy] Path calculated: {_worldPath.Count} waypoints");
    }
    
    private void Update()
    {
        if (!IsAlive || _hasReachedTarget) return;
        
        MoveAlongPath();
    }
    
    private void MoveAlongPath()
    {
        if (_worldPath == null || _worldPath.Count == 0 || _currentPathIndex >= _worldPath.Count)
        {
            _hasReachedTarget = true;
            OnReachedTarget();
            return;
        }
        
        var targetPos = _worldPath[_currentPathIndex];
        var direction = (targetPos - transform.position).normalized;
        var distance = Vector3.Distance(transform.position, targetPos);
        
        if (distance < 0.05f)
        {
            _currentPathIndex++;
            return;
        }
        
        var moveStep = _data.moveSpeed * Time.deltaTime;
        
        if (moveStep >= distance)
        {
            transform.position = targetPos;
            _currentPathIndex++;
        }
        else
        {
            transform.position += direction * moveStep;
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        
        _currentHP -= damage;
        
        Debug.Log($"[Enemy] {_data.enemyName} took {damage} damage. HP: {_currentHP}/{_data.maxHP}");
        
        if (_currentHP <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log($"[Enemy] {_data.enemyName} died");
        
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
        
        Destroy(gameObject);
    }
    
    private void OnReachedTarget()
    {
        Debug.Log($"[Enemy] Reached target, dealing {_data.damageToBase} damage to base");
        
        var baseCore = FindObjectOfType<BaseCore>();
        
        if (baseCore != null)
        {
            baseCore.TakeDamage(_data.damageToBase);
        }
        
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
        
        Destroy(gameObject);
    }
    
    public void RecalculatePath(Vector3 targetPosition)
    {
        CalculatePath(transform.position, targetPosition);
    }
}