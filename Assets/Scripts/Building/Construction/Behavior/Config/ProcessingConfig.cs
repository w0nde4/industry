using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcessingConfig_", menuName = "Factory/Behaviors/ProcessingConfig")]
public class ProcessingConfig : BehaviorConfig
{
    [Title("Input")]
    [Required]
    public ResourceData inputResource;
    
    [MinValue(1)]
    [Tooltip("Количество входного ресурса для обработки")]
    public int inputAmount = 1;
    
    [MinValue(1)]
    [Tooltip("Максимальная вместимость входного буфера")]
    public int maxInputBuffer = 10;
    
    [Title("Output")]
    [Required]
    public ResourceData outputResource;
    
    [MinValue(1)]
    [Tooltip("Количество выходного ресурса после обработки")]
    public int outputAmount = 1;
    
    [MinValue(1)]
    [Tooltip("Максимальное накопление на выходе")]
    public int maxOutputBuffer = 10;
    
    [Title("Processing")]
    [MinValue(0.1f)]
    [Tooltip("Время обработки одной порции (секунды)")]
    public float processingTime = 2f;
    
    [Title("Cell Modifiers (Future)")]
    [Tooltip("Множитель скорости от модификаторов клетки")]
    public bool useModifiers = false;
    
    public override IBuildingBehavior CreateBehavior()
    {
        return new ProcessingBehavior(this);
    }
}