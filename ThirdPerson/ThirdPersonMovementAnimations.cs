using GW.Combat;
using GW.Control;
using GW.Movement;
using UnityEngine;

public class ThirdPersonMovementAnimations : MonoBehaviour
{
    [SerializeField] public Animator m_Animator;
    [SerializeField] private AnimationClip m_JumpAnimation;
    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private GameObject m_Player;

    //[SerializeField] private bool m_PlayerIsFalling = false;
    //[SerializeField] private float m_FallingTimer;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleAnimations();

        //if (m_Player.GetComponent<Mover>().HasJumped())
        //{
        //    m_FallingTimer += Time.deltaTime;
        //    if (m_FallingTimer > 0.5f)
        //    {
        //        m_PlayerIsFalling = true;
        //    }
        //}

        //if (m_CharacterController.isGrounded)
        //{
        //    m_PlayerIsFalling = false;
        //    m_Animator.SetBool("IsFalling", m_PlayerIsFalling);
        //    m_FallingTimer = 0.0f;
        //}
    }

    void HandleAnimations()
    {
        if (Input.GetKey(KeyCode.W) || m_Player.GetComponentInChildren<Mover>().MoveCharacterWithMouse)
        {
            m_Animator.SetBool("IsRunning", true);
        }
        else
        {
            m_Animator.SetBool("IsRunning", false);
        }

        if (Input.GetKey(KeyCode.S))
        {
            m_Animator.SetBool("IsRunningBackward", true);
        }
        else
        {
            m_Animator.SetBool("IsRunningBackward", false);
        }

        if (Input.GetKey(KeyCode.A))
        {
            m_Animator.SetBool("IsStrafeLeft", true);
        }
        else
        {
            m_Animator.SetBool("IsStrafeLeft", false);
        }

        if (Input.GetKey(KeyCode.D))
        {
            m_Animator.SetBool("IsStrafeRight", true);
        }
        else 
        {
            m_Animator.SetBool("IsStrafeRight", false);
        }

        m_Animator.SetBool("IsJumping", m_Player.GetComponent<PlayerController>().IsJumping());
    }
}
