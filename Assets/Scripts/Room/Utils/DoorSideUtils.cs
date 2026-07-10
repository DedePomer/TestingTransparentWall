using System;
using System.Collections.Generic;
using UnityEngine;

public static class DoorSideUtils
{
    private static DoorSideEnum[] _sides =
    {
        DoorSideEnum.North,
        DoorSideEnum.East,
        DoorSideEnum.South,
        DoorSideEnum.West
    };

    public static DoorSideEnum GetRoatedSide(RoomRotationEnum rotation, DoorSideEnum side)
    {
        int steps = (int)rotation / (int)RoomRotationEnum.Right90;

        int index = Array.IndexOf(_sides, side);

        if (index == -1)
            return side;

        return _sides[(index + steps) % 4];
    }

    public static DoorSideEnum GetOppositeSide(DoorSideEnum side)
    {
        return side switch
        {
            DoorSideEnum.North => DoorSideEnum.South,
            DoorSideEnum.South => DoorSideEnum.North,
            DoorSideEnum.East => DoorSideEnum.West,
            DoorSideEnum.West => DoorSideEnum.East,
            _ => side
        };
    }

    public static Vector2Int ToVector(DoorSideEnum side)
    {
        return side switch
        {
            DoorSideEnum.North => Vector2Int.up,
            DoorSideEnum.East => Vector2Int.right,
            DoorSideEnum.South => Vector2Int.down,
            DoorSideEnum.West => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    public static Vector3 ToVector3(DoorSideEnum side)
    {
        return side switch
        {
            DoorSideEnum.North => Vector3.forward,
            DoorSideEnum.East => Vector3.right,
            DoorSideEnum.South => Vector3.back,
            DoorSideEnum.West => Vector3.left,
            _ => Vector3.zero
        };
    }
}

