using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Heightmap Settings", fileName = "HeightmapSettings")]
public class HeightMapSettings : UpdatableData
{
    public NoiseSettings noiseSettings;

    public bool useFalloff;

    public float heightMultiplier;

    public AnimationCurve heightCurve;
    public AnimationCurve falloffCurve;
}
