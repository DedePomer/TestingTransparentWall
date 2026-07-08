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
            _rotation = (_rotation + (int)RoomRotationEnum.R90) % (int)RoomRotationEnum.R360;
            _roomData.Room.RotatedRoom(RoomRotationEnum.R90);
            _roomPreview.transform.rotation = Quaternion.Euler(0, _rotation, 0);

            //Debug.Log($"{_roomPreview.transform.position}");
        }
    }





    private void SetRoomOnGrid(Vector3 pointPosition)
    {
        CellObject selectedCell = GetCellAt(pointPosition);
        if (selectedCell == null)
        {
            return;
        }

        Dictionary<Vector2Int, CellObject> occopyCells = GetCellsForRoom(selectedCell, _roomData);
        if (occopyCells == null || CellIsOccupy(occopyCells))
        {
            return;
        }

        if (IsMargin(occopyCells))
        {
            return;
        }

        Vector3 center = GetCellsCenter(occopyCells);

        if (_roomPreview != null)
        {
            Destroy(_roomPreview);
            _roomPreview = null;
        }

        Building building = new Building(_roomData, occopyCells);

        if (_cellIsOccupied)
        {
            var neighbours = GetNeighbourCells(occopyCells);

            if (neighbours.Count == 0)
            {
                Debug.Log("Нет соседей — комнату ставить нельзя");
                return;
            }

            bool hasAnyValidConnection = false;

            foreach (var (callingLocal, neighbourCell, side) in neighbours)
            {
                DoorSideEnum oppositeSide = DoorSideUtils.GetOppositeSide(side);

                bool newRoomHasDoor = RoomHasDoor(callingLocal, side);
                bool neighbourHasDoor = NeighbourHasDoor(neighbourCell, oppositeSide);

                Debug.Log($"Граница {side}: дверь новой комнаты={newRoomHasDoor}, дверь соседа={neighbourHasDoor}. Локальные координаты {callingLocal}");

                if (newRoomHasDoor && neighbourHasDoor)
                {
                    hasAnyValidConnection = true; 
                }
            }

            if (!hasAnyValidConnection)
            {
                return;
            }
        }

        GameObject instance = Instantiate(_roomData.Room.gameObject, center, Quaternion.Euler(0, _rotation, 0));


        foreach (var cell in occopyCells)
        {
            cell.Value.SetBuilding(building);
            cell.Value.SetOccupied(true);
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

        Dictionary<Vector2Int, CellObject> cells = GetCellsForRoom(cell, _roomData);
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



    private Dictionary<Vector2Int, CellObject> GetCellsForRoom(CellObject cell, RoomData roomData)
    {
        Dictionary<Vector2Int, CellObject> occupiedCells = new();

        int sizeX = (_rotation == 0 || _rotation == 180) ? roomData.Room.SizeX : roomData.Room.SizeZ;
        int sizeZ = (_rotation == 0 || _rotation == 180) ? roomData.Room.SizeZ : roomData.Room.SizeX;

        int localX = 0, localZ = 0;

        for (int x = cell.XIndex; x < cell.XIndex + sizeX; x++)
        {
            for (int z = cell.ZIndex; z < cell.ZIndex + sizeZ; z++)
            {
                if (x >= _cells.GetLength(0) || z >= _cells.GetLength(1))
                {
                    Debug.Log("Значение вне грида");
                    return null;
                }

                occupiedCells.Add(new Vector2Int(localX, localZ), _cells[x, z]);
                localZ++;
            }
            localZ = 0;
            localX++;
        }

        //Debug.Log($"{roomData.PreviewPrefab}");
        //foreach (var item in occupiedCells)
        //{
        //    Debug.Log($"{item.Key}");
        //}

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
    private Vector3 GetCellsCenter(Dictionary<Vector2Int, CellObject> cells)
    {
        Vector3 centerVector = Vector3.zero;
        foreach (var cell in cells)
        {
            centerVector += cell.Value.Center;
        }


        centerVector = centerVector / cells.Count;
        //Debug.Log($"Центральный вектор {centerVector}");
        return centerVector;
    }

    private bool RoomHasDoor(Vector2Int localCoord, DoorSideEnum side)
    {
        int wallIndex = (side == DoorSideEnum.North || side == DoorSideEnum.South) ? localCoord.x: localCoord.y;

        foreach (var door in _roomData.Room.Doors)
        {
            if (door.Side == side && door.Index == wallIndex)
                return true;
        }

        return false;
    }
    public bool NeighbourHasDoor(CellObject cell, DoorSideEnum doorSide)
    {
        Vector2Int? localCord = cell.Building.WorldToLocal(cell);

        if (localCord != null)
        {
            int localIndex = cell.Building.GetWallIndex(localCord, doorSide);

            return cell.Building.HasDoor(doorSide, localIndex);
        }

        return false;
    }

    
    private List<(Vector2Int callingLocal, CellObject neighbourCell, DoorSideEnum side)> GetNeighbourCells(Dictionary<Vector2Int, CellObject> cells)
    {
        var result = new List<(Vector2Int, CellObject, DoorSideEnum)>();

        var directions = new (int dx, int dz, DoorSideEnum side)[]
        {
            (1, 0, DoorSideEnum.East),
            (-1, 0, DoorSideEnum.West),
            (0, 1, DoorSideEnum.North),
            (0, -1, DoorSideEnum.South)
        };
        foreach (var cell in cells)
        {
            foreach (var direction in directions)
            {
                int x = cell.Value.XIndex + direction.dx;
                int z = cell.Value.ZIndex + direction.dz;

                if (x >= 0 && x < _cells.GetLength(0) &&
                    z >= 0 && z < _cells.GetLength(1))
                {
                    CellObject neighbourCandidate = _cells[x, z];

                    if (_cells[x, z].IsOccupied && !cells.ContainsValue(neighbourCandidate))
                    {
                        result.Add((cell.Key, neighbourCandidate, direction.side));
                    }
                }

            }
        }

        Debug.Log("Нет соседа");
        return result;
    }
    private bool IsMargin(Dictionary<Vector2Int, CellObject> cells)
    {
        foreach (var cell in cells)
        {
            if (cell.Value.IsBlock)
            {
                return true;
            }
        }
        return false;
    }
    private bool CellIsOccupy(Dictionary<Vector2Int, CellObject> cells)
    {
        foreach (var cell in cells)
        {
            if (cell.Value.IsOccupied)
            {
                Debug.Log($"Ячейки заняты {cell.Value.XIndex} {cell.Value.ZIndex}");
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
