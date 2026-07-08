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
        int steps = (int)rotation / (int)RoomRotationEnum.R90;

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
}

