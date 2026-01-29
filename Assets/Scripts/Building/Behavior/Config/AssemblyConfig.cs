using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AssemblyConfig_", menuName = "Factory/Behaviors/AssemblyConfig")]
public class AssemblyConfig : BehaviorConfig
{
    [Title("Unit Recipe")]
    public List<ResourceRequirement> ingredients = new List<ResourceRequirement>();
    
    [Title("Assembly Settings")]
    [MinValue(0.1f)]
    [Tooltip("Время сборки юнита")]
    public float assemblyTime = 10f;
    
    [Tooltip("Префаб юнита")]
    public GameObject unitPrefab;
    
    public override IBuildingBehavior CreateBehavior()
    {
        return new AssemblyBehavior(this);
    }
}