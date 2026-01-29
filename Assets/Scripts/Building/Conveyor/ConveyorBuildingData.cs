using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Conveyor_", menuName = "Factory/Building/ConveyorData")]
public class ConveyorBuildingData : BuildingData
{
    [Title("Conveyor Specific")]
    public ConveyorType conveyorType = ConveyorType.Straight;
    
    [Tooltip("Переопределить глобальную скорость для этого конвейера")]
    public bool overrideSpeed = false;
    
    [ShowIf("overrideSpeed")]
    [MinValue(0.1f)]
    public float customSpeed = 2f;
}