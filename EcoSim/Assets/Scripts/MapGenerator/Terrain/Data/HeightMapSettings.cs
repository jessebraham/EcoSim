using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Heightmap Settings", fileName = "HeightmapSettings")]
public class HeightMapSettings : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    public NoiseSettings noiseSettings;

    public bool useFalloff;

    public float heightMultiplier;

    public AnimationCurve heightCurve;
    public AnimationCurve falloffCurve;

    
#if UNITY_EDITOR
public void NotifyOfUpdatedValues() {
		UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        OnValuesUpdated?.Invoke();
    }
#endif
}
