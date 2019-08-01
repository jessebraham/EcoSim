using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update"))
        {
            (target as UpdatableData).NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}
