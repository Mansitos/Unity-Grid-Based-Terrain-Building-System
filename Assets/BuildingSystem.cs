using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BuildingSystem : MonoBehaviour
{

    public static BuildingSystem instance;

    public GridLayout gridLayout;
    private Grid grid;

    [SerializeField] private Tilemap BuildingOccupiedTilemap;
    [SerializeField] private Tilemap BuildingFreeIndicationArea;

    [SerializeField] private TileBase occupiedTile;
    [SerializeField] private TileBase freeTile;

    [SerializeField] public bool modeAllAreaInRed;

    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;

    private PlacebleObject objectToPlace;
    private BoundsInt prevArea;


    #region Unity Methods

    private void Awake()
    {
        instance = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();

        BuildingOccupiedTilemap.gameObject.SetActive(false);
        BuildingFreeIndicationArea.gameObject.SetActive(false);

        BuildingOccupiedTilemap.transform.position = Vector3.zero;
        BuildingFreeIndicationArea.transform.position = Vector3.zero;
    }

    public void onObjectPlaced()
    {
        Debug.Log("Building placed!");
        objectToPlace = null;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            InitializeWithObject(prefab1);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            InitializeWithObject(prefab2);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            InitializeWithObject(prefab3);
        }

        if (!objectToPlace)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (CanbePlaced(objectToPlace))
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                OccupyArea(start, objectToPlace.Size);
                Debug.Log("Can be placed!");
                onObjectPlaced();
            }
            else
            {
                Debug.Log("Can't be placed here!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(objectToPlace.gameObject);
            objectToPlace = null;
            Debug.Log("Placemente canceled!");
        }

        UpdateBuildingAreaIndication();
    }

    #endregion
    #region Utils


    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }

    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        Vector3 cellSize = gridLayout.cellSize;
        Vector3 offset = new Vector3(0.5f * cellSize.x, 0.0f * cellSize.y, 0.5f * cellSize.z);
        return position - offset;
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    #endregion
    #region Building Placement

    public void InitializeWithObject(GameObject prefab)
    {
        if (objectToPlace != null)
        {
            Destroy(objectToPlace.gameObject);
        }

        Vector3 position = SnapCoordinateToGrid(GetMouseWorldPosition());
        Debug.Log(position);

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = obj.GetComponent<PlacebleObject>();
        obj.AddComponent<ObjectDrag>();
    }

    private bool CanbePlaced(PlacebleObject placebleObject)
    {
        BoundsInt area = new BoundsInt();
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
        area.size = placebleObject.Size;

        Debug.Log("area Size:" + area.size.ToString());

        TileBase[] baseArray = GetTilesBlock(area, BuildingOccupiedTilemap);

        foreach (var b in baseArray)
        {
            if (b == occupiedTile)
            {
                return false;
            }
        }

        return true;
    }

    public void OccupyArea(Vector3Int start, Vector3Int size)
    {
        BuildingOccupiedTilemap.BoxFill(start, occupiedTile, start.x, start.y, start.x + size.x -1 , start.y + size.y -1);
    }

    /*
    private static void SetTilesBlock(BoundsInt area, TileBase type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }
    */

    private static void FillTiles(TileBase[] arr, TileBase type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = type;
        }
    }

    private void ClearArea()
    {
        if (prevArea != null)
        {
            TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
            FillTiles(toClear, null);
            BuildingFreeIndicationArea.SetTilesBlock(prevArea, toClear);
        }
    }

    public void UpdateBuildingAreaIndication()
    {
        ClearArea();

        if (objectToPlace != null)
        {
            BuildingFreeIndicationArea.gameObject.SetActive(true);

            objectToPlace.area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
            BoundsInt buildingArea = objectToPlace.area;

            TileBase[] baseArray = GetTilesBlock(buildingArea, BuildingOccupiedTilemap);

            int size = baseArray.Length;
            TileBase[] tileArray = new TileBase[size];

            for (int i = 0; i < baseArray.Length; i++)
            {
                if (baseArray[i] == null)
                {
                    tileArray[i] = freeTile;
                }
                else
                {
                    if(modeAllAreaInRed == false)
                    {
                        tileArray[i] = occupiedTile;
                    }
                    else
                    {
                        FillTiles(tileArray, occupiedTile);
                        break;
                    }
                    
                }
            }

            BuildingFreeIndicationArea.SetTilesBlock(buildingArea, tileArray);
            prevArea = buildingArea;
        }
        else
        {
            BuildingFreeIndicationArea.gameObject.SetActive(false);
        }
    }

    #endregion
}
