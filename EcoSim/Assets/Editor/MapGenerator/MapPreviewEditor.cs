using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGeneratorPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var preview = target as TerrainGeneratorPreview;

        if (DrawDefaultInspector() && preview.autoUpdate)
        {
            preview.DrawMapInEditor();
        }

        if (GUILayout.Button("Generate"))
        {
            preview.DrawMapInEditor();
        }
    }
}
