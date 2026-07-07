using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent, RequireComponent(typeof(GridInput))]
public class GridController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private UnityEngine.Camera mainCamera;
    [SerializeField] private GridView gridView;


    [Header("GridOptions")]
    [SerializeField] private int gridXSize;
    [SerializeField] private int gridZSize;
    [SerializeField] private int cellSize;
    [SerializeField] private int marginSize;
    [SerializeField] private LayerMask terrainMask;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color defaultCellColor;
    [SerializeField] private Color marginCellColor;

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

            Debug.Log($"{_roomPreview.transform.position}");
        }
    }





    private void SetRoomOnGrid(Vector3 pointPosition)
    {
        CellObject selectedCell = GetCellAt(pointPosition);
        if (selectedCell == null)
        {
            return;
        }

        List<CellObject> cells = GetCellsForRoom(selectedCell, _roomData);
        if (cells == null || CellIsOccupy(cells))
        {
            return;
        }

        if (IsMargin(cells))
        {
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

        GameObject instance = Instantiate(_roomData.Room.gameObject, center, Quaternion.Euler(0, _rotation, 0));
        Building building = new Building(_roomData, cells);

        foreach (var cell in cells)
        {
            cell.SetBuilding(building);
            cell.SetOccupied(true);
        }


        _rotation = 0;
        _roomData = null;
        _cellIsOccupied = true;
    
    }

    private void RoomPreview(Vector3 pointPosition)
    {

        CellObject cell = GetCellAt(pointPosition);
        if (cell == null || cell.IsOccupied)
        {
            return;
        }

        List<CellObject> cells = GetCellsForRoom(cell, _roomData);
        if (cells == null)
        {
             return;
        }

        Vector3 center = cells != null ? GetCellsCenter(cells) : cell.Center;

        if (_roomPreview == null)
            _roomPreview = Instantiate(_roomData.PreviewPrefab, center, Quaternion.identity);


        _roomPreview.transform.position = center;
        _roomPreview.transform.rotation = Quaternion.Euler(0, _rotation, 0);

        //string s = "";
        //foreach (var c in cells)
        //{
        //    s += $"{c.XIndex} {c.ZIndex}";
        //}
        //Debug.Log(s);
    }



    private List<CellObject> GetCellsForRoom(CellObject cell, RoomData roomData)
    {
        List<CellObject> occupiedCells = new List<CellObject>();

        int sizeX = (_rotation == 0 || _rotation == 180) ? roomData.Room.SizeX : roomData.Room.SizeZ;
        int sizeZ = (_rotation == 0 || _rotation == 180) ? roomData.Room.SizeZ : roomData.Room.SizeX;

        for (int x = cell.XIndex; x < cell.XIndex + sizeX; x++)
        {
            for (int z = cell.ZIndex; z < cell.ZIndex + sizeZ; z++)
            {
                if (x >= _cells.GetLength(0) || z >= _cells.GetLength(1))
                {
                    Debug.Log("Значение вне грида");
                    return null;
                }

                occupiedCells.Add(_cells[x, z]);      
            }
        }

        return occupiedCells;
    }
    private CellObject GetCellAt(Vector3 pointPosition)
    {
        Vector3 local = pointPosition - _terrainPosition;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int z = Mathf.FloorToInt(local.z / cellSize);

        if (x >= _cells.GetLength(0) || z >= _cells.GetLength(1))
        {
            return null;
        }

        return _cells[x, z];
    }
    private bool TryGetTerrainHit(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(_gridInput.MousePositionVector);
        return Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask);
    }
    private Vector3 GetCellsCenter(List<CellObject> cells)
    {
         Vector3 centerVector = Vector3.zero;
        foreach (var cell in cells)
        {
            centerVector += cell.Center;
        }    
            

        centerVector = centerVector / cells.Count;
        Debug.Log($"Центральный вектор {centerVector}");
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
        foreach (var cell in cells)
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
    private bool IsMargin(List<CellObject> cells)
    {
        foreach (var cell in cells)
        {
            if (cell.IsBlock)
            {
                return true;
            }
        }
        return false;
    }
    private bool CellIsOccupy(List<CellObject> cells)
    {
        foreach (var cell in cells)
        {
            if (cell.IsOccupied)
            {
                Debug.Log($"Ячейки заняты {cell.XIndex} {cell.ZIndex}");
                return true;
            }
        }
        return false;
    }


    private void BuildGrid()
    {
        int countXCell = gridXSize + marginSize * 2;
        int countZCell = gridZSize + marginSize * 2;

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

                bool isMargin = x < marginSize || x >= gridXSize + marginSize ||
                                z < marginSize || z >= gridZSize + marginSize;


                _cells[x, z] = new CellObject(_terrainHeight, center, x, z, isMargin);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || _cells == null) return;

        foreach (var cell in _cells)
        {
            Gizmos.color = defaultCellColor;
            if (cell == null) continue;
            if (cell.IsBlock)
            {
                Gizmos.color = marginCellColor;
            }
            Vector3 center = new Vector3(cell.Center.x, _terrainPosition.y, cell.Center.z);
            Vector3 size = new Vector3(cellSize, 1f, cellSize);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
