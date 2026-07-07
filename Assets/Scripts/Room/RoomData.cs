using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "Rooms")]
public class RoomData : ScriptableObject
{
    [SerializeField] private Room room;
    [SerializeField] private GameObject previewPrefab;
   
    public Room Room => room;
    public GameObject PreviewPrefab => previewPrefab;

}
