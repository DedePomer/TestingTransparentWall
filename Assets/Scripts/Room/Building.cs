using System.Collections.Generic;

public class Building
{
    public RoomPlacement RoomPlacement { get; }
    public List<CellObject> OccupiedCells { get; }

    public Building(RoomPlacement roomPlacement, List<CellObject> occupiedCells)
    {
        RoomPlacement = roomPlacement;
        OccupiedCells = occupiedCells;
    }
}
