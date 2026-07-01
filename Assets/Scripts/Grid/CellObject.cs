
using UnityEngine;

public class CellObject
{
    public int Width { get; private set; }
    public int Length { get; private set; }
    public int Height { get; private set; }
    public Vector3 Center { get; private set; } // Центр в мировы координатах


    // Кординты двух противоположных углов в мировых координатах
    public Vector3 min { get; private set; }
    public Vector3 max { get; private set; }



    public CellObject(int width, int length, int height, Vector3 center)
    {
        Width = width;
        Length = length;
        Height = height;
        Сenter = center;
    }
}
