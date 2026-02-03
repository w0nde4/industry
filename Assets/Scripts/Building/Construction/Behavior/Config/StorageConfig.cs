using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageConfig_", menuName = "Factory/Behaviors/StorageConfig")]
public class StorageConfig : BehaviorConfig
{
    [Title("Storage Settings")]
    [MinValue(1)]
    [Tooltip("Максимальная вместимость склада")]
    public int maxCapacity = 100;
    
    [Tooltip("Может ли склад отдавать ресурсы")]
    public bool canOutput = true;
    
    public override IBuildingBehavior CreateBehavior()
    {
        return new StorageBehavior(this);
    }
}