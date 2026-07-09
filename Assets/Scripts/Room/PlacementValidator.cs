using System.Collections.Generic;
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


        foreach (Vector2Int position in placement.GetOccupiedCells())
        {
            CellObject cell = GetCell(position);

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
            position.x >= _cells.GetLength(0) ||
            position.y >= _cells.GetLength(1))
        {
            return null;
        }

        return _cells[position.x, position.y];
    }

}


