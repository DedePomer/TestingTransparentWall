using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "Rooms")]
public class RoomData : ScriptableObject
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeZ;

    public GameObject RoomPrefab => roomPrefab;
    public GameObject PreviewPrefab => previewPrefab;
    public int SizeX => sizeX;
    public int SizeZ => sizeZ;
}
