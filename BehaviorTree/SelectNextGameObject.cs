using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectNextGameObject : Node {
    public string ArrayKey;
    public string IndexKey;
    public string GameObjectKey;
    public string DirectionKey;
    // Use this for initialization

    public override NodeResult Execute()
    {

        int index = (int)tree.GetValue(IndexKey);
        int direction = (int)tree.GetValue(DirectionKey);
        GameObject[] goa = (GameObject[])(tree.GetValue(ArrayKey));
        index = (index + direction + goa.Length) %goa.Length;
        tree.SetValue(IndexKey, index);
        tree.SetValue(GameObjectKey, goa[index]);
        return NodeResult.SUCCESS;
    }

    public override void Reset()
    {
        GameObject[] goa = (GameObject[])(tree.GetValue(ArrayKey));
        int index = (int)tree.GetValue(IndexKey);

        tree.SetValue(GameObjectKey, goa[index]);
        base.Reset();
    }
}
