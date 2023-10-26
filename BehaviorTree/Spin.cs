using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : Task {
    public float TurnSpeed;
    public float currentRotationAmount = 0.0f;
    
	// Use this for initialization
    public override void Reset () {
        TurnSpeed = (float)(tree.GetValue("TurnSpeed"));
        currentRotationAmount = 0.0f;

    }
	
	// Update is called once per frame
	public override NodeResult Execute () {
        currentRotationAmount += TurnSpeed * Time.deltaTime;
        tree.SetValue("CurrentSpinAngle", currentRotationAmount);
        if (currentRotationAmount > 360)
        {
            tree.SetValue("CurrentSpinAngle", 0);
            tree.gameObject.transform.Rotate(Vector3.up, 0, Space.Self);
            return NodeResult.SUCCESS;
        }
        else
        {
            tree.gameObject.transform.Rotate(Vector3.up, TurnSpeed * Time.deltaTime, Space.Self);
        }
        return NodeResult.RUNNING;
	}
}
