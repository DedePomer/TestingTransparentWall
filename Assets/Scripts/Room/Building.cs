using System.Collections.Generic;

public class Building
{
    public RoomData RoomData { get; }
    public List<CellObject> OccupiedCells { get; }

    public Building(RoomData roomData, List<CellObject> occupiedCells)
    {
        RoomData = roomData;
        OccupiedCells = occupiedCells;
    }
}
