
using Scripts.Camera;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static Unity.VisualScripting.Member;
using static UnityEditor.Rendering.CameraUI;

public class CellObject
{
    public int Width { get; private set; }
    public int Length { get; private set; }
    public int Height { get; private set; }
    public Vector3 Center { get; private set; } // Центр в мировы координатах


    // Кординты двух противоположных углов в мировых координатах
    public Vector3 min { get; private set; }
    public Vector3 max { get; private set; }



    public CellObject(int width, int length, int height, Vector3 center)
    {
        Width = width;
        Length = length;
        Height = height;
        Сenter = center;
    }
}

//мне нужен менеджер которыйразделт тиррейн на ячейки одинакового размераОпределил задачу разработки и уточнил требуемый движокОпределил задачу разработки и уточнил требуемый движокУточните, пожалуйста, пару деталей, чтобы я сделал точно то, что нужно:

//Движок / платформа — это Unity(компонент Terrain), Unreal Engine, собственный движок или что-то другое?
//Что нужно на выходе — просто разбить террейн на сетку ячеек (для навигации, спавна объектов, LOD-чанков и т.п.), или ещё нужна визуализация/редактор-инструмент в сцене?
//Язык программирования — C#, C++, Python или другой?

//Если это Unity и C# (самый частый случай для "Terrain manager"), я могу сразу написать скрипт, который:

//берёт Terrain (или произвольный размер области),
//делит его на сетку ячеек заданного размера (например, 10?10 юнитов),
//хранит ссылки на ячейки (координаты, границы, высоту в центре и т.д.),
//даёт методы вроде GetCellAt(Vector3 worldPos), GetNeighbors(cell) и т.п.

//Скажите, какой вариант ваш — и я сразу подготовлю код.UnityArchitected terrain division system with configurable parametersArchitected terrain division system with configurable parametersКак использовать:

//Повесьте TerrainGridManager на объект с компонентом Terrain (или укажите ссылку в поле terrain).
//Задайте cellSize — размер ячейки в метрах.
//Сетка строится автоматически в Awake, либо вызовите BuildGrid() вручную (есть пункт в контекстном меню компонента).

//Полезные методы:

//GetCellAt(worldPos) — найти ячейку по мировой позиции.
//GetCell(x, z) — по индексам сетки.
//GetNeighbors(cell) — соседние ячейки (можно с диагоналями).
//GetAllCells() — перебрать все ячейки.

//В каждой Cell хранятся границы (min/max), центр и высота террейна в этой точке — удобно для спавна объектов, патфайндинга или чанковой подгрузки. Гизмо в сцене показывают сетку зелёными рамками.
//Если нужно что-то ещё — например, привязка объектов к ячейкам, сохранение/загрузка данных на ячейку, или интеграция с NavMesh — скажите, и я дополню скрипт.TerraingridmanagerCS         public Vector3 min;    // минимальный угол (world space)
//public Vector3 max;    // максимальный угол (world space)а что этоmin и max — это координаты противоположных углов ячейки в мировом пространстве (world space), то есть реальные координаты в сцене Unity, а не локальные относительно террейна.
//Представьте ячейку как прямоугольник на земле:

//min — координата её "ближнего" угла (меньшие X и Z)
//max — координата её "дальнего" угла (большие X и Z)

//Например, если cellSize = 10, а ячейка имеет индексы x=2, z = 3, то:
//min = (20, 0, 30)   // 2*10, 0, 3*10 — плюс позиция террейна
//max = (30, 0, 40)   // 3*10, 0, 4*10 — плюс позиция террейна
//Y - координата в min/max не используется (оставлена 0) — реальная высота хранится отдельно в поле height, потому что террейн неровный и высота у каждой ячейки своя.
//Зачем это нужно:

//Чтобы проверить, попадает ли точка в ячейку (сравнить с min и max)
//Чтобы нарисовать границу ячейки
//Чтобы вычислить размер зоны (max - min)
//Чтобы разместить объект в случайном месте внутри ячейки (интерполяция между min и max)

//Это стандартный способ описания прямоугольной области — как bounding box, только по плоским X/Z осям (без учёта высоты).т.е. это координаты противоположных углов относительно самого тиррейна без подсчёта в глобальных координатахНет, наоборот — это именно глобальные (мировые) координаты, уже с учётом позиции террейна.
//Посмотрите на строку в коде:
//csharpVector3 min = TerrainOrigin + new Vector3(x * cellSize, 0f, z * cellSize);
//Здесь TerrainOrigin — это terrain.transform.position, то есть позиция террейна в сцене. Она прибавляется, значит результат — глобальные координаты.

//Пример:
//Допустим, террейн стоит не в начале сцены, а сдвинут:
//terrain.transform.position = (100, 0, 50)   // TerrainOrigin
//cellSize = 10
//ячейка x = 2, z = 3
//Тогда:
//min = (100, 0, 50) + (20, 0, 30) = (120, 0, 80)   // глобальные
//max = (100, 0, 50) + (30, 0, 40) = (130, 0, 90)   // глобальные
//Если бы это были локальные координаты (относительно террейна), то TerrainOrigin не прибавлялся бы, и min было бы просто (20, 0, 30).

//Если хотите, могу добавить в Cell и локальные координаты тоже — иногда удобно иметь оба варианта.Как лучше сделать Raycast из камеры чтобы здания ставить?Классический способ — Physics.Raycast из камеры через позицию мыши:
//csharpvoid Update()
//{
//    if (Input.GetMouseButtonDown(0))
//    {
//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
//        {
//            // hit.point — точка попадания в мировых координатах
//            Cell cell = gridManager.GetCellAt(hit.point);
//            if (cell != null)
//                PlaceBuilding(cell);
//        }
//    }
//}
//Но есть важный вопрос — что именно рейкаст будет бить:

//Вариант 1 — Terrain Collider (проще всего)
//Terrain автоматически имеет коллайдер, ничего настраивать не нужно. Рейкаст бьёт прямо по рельефу — hit.point уже с правильной высотой.
//Вариант 2 — LayerMask (надёжнее)
//Чтобы рейкаст не попадал в уже стоящие здания или другие объекты, укажите слой:
//csharpLayerMask terrainLayer = LayerMask.GetMask("Terrain");
//Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer);
//Террейну нужно вручную выставить Layer ? Terrain в инспекторе.
//Вариант 3 — превью здания перед установкой
//Двигаете призрак-объект каждый Update, ставите по клику:
//csharpvoid Update()
//{
//    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
//    {
//        Cell cell = gridManager.GetCellAt(hit.point);
//        if (cell != null)
//        {
//            // двигаем превью в центр ячейки
//            previewObject.transform.position = new Vector3(
//                cell.center.x, cell.height, cell.center.z);

//            if (Input.GetMouseButtonDown(0))
//                PlaceBuilding(cell);
//        }
//    }
//}

//Какой вариант вам нужен? Могу написать готовый BuildingPlacer скрипт с превью, проверкой занятости ячейки и подсветкой.я не хочу делать метод публичным, который будет возвращать cell, наверно лучше делат ивентомДа, хорошая идея. Тогда BuildingPlacer кастует рейкаст, а TerrainGridManager подписывается и сам решает что делать с ячейкой.
//Схема такая:
//csharp// BuildingPlacer.cs
//public static event Action<Vector3> OnTerrainClicked;

//void Update()
//{
//    if (Input.GetMouseButtonDown(0))
//    {
//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
//            OnTerrainClicked?.Invoke(hit.point);
//    }
//}
//csharp// TerrainGridManager.cs
//private void OnEnable() => BuildingPlacer.OnTerrainClicked += HandleTerrainClicked;
//private void OnDisable() => BuildingPlacer.OnTerrainClicked -= HandleTerrainClicked;

//private void HandleTerrainClicked(Vector3 worldPoint)
//{
//    Cell cell = GetCellAt(worldPoint); // метод остаётся приватным
//    if (cell == null) return;

//    // логика размещения здания...
//}
//Плюсы такого подхода:

//GetCellAt остаётся приватным
//BuildingPlacer ничего не знает о сетке — слабая связность
//Подписаться на клик может любой другой система (UI, звук, туман войны и т.д.)

//Хотите — напишу оба скрипта полностью с таким ивентом?а как можно отрисовать полупрозрачную модельку при наведении на ячейкуДля полупрозрачного превью нужно три вещи:
//1 — Материал с прозрачностью
//В инспекторе на материале превью:

//Rendering Mode ? Transparent
//Альфа цвета ? ~0.5

//Или через код:
//csharpvoid SetupPreviewMaterial(GameObject preview)
//{
//    foreach (var renderer in preview.GetComponentsInChildren<Renderer>())
//    {
//        foreach (var mat in renderer.materials)
//        {
//            mat.SetFloat("_Mode", 3); // Transparent
//            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//            mat.SetInt("_ZWrite", 0);
//            mat.EnableKeyword("_ALPHABLEND_ON");
//            mat.renderQueue = 3000;

//            Color c = mat.color;
//            mat.color = new Color(c.r, c.g, c.b, 0.5f);
//        }
//    }
//}

//2 — Двигать превью каждый Update
//csharpvoid Update()
//{
//    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
//{
//    OnTerrainHovered?.Invoke(hit.point); // ? новый ивент для hover
//}
//}
//И в менеджере:
//csharpprivate void HandleTerrainHovered(Vector3 worldPoint)
//{
//    Cell cell = GetCellAt(worldPoint);
//if (cell == null) return;

//// ставим превью в центр ячейки
//previewObject.transform.position =
//    new Vector3(cell.center.x, cell.height, cell.center.z);
//}

//3 — Плавное перемещение(опционально)
//Чтобы превью не телепортировалось а плавно скользило между ячейками:
//csharppreviewObject.transform.position = Vector3.Lerp(
//    previewObject.transform.position,
//    targetPosition,
//    Time.deltaTime * 15f
//);

//Хотите — соберу всё это в готовый BuildingPlacer с двумя ивентами (OnTerrainHovered и OnTerrainClicked)?префаб состоит и объектов с разным материаломТогда при создании превью нужно сохранить оригинальные материалы и заменить их на прозрачные копии — чтобы не испортить исходный префаб:
//csharpprivate GameObject previewInstance;
//private Dictionary<Renderer, Material[]> originalMaterials = new();

//void CreatePreview(GameObject prefab)
//{
//    previewInstance = Instantiate(prefab);

//    // отключаем коллайдеры чтобы рейкаст не бил в превью
//    foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
//        col.enabled = false;

//    // для каждого рендерера делаем прозрачные копии материалов
//    foreach (var rend in previewInstance.GetComponentsInChildren<Renderer>())
//    {
//        originalMaterials[rend] = rend.materials;

//        var transparentMats = new Material[rend.materials.Length];
//        for (int i = 0; i < rend.materials.Length; i++)
//            transparentMats[i] = MakeTransparent(rend.materials[i]);

//        rend.materials = transparentMats;
//    }
//}

//Material MakeTransparent(Material source)
//{
//    // копируем материал чтобы не менять оригинал
//    var mat = new Material(source);

//    mat.SetFloat("_Mode", 3);
//    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//    mat.SetInt("_ZWrite", 0);
//    mat.EnableKeyword("_ALPHABLEND_ON");
//    mat.renderQueue = 3000;

//    Color c = mat.color;
//    mat.color = new Color(c.r, c.g, c.b, 0.5f);

//    return mat;
//}
//При размещении здания — уничтожаем превью и инстанцируем настоящий префаб:
//csharpvoid PlaceBuilding(Cell cell)
//{
//    Destroy(previewInstance);
//    originalMaterials.Clear();

//    Instantiate(buildingPrefab,
//        new Vector3(cell.center.x, cell.height, cell.center.z),
//        Quaternion.identity);
//}
//Ключевой момент — new Material(source) создаёт копию каждого материала, поэтому оригинальные материалы префаба не затрагиваются и у всех объектов сохраняется свой цвет/текстура, просто с добавленной прозрачностью.
//Если используете URP или HDRP — там _Mode не работает, скажите какой рендер-пайплайн и я подправлю под него.а как помещать префаб комнаты на сцену т.е. его надо создать и выбрать местоДля этого нужен небольшой стейт-машин — три состояния:
//Idle ? (выбрали префаб) ? Placing ? (кликнули) ? Idle
//csharppublic class BuildingPlacer : MonoBehaviour
//{
//    public static event Action<Vector3> OnTerrainHovered;
//    public static event Action<Vector3> OnTerrainClicked;

//    [SerializeField] private LayerMask terrainLayer;

//    private enum State { Idle, Placing }
//    private State state = State.Idle;

//    private GameObject currentPrefab;
//    private GameObject previewInstance;
//    private Dictionary<Renderer, Material[]> originalMaterials = new();

//    // вызываете когда игрок выбрал комнату (из UI кнопки, например)
//    public void SelectPrefab(GameObject prefab)
//    {
//        CancelPlacing();

//        currentPrefab = prefab;
//        previewInstance = CreatePreview(prefab);
//        state = State.Placing;
//    }

//    void Update()
//    {
//        if (state == State.Idle) return;

//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
//            return;

//        // двигаем превью
//        OnTerrainHovered?.Invoke(hit.point);

//        // ставим по клику
//        if (Input.GetMouseButtonDown(0))
//            OnTerrainClicked?.Invoke(hit.point);

//        // отмена по правой кнопке или Escape
//        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
//            CancelPlacing();
//    }

//    public void CancelPlacing()
//    {
//        if (previewInstance != null)
//            Destroy(previewInstance);

//        originalMaterials.Clear();
//        currentPrefab = null;
//        state = State.Idle;
//    }

//    private GameObject CreatePreview(GameObject prefab)
//    {
//        var instance = Instantiate(prefab);

//        foreach (var col in instance.GetComponentsInChildren<Collider>())
//            col.enabled = false;

//        foreach (var rend in instance.GetComponentsInChildren<Renderer>())
//        {
//            originalMaterials[rend] = rend.materials;

//            var transparentMats = new Material[rend.materials.Length];
//            for (int i = 0; i < rend.materials.Length; i++)
//                transparentMats[i] = MakeTransparent(rend.materials[i]);

//            rend.materials = transparentMats;
//        }

//        return instance;
//    }

//    private Material MakeTransparent(Material source)
//    {
//        var mat = new Material(source);
//        mat.SetFloat("_Mode", 3);
//        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//        mat.SetInt("_ZWrite", 0);
//        mat.EnableKeyword("_ALPHABLEND_ON");
//        mat.renderQueue = 3000;
//        Color c = mat.color;
//        mat.color = new Color(c.r, c.g, c.b, 0.5f);
//        return mat;
//    }
//}
//В TerrainGridManager добавляете подписки:
//csharpprivate void OnEnable()
//{
//    BuildingPlacer.OnTerrainHovered += HandleHovered;
//BuildingPlacer.OnTerrainClicked += HandleClicked;
//}

//private void OnDisable()
//{
//    BuildingPlacer.OnTerrainHovered -= HandleHovered;
//    BuildingPlacer.OnTerrainClicked -= HandleClicked;
//}

//private void HandleHovered(Vector3 worldPoint)
//{
//    Cell cell = GetCellAt(worldPoint);
//    if (cell == null) return;

//    // плавно двигаем превью в центр ячейки
//    var target = new Vector3(cell.center.x, cell.height, cell.center.z);
//    previewInstance.transform.position = Vector3.Lerp(
//        previewInstance.transform.position, target, Time.deltaTime * 20f);
//}

//private void HandleClicked(Vector3 worldPoint)
//{
//    Cell cell = GetCellAt(worldPoint);
//    if (cell == null || cell.isOccupied) return;

//    cell.isOccupied = true; // добавьте это поле в Cell
//    Instantiate(currentPrefab,
//        new Vector3(cell.center.x, cell.height, cell.center.z),
//        Quaternion.identity);

//    placer.CancelPlacing();
//}
//Из UI кнопки просто вызываете:
//csharp// на кнопке в инспекторе или через код
//placer.SelectPrefab(roomPrefab);
//Поток получается такой:
//[UI кнопка] ? SelectPrefab()
//                    ?
//             появляется превью, следует за мышью
//                    ?
//             [ЛКМ] ? комната ставится, ячейка помечается занятой
//             [ПКМ / Escape] ? отмена, превью удаляется
//Хотите добавить подсветку ячейки (красная если занята, зелёная если свободна)?а зачем машина состоянийВ данном случае она очень простая — всего два состояния, так что можно заменить одним булевым флагом:
//csharpprivate bool isPlacing = false;

//public void SelectPrefab(GameObject prefab)
//{
//    currentPrefab = prefab;
//    previewInstance = CreatePreview(prefab);
//    isPlacing = true;
//}

//void Update()
//{
//    if (!isPlacing) return;
//    // ...
//}
//Стейт - машина оправдана если состояний больше, например:
//Idle ? Placing ? Rotating ? Confirming
//Где игрок сначала выбирает место, потом крутит здание, потом подтверждает. Вот тогда enum State читается гораздо чище чем куча булевых флагов типа isPlacing, isRotating, isConfirming.
//В вашем случае — достаточно простого bool.у меня такая реализация using Scripts.Camera;
//using UnityEngine;
//public class GridManager : MonoBehaviour
//{
//    [Header("Components")]
//    [SerializeField] private Terrain terrain;
//    [SerializeField] private GameObject roomPreviewPrefab;
//    [SerializeField] private CameraConroller mainCamera;
//    [Header("GridOptions")]
//    [SerializeField] private int cellSize;
//    [Header("Debug")]
//    [SerializeField] private bool drawGizmos = true;
//    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.5f);
//    private CellObject[,] _cells;
//    private int _terrainWidth;
//    private int _terrainHeight;
//    private int _terrainLength;
//    private Vector3 _terrainPosition;
//    private GameObject _plug;
//    private void OnEnable()
//    {
//        mainCamera.OnTerrainCliked += HandleTerrainClicked;
//        mainCamera.OnLeftButtonCliked += HandleSetRoomClicked;
//    }
//    private void OnDisable()
//    {
//        mainCamera.OnTerrainCliked -= HandleTerrainClicked;
//        mainCamera.OnLeftButtonCliked -= HandleSetRoomClicked;
//    }
//    private void Awake()
//    {
//        _plug = Instantiate(
//            roomPreviewPrefab,
//            new Vector3(0, 0, 0),
//            Quaternion.identity);
//        Vector3 terrainSize = terrain.terrainData.size;
//        _terrainPosition = terrain.transform.position;
//        _terrainWidth = Mathf.FloorToInt(terrainSize.x);
//        _terrainHeight = Mathf.FloorToInt(terrainSize.y);
//        _terrainLength = Mathf.FloorToInt(terrainSize.z);
//        BuildGrid();
//    }
//    private void HandleSetRoomClicked()
//    { }
//    private void HandleTerrainClicked(Vector3 pointPosition)
//    {
//        CellObject cell = GetCellAt(pointPosition);
//        _plug.transform.position =
//            new Vector3(cell.Сenter.x, cell.Сenter.y, cell.Сenter.z);
//    }
//    private CellObject GetCellAt(Vector3 pointPosition)
//    {
//        Vector3 local = pointPosition - _terrainPosition;
//        int x = Mathf.FloorToInt(local.x / cellSize);
//        int z = Mathf.FloorToInt(local.z / cellSize);
//        return _cells[x, z];
//    }
//    private void BuildGrid()
//    {
//        int countXCell = _terrainWidth / cellSize;
//        int countZCell = _terrainLength / cellSize;
//        _cells = new CellObject[countXCell, countZCell];
//        for (int x = 0; x < countXCell; x++)
//        {
//            for (int z = 0; z < countZCell; z++)
//            {
//                Vector3 min = _terrainPosition + new Vector3(x * cellSize, 0f, z * cellSize);
//                Vector3 max = _terrainPosition + new Vector3((x + 1) * cellSize, 0f, (z + 1) * cellSize);
//                Vector3 center = new Vector3
//                {
//                    x = (min.x + max.x) * 0.5f,
//                    y = _terrainPosition.y,
//                    z = (min.z + max.z) * 0.5f
//                };
//                _cells[x, z] = new CellObject(cellSize, cellSize, _terrainHeight, center);
//            }
//        }
//    }
//    private void OnDrawGizmos()
//    {
//        if (!drawGizmos || _cells == null) return;
//        Gizmos.color = gizmoColor;
//        foreach (var cell in _cells)
//        {
//            if (cell == null) continue;
//            Vector3 center = new Vector3(cell.Сenter.x, _terrainPosition.y, cell.Сenter.z);
//            Vector3 size = new Vector3(cellSize, 1f, cellSize);
//            Gizmos.DrawWireCube(center, size);
//        }
//    }
//}
//Show moreХорошая реализация. Вижу несколько моментов которые стоит поправить:
//1 — _terrainHeight используется не по назначению
//Вы передаёте высоту террейна в CellObject как размер ячейки по Y, но для размещения здания нужна реальная высота рельефа в центре ячейки:
//csharp// вместо _terrainHeight
//float height = terrain.SampleHeight(center) + _terrainPosition.y;
//_cells[x, z] = new CellObject(cellSize, cellSize, height, center);
//2 — превью появляется в (0,0,0) при старте
//Лучше сразу скрыть его и показывать только когда нужно:
//csharp_plug = Instantiate(roomPreviewPrefab, Vector3.zero, Quaternion.identity);
//_plug.SetActive(false); // скрыть сразу
//И в HandleTerrainClicked:
//csharpprivate void HandleTerrainClicked(Vector3 pointPosition)
//{
//    CellObject cell = GetCellAt(pointPosition);
//if (cell == null) return;

//_plug.SetActive(true);
//_plug.transform.position = cell.Center;
//}
//3 — GetCellAt может выйти за границы массива
//Если кликнуть на самый край террейна — будет IndexOutOfRangeException:
//csharpprivate CellObject GetCellAt(Vector3 pointPosition)
//{
//    Vector3 local = pointPosition - _terrainPosition;
//int x = Mathf.Clamp(Mathf.FloorToInt(local.x / cellSize), 0, _cells.GetLength(0) - 1);
//int z = Mathf.Clamp(Mathf.FloorToInt(local.z / cellSize), 0, _cells.GetLength(1) - 1);
//return _cells[x, z];
//}
//4 — HandleSetRoomClicked пустой
//Судя по логике — это момент когда игрок подтверждает размещение. Там должно быть:
//csharpprivate void HandleSetRoomClicked()
//{
//    if (!_plug.activeSelf) return; // превью не показано — ничего не делаем

//CellObject cell = GetCellAt(_plug.transform.position);
//if (cell == null || cell.IsOccupied) return;

//cell.IsOccupied = true;
//Instantiate(roomPreviewPrefab, cell.Center, Quaternion.identity);
//}
//Добавьте IsOccupied в CellObject если его ещё нет.
//Также — у вас опечатка в названии: cell.Сenter — кириллическая С вместо латинской C. Может случайно ломать билд на некоторых системах, лучше поправить.ну просто зачем в моей реализации машина состояний. япросто запишу в CellObject что за объект там лежитДа, всё верно — в вашем случае машина состояний не нужна.
//У вас логика простая:

//кликнул на ячейку ? превью встало
//кликнул ещё раз ? здание поставилось, в CellObject записался объект

//Всё состояние хранится в самих ячейках, а не в отдельном стейт-машине. Это чище и проще.
//csharppublic class CellObject
//{
//    public Vector3 Center;
//    public GameObject PlacedObject; // null = свободна

//    public bool IsOccupied => PlacedObject != null;
//}
//И при размещении:
//csharpprivate void HandleSetRoomClicked()
//{
//    CellObject cell = GetCellAt(_plug.transform.position);
//if (cell == null || cell.IsOccupied) return;

//cell.PlacedObject = Instantiate(roomPrefab, cell.Center, Quaternion.identity);
//_plug.SetActive(false);
//}
//Стейт - машина была бы нужна если бы у вас был выбор из нескольких режимов — например, режим размещения, режим сноса, режим редактирования. Тогда переключение между ними удобнее описывать состояниями. В вашем случае это оверинжиниринг.ну вобще снос мне понадобитсяТогда два состояния — это уже оправдано. Но всё равно не нужна полноценная стейт-машина, достаточно простого enum:
//csharpprivate enum PlacingMode { None, Placing, Demolishing }
//private PlacingMode _mode = PlacingMode.None;
//И логика переключается из UI:
//csharppublic void EnterPlacingMode()
//{
//    _mode = PlacingMode.Placing;
//_plug.SetActive(true);
//}

//public void EnterDemolishingMode()
//{
//    _mode = PlacingMode.Demolishing;
//    _plug.SetActive(false); // превью не нужно при сносе
//}

//public void ExitMode()
//{
//    _mode = PlacingMode.None;
//    _plug.SetActive(false);
//}
//И в обработчиках кликов просто смотрите на режим:
//csharpprivate void HandleTerrainClicked(Vector3 pointPosition)
//{
//    CellObject cell = GetCellAt(pointPosition);
//if (cell == null) return;

//switch (_mode)
//{
//    case PlacingMode.Placing:
//        _plug.SetActive(true);
//        _plug.transform.position = cell.Center;
//        break;

//    case PlacingMode.Demolishing:
//        if (!cell.IsOccupied) return;
//        Destroy(cell.PlacedObject);
//        cell.PlacedObject = null;
//        break;
//}
//}

//private void HandleSetRoomClicked()
//{
//    if (_mode != PlacingMode.Placing) return;

//    CellObject cell = GetCellAt(_plug.transform.position);
//    if (cell == null || cell.IsOccupied) return;

//    cell.PlacedObject = Instantiate(roomPrefab, cell.Center, Quaternion.identity);
//    _plug.SetActive(false);
//    _mode = PlacingMode.None;
//}
//Логика остаётся простой — всё состояние по-прежнему в CellObject, а enum просто говорит что сейчас делает игрок.может как то связать мой массив с рендером чтобы изменения массива изменяли расположение комнат на тирейнеЭто паттерн Model-View — массив ячеек это модель, а рендер это вью. Самый чистый способ — подписать рендер на события изменения ячейки:
//csharppublic class CellObject
//{
//    public Vector3 Center;
//    public GameObject PlacedObject;
//    public bool IsOccupied => PlacedObject != null;

//    public event Action<CellObject> OnChanged; // ? ячейка сообщает об изменении

//    public void Place(GameObject prefab)
//    {
//        PlacedObject = Object.Instantiate(prefab, Center, Quaternion.identity);
//        OnChanged?.Invoke(this);
//    }

//    public void Demolish()
//    {
//        Object.Destroy(PlacedObject);
//        PlacedObject = null;
//        OnChanged?.Invoke(this);
//    }
//}
//Отдельный класс который следит за всеми ячейками и реагирует:
//csharppublic class GridRenderer : MonoBehaviour
//{
//    public void Subscribe(CellObject[,] cells)
//    {
//        foreach (var cell in cells)
//            cell.OnChanged += HandleCellChanged;
//    }

//    public void Unsubscribe(CellObject[,] cells)
//    {
//        foreach (var cell in cells)
//            cell.OnChanged -= HandleCellChanged;
//    }

//    private void HandleCellChanged(CellObject cell)
//    {
//        // здесь реагируете на изменение — подсветка, UI, звук и т.д.
//        Debug.Log($"Ячейка {cell.Center} изменилась. Занята: {cell.IsOccupied}");
//    }
//}
//В GridManager после BuildGrid:
//csharp[SerializeField] private GridRenderer gridRenderer;

//private void Awake()
//{
//    BuildGrid();
//    gridRenderer.Subscribe(_cells);
//}

//private void OnDestroy()
//{
//    gridRenderer.Unsubscribe(_cells);
//}
//И в обработчиках больше не трогаете GameObject напрямую — только через методы ячейки:
//csharp// вместо Instantiate/Destroy напрямую
//cell.Place(roomPrefab);
//cell.Demolish();
//Плюс такого подхода — если вы захотите загрузить сохранение и восстановить расположение комнат, просто вызываете cell.Place() для нужных ячеек и рендер сам всё отрисует. GridManager не знает про визуал, GridRenderer не знает про логику.
