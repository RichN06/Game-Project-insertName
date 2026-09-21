using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Unity 6 Cinemachine Namespace

[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class CinemachineZoom : MonoBehaviour
{
    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference zoomAction;

    [Header("Zoom Constraints")]
    [SerializeField] private float minZoomDistance = -3f;  // Closest bounds offset
    [SerializeField] private float maxZoomDistance = 5f;   // Furthest bounds offset
    [SerializeField] private float zoomSensitivity = 0.5f; // Scroll speed modifier
    [SerializeField] private float smoothSpeed = 10f;      // Smoothing rate

    private CinemachineOrbitalFollow orbitalFollow;
    private float targetZoomValue = 0f;

    private void Start()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        
        // Grab the starting camera distance from your inspector setup
        targetZoomValue = orbitalFollow.RadialAxis.Value;
    }

    private void OnEnable()
    {
        // Only enable if the action reference isn't empty!
        if (zoomAction != null && zoomAction.action != null)
        {
            zoomAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Only disable if the action reference isn't empty!
        if (zoomAction != null && zoomAction.action != null)
        {
            zoomAction.action.Disable();
        }
    }


    private void Update()
    {
        // 1. Read input scrolling values from the mouse wheel
        Vector2 scrollDelta = zoomAction.action.ReadValue<Vector2>();

        if (scrollDelta.y != 0)
        {
            // Scrolling forward yields positive values (zooming in), so we subtract distance
            float direction = scrollDelta.y > 0 ? -zoomSensitivity : zoomSensitivity;
            targetZoomValue += direction;

            // Lock boundaries securely within your constraints range
            targetZoomValue = Mathf.Clamp(targetZoomValue, minZoomDistance, maxZoomDistance);
        }

        // 2. Smoothly slide the Radial Axis distance parameter for cinematic scaling style
        orbitalFollow.RadialAxis.Value = Mathf.Lerp(orbitalFollow.RadialAxis.Value, targetZoomValue, smoothSpeed * Time.deltaTime);
    }
}
