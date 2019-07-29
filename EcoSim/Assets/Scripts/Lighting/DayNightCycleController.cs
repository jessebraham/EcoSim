using UnityEngine;

public class DayNightCycleController : MonoBehaviour
{
    public bool Active         = true;
    public float SecondsPerDay = 30f;


    void Update()
    {
        if (Active)
        {
            transform.RotateAround(Vector3.zero, Vector3.right, 360f / SecondsPerDay * Time.deltaTime);
            transform.LookAt(Vector3.zero);
        }
    }
}
