using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "Factory/Combat/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Title("Basic Info")]
    public string enemyName = "New Enemy";
    
    [TextArea(2, 4)]
    public string description;
    
    [Title("Stats")]
    [MinValue(1)]
    public int maxHP = 100;
    
    [MinValue(0.1f)]
    public float moveSpeed = 2f;
    
    [MinValue(1)]
    public int damageToBase = 10;
    
    [Title("Visual")]
    [PreviewField(100)]
    public Sprite sprite;
    
    public Color enemyColor = Color.red;
    
    [Title("Prefab")]
    [Required]
    [AssetsOnly]
    public GameObject prefab;
}