using System.Drawing;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Parametrs")]
    [SerializeField] DoorData[] doors;
    [SerializeField] int sizeX;
    [SerializeField] int sizeZ;

    public int SizeX => sizeX;
    public int SizeZ => sizeZ;
    public DoorData[] Doors => doors;

    private void Awake()
    {
        PlacePlug();
    }

    public void RotatedRoom(RoomRotationEnum rotation)
    {
        foreach (var door in doors) 
        {
            door.Side = DoorSideUtils.GetRoatedSide(rotation, door.Side);
        }
    }

    private void PlacePlug()
    {
        if (doors == null) 
        {
            Debug.Log($"Нету информации о двкрях на комнате {this}.");
        }
        foreach (DoorData door in doors)
        { 
            door.Plug.SetActive(true);
        }
    }
}
