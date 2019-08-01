using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGeneratorPreview))]
public class MapGeneratorPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = target as MapGeneratorPreview;

        if (DrawDefaultInspector() && generator.autoUpdate)
        {
            generator.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
        {
            generator.GenerateMap();
        }
    }
}
