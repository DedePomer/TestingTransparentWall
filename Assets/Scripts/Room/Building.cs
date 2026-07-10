using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public RoomData RoomData { get; }
    public List<CellObject> OccupiedCells { get; }
    public List<DoorData> Doors { get; }
    public RoomRotationEnum Rotation { get; }
    public GameObject Instance { get; }

    public Building(RoomPlacement roomPlacement, List<CellObject> occupiedCells, GameObject instance)
    {
        Rotation = roomPlacement.Rotation;
        RoomData = roomPlacement.RoomData;
        Doors = roomPlacement.GetDoorPositions();
        OccupiedCells = occupiedCells;

        Instance = instance;

        Room room = instance.GetComponent<Room>();
        DoorData[] instanceDoor = room.Doors;

        for (int i = 0; i < Doors.Count; i++)
        {
            Doors[i].Plug = instanceDoor[i].Plug;
        }
    }
}
