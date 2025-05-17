using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LongBlock : MovingBlock
{
    public float length;
    

    
    private List<Vector3>_midPoints;
    private void Start()
    {
        SetWorldHeight(gameObject, length);
        _renderer = GetComponent<Renderer>();
        
;

        _midPoints = new List<Vector3>();
        for (int i = 0; i < (int)speed; i++) {
            float step = length / (int)speed;
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
    
    void SetWorldHeight(GameObject obj, float length)
    {
        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        var mesh = meshFilter.sharedMesh;
        if (mesh == null) return;

        float baseHeight = mesh.bounds.size.y; // base mesh height (usually 1 for cube)
        float parentScaleY = 1f;

        if (obj.transform.parent != null)
            parentScaleY = obj.transform.parent.lossyScale.y;

        // Calculate needed localScale.y
        float localScaleY = length / (baseHeight * parentScaleY);

        Vector3 scale = obj.transform.localScale;
        scale.y = localScaleY;
        obj.transform.localScale = scale;
    }

}
