using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoteLines))]
public class NoteLineEditor : Editor
{
    void OnSceneGUI()
    {
        NoteLines script = (NoteLines)target;

        // Editable center position
        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.PositionHandle(script.center, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(script, "Move Center");
            script.center = newCenter;
            UpdateNoteLineYs(script);
        }

        // Draw lines
        Handles.color = Color.green;
        int half = script.noteLines.Count / 2;
        for (int i = -half; i <= half; i++)
        {
            float y = script.center.y + i * script.spacing;
            Vector3 left = new Vector3(-1000, y, 0);
            Vector3 right = new Vector3(1000, y, 0);
            Handles.DrawLine(left, right);
        }
    }

    public override void OnInspectorGUI()
    {
        NoteLines script = (NoteLines)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Update NoteLines"))
        {
            UpdateNoteLineYs(script);
        }

        if (GUILayout.Button("Center on Camera"))
        {
            script.transform.position = new Vector3(script.center.x, Camera.main.transform.position.y, script.center.z);
            script.center = script.transform.position;
        }
        
    }

    void UpdateNoteLineYs(NoteLines script)
    {
        int half = script.noteLines.Count / 2;
        
        for (int i = -half, idx = 0; i <= half; i++, idx++)
        {
            float y = script.center.y + i * script.spacing;
            NoteLine line = script.noteLines[idx];
            line.position = new Vector3(script.center.x, y, script.center.z);
        }

        EditorUtility.SetDirty(script);
    }
}