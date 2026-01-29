using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource_", menuName = "Factory/Resources/ResourceData")]
public class ResourceData : ScriptableObject
{
    [Title("Basic Info")]
    public string resourceName = "New Resource";
    
    [TextArea(2, 4)]
    public string description;
    
    [Title("Classification")]
    public ResourceCategory category = ResourceCategory.Primary;

    public Color resourceColor = Color.white;

    [Title("Visual")]
    [PreviewField(100)]
    public Sprite sprite;

    [Title("Properties")]
    [MinValue(1)]
    [Tooltip("Максимальное количество в одном стаке")]
    public int maxStack = 50;
    
    [Tooltip("Базовое время производства (если это производимый ресурс)")]
    [MinValue(0.1f)]
    public float baseProductionTime = 1f;
    
    [Title("Advanced")]
    [SerializeField] private bool _showAdvanced = false;
    
    [ShowIf("_showAdvanced")]
    [TextArea(3, 10)]
    public string customData;
}