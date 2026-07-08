using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public RoomData RoomData { get; private set; }

    public Dictionary<Vector2Int, CellObject> OccupiedCells { get; private set; }

    public Building(RoomData roomData, Dictionary<Vector2Int, CellObject> occupiedCells) 
    {
        RoomData = roomData;
        OccupiedCells = occupiedCells;
    }

    public Vector2Int? WorldToLocal(CellObject cellObject)
    {
        foreach (var cell in OccupiedCells)
        {
            if (cell.Value == cellObject)
            {
                return cell.Key;
            }
        }
        return null;
    }

    public bool HasDoor(DoorSideEnum side, int localIndex)
    {
        foreach (var door in RoomData.Room.Doors)
        {
            if (door.Side == side && door.Index == localIndex)
                return true;
        }

        return false;
    }

    public int GetWallIndex(Vector2Int? local, DoorSideEnum side)
    {
        return side switch
        {
            DoorSideEnum.North => local.Value.x,
            DoorSideEnum.South => local.Value.x,
            DoorSideEnum.East => local.Value.y,
            DoorSideEnum.West => local.Value.y,
            _ => -1
        };
    }
}
