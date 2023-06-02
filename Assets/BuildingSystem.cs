using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{

    public static BuildingSystem instance;

    public GridLayout gridLayout;
    private Grid grid;

    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase occupiedTile;

    public GameObject prefab1;
    public GameObject prefab2;

    private PlacebleObject objectToPlace;

    #region Unity Methods

    private void Awake()
    {
        instance = this;
        grid = gridLayout.gameObject.GetComponent<Grid>(); 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            InitializeWithObject(prefab1);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            InitializeWithObject(prefab2);
        }

        if (!objectToPlace)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CanbePlaced(objectToPlace))
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                OccupyArea(start, objectToPlace.Size);
            }
            else
            {
                Destroy(objectToPlace.gameObject);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(objectToPlace.gameObject);
        }
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
        position = position - new Vector3(0.5f, 0, 0);
        return position;
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

        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        foreach(var b in baseArray)
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
        MainTilemap.BoxFill(start, occupiedTile, start.x, start.y, start.x + size.x, start.y + size.y);
    }


    #endregion
}
