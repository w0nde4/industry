using System;

[Flags]
public enum BlockType
{
    None = 0,
    Starter = 1 << 0,
    Resource = 1 << 1,
    Combat = 1 << 2,
    Empty = 1 << 3,
    Obstacle = 1 << 4,
    Special = 1 << 5
}