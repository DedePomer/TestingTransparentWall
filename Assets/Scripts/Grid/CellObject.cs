using UnityEngine;

public class CellObject
{
    public int XIndex { get; private set; }
    public int ZIndex { get; private set; }
    public int Height { get; private set; }
    public Vector3 Center { get; private set; }
    public Building Building { get; private set; }
    //public Vector3 min { get; private set; }
    //public Vector3 max { get; private set; }

    public bool IsOccupied => Building != null;
    public bool IsBlock { get; private set; }

    public CellObject(int height, Vector3 center, int x, int z, bool isBlock)
    {
        Height = height;
        Center = center;
        XIndex = x;
        ZIndex = z;
        IsBlock = isBlock;
    }

    public void SetBuilding(Building building)
    {
        Building = building;
    }

    public void ClearBuilding()
    {
        Building = null;
    }

}

