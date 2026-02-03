using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class BaseCore : MonoBehaviour
{
    [Title("Stats")]
    [SerializeField] private int maxHP = 1000;
    
    [ShowInInspector, ReadOnly]
    private int _currentHP;
    
    public int CurrentHP => _currentHP;
    public int MaxHP => maxHP;
    public bool IsAlive => _currentHP > 0;
    
    public event Action<int, int> OnHealthChanged;
    public event Action OnDestroyed;
    
    private void Start()
    {
        _currentHP = maxHP;
        OnHealthChanged?.Invoke(_currentHP, maxHP);
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        
        _currentHP = Mathf.Max(0, _currentHP - damage);
        
        Debug.Log($"[BaseCore] Took {damage} damage. HP: {_currentHP}/{maxHP}");
        
        OnHealthChanged?.Invoke(_currentHP, maxHP);
        
        if (_currentHP <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        
        _currentHP = Mathf.Min(maxHP, _currentHP + amount);
        
        OnHealthChanged?.Invoke(_currentHP, maxHP);
    }
    
    private void Die()
    {
        Debug.Log("[BaseCore] Base destroyed!");
        
        OnDestroyed?.Invoke();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}