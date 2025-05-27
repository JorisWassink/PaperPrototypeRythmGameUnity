using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoteLine
{
    [HideInInspector] public Vector3 position = Vector3.zero;
    public KeyCode key;
}

public class NoteLines : MonoBehaviour
{
    public static NoteLines Instance { get; private set; }

    public float spacing = 1f;
    public Vector3 center = Vector3.zero;

    [SerializeField] public List<NoteLine> noteLines = new List<NoteLine>();
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
            obj.transform.position = noteLines[i].position;
            SpawnPoints.Add(obj.transform);
        }
    }
}