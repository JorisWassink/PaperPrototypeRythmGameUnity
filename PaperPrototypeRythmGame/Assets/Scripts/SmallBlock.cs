using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmallBlock : MovingBlock
{
    private void Start()
    {
        var text = GetComponentInChildren<TextMeshPro>();
        text.text = Key.ToString();
        Initialize();
    }
    public override void StartHolding(GameObject goal)
    {
    }

    public override void IsHolding(GameObject goal) { }

    public override void StopHolding(GameObject goal)
    {
        CalculatePoints(goal.transform.position, transform.position, minDistance);
        DestroyBlock(this);
        gameObject.tag = "Destroyed";
    }

}
