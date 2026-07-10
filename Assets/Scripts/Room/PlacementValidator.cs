using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class PlacementValidator
{
    private readonly CellObject[,] _cells;


    public PlacementValidator(CellObject[,] cells)
    {
        _cells = cells;
    }


    public bool CanPlace(RoomPlacement placement)
    {
        if (placement == null)
        {
            return false;
        }

        foreach (Vector2Int ocuupiedCell in placement.GetOccupiedCells())
        {
            CellObject cell = GetCell(ocuupiedCell);

            

            if (cell == null)
            {
                return false;
            }

            if (cell.IsOccupied || cell.IsBlock)
            {
                return false;
            }


        }


        return true;
    }

    private CellObject GetCell(Vector2Int position)
    {
        if (position.x < 0 ||
            position.y < 0 ||
            position.x > _cells.GetLength(0) - 1 ||
            position.y > _cells.GetLength(1) - 1)
        {
            return null;           
        }
        

        return _cells[position.x, position.y];
    }

    public bool HasDoorConnection(RoomPlacement placement)
    {
        var doors = placement.GetDoorPositions();

        foreach (DoorData door in doors)
        {
            Vector2Int neighbourPosition =
                door.LocalPosition + DoorSideUtils.ToVector(door.Side);

            CellObject neighbourCell = GetCell(neighbourPosition);

            if (neighbourCell == null)
                continue;

            if (!neighbourCell.IsOccupied)
                continue;

            Building neighbourBuilding = neighbourCell.Building;

            if (neighbourBuilding == null)
                continue;


            foreach (DoorData neighbourDoor in neighbourBuilding.Doors)
            {
                if (neighbourDoor.LocalPosition != neighbourPosition)
                    continue;

                if (neighbourDoor.Side !=
                    DoorSideUtils.GetOppositeSide(door.Side))
                    continue;


                Debug.Log(
                    $"Найдена связь {door.Side} - {neighbourDoor.Side}");

                return true;
            }
        }

        return false;
    }

    public void ConnectDoors(Building building)
    {
        foreach (DoorData door in building.Doors)
        {
            Vector2Int neighbourPosition =
                door.LocalPosition +
                DoorSideUtils.ToVector(door.Side);


            CellObject neighbourCell =
                GetCell(neighbourPosition);


            if (neighbourCell == null)
                continue;


            Building neighbourBuilding =
                neighbourCell.Building;


            if (neighbourBuilding == null)
                continue;


            foreach (DoorData neighbourDoor in neighbourBuilding.Doors)
            {
                if (neighbourDoor.LocalPosition != neighbourPosition)
                    continue;


                if (neighbourDoor.Side !=
                    DoorSideUtils.GetOppositeSide(door.Side))
                    continue;


                Debug.Log(
                    $"New door: {door.Plug.name} scene={door.Plug.scene.name} active={door.Plug.activeSelf}");

                Debug.Log(
                    $"Neighbour door: {neighbourDoor.Plug.name} scene={neighbourDoor.Plug.scene.name} active={neighbourDoor.Plug.activeSelf}");

                door.Plug.SetActive(false);
                neighbourDoor.Plug.SetActive(false);
            }
        }
    }


}




