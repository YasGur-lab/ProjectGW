using GW.Control;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveToLocation_Behavior : MonoBehaviour
{
    [SerializeField] protected GameObject m_Target;
    [SerializeField] protected GameObject m_Player;
    [SerializeField] protected Transform m_Destination;
    private TargetSystem m_TargetSystem;
    [SerializeField][Range(0.0f, 10.0f)] protected float m_Speed;

    // Start is called before the first frame update
    void Start()
    {
        m_Player = GameObject.Find("Player");
        m_TargetSystem = m_Player.GetComponent<TargetSystem>();
        m_Target = m_TargetSystem.GetTarget();
        m_Destination = m_Player.transform.Find("KineticObjectPlacement");
    }

    // Update is called once per frame
    void Update()
    {
        //if(m_Target.transform.position != m_Destination.position)
            m_Target.transform.position = Vector3.Lerp(m_Target.transform.position, m_Player.transform.position, m_Speed * Time.deltaTime);
    }
}
