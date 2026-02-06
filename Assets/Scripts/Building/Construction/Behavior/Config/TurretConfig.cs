using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretConfig_", menuName = "Factory/Behaviors/TurretConfig")]
public class TurretConfig : BehaviorConfig
{
    [Title("Ammo")]
    [Required]
    public ResourceData ammoResource;
    
    [MinValue(1)]
    [Tooltip("Максимальное количество боеприпасов в буфере")]
    public int maxAmmoBuffer = 10;
    
    [Title("Combat")]
    [MinValue(0.1f)]
    [Tooltip("Радиус атаки (в единицах мира)")]
    public float attackRange = 5f;
    
    [MinValue(0.1f)]
    [Tooltip("Время между выстрелами (секунды)")]
    public float attackCooldown = 1f;
    
    [MinValue(1)]
    [Tooltip("Урон за выстрел")]
    public int damage = 25;
    
    [Title("Projectile")]
    [Required]
    [AssetsOnly]
    [Tooltip("Префаб снаряда")]
    public GameObject projectilePrefab;
    
    [MinValue(0.1f)]
    [Tooltip("Скорость снаряда")]
    public float projectileSpeed = 10f;
    
    [Title("Rotation")]
    [MinValue(1f)]
    [Tooltip("Скорость поворота турели (градусы в секунду)")]
    public float rotationSpeed = 180f;
    
    public override IBuildingBehavior CreateBehavior()
    {
        Debug.Log($"[TurretConfig] Creating TurretBehavior from {name}");
        return new TurretBehavior(this);
    }
}