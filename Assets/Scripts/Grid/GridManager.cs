using NUnit.Framework;
using Scripts.Camera;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private CameraConroller mainCamera;

    [Header("GridOptions")]
    [SerializeField] private int cellSize;


    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.5f);


    private CellObject[,] _cells;
    private List<Building> _buildings = new List<Building>();

    private int _terrainWidth;
    private int _terrainHeight;
    private int _terrainLength;

    private Vector3 _terrainPosition;

    private GameObject _roomPreview;

    private void OnEnable()
    {
        mainCamera.OnRoomHovered += HandleRoomHovered;
        mainCamera.OnLeftButtonCliked += HandleSetRoomClicked;
    }

    private void OnDisable()
    {
        mainCamera.OnRoomHovered -= HandleRoomHovered;
        mainCamera.OnLeftButtonCliked -= HandleSetRoomClicked;
    }

    private void Awake()
    {

        Vector3 terrainSize = terrain.terrainData.size;
        _terrainPosition = terrain.transform.position;

        _terrainWidth = Mathf.FloorToInt(terrainSize.x);
        _terrainHeight = Mathf.FloorToInt(terrainSize.y);
        _terrainLength = Mathf.FloorToInt(terrainSize.z);

        BuildGrid();
    }

    private void HandleSetRoomClicked(Vector3 pointPosition, RoomData room)
    {
        Debug.Log("HandleSetRoomClicked START");

        CellObject cell = GetCellAt(pointPosition);

        List<CellObject> cells = GetCellsForRoom(cell, room);

        Vector3 locationRoomVector = new Vector3(cell.Center.x, cell.Center.y, cell.Center.z);

        if (_roomPreview != null)
        {
            Destroy(_roomPreview);
            _roomPreview = null;
        }

        GameObject instance = Instantiate(room.RoomPrefab, locationRoomVector, Quaternion.identity);
        Building building = new Building(room, cells, instance);

        foreach (var c in cells)
            c.SetBuilding(building);

        _buildings.Add(building);

        Debug.Log("HandleSetRoomClicked END");
    }


    private void HandleRoomHovered(Vector3 pointPosition, RoomData room)
    {
        CellObject cell = GetCellAt(pointPosition);

        Vector3 locationPreviewVector = new Vector3(cell.Center.x, cell.Center.y, cell.Center.z);

        if (_roomPreview == null)
            _roomPreview = Instantiate(room.PreviewPrefab, locationPreviewVector, Quaternion.identity);


        _roomPreview.transform.position = locationPreviewVector;
    }

    private List<CellObject> GetCellsForRoom(CellObject cell, RoomData room)
    {
        List<CellObject> occupiedCells = new List<CellObject>();
      
        for (int x = cell.XIndex; x < cell.XIndex + room.SizeX; x++)
        {
            for (int z = cell.ZIndex; z < cell.ZIndex + room.SizeZ; z++)
            {
                if (x >= _cells.GetLength(0) || z >= _cells.GetLength(1))
                    return null;

                if (!_cells[x, z].IsOccupied)
                {
                    occupiedCells.Add(cell);
                }
                else
                    return null;
            }
        }

        return occupiedCells;
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

                _cells[x, z] = new CellObject(_terrainHeight, center, x, z);
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
