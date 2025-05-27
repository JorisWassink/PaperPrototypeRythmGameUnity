using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    private void Start()
    {
        var lines = NoteLines.Instance.noteLines;
        foreach (var line in lines)
        {
            var position = new Vector3(0, line.position.y, 0);
            var l = Instantiate(linePrefab, position, Quaternion.identity);
            l.transform.SetParent(transform, true);
        }
    }
}
