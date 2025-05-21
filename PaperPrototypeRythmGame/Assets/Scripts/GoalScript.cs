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
    
    private List<Vector3> _positions;
    private int _currentIndex = 0;
    
    [SerializeField] private float minDistance;
    
    [SerializeField] private List<KeyCode> inputKeys;
    [SerializeField] private GameObject blockParent;
    
    [SerializeField] private int MaxKeys;

    [HideInInspector] public Transform spawnPoint;


    private bool _wasPressedLastFrame;



    private void Start()
    {
        spawnPoint = blockParent.transform;
        _renderer = GetComponent<Renderer>();
        _alpha = _renderer.material.color.a;
        _blocks = new List<GameObject>();
        _positions = NoteLines.Instance.noteLines;
        RefreshBlocks();
        UpdateBlock();
        UpdatePosition(0);
    }


    private void UpdateBlock()
    {
        
        Transform closestBlock = null;
        float closestDistance = minDistance;
        Vector3 currentPosition = transform.position;

        foreach (var block in _blocks)
        {
            Debug.Log(block.name);
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
        if (_currentBlock == null || _currentBlock.Equals(null))
        {
            UpdateBlock();
        }


        if (Input.GetKeyDown(KeyCode.DownArrow) && _currentIndex > 0)
            UpdatePosition(_currentIndex - 1);

        if (Input.GetKeyDown(KeyCode.UpArrow) && _currentIndex + 1 < _positions.Count)
            UpdatePosition(_currentIndex  + 1);
        
        
        int isPressed = 0;

        for (int i = 1; i <= MaxKeys; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                _renderer.material = (KeyMaterialMapper.Instance.GetMaterial(KeyCode.Alpha0 + i));
                isPressed = i;
                break;
            }
        }
        
        MovingBlock block = null;
        if (_currentBlock != null)
        {
            block = _currentBlock.GetComponent<MovingBlock>();
        }


        
        if (block == null)
            return;

        
        
        if (isPressed != 0)
        {
            if (_currentBlock != null && block.Key == isPressed)
            {
                if (!_wasPressedLastFrame)
                {
                    block.StartHolding(gameObject);
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

       
                
        if (isPressed == 0)
            _wasPressedLastFrame = false; // Update the last frame state
        else if (block.Key == isPressed)
            _wasPressedLastFrame = true;
    }

    private void UpdatePosition(int index)
    {
        _currentIndex = index;
        transform.position = _positions[index];
    }

    private void RefreshBlocks()
    {
        _blocks.Clear();
        foreach (Transform child in blockParent.GetComponentsInChildren<Transform>())
        {
            if (child != blockParent.transform && child.CompareTag("Block") && child.gameObject.activeInHierarchy)
            {
                _blocks.Add(child.gameObject);
                Debug.Log(child.gameObject.name);
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        _blocks.Remove(other.gameObject);
        Destroy(_currentBlock);
        UpdateBlock();
    }
}
