using GW.Control;
using GW.Movement;
using UnityEngine;

public class MountController : MonoBehaviour
{
    public PlayerController m_PlayerController;
    public PlayerController m_MountedController;

    [SerializeField] private float movementSpeed = 7.0f;
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float jumpForce = 5.0f;
    private CharacterController m_CharacterController;
    private Vector3 m_MoveDirection;
    private bool grounded;
    float m_Horizontal;

    //--Gravity--
    [Range(0, 1)] public float m_Gravity = 1.0f;
    [Range(0, 20)] public float m_Speed = 7.0f;

    //--MOVEMENTS--
    [SerializeField] private float m_RotationSpeed = 3.0f;
    public Transform DismountPosition;

    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!m_PlayerController.IsMounted())
            return;

        grounded = m_CharacterController.isGrounded;

        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        if (m_PlayerController.GetComponent<Mover>().HorizontalMovement)
            m_Horizontal = Input.GetAxisRaw("Horizontal");
        else
        {
            m_Horizontal = 0.0f;
            if (!m_PlayerController.GetComponent<Mover>().FixedCamera)
                transform.Rotate(0, Input.GetAxis("Horizontal") * m_RotationSpeed, 0);
        }

        Vector3 inputDirection;
        if (!GetComponent<Mover>().MoveCharacterWithMouse)
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
        Vector3 flatMovement = transformDirection * m_Speed * Time.deltaTime;

        //if (m_PlayerHasJumped)
        //{
        //    m_MoveDirection.y = m_JumpForce;
        //}

        if (m_CharacterController.isGrounded)
        {
            m_MoveDirection.y -= m_Gravity * Time.deltaTime;
        }
        else
        {
            m_MoveDirection.y = 0.0f;
        }

        m_MoveDirection = new Vector3(flatMovement.x, m_MoveDirection.y, flatMovement.z);
        
        GetComponent<CharacterController>().Move(m_MoveDirection);
        m_PlayerController.gameObject.transform.rotation = transform.rotation;
    }

    private void HandleJump()
    {
        // Implement jump logic here if the mount can jump
        // For example, you can check for jump input and add force to the mount in the y-axis direction.
    }

    private bool IsMounted()
    {
        // Check if the mount has a PlayerController reference (meaning it is mounted)
        return !m_PlayerController.enabled;
    }

    public void Mount(PlayerController newController)
    {
        // Dismount the current controller before mounting the new one
        Dismount();

        // Set the new mounted controller
        m_MountedController = newController;

        // Disable the PlayerController script on the player
        //m_PlayerController.enabled = false;

        // Set the mounted controller's mount reference to this mount
        m_PlayerController.SetMount(this);
    }

    public void Dismount()
    {
        if (m_MountedController != null)
        {
            m_MountedController = null;
        }
    }
}
