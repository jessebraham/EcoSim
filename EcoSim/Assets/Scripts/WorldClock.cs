using UnityEngine;

public class WorldClock : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // PUBLIC

    public bool  active         = true;
    public float secondsPerDay  = 30f;
    public float dayLengthHours = 24f;

    public float PercentComplete
    {
        get
        {
            return timer / secondsPerDay;
        }
    }
    public bool IsNight
    {
        get
        {
            return PercentComplete > 0.5f;
        }
    }
    public System.TimeSpan WorldTime
    {
        get
        {
            float progress = dayLengthHours * PercentComplete;

            int hour = (int)progress;
            if (hour > 23) hour = 0;

            float fMinute = (progress - hour) * 60f;
            int   minute  = (int)fMinute;
            if (minute > 59) minute = 0;

            int second = (int)((fMinute - minute) * 60f);
            if (second > 59) second = 0;

            return new System.TimeSpan(hour, minute, second);
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
