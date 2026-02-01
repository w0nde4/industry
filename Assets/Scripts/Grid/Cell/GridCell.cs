using UnityEngine;

public class GridCell
{
    public Vector2Int GridPosition { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    
    public bool IsOccupied { get; private set; }
    public GameObject OccupyingObject { get; private set; }
    
    public CellModifiers Modifiers { get; private set; }
    
    public GridCell(Vector2Int gridPosition, Vector3 worldPosition)
    {
        GridPosition = gridPosition;
        WorldPosition = worldPosition;
        IsOccupied = false;
        OccupyingObject = null;
        Modifiers = new CellModifiers();
    }
    
    public void Occupy(GameObject obj)
    {
        IsOccupied = true;
        OccupyingObject = obj;
    }
    
    public void Free()
    {
        IsOccupied = false;
        OccupyingObject = null;
    }
    
    public void SetModifiers(CellModifiers modifiers)
    {
        Modifiers = modifiers.Clone();
    }
}