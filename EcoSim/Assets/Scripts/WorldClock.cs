using UnityEngine;

public class WorldClock : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // PUBLIC

    public bool  active        = true;
    public float secondsPerDay = 30f;

    public int PercentComplete
    {
        get
        {
            return Mathf.RoundToInt(timer / secondsPerDay * 100);
        }
    }
    public bool IsNight
    {
        get
        {
            return PercentComplete > 50;
        }
    }


    // ------------------------------------------------------------------------
    // PRIVATE

    private float timer = 0f;


    public void Update()
    {
        if (active)
        {
            timer += Time.deltaTime;
            if (timer > secondsPerDay)
            {
                timer = 0f;
            }
        }
    }
}
