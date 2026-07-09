using UnityEngine;

public class CellObject
{
    public int XIndex { get; private set; }
    public int ZIndex { get; private set; }
    public Vector2Int GridPosition => new Vector2Int(XIndex, ZIndex);
    public Vector3 Center { get; private set; }
    public Building Building { get; private set; }
    //public Vector3 min { get; private set; }
    //public Vector3 max { get; private set; }

    public bool IsOccupied { get; private set; }
    public bool IsBlock { get; private set; }

    public CellObject(Vector3 center, int x, int z, bool isBlock = false)
    {
        IsOccupied = false;

        Center = center;
        XIndex = x;
        ZIndex = z;
        IsBlock = isBlock;
    }

    public void SetBuilding(Building building)
    {
        Building = building;
        IsOccupied = building != null;
    }
    public void ClearBuilding()
    {
        Building = null;
        IsOccupied = false;
    }

}

