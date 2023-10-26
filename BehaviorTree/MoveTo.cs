using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTo : Task {
    public string TargetName;
    // Use this for initialization
    public float Speed = 5.0f;
    public float TurnSpeed =2.0f;
    public float Accuracy = 1.5f;

    public override NodeResult Execute()
    { 
        GameObject go = tree.gameObject;
        GameObject target = (GameObject)tree.GetValue(TargetName);

        Speed = (float)tree.GetValue("Speed"); // should, like targetname, pass the variable names in.
        TurnSpeed = (float)tree.GetValue("TurnSpeed");
        Accuracy = (float)tree.GetValue("Accuracy");

        if (Vector3.Distance(go.transform.position,target.transform.position) < Accuracy)
        {
            return NodeResult.SUCCESS;
        }
        
        Vector3 direction = target.transform.position - go.transform.position;
        go.transform.rotation = Quaternion.Slerp(go.transform.rotation, Quaternion.LookRotation(direction), TurnSpeed * Time.deltaTime);
        if (Vector3.Distance(go.transform.position, target.transform.position) < Speed * Time.deltaTime)
        {
            go.transform.position = target.transform.position;
        }
        else
        {
            go.transform.Translate(0, 0, Speed * Time.deltaTime);
        }
        return NodeResult.RUNNING;
    }

}
