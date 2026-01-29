using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretConfig_", menuName = "Factory/Behaviors/TurretConfig")]
public class TurretConfig : BehaviorConfig
{
    [Title("Ammunition")]
    [Required]
    [Tooltip("Тип ресурса для выстрелов (обработанное железо)")]
    public ResourceData ammoResource;
    
    [MinValue(1)]
    [Tooltip("Расход ресурса за выстрел")]
    public int ammoPerShot = 1;
    
    [Title("Combat Stats")]
    [MinValue(0.1f)]
    public float damage = 10f;
    
    [MinValue(0.1f)]
    [Tooltip("Интервал между выстрелами")]
    public float fireRate = 1f;
    
    [MinValue(1f)]
    public float range = 5f;
    
    public override IBuildingBehavior CreateBehavior()
    {
        return new TurretBehavior(this);
    }
}