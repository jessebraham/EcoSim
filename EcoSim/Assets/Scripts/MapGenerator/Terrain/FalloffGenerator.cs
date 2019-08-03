using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size, AnimationCurve falloffCurve)
    {
        float[,] map = new float[size, size];
        var center   = size / 2f;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var distance = Vector2.Distance(new Vector2(i, j), new Vector2(center, center)) / size / 0.5f;
                map[i, j] = falloffCurve.Evaluate(Mathf.Clamp01(distance));
            }
        }

        return map;
    }
}
