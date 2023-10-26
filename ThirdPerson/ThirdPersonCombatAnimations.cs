using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCombatAnimations : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private GameObject m_Player;
    [SerializeField] private ParticleSystem m_Effect;

    void Awake()
    {
        //var emissionModule = m_Effect.emission;
        //emissionModule.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Player.GetComponent<ThirdPersonCombat>().GetCombatStance()) // && m_Player.GetComponent<SkillBar>().GetSkillState() == SkillBar.SkillState.ready
        {
            m_Animator.SetBool("IsCasting", m_Player.GetComponent<ThirdPersonCombat>().HasCasted());
        }

        if (m_Player.GetComponent<ThirdPersonCombat>().HasCasted())
        {
            var emissionModule = m_Effect.emission;
            emissionModule.enabled = true;
        }
        else
        {
            var emissionModule = m_Effect.emission;
            emissionModule.enabled = false;
        }


        if (m_Player.GetComponent<ThirdPersonCombat>().GetCombatStance())
        {
            m_Animator.SetLayerWeight(1, 1.0f);
        }
        else
        {
            m_Animator.SetLayerWeight(1, 0.0f);
        }
    }
}
