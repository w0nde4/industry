using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ConnectionPointSettings", menuName = "Factory/ConnectionPoint/Settings")]
public class ConnectionPointSettings : ScriptableObject
{
    [Title("Connection Distances")]
    [MinValue(0.1f)]
    [Tooltip("Максимальное расстояние для соединения точек")]
    public float maxConnectionDistance = 2f;
    
    [MinValue(0.1f)]
    [Tooltip("Радиус поиска ближайших точек")]
    public float searchRadius = 1.5f;
    
    [MinValue(0.1f)]
    [Tooltip("Радиус поиска соседних точек")]
    public float adjacentSearchRadius = 1.5f;
    
    [Title("Visualization")]
    [Tooltip("Цвет для Input точек")]
    public Color inputColor = Color.green;
    
    [Tooltip("Цвет для Output точек")]
    public Color outputColor = Color.red;
    
    [MinValue(0.05f)]
    [Tooltip("Размер гизмо для точек подключения")]
    public float gizmoSize = 0.2f;
    
    [MinValue(0.05f)]
    [Tooltip("Размер гизмо при выборе")]
    public float selectedGizmoSize = 0.25f;
    
    [MinValue(0.1f)]
    [Tooltip("Длина направляющей линии")]
    public float directionLineLength = 0.3f;
}