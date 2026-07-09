using System;

[Flags]
public enum DoorSideEnum
{
    None = 0,
    North = 1 << 0,
    East = 1 << 1,
    South = 1 << 2,
    West = 1 << 3,
    All = North | East | South | West
}
