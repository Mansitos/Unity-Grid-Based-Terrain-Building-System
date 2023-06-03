using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 10f; // Speed at which the camera moves
    [SerializeField] public float zoomSpeed = 10f; // Speed at which the camera zooms
    [SerializeField] public float rotateSpeed = 10f; // Speed at which the camera rotates
    [SerializeField] public float minZoom = 5f; // Minimum zoom distance
    [SerializeField] public float maxZoom = 20f; // Maximum zoom distance
    [SerializeField] public KeyCode upKey = KeyCode.W; // Key to move camera forward
    [SerializeField] public KeyCode downKey = KeyCode.S; // Key to move camera backward
    [SerializeField] public KeyCode leftKey = KeyCode.A; // Key to move camera left
    [SerializeField] public KeyCode rightKey = KeyCode.D; // Key to move camera right
    [SerializeField] public string lookPointLayer = "terrain_main_plane"; // The main terrain tag on which lookPoint raycasting is calculated

    [SerializeField] public GameObject redBlockPrefab; // Prefab for the red block to instantiate

    private Camera mainCamera; // Reference to the main camera
    private Vector3 lookPoint; // Point on the terrain plane that the camera is looking at
    private float initialZoom; // Initial zoom value, set to the middle point between min and max zoom

    private void Awake()
    {
        mainCamera = Camera.main;
        lookPoint = Vector3.zero;
        initialZoom = Mathf.Lerp(minZoom, maxZoom, 0.5f);
        SetZoom(initialZoom);
    }

    private void Update()
    {
        // Move camera with arrow keys
        if (Input.GetKey(upKey))
        {
            // Move camera forward
            Vector3 forward = mainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0f;
            forward = forward.normalized;
            transform.position += forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(downKey))
        {
            // Move camera backward
            Vector3 forward = mainCamera.transform.TransformDirection(Vector3.back);
            forward.y = 0f;
            forward = forward.normalized;
            transform.position += forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(leftKey))
        {
            // Move camera left
            Vector3 right = mainCamera.transform.TransformDirection(Vector3.left);
            right.y = 0f;
            right = right.normalized;
            transform.position += right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rightKey))
        {
            // Move camera right
            Vector3 right = mainCamera.transform.TransformDirection(Vector3.right);
            right.y = 0f;
            right = right.normalized;
            transform.position += right * moveSpeed * Time.deltaTime;
        }

        // Zoom camera with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // Move the camera towards or away from the look point
            Vector3 direction = (transform.position - lookPoint).normalized;
            float distance = Vector3.Distance(transform.position, lookPoint);
            float zoomAmount = 100 * scroll * zoomSpeed * Time.deltaTime;
            float newDistance = Mathf.Clamp(distance - zoomAmount, minZoom, maxZoom);
            SetZoom(newDistance);
        }

        // Rotate camera while pressing scroll button
        if (Input.GetMouseButton(2))
        {
            // Rotate camera around look point
            UpdateLookPoint();
            float rotateAmount = 100 * Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            transform.RotateAround(lookPoint, Vector3.up, rotateAmount);
        }
    }

    // Update the look point to the current point on the terrain plane
    private void UpdateLookPoint()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(lookPointLayer)))
        {
            // Update look point to hit point on terrain
            lookPoint = hit.point;
        }
    }

    // Set the zoom value for the camera
    private void SetZoom(float distance)
    {
        if (mainCamera.orthographic)
        {
            // Set orthographic zoom
            mainCamera.orthographicSize = distance;
        }
        else
        {
            // Set perspective zoom
            mainCamera.fieldOfView = Mathf.Lerp(minZoom, maxZoom, (distance - minZoom) / (maxZoom - minZoom));
        }

        // Update camera position to match new zoom value
        Vector3 direction = (transform.position - lookPoint).normalized;
        transform.position = lookPoint + direction * distance;
    }
}