using Scripts.Camera;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private GameObject roomPreviewPrefab;
    [SerializeField] private CameraConroller mainCamera;

    [Header("GridOptions")]
    [SerializeField] private int cellSize;


    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.5f);


    private CellObject[,] _cells;

    private int _terrainWidth;
    private int _terrainHeight;
    private int _terrainLength;

    private Vector3 _terrainPosition;

    private GameObject _plug;

    private void OnEnable()
    {
        mainCamera.OnTerrainCliked += HandleTerrainHovered;
        mainCamera.OnLeftButtonCliked += HandleSetRoomClicked;
    }

    private void OnDisable()
    {
        mainCamera.OnTerrainCliked -= HandleTerrainHovered;
        mainCamera.OnLeftButtonCliked -= HandleSetRoomClicked;
    }

    private void Awake()
    {
        _plug = Instantiate(
            roomPreviewPrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity);
        _plug.SetActive(false);


        Vector3 terrainSize = terrain.terrainData.size;
        _terrainPosition = terrain.transform.position;

        _terrainWidth = Mathf.FloorToInt(terrainSize.x);
        _terrainHeight = Mathf.FloorToInt(terrainSize.y);
        _terrainLength = Mathf.FloorToInt(terrainSize.z);

        BuildGrid();
    }

    private void HandleSetRoomClicked()
    { 
        
    }


    private void HandleTerrainHovered(Vector3 pointPosition)
    {
        CellObject cell = GetCellAt(pointPosition);

        _plug.SetActive(true);
        _plug.transform.position =
            new Vector3(cell.Center.x, cell.Center.y, cell.Center.z);
    }

    private CellObject GetCellAt(Vector3 pointPosition)
    {
        Vector3 local = pointPosition - _terrainPosition;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int z = Mathf.FloorToInt(local.z / cellSize);

        return _cells[x, z];
    }


    private void BuildGrid()
    {
        int countXCell = _terrainWidth / cellSize;
        int countZCell = _terrainLength / cellSize;

        _cells = new CellObject[countXCell, countZCell];

        for (int x = 0; x < countXCell; x++)
        {
            for (int z = 0; z < countZCell; z++)
            {
                Vector3 min = _terrainPosition + new Vector3(x * cellSize, 0f, z * cellSize);
                Vector3 max = _terrainPosition + new Vector3((x + 1) * cellSize, 0f, (z + 1) * cellSize);

                Vector3 center = new Vector3
                {
                    x = (min.x + max.x) * 0.5f,
                    y = _terrainPosition.y,
                    z = (min.z + max.z) * 0.5f
                };

                _cells[x, z] = new CellObject(cellSize, cellSize, _terrainHeight, center);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || _cells == null) return;

        Gizmos.color = gizmoColor;
        foreach (var cell in _cells)
        {
            if (cell == null) continue;
            Vector3 center = new Vector3(cell.Center.x, _terrainPosition.y, cell.Center.z);
            Vector3 size = new Vector3(cellSize, 1f, cellSize);
            Gizmos.DrawWireCube(center, size);
        }
    }

}
