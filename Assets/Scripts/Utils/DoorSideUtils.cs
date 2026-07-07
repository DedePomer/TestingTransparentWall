using System;
using UnityEngine;
using UnityEngine.UIElements;

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

    public static DoorSideEnum GetDorSideInCell(CellObject cell, DoorData[] doordatas)
    { 
        
    }

    //public static bool CanConnectWalls(DoorData[] firstRoomDoors, DoorData[] secondRoomDoors)
    //{ 
        
    //}
}

