using System;
using UnityEngine;

[Serializable]
public class CellModifiers
{
    public bool isSpawnable = true;
    public bool isWalkable = true;
    
    public float productionBonus = 1f;
    public string biomeType = "default";
    
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