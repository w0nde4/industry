using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionConfig_", menuName = "Factory/Behaviors/ProductionConfig")]
public class ProductionConfig : BehaviorConfig
{
    [Title("Production Settings")]
    [Required]
    public ResourceData outputResource;
    
    [MinValue(0.1f)]
    [Tooltip("Интервал производства (секунды)")]
    public float productionInterval = 2f;
    
    [MinValue(1)]
    [Tooltip("Максимальное накопление на выходе")]
    public int maxOutputStack = 10;
    
    [Title("Cell Modifiers (Future)")]
    [Tooltip("Множитель скорости от модификаторов клетки")]
    public bool useModifiers = false;
    
    public override IBuildingBehavior CreateBehavior()
    {
        return new ProductionBehavior(this);
    }
}