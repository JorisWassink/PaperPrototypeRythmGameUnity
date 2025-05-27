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
    [SerializeField] private List<KeyCode> inputKeys;
    [SerializeField] private GameObject blockParent;
    [SerializeField] private float minDistance;
    [SerializeField] private int maxKeys;
    
    [HideInInspector] public List<GameObject> blocks;
    [HideInInspector] public GameObject currentBlock;
    [HideInInspector] public bool isGoing;
    
    private Renderer _renderer;
    private float _alpha;
    private List<Vector3> _positions = new List<Vector3>();
    private int _currentIndex = 0;
    private Dictionary<KeyCode, int> _keyMap = new Dictionary<KeyCode, int>();




    private bool _wasPressedLastFrame;



    private void Start()
    {
        Cursor.visible = false;
        _renderer = GetComponent<Renderer>();
        _alpha = _renderer.material.color.a;
        blocks = new List<GameObject>();

        for (int i = 0; i < NoteLines.Instance.noteLines.Count; i++)
        {
            NoteLine line = NoteLines.Instance.noteLines[i];
            _positions.Add(line.position);
            _keyMap.Add(line.key, i);
        }
        RefreshBlocks();
        currentBlock = blocks[0];
        UpdateBlock();
        UpdatePosition(0);
    }


    public void UpdateBlock()
    {
        
        Transform closestBlock = null;
        float closestDistance = minDistance;
        Vector3 currentPosition = transform.position;

        foreach (var block in blocks)
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
            currentBlock = closestBlock.gameObject;
        else
            currentBlock = null;
    }


    private void Update()
    {
        RefreshBlocks(); 
        if (currentBlock == null || currentBlock.Equals(null))
            UpdateBlock();
        
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z; 
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        transform.position = new Vector3(transform.position.x, worldMousePosition.y, transform.position.z);
        
        
        int isPressed = 0;

        for (int i = 1; i <= maxKeys; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                _renderer.material = (KeyMaterialMapper.Instance.GetMaterial(KeyCode.Alpha0 + i));
                isPressed = i;
            }
        }
        
        MovingBlock block = null;
        if (currentBlock != null)
            block = currentBlock.GetComponent<MovingBlock>();
        
        if (block == null)
            return;

        
        
        if (isPressed != 0)
        {
            if (currentBlock != null && block.Key == isPressed)
            {
                if (!_wasPressedLastFrame)
                {
                    block.StartHolding(gameObject);
                }

                currentBlock.GetComponent<MovingBlock>().IsHolding(gameObject);
            }

            var color = _renderer.material.color;
            color.a = 1;
            _renderer.material.color = color;
            
        }
        else
        {
            if (_wasPressedLastFrame && currentBlock != null)
            {
                currentBlock.GetComponent<MovingBlock>().StopHolding(gameObject);
                blocks.Remove(currentBlock);
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
        blocks.Clear();
        foreach (Transform child in blockParent.GetComponentsInChildren<Transform>())
        {
            if (child != blockParent.transform && child.CompareTag("Block") && child.gameObject.activeInHierarchy)
            {
                blocks.Add(child.gameObject);
            }
        }
    }



}
