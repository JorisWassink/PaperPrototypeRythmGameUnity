using System.Collections.Generic;
using UnityEngine;

public class NoteLines : MonoBehaviour
{
    public static NoteLines Instance { get; private set; }

    public int lineCount = 5;
    public float spacing = 1f;
    public Vector3 center = Vector3.zero;

    public List<Vector3> noteLines = new List<Vector3>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}