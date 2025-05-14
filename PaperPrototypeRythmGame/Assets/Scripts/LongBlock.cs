using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongBlock : MovingBlock
{
    [SerializeField] private float _Length;
    
    Vector3 StartPosition => new Vector3(
        _renderer.bounds.center.x,
        _renderer.bounds.min.y,
        _renderer.bounds.center.z
    );

    private Vector3 EndPosition => new Vector3(
        _renderer.bounds.center.x,
        _renderer.bounds.max.y,
        _renderer.bounds.center.z
    );
    
    private List<Vector3>_midPoints;
    private void Start()
    {
        transform.localScale = new Vector3(transform.localScale.x, _Length, transform.localScale.z);
        _renderer = GetComponent<Renderer>();
        
;

        _midPoints = new List<Vector3>();
        for (int i = 0; i < (int)speed; i++) {
            float step = _Length / (int)speed;
            float y = step * i;
            _midPoints.Add(new Vector3(0, y, 0));
        }
        

        
        Initialize();
    }

    public override void StartHolding(GameObject goal)
    {
        if (Vector3.Distance(goal.transform.position, StartPosition) < minDistance) 
            CalculatePoints(goal.transform.position, StartPosition, minDistance);
    }

    public override void IsHolding(GameObject goal)
    {
        Vector3 worldMid = transform.TransformPoint(_midPoints[0]);
        
        if (Vector3.Distance(goal.transform.position, worldMid) < 0.2f)
        {
            CalculatePoints(goal.transform.position, worldMid, minDistance);
            _midPoints.RemoveAt(0);
        }

    }

    public override void StopHolding(GameObject goal)
    {
        if (goal.transform.position.y >= StartPosition.y - minDistance &&
            goal.transform.position.y <= EndPosition.y + minDistance)
        {
            CalculatePoints(goal.transform.position, EndPosition, minDistance);
            DestroyBlock(true);
        }
    }
}
