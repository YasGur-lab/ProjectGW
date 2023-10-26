using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage_Task : Task
{
    // Start is called before the first frame update

    public float TurnSpeedDecreaseValue = 5.0f;
    public float SpeedIncreaseValue = 5.0f;

    public override void Reset()
    {
        base.Reset();
    }

    public override NodeResult Execute()
    {
        Debug.Log("It worked!");
        //Damage boid needs to increase the speed and decrease the turn speed of the boid that was hit.
        GameObject hitObject = (GameObject)tree.GetValue("Hit");
        if (hitObject != null)
        {
            //might need to play with values
            Debug.Log("It worked again!");
            //hitObject.GetComponent<Boid>().turnspeed = (float)tree.GetValue("TurnSpeed");/*TurnSpeedDecreaseValue * Time.deltaTime*/;
            //hitObject.GetComponent<Boid>().speed = (float)tree.GetValue("Speed");
            tree.SetValue("Hit", null);
            return NodeResult.SUCCESS;
        }
        else
        {
            Debug.Log("It failed!");
            return NodeResult.RUNNING;
        }
    }
}
