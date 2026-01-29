using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ConveyorSettings", menuName = "Factory/Conveyor/Settings")]
public class ConveyorSettings : ScriptableObject
{
    [Title("Movement")]
    [MinValue(0.1f)]
    [Tooltip("Скорость движения ресурса по конвейеру (units per second)")]
    public float conveyorSpeed = 2f;
    
    [Title("Capacity")]
    [MinValue(1)]
    [Tooltip("Максимальное количество ресурсов на одном конвейере")]
    public int maxResourcesPerConveyor = 5;
    
    [Title("Visuals")]
    public Color straightConveyorColor = Color.gray;
    public Color cornerConveyorColor = Color.yellow;
    public Color splitterConveyorColor = Color.blue;
    public Color junctionConveyorColor = Color.magenta;
}