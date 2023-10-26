//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;

//public class Zap_Task : Task
//{
//    public LineRenderer LR;
//    public Transform player;
//    public float timer;

//    // Start is called before the first frame update
//    public override void Reset()
//    {
//        base.Reset();
//    }

//    public void OnStop()
//    {
//        if (LR != null)
//        {
//            LR.positionCount = 0;
//            LR.enabled = false;
//        }
//    }

//    public override NodeResult Execute()
//    {
//        //Zap target will pull that hit object out, and activate the line renderer to draw a line from the boid to the target. 
//        GameObject hitObject = (GameObject)tree.GetValue("Hit");
//        if (hitObject != null)
//        {
//            //check the boid for null when you get it out of the blackboard
//            if (hitObject.GetComponent<AI_BT>() != null)
//            {
//                if (LR == null)
//                {
//                    LR = GameObject.Instantiate(GameObject.FindObjectOfType<LineRenderer>(), Vector3.zero,
//    Quaternion.identity);
//                }

//                LR.positionCount = 2;
//                LR.enabled = true;
//                LR.sortingOrder = 1;
//                LR.SetPosition(0, player.position);
//                LR.SetPosition(1, hitObject.transform.position);
//                tree.SetValue("Zap", true);
//                return NodeResult.SUCCESS;
//            }
//        }
//        else
//        {
//            return NodeResult.RUNNING;
//        }

//        return NodeResult.FAILURE;
//    }
//}
