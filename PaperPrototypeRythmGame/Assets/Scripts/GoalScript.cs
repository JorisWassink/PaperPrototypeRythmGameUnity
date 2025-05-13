using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Renderer))]
public class GoalScript : MonoBehaviour
{
    private List<GameObject> _blocks;
    private GameObject _currentBlock;
    private Renderer _renderer;
    private float _alpha;
    
    [SerializeField] private float minDistance;
    
    [SerializeField] private List<KeyCode> inputKeys;
    [SerializeField] private GameObject smoke;
    [SerializeField] private GameObject blockParent;
        
    [SerializeField] private Material meh;
    [SerializeField] private Material nice;
    [SerializeField] private Material great;



    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _alpha = _renderer.material.color.a;
        _blocks = new List<GameObject>();
        foreach (Transform child in blockParent.transform)
        {
            _blocks.Add(child.gameObject);
        }
        UpdateBlock();
    }


    private void UpdateBlock()
    {
        Transform closestBlock = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (var block in _blocks)
        {
            float distance = Vector3.Distance(currentPosition, block.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBlock = block.transform;
            }
        }

        if (closestBlock != null)
            _currentBlock = closestBlock.gameObject;
        else
            _currentBlock = null;
        
    }
    

    private void Update()
    {
        RefreshBlocks(); // Refresh block list every frame
        UpdateBlock();   // Update the closest block

        if (AllKeysPressed())
        {
            var color = _renderer.material.color;
            color.a = 1;
            _renderer.material.color = color;
            if (_currentBlock != null)
            {
                BlockHit(_currentBlock);
            }
        }
        else
        {
            var color = _renderer.material.color;
            color.a = _alpha;
            _renderer.material.color = color;
        }
    }

    private void RefreshBlocks()
    {
        _blocks.Clear();
        foreach (Transform child in blockParent.transform)
        {
            if (child != null && child.gameObject.activeInHierarchy && !child.gameObject.CompareTag("Destroyed"))
            {
                _blocks.Add(child.gameObject);
            }
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

    private void OnTriggerExit(Collider other)
    {
        _blocks.Remove(other.gameObject);
        Destroy(_currentBlock);
        UpdateBlock();
    }

    private void BlockHit(GameObject block)
    {
        var renderer = GetComponent<Renderer>();
        var worldCenter = renderer.bounds.center;
        var maxDistance = renderer.bounds.extents.magnitude;

        var distanceToBlock = Vector3.Distance(worldCenter, block.transform.position);
        if (distanceToBlock > minDistance)
            return;
        
        
        var normalized = Mathf.Clamp01(1f - (distanceToBlock / maxDistance));
        var points = normalized * 3f;

        GameManager.Instance.AddPoints(points);

        block.tag = "Destroyed";
        _blocks.Remove(block);
        block.GetComponent<MovingBlock>().DestroyBlock(true);
        UpdateBlock();
        
        GameObject smokePuff = Instantiate(smoke, transform.position, transform.rotation);
    
        // Choose material based on points
        Material selectedMaterial;
        if (points < 1f)
            selectedMaterial = meh;
        else if (points < 2f)
            selectedMaterial = nice;
        else
            selectedMaterial = great;

        // Assign to particle system renderer
        var particleRenderer = smokePuff.GetComponent<ParticleSystemRenderer>();
        if (particleRenderer != null)
            particleRenderer.material = selectedMaterial;

        ParticleSystem parts = smokePuff.GetComponent<ParticleSystem>();
        float totalDuration = parts.main.duration;
        Destroy(smokePuff, totalDuration);
    }
    
    

}
