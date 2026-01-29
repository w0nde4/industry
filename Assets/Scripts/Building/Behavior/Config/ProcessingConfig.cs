using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcessingConfig_", menuName = "Factory/Behaviors/ProcessingConfig")]
public class ProcessingConfig : BehaviorConfig
{
    [Title("Input Requirements")]
    public List<ResourceRequirement> inputs = new List<ResourceRequirement>();
    
    [Title("Output")]
    [Required]
    public ResourceData outputResource;
    
    [MinValue(1)]
    public int outputAmount = 1;
    
    [Title("Processing Settings")]
    [MinValue(0.1f)]
    [Tooltip("Время обработки (секунды)")]
    public float processingTime = 5f;
    
    [MinValue(1)]
    [Tooltip("Максимальное накопление на выходе")]
    public int maxOutputStack = 10;
    
    public override IBuildingBehavior CreateBehavior()
    {
        return new ProcessingBehavior(this);
    }
}