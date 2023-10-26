using GW.Control;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ProjectileDirection : MonoBehaviour
{
    [SerializeField] protected GameObject m_Target;
    [SerializeField] protected GameObject m_Player;
    private TargetSystem m_TargetSystem;
    [SerializeField] private float m_Speed = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_Player = GameObject.Find("Player");
        m_TargetSystem = m_Player.GetComponent<TargetSystem>();
        m_Target = m_TargetSystem.GetTarget();
        //gameObject.transform.position = GameObject.Find("InCombatProfessionEffect").transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = m_Target.transform.position - gameObject.transform.position;
        direction.Normalize();

        gameObject.transform.position += direction * m_Speed * Time.deltaTime;

        //float distance = Vector3.Distance(m_Target.transform.position, gameObject.transform.position);
    }
}
