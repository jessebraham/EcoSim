using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // PUBLIC

    public Transform CameraTarget;

    public float FollowDistance    = 200f;
    public float MinFollowDistance = 30f;
    public float MaxFollowDistance = 300f;

    public float ElevationAngle    = 30f;
    public float MinElevationAngle = 5f;
    public float MaxElevationAngle = 85f;

    public float OrbitalAngle = 45f;

    public float MovementSmoothingValue = 10f;
    public float RotationSmoothingValue = 20f;

    public float MoveSensitivity   = 5f;
    public float ScrollSensitivity = 10f;


    // ------------------------------------------------------------------------
    // PRIVATE

    private Vector3 currentVelocity = Vector3.zero;


    void Awake()
    {
        if (QualitySettings.vSyncCount > 0)
        {
            Application.targetFrameRate = 60;
        }
        else
        {
            Application.targetFrameRate = -1;
        }
    }

    void Update()
    {
        GetPlayerInput();

        Vector3 desiredPosition = CameraTarget.position + CameraTarget.TransformDirection(Quaternion.Euler(ElevationAngle, OrbitalAngle, 0f) * new Vector3(0, 0, -FollowDistance));

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, MovementSmoothingValue * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(CameraTarget.position - transform.position), RotationSmoothingValue * Time.deltaTime);
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

        // Check Mouse Wheel input prior to Shift key so we can apply a
        // multiplier on Shift for scrolling.
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity;

        // Check for right mouse button to change camera follow and elevation
        // angle.
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (mouseX > 0.01f || mouseX < -0.01f)
            {
                OrbitalAngle += mouseX * MoveSensitivity;

                if (OrbitalAngle > 360)
                {
                    OrbitalAngle -= 360;
                }

                if (OrbitalAngle < 0)
                {
                    OrbitalAngle += 360;
                }
            }

            if (mouseY > 0.01f || mouseY < -0.01f)
            {
                ElevationAngle -= mouseY * MoveSensitivity;
                ElevationAngle  = Mathf.Clamp(ElevationAngle, MinElevationAngle, MaxElevationAngle);
            }
        }

        // Check MouseWheel to zoom in/out.
        if (mouseWheel < -0.01f || mouseWheel > 0.01f)
        {
            // It is assumed that the camera's Projection setting is always set
            // to 'Perspective'.
            FollowDistance -= mouseWheel * 5.0f;
            FollowDistance  = Mathf.Clamp(FollowDistance, MinFollowDistance, MaxFollowDistance);
        }
    }
}
