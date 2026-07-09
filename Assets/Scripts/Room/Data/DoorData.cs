using UnityEngine;

[System.Serializable]
public class DoorData
{
    /// <summary>
    /// Ћокальна€ позици€ двери внутри комнаты.
    /// Ќапример:
    /// (0,0) - левый нижний угол комнаты
    /// </summary>
    public Vector2Int LocalPosition;
    public GameObject Plug;
    public DoorSideEnum Side;
}
