using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public RoomData RoomData { get; private set; }

    public List<CellObject> OccupiedCells { get; private set; }

    public GameObject Instance { get; private set; }

    public Building(RoomData roomData, List<CellObject> occupiedCells, GameObject instance) 
    {
        RoomData = roomData;
        OccupiedCells = occupiedCells;
        Instance = instance;
    }
}
