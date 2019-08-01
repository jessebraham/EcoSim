using UnityEngine;

public class DayNightCycleController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // PUBLIC

    public bool  active        = true;
    public float secondsPerDay = 30f;


    // ------------------------------------------------------------------------
    // PRIVATE

    private Light sun;

    private float timer = 0f;

    private float PercentComplete
    {
        get
        {
            return timer / secondsPerDay;
        }
    }
    private bool IsNight
    {
        get
        {
            return PercentComplete > 0.5f;
        }
    }


    public void Awake()
    {
        sun = GameObject.Find("/Sun").GetComponent<Light>();
    }

    public void Update()
    {
        if (active)
        {
            timer += Time.deltaTime;
            if (timer > secondsPerDay)
            {
                timer = 0f;
            }

            UpdateSunIntensity();

            transform.RotateAround(Vector3.zero, Vector3.right, 360f / secondsPerDay * Time.deltaTime);
            transform.LookAt(Vector3.zero);
        }
    }


    private void UpdateSunIntensity()
    {
        if (IsNight)
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
