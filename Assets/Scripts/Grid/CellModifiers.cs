using System;
using UnityEngine;

[Serializable]
public class CellModifiers
{
    // Базовые модификаторы для генерации
    public bool isSpawnable = true;
    public bool isWalkable = true;
    
    // Можно расширять дополнительными полями
    public float productionBonus = 1f;
    public string biomeType = "default";
    
    // Кастомные данные (для будущего расширения)
    [SerializeField] private string customData;
    
    public CellModifiers()
    {
        isSpawnable = true;
        isWalkable = true;
        productionBonus = 1f;
        biomeType = "default";
    }
    
    public CellModifiers Clone()
    {
        return new CellModifiers
        {
            isSpawnable = this.isSpawnable,
            isWalkable = this.isWalkable,
            productionBonus = this.productionBonus,
            biomeType = this.biomeType,
            customData = this.customData
        };
    }
    
    public void SetCustomData(string data) => customData = data;
    public string GetCustomData() => customData;
}