using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


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

    private RoomPlacement _currentPlacement;
    private GameObject _placementPreview;
    private PlacementValidator _placementValidator;


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

        _placementValidator = new PlacementValidator(_cells);
    }

    private void Update()
    {
        RaycastHit raycastHit;

        if (TryGetTerrainHit(out raycastHit) && _placementPreview != null)
        {
            UpdatePlacementPreview(raycastHit.point);
        }
    }

    private void OnRoomSelected(RoomData roomData)
    {
        _currentPlacement = new RoomPlacement(roomData);
    }

    private void OnSetRoom()
    {
        RaycastHit raycastHit;
        if (TryGetTerrainHit(out raycastHit) && _placementPreview != null)
            SetRoomOnGrid(raycastHit.point);
    }

    private void OnRotateRoom()
    {
        if (_currentPlacement == null)
            return;

        _currentPlacement.RotateRoom();

        //UpdatePreview();
    }



    private void UpdatePlacementPreview(Vector3 pointPosition)
    {
        if (_currentPlacement == null)
        {
            return;
        }

        CellObject targetCell = GetCellAt(pointPosition);
        if (targetCell == null)
        {
            return;
        }
        _currentPlacement.SetAnchor(targetCell);
        if (!_currentPlacement.IsValid())
        {
            return;
        }

        var cells = _currentPlacement.GetOccupiedCells();
        if (cells.Count == 0)
        {
            return;
        }

        Vector3 center = GetCellsCenter(cells);

        if (_placementPreview == null)
        {
            _placementPreview = Instantiate(_currentPlacement.RoomData.PreviewPrefab, center, Quaternion.identity);
        }

        _placementPreview.transform.position = center;

        _placementPreview.transform.rotation = Quaternion.Euler(0, (int)_currentPlacement.Rotation, 0);
    }

    private void SetRoomOnGrid(Vector3 pointPosition)
    {
        if (_currentPlacement == null || !_currentPlacement.IsValid())
        {
            return;
        }

        CellObject targetCell = GetCellAt(pointPosition);
        if (targetCell == null)
        {
            return;
        }

        var cells = _currentPlacement.GetOccupiedCells();
        if (cells.Count == 0 || _placementValidator.CanPlace(_currentPlacement))
        {
            return;
        }
        Vector3 center = GetCellsCenter(cells);

        if (_placementPreview != null)
        {
            Destroy(_placementPreview);
            _placementPreview = null;
        }



        if (_cellIsOccupied)
        {
            //var neighbours = GetNeighbourCells(occopyCells);

            //if (neighbours.Count == 0)
            //{
            //    Debug.Log("Ќет соседей Ч комнату ставить нельз€");
            //    return;
            //}

            //bool hasAnyValidConnection = false;

            //foreach (var (callingLocal, neighbourCell, side) in neighbours)
            //{
            //    DoorSideEnum oppositeSide = DoorSideUtils.GetOppositeSide(side);

            //    bool newRoomHasDoor = RoomHasDoor(callingLocal, side);
            //    bool neighbourHasDoor = NeighbourHasDoor(neighbourCell, oppositeSide);

            //    Debug.Log($"√раница {side}: дверь новой комнаты={newRoomHasDoor}, дверь соседа={neighbourHasDoor}. Ћокальные координаты {callingLocal}");

            //    if (newRoomHasDoor && neighbourHasDoor)
            //    {
            //        hasAnyValidConnection = true;
            //    }
            //}

            //if (!hasAnyValidConnection)
            //{
            //    return;
            //}
        }

        //Building building = new Building(_currentPlacement.RoomData, cells);
        //GameObject instance = Instantiate(_roomData.Room.gameObject, center, Quaternion.Euler(0, _rotation, 0));


        //foreach (var cell in cells)
        //{
        //    cell.SetBuilding(building);
        //}


        _cellIsOccupied = true;
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

    private Vector3 GetCellsCenter(List<Vector2Int> positions)
    {
        Vector3 center = Vector3.zero;

        foreach (Vector2Int position in positions)
        {
            center += _cells[position.x, position.y].Center;
        }

        return center / positions.Count;
    }

    //private bool RoomHasDoor(Vector2Int localCoord, DoorSideEnum side)
    //{
    //    int wallIndex = (side == DoorSideEnum.North || side == DoorSideEnum.South) ? localCoord.x : localCoord.y;

    //    foreach (var door in _roomData.Room.Doors)
    //    {
    //        if (door.Side == side && door.Index == wallIndex)
    //            return true;
    //    }

    //    return false;
    //}
    //public bool NeighbourHasDoor(CellObject cell, DoorSideEnum doorSide)
    //{
    //    Vector2Int? localCord = cell.Building.WorldToLocal(cell);

    //    if (localCord != null)
    //    {
    //        int localIndex = cell.Building.GetWallIndex(localCord, doorSide);

    //        return cell.Building.HasDoor(doorSide, localIndex);
    //    }

    //    return false;
    //}


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

        Debug.Log("Ќет соседа");
        return result;
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


                _cells[x, z] = new CellObject(center, x, z, isMargin);
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
