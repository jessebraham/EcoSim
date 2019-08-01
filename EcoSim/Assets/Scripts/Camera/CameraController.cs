using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // PUBLIC

    public float followDistance    = 200f;
    public float minFollowDistance = 30f;
    public float maxFollowDistance = 300f;

    public float elevationAngle    = 30f;
    public float minElevationAngle = 5f;
    public float maxElevationAngle = 85f;

    public float orbitalAngle = 45f;

    public float movementSmoothingValue = 10f;
    public float rotationSmoothingValue = 20f;

    public float moveSensitivity   = 5f;
    public float scrollSensitivity = 10f;


    // ------------------------------------------------------------------------
    // PRIVATE

    private Transform cameraTarget;
    private Vector3   currentVelocity = Vector3.zero;


    public void Awake()
    {
        cameraTarget = GameObject.Find("/Camera Target").transform;

        if (QualitySettings.vSyncCount > 0)
        {
            Application.targetFrameRate = 60;
        }
        else
        {
            Application.targetFrameRate = -1;
        }
    }

    public void Update()
    {
        GetPlayerInput();

        Vector3 desiredPosition = cameraTarget.position + cameraTarget.TransformDirection(Quaternion.Euler(elevationAngle, orbitalAngle, 0f) * new Vector3(0, 0, -followDistance));

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, movementSmoothingValue * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cameraTarget.position - transform.position), rotationSmoothingValue * Time.deltaTime);
    }


    private void GetPlayerInput()
    {
        var  screenRect    = new Rect(0, 0, Screen.width, Screen.height);
        bool mouseInWindow = screenRect.Contains(Input.mousePosition);
        bool overUI        = EventSystem.current == null ? false : EventSystem.current.IsPointerOverGameObject();

        if (!mouseInWindow || overUI)
        {
            return;
        }

        // Check for right mouse button to change camera follow and elevation
        // angle.
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (mouseX > 0.01f || mouseX < -0.01f)
            {
                orbitalAngle += mouseX * moveSensitivity;

                if (orbitalAngle > 360)
                {
                    orbitalAngle -= 360;
                }

                if (orbitalAngle < 0)
                {
                    orbitalAngle += 360;
                }
            }

            if (mouseY > 0.01f || mouseY < -0.01f)
            {
                elevationAngle -= mouseY * moveSensitivity;
                elevationAngle  = Mathf.Clamp(elevationAngle, minElevationAngle, maxElevationAngle);
            }
        }

        // Check Mouse Wheel input prior to Shift key so we can apply a
        // multiplier on Shift for scrolling.
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;

        // Check MouseWheel to zoom in/out.
        if (mouseWheel < -0.01f || mouseWheel > 0.01f)
        {
            // It is assumed that the camera's Projection setting is always set
            // to 'Perspective'.
            followDistance -= mouseWheel * 5.0f;
            followDistance  = Mathf.Clamp(followDistance, minFollowDistance, maxFollowDistance);
        }
    }
}
