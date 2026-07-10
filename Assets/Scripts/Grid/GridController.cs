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

    private RoomPlacement _currentPlacement;
    private GameObject _placementPreview;
    private PlacementValidator _placementValidator;


    private void OnEnable()
    {
        gridView.OnRoomSelected += HandleRoomSelected;
        _gridInput.OnMouseLeftButtonClicked += HandleSetRoom;
        _gridInput.OnSpaceButtonClicked += HandleRotateRoom;
    }

    private void OnDisable()
    {
        gridView.OnRoomSelected -= HandleRoomSelected;
        _gridInput.OnMouseLeftButtonClicked -= HandleSetRoom;
        _gridInput.OnSpaceButtonClicked -= HandleRotateRoom;
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

        if (TryGetTerrainHit(out raycastHit) && _currentPlacement != null)
        {
            UpdatePlacementPreview(raycastHit.point);
        }
    }

    private void HandleRoomSelected(RoomData roomData)
    {
        _currentPlacement = new RoomPlacement(roomData);
    }

    private void HandleSetRoom()
    {
        RaycastHit raycastHit;
        if (TryGetTerrainHit(out raycastHit) && _placementPreview != null)
            SetRoomOnGrid(raycastHit.point);
    }

    private void HandleRotateRoom()
    {
        if (_currentPlacement == null)
            return;

        _currentPlacement.RotateRoom();

        UpdatePreview();
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

        Vector3? center = GetCellsCenter(cells);
        if (center == null)
        {
            return;
        }


        if (_placementPreview == null)
        {
            _placementPreview = Instantiate(_currentPlacement.RoomData.PreviewPrefab, center.Value, Quaternion.identity);
        }

        _placementPreview.transform.position = center.Value;

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
        
        List<Vector2Int> cellsCord = _currentPlacement.GetOccupiedCells();
        if (cellsCord.Count == 0 || !_placementValidator.CanPlace(_currentPlacement))
        {
            return;
        }
        List<CellObject> cells = GetCells(cellsCord);

        Vector3? center = GetCellsCenter(cells);
        if (center == null)
        {
            return;
        }




        if (_cellIsOccupied)
        {

            if (!_placementValidator.HasDoorConnection(_currentPlacement)) 
            { 
                return ;
            }
        }

        if (_placementPreview != null)
        {
            Destroy(_placementPreview);
            _placementPreview = null;
        }


        Quaternion rotation = Quaternion.Euler(0, (int)_currentPlacement.Rotation, 0);
        GameObject roomObject = Instantiate(_currentPlacement.RoomData.PreviewPrefab, center.Value, rotation);
        Building building = new Building(_currentPlacement, cells, roomObject);
        

        foreach (var cell in cells)
        {
            cell.SetBuilding(building);
        }
        _placementValidator.ConnectDoors(building);

        _currentPlacement = null;
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
    private Vector3? GetCellsCenter(List<Vector2Int> positions)
    {
        Vector3 center = Vector3.zero;

        foreach (Vector2Int position in positions)
        {
            if (position.x < 0 ||
                position.y < 0 ||
                position.x > _cells.GetLength(0) - 1 ||
                position.y > _cells.GetLength(1) - 1)
                return null;

            center += _cells[position.x, position.y].Center;
        }

        return center / positions.Count;
    }
    private Vector3? GetCellsCenter(List<CellObject> cells)
    {
        Vector3 center = Vector3.zero;

        foreach (var cell in cells)
        {
            if (cell.XIndex < 0 ||
                cell.ZIndex < 0 ||
                cell.XIndex > _cells.GetLength(0) - 1 ||
                cell.ZIndex > _cells.GetLength(1) - 1)
            {
                return null;
            }


            center += _cells[cell.XIndex, cell.ZIndex].Center;
        }

        return center / cells.Count;
    }
    private List<CellObject> GetCells(List<Vector2Int> positions)
    {
        List<CellObject> cells = new List<CellObject>();

        foreach (Vector2Int position in positions)
        {
            cells.Add(_cells[position.x, position.y]);
        }

        return cells;
    }
    private void UpdatePreview()
    {
        RaycastHit hit;

        if (!TryGetTerrainHit(out hit))
            return;

        UpdatePlacementPreview(hit.point);
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

        //if (_placementValidator.DebugNeighbours.Count != 0)
        //{
        //    Gizmos.color = Color.blue;
        //    List<CellObject> test = GetCells(_placementValidator.DebugNeighbours);
        //    int t = 0;
        //    foreach (var ne in test)
        //    {
        //        Vector3 center = new Vector3(ne.Center.x, _terrainPosition.y + 6, ne.Center.z);
        //        Vector3 size = new Vector3(cellSize, 1f, cellSize);
        //        Gizmos.DrawCube(center, size);
        //        t += 10;
        //    }
        //}
    }
}
