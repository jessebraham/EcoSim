using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Environment Settings", fileName = "EnvironmentSettings")]
public class EnvironmentSettings : ScriptableObject
{
    public MeshRenderer coniferousTreePrefab;
    public MeshRenderer deciduousTreePrefab;
    public MeshRenderer rockPrefab;

    [Space(10)]
    [Header("Common Settings")]
    [Range(0, 5)]
    public int maxOccupantsPerNode = 2;

    [Space(10)]
    [Header("Spawn Densities")]
    [Range(0, 2)]
    public float coniferousAvgDensity = 1f;
    [Range(0, 2)]
    public float deciduousAvgDensity  = 1f;
    [Range(0, 2)]
    public float rockAvgDensity       = 0.25f;

    [Space(10)]
    [Header("Tree Settings")]
    [Range(0, 5)]
    public float treePositionDeviation = 2f;
    [Range(0, 10)]
    public float treeRotationDeviation = 5f;
    [Range(0, 1)]
    public float coniferousTreeScale = 1.0f;
    [Range(0, 1)]
    public float deciduousTreeScale = 1.0f;
    [Range(0, 1)]
    public float treeScaleDeviation = 0.25f;

    [Space(10)]
    [Header("Rock Settings")]
    [Range(0, 5)]
    public float rockPositionDeviation = 2f;
    [Range(0, 10)]
    public float rockRotationDeviation = 5f;
    [Range(0, 1)]
    public float rockScale             = 1.0f;
    [Range(0, 1)]
    public float rockScaleDeviation    = 0.25f;
}
