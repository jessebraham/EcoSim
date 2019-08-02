using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGeneratorPreview))]
public class MapGeneratorPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = target as MapGeneratorPreview;

        if ((DrawDefaultInspector() && generator.heightMapSettings.autoUpdate)
            || GUILayout.Button("Generate"))
        {
            generator.GenerateMap();
        }
    }
}
