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
        int half = script.lineCount / 2;
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
    }

    void UpdateNoteLineYs(NoteLines script)
    {
        int half = script.lineCount / 2;
        script.noteLines.Clear();
        for (int i = -half; i <= half; i++)
        {
            float y = script.center.y + i * script.spacing;
            script.noteLines.Add(new Vector3(script.center.x, y, script.center.z));
        }

        EditorUtility.SetDirty(script);
    }
}