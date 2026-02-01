using System;
using UnityEngine;

[Serializable]
public class CellData
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private TerrainType terrainType;
    [SerializeField] private CellModifiers modifiers;
    
    public Sprite Sprite => sprite;
    public TerrainType Terrain => terrainType;
    public CellModifiers Modifiers => modifiers;
    
    public CellData()
    {
        sprite = null;
        terrainType = TerrainType.Grass;
        modifiers = new CellModifiers();
    }
    
    public CellData(Sprite sprite, TerrainType terrainType, CellModifiers modifiers)
    {
        this.sprite = sprite;
        this.terrainType = terrainType;
        this.modifiers = modifiers.Clone();
    }
    
    public void SetSprite(Sprite newSprite) => sprite = newSprite;

    public void SetTerrainType(TerrainType newTerrainType) //hardcoded modifiers based on terrain type
    {
        terrainType = newTerrainType;

        switch (newTerrainType)
        {
            case TerrainType.Grass:
            case TerrainType.Sand:
                modifiers.isSpawnable = true;
                modifiers.isWalkable = true;
                break;
                
            case TerrainType.Water:
                modifiers.isSpawnable = false;
                modifiers.isWalkable = false;
                break;
                
            case TerrainType.Stone:
                modifiers.isSpawnable = true;
                modifiers.isWalkable = true;
                modifiers.productionBonus = 1.2f;
                break;
                
            case TerrainType.Obstacle:
                modifiers.isSpawnable = false;
                modifiers.isWalkable = false;
                break;
                
            case TerrainType.Destructible:
                modifiers.isSpawnable = false;
                modifiers.isWalkable = false;
                break;
        }
    }

    public void SetModifiers(CellModifiers newModifiers) => modifiers = newModifiers.Clone();
    
    public CellData Clone()
    {
        return new CellData(sprite, terrainType, modifiers);
    }
}