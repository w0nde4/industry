using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Factory/Grid/Settings")]
public class GridSettings : ScriptableObject
{
    [Title("Grid Configuration")]
    [MinValue(0.1f)]
    public float cellSize = 1f;
    
    [MinValue(10)]
    public int initialGridWidth = 100;
    
    [MinValue(10)]
    public int initialGridHeight = 100;
    
    [Title("Expansion Settings")]
    [Tooltip("Количество клеток, на которое расширяется сетка за раз")]
    [MinValue(1)]
    public int expansionStep = 20;
    
    [Tooltip("Максимальный размер сетки (0 = без лимита). Будет устанавливаться по размеру генерируемой карты")]
    [MinValue(0)]
    public int maxGridSize = 0;
    
    [Title("Debug")]
    public bool showDebugGizmos = true;
    [Tooltip("Радиус отрисовки гизмо вокруг камеры")]
    [MinValue(5)]
    [ShowIf("showDebugGizmos")]
    public int gizmosDrawRadius = 25;
    public Color occupiedCellColor = new Color(1f, 0f, 0f, 0.3f);
    public Color freeCellColor = new Color(0f, 1f, 0f, 0.1f);
}