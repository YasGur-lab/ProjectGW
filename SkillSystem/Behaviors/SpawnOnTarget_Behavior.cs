using GW.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnTarget_Behavior : MonoBehaviour
{
    protected GameObject m_Target;
    protected GameObject m_Player;
    private TargetSystem m_TargetSystem;

    // Start is called before the first frame update
    void Start()
    {
        m_Player = GameObject.Find("Player");
        m_TargetSystem = m_Player.GetComponent<TargetSystem>();
        m_Target = m_TargetSystem.GetTarget();
        gameObject.transform.position = m_Target.transform.position;
    }
}
