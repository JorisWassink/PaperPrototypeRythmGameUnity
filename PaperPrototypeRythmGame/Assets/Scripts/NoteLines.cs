using System.Collections.Generic;
using UnityEngine;

public class NoteLines : MonoBehaviour
{
    public static NoteLines Instance { get; private set; }

    public int lineCount = 5;
    public float spacing = 1f;
    public Vector3 center = Vector3.zero;

    [HideInInspector] public List<Vector3> noteLines = new List<Vector3>();
    [HideInInspector] public List<Transform> SpawnPoints = new List<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (var i = 0; i < noteLines.Count; i++)
        {
            GameObject obj = new GameObject();
            obj.name = $"SpawnPoint {i}";
            obj.transform.SetParent(transform);
            obj.transform.position = noteLines[i];
            SpawnPoints.Add(obj.transform);
        }
    }
}