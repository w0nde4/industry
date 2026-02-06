using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private static Sprite _circleSprite;
    
    private Enemy _target;
    private int _damage;
    private float _speed;
    
    private SpriteRenderer _spriteRenderer;

    public event Action OnDestroyed;
    
    public void Initialize(Enemy target, int damage, float speed)
    {
        _target = target;
        _damage = damage;
        _speed = speed;
        
        SetupVisuals();
    }
    
    private void SetupVisuals()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        _spriteRenderer.color = Color.yellow;
        
        if(_circleSprite == null)
        {
            _circleSprite = Resources.Load<Sprite>("Circle");
        }
        
        if (_circleSprite != null)
        {
            _spriteRenderer.sprite = _circleSprite;
        }
        
        transform.localScale = Vector3.one * 0.2f;
    }
    
    private void Update()
    {
        if (_target == null || !_target.IsAlive)
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
            return;
        }
        
        MoveToTarget();
    }
    
    private void MoveToTarget()
    {
        var direction = (_target.Position - transform.position).normalized;
        var distance = Vector3.Distance(transform.position, _target.Position);
        
        if (distance < 0.2f)
        {
            OnHit();
            return;
        }
        
        transform.position += direction * (_speed * Time.deltaTime);
        
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void OnHit()
    {
        if (_target != null && _target.IsAlive)
        {
            _target.TakeDamage(_damage);
        }
        
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
}