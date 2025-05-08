using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class GoalScript : MonoBehaviour
{
    [SerializeField] private GameObject blockParent;
    private List<GameObject> blocks;
    [SerializeField] private List<KeyCode> inputKeys;

    private GameObject _currentBlock;

    private void Start()
    {
        blocks = new List<GameObject>();
        foreach (Transform child in blockParent.transform)
        {
            blocks.Add(child.gameObject);
        }
        UpdateBlock();
    }


    private void UpdateBlock()
    {
        Transform closestBlock = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (var block in blocks)
        {
            float distance = Vector3.Distance(currentPosition, block.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBlock = block.transform;
            }
        }

        if (closestBlock != null)
        {
            _currentBlock = closestBlock.gameObject;
        }
    }
    

    void Update()
    {
        if (AllKeysPressed())
        {
            Debug.Log("AAAAAAAAAAAAAAAA OH GOD PLESE HELP ME THE BUTTON HAS BEEN PRESSED THE PAIN IS AGONIZING OH GOD OH MAN AH JEEZ");
            BlockHit(_currentBlock);
        }
    }
    
    private bool AllKeysPressed()
    {
        foreach (var key in inputKeys)
        {
            if (!Input.GetKey(key)) return false;
        }
        return true;
    }

    private void BlockHit(GameObject block)
    {
        var renderer = GetComponent<Renderer>();
        var worldCenter = renderer.bounds.center;
        var maxDistance = renderer.bounds.extents.magnitude;

        var distanceToBlock = Vector3.Distance(worldCenter, block.transform.position);
        var normalized = Mathf.Clamp01(1f - (distanceToBlock / maxDistance));
        var points = normalized * 3f;

        GameManager.Instance.currentPoints += points;
        Debug.Log(points);

        blocks.Remove(block);
        Destroy(block);
        UpdateBlock();
    }

}
