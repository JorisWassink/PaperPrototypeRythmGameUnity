using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallBlock : MovingBlock
{
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
