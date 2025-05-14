using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallBlock : MovingBlock
{
    public override void StartHolding(GameObject goal)
    {
        BlockHit(goal);
        DestroyBlock(this);
        gameObject.tag = "Destroyed";
    }

    public override void IsHolding(GameObject goal) { }

    public override void StopHolding(GameObject goal) { }

}
