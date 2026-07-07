using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public RoomData RoomData { get; private set; }

    public List<CellObject> OccupiedCells { get; private set; }

    public Building(RoomData roomData, List<CellObject> occupiedCells) 
    {
        RoomData = roomData;
        OccupiedCells = occupiedCells;
    }

    public Dictionary<CellObject, Vector2> GetLocal—oordinates()
    { 
        
    
    }
}
