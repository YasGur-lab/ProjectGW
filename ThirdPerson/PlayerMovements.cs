using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovements : MonoBehaviour
{
    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] private ThirdPersonDodging m_Dodge;


    //--Movement--
    [SerializeField] private float m_PlayerSpeed = 5.0f;
    [SerializeField] private float m_RotationSpeed = 3.0f;
    private Vector3 m_MoveDirection;
    private float m_Horizontal;

    //--Jump--
    [SerializeField] private float m_JumpForce = 3.0f; 
    private bool m_PlayerHasJumped;
    private float m_JumpCooldown = 0.5f;
    private float m_JumpTimer;

    //--Gravity--
    public float m_Gravity = 1.0f;

    void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        m_JumpTimer += Time.deltaTime;
        if (m_JumpTimer > m_JumpCooldown)
        {
            if (m_CharacterController.isGrounded)
            {
                m_PlayerHasJumped = false;
                m_JumpTimer = 0.0f;
            }
        }
    }

    void FixedUpdate()
    {
        if (HorizontalMovement)
                m_Horizontal = Input.GetAxisRaw("Horizontal");
        else
        {
            m_Horizontal = 0.0f;
            if (!FixedCamera)
                transform.Rotate(0, Input.GetAxis("Horizontal") * m_RotationSpeed, 0);
        }

        Vector3 inputDirection;
        if (!MoveCharacterWithMouse)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector3(m_Horizontal, 0.0f, vertical);
        }
        else
        {
            inputDirection = new Vector3(m_Horizontal, 0.0f, 1.0f);
        }

        Vector3 transformDirection = transform.TransformDirection(inputDirection);
        transformDirection.Normalize();
        Vector3 flatMovement = transformDirection * m_PlayerSpeed * Time.deltaTime;

        if (PlayerJumped && !m_PlayerHasJumped)
        {
            m_MoveDirection.y = m_JumpForce;
            m_PlayerHasJumped = true;
        }
        else if (!m_CharacterController.isGrounded)
        {
            m_MoveDirection.y -= m_Gravity * Time.deltaTime;
        }
        else
        {
            m_MoveDirection.y = 0.0f;
        }

        m_MoveDirection = new Vector3(flatMovement.x, m_MoveDirection.y, flatMovement.z);

        //GetComponent<NavMeshAgent>().destination = m_MoveDirection;

        m_CharacterController.Move(m_MoveDirection);
    }

    public bool PlayerJumped => m_CharacterController.isGrounded && Input.GetKey(KeyCode.Space);

    public bool HorizontalMovement => Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.Mouse1) ||
                                      Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Mouse1) ||
                                      Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1) ||
                                      Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1);
    public bool FixedCamera => Input.GetKey(KeyCode.Mouse1);
    public bool MoveCameraWithMouse => Input.GetKey(KeyCode.Mouse0);
    public bool MoveCharacterWithMouse => Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1);
    public bool HasJumped() { return m_PlayerHasJumped; }
}
