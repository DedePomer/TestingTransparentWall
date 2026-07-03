using NUnit.Framework;
using Scripts.Camera;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[DisallowMultipleComponent, RequireComponent(typeof(GridInput))]
public class GridController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private UnityEngine.Camera mainCamera;
    [SerializeField] private GridView gridView;


    [Header("GridOptions")]
    [SerializeField] private int cellSize;
    [SerializeField] private LayerMask terrainMask;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.5f);

    private GridInput _gridInput;

    private CellObject[,] _cells;
    private bool _cellIsOccupied = false;

    private int _terrainWidth;
    private int _terrainHeight;
    private int _terrainLength;
    private Vector3 _terrainPosition;

    private RoomData _roomData;
    private GameObject _roomPreview;

    private int _rotation = 0;

    private void OnEnable()
    {
        gridView.OnRoomSelected += OnRoomSelected;
        _gridInput.OnMouseLeftButtonClicked += OnSetRoom;
        _gridInput.OnSpaceButtonClicked += OnRotateRoom;
    }

    private void OnDisable()
    {
        gridView.OnRoomSelected -= OnRoomSelected;
        _gridInput.OnMouseLeftButtonClicked -= OnSetRoom;
        _gridInput.OnSpaceButtonClicked -= OnRotateRoom;
    }

    private void Awake()
    {
        _gridInput = GetComponent<GridInput>();

        Vector3 terrainSize = terrain.terrainData.size;
        _terrainPosition = terrain.transform.position;

        _terrainWidth = Mathf.FloorToInt(terrainSize.x);
        _terrainHeight = Mathf.FloorToInt(terrainSize.y);
        _terrainLength = Mathf.FloorToInt(terrainSize.z);

        BuildGrid();
    }

    private void Update()
    {
        RaycastHit raycastHit;

        if (TryGetTerrainHit(out raycastHit) && _roomData != null)
        {
            RoomPreview(raycastHit.point);
        }
    }

    private void OnRoomSelected(RoomData roomData)
    {
        if (_roomData != null)
        {
            _roomData = null;
        }
        _roomData = roomData;
    }

    private void OnSetRoom()
    {
        RaycastHit raycastHit;
        if (TryGetTerrainHit(out raycastHit) && _roomData != null)
            SetRoomOnGrid(raycastHit.point);
    }

    private void OnRotateRoom()
    {
        if (_roomPreview != null)
        {
            _rotation = (_rotation + 90) % 360;
            _roomPreview.transform.rotation = Quaternion.Euler(0, _rotation, 0); 
        }
    }





    private void SetRoomOnGrid(Vector3 pointPosition)
    {
        CellObject cell = GetCellAt(pointPosition);

        List<CellObject> cells = GetCellsForRoom(cell, _roomData);
        if (cells == null)
        {
            Debug.Log("Занято");
            return;
        }

        Vector3 center = GetCellsCenter(cells);

        if (_roomPreview != null)
        {
            Destroy(_roomPreview);
            _roomPreview = null;
        }

        if (_cellIsOccupied) 
        {
            if (!IsNeighbourExist(cells)) 
            {
                return;
            }
        }

        GameObject instance = Instantiate(_roomData.RoomPrefab, center, Quaternion.Euler(0,_rotation,0));
        Building building = new Building(_roomData, cells);

        foreach (var c in cells)
            c.SetBuilding(building);

        _rotation = 0;
        _roomData = null;
        _cellIsOccupied = true;
    }

    private void RoomPreview(Vector3 pointPosition)
    {

        CellObject cell = GetCellAt(pointPosition);

        if (cell.IsOccupied)
        {
            Debug.Log("Занято");
            return;
        }

        List<CellObject> cells = GetCellsForRoom(cell, _roomData);

        Vector3 center = cells != null ? GetCellsCenter(cells) : cell.Center;

        if (_roomPreview == null)
            _roomPreview = Instantiate(_roomData.PreviewPrefab, center, Quaternion.identity);


        _roomPreview.transform.position = center;
        _roomPreview.transform.rotation = Quaternion.Euler(0, _rotation, 0);
    }



    private List<CellObject> GetCellsForRoom(CellObject cell, RoomData room)
    {
        List<CellObject> occupiedCells = new List<CellObject>();

        int sizeX = (_rotation == 0 || _rotation == 180) ? room.SizeX : room.SizeZ;
        int sizeZ = (_rotation == 0 || _rotation == 180) ? room.SizeZ : room.SizeX;

        for (int x = cell.XIndex; x < cell.XIndex + sizeX; x++)
        {
            for (int z = cell.ZIndex; z < cell.ZIndex + sizeZ; z++)
            {
                if (x >= _cells.GetLength(0) || z >= _cells.GetLength(1))
                {
                    return null;
                }
                    

                if (!_cells[x, z].IsOccupied)
                {
                    occupiedCells.Add(_cells[x, z]);
                }
                else
                {
                    return null;
                }    
                    
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
    private bool TryGetTerrainHit(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(_gridInput.MousePositionVector);
        return Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask);
    }
    private Vector3 GetCellsCenter(List<CellObject> cells)
    {
        Vector3 center = Vector3.zero;
        foreach (var c in cells) center += c.Center;

        Vector3 centerVector = center / cells.Count;
        return centerVector;

    }

    private bool IsNeighbourExist(List<CellObject> cells)
    {
        int[,] directions =
        {
            { 1, 0 },   
            { -1, 0 },  
            { 0, 1 },   
            { 0, -1 }   
        };
        foreach(var cell in cells)
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int x = cell.XIndex + directions[i, 0];
                int z = cell.ZIndex + directions[i, 1];

                if (x >= 0 && x < _cells.GetLength(0) &&
                    z >= 0 && z < _cells.GetLength(1))
                {
                    if (_cells[x, z].IsOccupied)
                    {
                        return true;
                    }
                }

            }
        }
        
        Debug.Log("Нет соседа");
        return false;
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
                //назвать расположение углов нормально
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
