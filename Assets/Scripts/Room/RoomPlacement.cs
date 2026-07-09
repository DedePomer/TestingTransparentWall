using System.Collections.Generic;
using UnityEngine;

public class RoomPlacement
{
    public RoomData RoomData { get; }

    public CellObject AnchorCell { get; private set; }

    public RoomRotationEnum Rotation { get; private set; }

    public RoomPlacement(RoomData roomData)
    {
        RoomData = roomData;
        Rotation = RoomRotationEnum.None;
    }

    public void Reset()
    {
        AnchorCell = null;
        Rotation = RoomRotationEnum.None;
    }

    public bool IsValid()
    {
        return RoomData != null && AnchorCell != null;
    }

    public void SetAnchor(CellObject cell)
    {
        AnchorCell = cell;
    }

    public void RotateRoom()
    {
        Rotation = Rotation switch
        {
            RoomRotationEnum.None => RoomRotationEnum.Right90,
            RoomRotationEnum.Right90 => RoomRotationEnum.Right180,
            RoomRotationEnum.Right180 => RoomRotationEnum.Right270,
            RoomRotationEnum.Right270 => RoomRotationEnum.None,
            _ => RoomRotationEnum.None
        };
    }

    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> cells = new();

        Room room = RoomData.Room;

        Vector2Int anchor = new Vector2Int(AnchorCell.XIndex, AnchorCell.ZIndex);

        for (int x = 0; x < room.SizeX; x++)
        {
            for (int y = 0; y < room.SizeZ; y++)
            {
                Vector2Int local = RotateCell(new Vector2Int(x, y), room.SizeX, room.SizeZ);

                cells.Add(anchor + local);
            }
        }

        return cells;
    }

    public List<DoorData> GetDoorPositions()
    {
        List<DoorData> result = new();

        foreach (DoorData door in RoomData.Room.Doors)
        {
            DoorData rotatedDoor = RotateDoor(door);

            rotatedDoor.LocalPosition += AnchorCell.GridPosition;

            result.Add(rotatedDoor);
        }

        return result;
    }

    private DoorData RotateDoor(DoorData door)
    {
        return new DoorData
        {
            LocalPosition = RotateCell(door.LocalPosition, RoomData.Room.SizeX, RoomData.Room.SizeZ),

            Side = DoorSideUtils.GetRoatedSide(Rotation, door.Side),

            Plug = door.Plug
        };
    }

    private Vector2Int RotateCell(Vector2Int local, int width, int height)
    {
        return Rotation switch
        {
            RoomRotationEnum.None =>
                local,

            RoomRotationEnum.Right90 =>
                new Vector2Int(height - 1 - local.y, local.x),

            RoomRotationEnum.Right180 =>
                new Vector2Int(width - 1 - local.x, height - 1 - local.y),

            RoomRotationEnum.Right270 =>
                new Vector2Int(local.y, width - 1 - local.x),

            _ => local
        };
    }


}
