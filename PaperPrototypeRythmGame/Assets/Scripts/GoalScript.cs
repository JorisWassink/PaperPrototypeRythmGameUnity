using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private GameObject blockParent;

    


    private bool _wasPressedLastFrame;



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

    private void OnEnable()
    {
        BlockSpawner.Instance.allGoals.Add(transform);
    }


    private void UpdateBlock()
    {
        Transform closestBlock = null;
        float closestDistance = minDistance;
        Vector3 currentPosition = transform.position;

        foreach (var block in _blocks)
        {
            MovingBlock movingBlock = block.GetComponent<MovingBlock>();

            float distance = Vector3.Distance(currentPosition, movingBlock.StartPosition);
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
        if (_currentBlock == null)
            UpdateBlock();

        bool isPressed = AllKeysPressed();

        if (isPressed)
        {
            if (_currentBlock != null)
            {
                if (!_wasPressedLastFrame)
                {
                    _currentBlock.GetComponent<MovingBlock>().StartHolding(gameObject);
                }

                _currentBlock.GetComponent<MovingBlock>().IsHolding(gameObject);
            }

            var color = _renderer.material.color;
            color.a = 1;
            _renderer.material.color = color;
            
        }
        else
        {
            if (_wasPressedLastFrame && _currentBlock != null)
            {
                _currentBlock.GetComponent<MovingBlock>().StopHolding(gameObject);
                _blocks.Remove(_currentBlock);
            }

            var color = _renderer.material.color;
            color.a = _alpha;
            _renderer.material.color = color;
        }

        _wasPressedLastFrame = isPressed; // Update the last frame state
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

    
}
