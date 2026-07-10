using UnityEngine;

public class DoorPlug : MonoBehaviour
{
    public Vector2Int LocalPosition;
    public DoorSideEnum Side;

    public void Open()
    {
        gameObject.SetActive(false);
    }
}
