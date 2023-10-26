using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCombat : MonoBehaviour
{
    private bool m_PlayerHasCasted;
    private bool m_CombatStance;
    private float m_CastTimer;
    [SerializeField] float m_CastCooldown = 1.8f;
    // Start is called before the first frame update
    void Start()
    {
        m_CombatStance = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_CombatStance = true;
            m_PlayerHasCasted = false;
            m_PlayerHasCasted = true;
        }

        if (m_PlayerHasCasted)
        {
            m_CastTimer += Time.deltaTime;
            if (m_CastTimer > m_CastCooldown)
            {
                m_PlayerHasCasted = false;
                m_CastTimer = 0.0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (m_CombatStance)
                m_CombatStance = false;
            else
                m_CombatStance = true;
        }
    }

    public bool HasCasted() { return m_PlayerHasCasted; }

    public bool GetCombatStance() { return m_CombatStance; }
}
