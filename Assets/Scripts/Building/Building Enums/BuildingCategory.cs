using System;

[Flags]
public enum BuildingCategory
{
    None = 0,
    Production = 1 << 0,
    Logistics = 1 << 1,
    Defense = 1 << 2,
    Special = 1 << 3
}
