using System;
using UnityEngine;

[Serializable]
public class BlockDoor
{
    [SerializeField] private DoorSide side;
    [SerializeField] private int position;
    [SerializeField] private int width;
    
    public DoorSide Side => side;
    public int Position => position;
    public int Width => width;
    
    public BlockDoor(DoorSide side, int position, int width = 1)
    {
        this.side = side;
        this.position = position;
        this.width = width;
    }

    public Vector2Int[] GetDoorCells(int blockSize = 16)
    {
        var cells = new Vector2Int[width];

        for (int i = 0; i < width; i++)
        {
            cells[i] = side switch
            {
                DoorSide.North => //top side
                    new Vector2Int(position + i, blockSize - 1),
                DoorSide.East => //right side
                    new Vector2Int(blockSize - 1, position + i),
                DoorSide.South => //bottom side
                    new Vector2Int(position + i, 0),
                DoorSide.West => //left side
                    new Vector2Int(0, position + i),
                _ => cells[i]
            };
        }

        return cells;
    }

    public bool IsCompatibleWith(BlockDoor otherDoor, DoorSide expectedOppositeSide)
    {
        if(otherDoor.side != expectedOppositeSide) return false;

        if (position != otherDoor.position) return false;
        
        return true;
    }
    
    public static DoorSide GetOppositeSide(DoorSide side)
    {
        return side switch
        {
            DoorSide.North => DoorSide.South,
            DoorSide.East => DoorSide.West,
            DoorSide.South => DoorSide.North,
            DoorSide.West => DoorSide.East,
            _ => side
        };
    }
}