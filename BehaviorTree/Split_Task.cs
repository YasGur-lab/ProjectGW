using System.Collections;
using System.Collections.Generic;
using GW.Attributes;
using GW.Combat;
using UnityEngine;

public class Split_Task : Node
{
    public float skillUsageProbability = 0.1f; // Adjust the probability as needed
    private Node currentChild;
    private new BehaviorTree tree;
    public Split_Task(BehaviorTree tree)
    {
        this.tree = tree;
    }

    public override NodeResult Execute()
    {
        float randomValue = Random.value;
        currentChild = randomValue <= skillUsageProbability ? children[0] : children[1];
        NodeResult result = currentChild.Execute();
        return result;
    }
}
