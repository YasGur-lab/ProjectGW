using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree : MonoBehaviour {
    Stack<Node> CallStack;
    public Hashtable Blackboard;
    public Node root;
    public Node m_PreviousNode;
    protected bool m_HasStopped;

    // Use this for initialization
    void Awake()
    {
        SetBT();
    }

    public virtual void Start()
    {
        float cooldownUpdateInterval = 0.01f; // Update every 0.1 seconds (adjust as needed)
        InvokeRepeating("UpdateCooldowns", cooldownUpdateInterval, cooldownUpdateInterval);
    }

    public void UpdateCooldowns()
    {
        if (!m_HasStopped)
            RunStack();
    }

    // Update is called once per frame
    //public virtual void FixedUpdate()
    //{
    //    if (!m_HasStopped)
    //        RunStack();
    //}


    public void AddKey(string key)
    {
        if (Blackboard.ContainsKey(key) == false)
        {
            Blackboard.Add(key, null);
        }
    }

    public object GetValue(string key)
    {
        SetBT();

        if (Blackboard.ContainsKey(key))
        {
            return Blackboard[key];
        }
        else
        {
            return null;
        }
    }



    public void SetValue(string key, object value)
    {
        //if (key == "ClosestTarget")
        //{
        //    Debug.Log("Setting ClosestTarget: " + value);
        //}

        SetBT();

        Blackboard[key] = value;
    }

    public void PushNode(Node node)
    {
        node.Reset();
        node.tree = this;
        CallStack.Push(node);
    }

    public void PushNodeWithOutReset(Node node)
    {
        node.tree = this;
        CallStack.Push(node);
    }
    public void PopTop()
    {
        CallStack.Pop();
    }

    public NodeResult RunStack()
    {
        if (CallStack.Count == 0)
        {
            // stack is empty - add to it
            PushNode(root);
        }

        Node top = CallStack.Peek();
        m_PreviousNode = top;
        NodeResult result = top.Execute();
        switch (result)
        {
            case NodeResult.STOP:
            {
                CallStack.Clear();
                PushNode(root); // push the root node onto the stack again
                //m_PreviousNode = top;
                m_HasStopped = true;
                return result;
            }
            case NodeResult.FAILURE:
                {
                    CallStack.Clear(); // clear the stack
                    PushNode(root); // push the root node onto the stack again
                    m_PreviousNode = top;
                    return result; //NodeResult.SUCCESS;
                }
            case NodeResult.SUCCESS:
                CallStack.Pop(); // remove this node
                if (CallStack.Count == 0)
                {
                    m_PreviousNode = top;
                    return result;
                }
                Node parent = CallStack.Peek();
                bool runstack = parent.SetChildResult(result);
                if (runstack == true)
                {
                    m_PreviousNode = top;
                    return RunStack(); // and let the parent node continue
                }
                else
                {
                    m_PreviousNode = top;
                    return result;
                }
            case NodeResult.RUNNING:
                return result; // we do not need to do anything in this case.
                        // we will continue with this node in the next frame.
            case NodeResult.STACKED:
                m_PreviousNode = top;
                return RunStack(); // let the newly added child node have some CPU
                ;
            default:
                m_PreviousNode = top;
                return result;
        }
    }

    void SetBT()
    {
        if (CallStack == null)
        {
            CallStack = new Stack<Node>();
        }
        if (Blackboard == null)
        {
            Blackboard = new Hashtable();
        }
    }

    public virtual void ResetBehabviorTree(string taskIdentifier, int depth = 0)
    {
        m_HasStopped = false;
    }

    public bool IsTargetInTagList(GameObject target, List<string> tagList)
    {
        return tagList.Contains(target.tag);
    }
}
