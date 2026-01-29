using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSettings", menuName = "Factory/Building/Settings")]
public class BuildingSettings : ScriptableObject
{
    [Title("Preview Colors")]
    public Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);
    public Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);
    public Color movePreviewColor = new Color(0f, 0.5f, 1f, 0.5f);
    
    [Title("Preview Settings")]
    [Range(0f, 1f)]
    public float previewAlpha = 0.5f;
    
    [Tooltip("Материал для ghost превью")]
    public Material ghostMaterial;
    
    [Title("Object Pooling")]
    public int initialPoolSize = 5;
    public int maxPoolSize = 20;
    
    [Title("Input Settings")]
    [Tooltip("Скорость ротации колесом мыши")]
    [Range(0.1f, 2f)]
    public float rotationScrollSensitivity = 1f;
    
    [Title("Grid Highlight")]
    public bool showGridHighlight = true;
    public Color gridHighlightColor = new Color(1f, 1f, 1f, 0.3f);
}