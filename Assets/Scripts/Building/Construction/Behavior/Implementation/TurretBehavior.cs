using UnityEngine;

public class TurretBehavior : IBuildingBehavior
{
    private TurretConfig _config;
    private PlacedBuilding _owner;
    
    private int _ammoBuffer;
    private float _attackCooldownTimer;
    
    private ConnectionPoint _inputPoint;
    private bool _isInputInitialized;
    
    private Enemy _currentTarget;
    private Transform _turretTransform;
    
    public TurretBehavior(TurretConfig config)
    {
        _config = config;
    }
    
    public void Initialize(PlacedBuilding owner, BuildingData data)
    {
        _owner = owner;
        _ammoBuffer = 0;
        _attackCooldownTimer = 0f;
        _isInputInitialized = false;
        _currentTarget = null;
        
        _turretTransform = owner.transform;
        
        Debug.Log($"[TurretBehavior] Initialized for {data.buildingName}");
    }
    
    private void EnsureInputInitialized()
    {
        if (_isInputInitialized) return;
        
        var inputs = _owner.Inputs;
        
        if (inputs == null || inputs.Length == 0)
        {
            Debug.LogError($"[TurretBehavior] {_owner.Data.buildingName} has no Input ConnectionPoint!");
            _isInputInitialized = true;
            return;
        }
        
        _inputPoint = inputs[0];
        _isInputInitialized = true;
        
        Debug.Log($"[TurretBehavior] Found input point for {_owner.Data.buildingName}");
    }
    
    public void OnTick(float deltaTime)
    {
        EnsureInputInitialized();
        
        if (_owner == null || _turretTransform == null)
        {
            return;
        }
    
        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= deltaTime;
        }
    
        if (_ammoBuffer <= 0)
        {
            return;
        }
    
        UpdateTarget();
    
        if (_currentTarget != null && _currentTarget.IsAlive)
        {
            RotateToTarget(deltaTime);
        
            if (IsAimedAtTarget() && _attackCooldownTimer <= 0)
            {
                Shoot();
            }
        }
    }
    
    private void UpdateTarget()
    {
        if (_currentTarget != null && _currentTarget.IsAlive)
        {
            var distance = Vector3.Distance(_owner.transform.position, _currentTarget.Position);
            
            if (distance <= _config.attackRange)
            {
                return;
            }
        }
        
        _currentTarget = FindTarget();
    }
    
    private Enemy FindTarget()
    {
        if (EnemyManager.Instance == null)
        {
            return null;
        }
        
        return EnemyManager.Instance.GetClosestEnemy(_owner.transform.position, _config.attackRange);
    }
    
    private void RotateToTarget(float deltaTime)
    {
        if (_currentTarget == null || _turretTransform == null) return;
    
        var direction = (_currentTarget.Position - _turretTransform.position);
    
        if (direction.sqrMagnitude < 0.001f) return;
    
        direction.Normalize();
    
        var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        var currentAngle = _turretTransform.eulerAngles.z;
    
        if (currentAngle > 180f) currentAngle -= 360f;
        if (targetAngle > 180f) targetAngle -= 360f;
    
        var newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _config.rotationSpeed * deltaTime);
    
        _turretTransform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    private bool IsAimedAtTarget()
    {
        if (_currentTarget == null || _turretTransform == null) return false;
    
        var direction = (_currentTarget.Position - _turretTransform.position);
    
        if (direction.sqrMagnitude < 0.001f) return true;
    
        direction.Normalize();
    
        var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        var currentAngle = _turretTransform.eulerAngles.z;
    
        var angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
    
        return angleDiff < 5f;
    }
    
    private void Shoot()
    {
        if (_currentTarget == null || _ammoBuffer <= 0) return;
        
        _ammoBuffer--;
        _attackCooldownTimer = _config.attackCooldown;
        
        SpawnProjectile();
        
        Debug.Log($"[TurretBehavior] Fired! Ammo remaining: {_ammoBuffer}/{_config.maxAmmoBuffer}");
    }
    
    private void SpawnProjectile()
    {
        if (_config.projectilePrefab == null)
        {
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                _currentTarget.TakeDamage(_config.damage);
            }
            return;
        }
    
        if (ProjectileSpawner.Instance == null)
        {
            Debug.LogError("[TurretBehavior] ProjectileSpawner.Instance is null!");
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                _currentTarget.TakeDamage(_config.damage);
            }
            return;
        }
    
        ProjectileSpawner.Instance.SpawnProjectile(
            _config.projectilePrefab,
            _turretTransform.position,
            _currentTarget,
            _config.damage,
            _config.projectileSpeed
        );
    }
    
    public void OnResourceReceived(ConnectionPoint input, ResourceInstance resource)
    {
        if (resource.Data != _config.ammoResource)
        {
            Debug.LogWarning($"[TurretBehavior] Received wrong resource type! Expected {_config.ammoResource.resourceName}, got {resource.Data.resourceName}");
            return;
        }
        
        if (_ammoBuffer >= _config.maxAmmoBuffer)
        {
            Debug.LogWarning($"[TurretBehavior] Ammo buffer full!");
            return;
        }
        
        _ammoBuffer += resource.Amount;
        
        if (_ammoBuffer > _config.maxAmmoBuffer)
        {
            _ammoBuffer = _config.maxAmmoBuffer;
        }
        
        Debug.Log($"[TurretBehavior] Received {resource.Data.resourceName} x{resource.Amount}. Ammo buffer: {_ammoBuffer}/{_config.maxAmmoBuffer}");
        
        ResourceService.Destroy(resource);
    }
    
    public void CleanUp()
    {
        Debug.Log($"[TurretBehavior] Cleanup - Ammo buffer: {_ammoBuffer}");
    }
}