using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{

    private Vector3 mouseOffset;

    private void OnMouseDown()
    {
        mouseOffset = transform.position - BuildingSystem.GetMouseWorldPosition();
    }

    private void Update()
    {
        Vector3 pos = BuildingSystem.GetMouseWorldPosition() + mouseOffset;
        transform.position = BuildingSystem.instance.SnapCoordinateToGrid(pos);
    }

}
