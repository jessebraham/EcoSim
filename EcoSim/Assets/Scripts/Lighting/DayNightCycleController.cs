using UnityEngine;

public class DayNightCycleController : MonoBehaviour
{
    private WorldClock clock;
    private Light      sun;


    public void Awake()
    {
        clock = GameObject.Find("/Clock").GetComponent<WorldClock>();
        sun   = GameObject.Find("/Sun").GetComponent<Light>();
    }

    public void Update()
    {
        if (clock.active)
        {
            UpdateSunIntensity();

            transform.RotateAround(Vector3.zero, Vector3.right, 360f / clock.secondsPerDay * Time.deltaTime);
            transform.LookAt(Vector3.zero);
        }
    }


    private void UpdateSunIntensity()
    {
        if (clock.IsNight)
        {
            if (sun.intensity > 0f)
            {
                sun.intensity -= 0.05f;
            }
        }
        else
        {
            if (sun.intensity < 1f)
            {
                sun.intensity += 0.05f;
            }
        }
    }
}
