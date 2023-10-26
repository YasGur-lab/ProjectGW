using UnityEngine;

public class ThirdPersonDodging : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;

    //--DoubleTap--
    private int m_DoubleTapBackward;
    private int m_DoubleTapForward;
    private int m_DoubleTapLeft;
    private int m_DoubleTapRight;
    private float m_DoubleTapCooldown = 0.3f;
    private float m_DoubleTapTimer;

    //--Dodge--
    [SerializeField] private bool m_IsReadyToDodge;
    private bool m_HasDodgeBackward;
    private bool m_HasDodgeForward;
    private bool m_HasDodgeLeft;
    private bool m_HasDodgeRight;
    private float m_DodgeCooldown = 1.5f;
    private float m_DodgeTimer = 1.5f;
    private float m_ForceTimer;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            m_DoubleTapBackward++;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            m_DoubleTapLeft++;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            m_DoubleTapForward++;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            m_DoubleTapRight++;
        }

        if (m_DoubleTapBackward == 2 ||
            m_DoubleTapForward == 2 ||
            m_DoubleTapLeft == 2 ||
            m_DoubleTapRight == 2)
        {
            if (m_IsReadyToDodge)
            {
                if (m_DoubleTapBackward == 2)
                {
                    m_HasDodgeBackward = true;
                    m_Animator.Play("DodgeBackward");
                }
                else if (m_DoubleTapForward == 2)
                {
                    m_HasDodgeForward = true;
                    m_Animator.Play("DodgeForward");
                }
                else if (m_DoubleTapLeft == 2)
                {
                    m_HasDodgeLeft = true;
                    m_Animator.Play("DodgeLeft");
                }
                else if (m_DoubleTapRight == 2)
                {
                    m_HasDodgeRight = true;
                    m_Animator.Play("DodgeRight");
                }

                m_IsReadyToDodge = false;
            }

            m_DoubleTapBackward = 0;
            m_DoubleTapForward = 0;
            m_DoubleTapLeft = 0;
            m_DoubleTapRight = 0;
        }

        if (m_DoubleTapBackward > 0 ||
            m_DoubleTapForward > 0 ||
            m_DoubleTapLeft > 0 ||
            m_DoubleTapRight > 0)
        {
            m_DoubleTapTimer += Time.deltaTime;

            if (m_DoubleTapTimer > m_DoubleTapCooldown && m_DoubleTapBackward < 2 ||
                m_DoubleTapTimer > m_DoubleTapCooldown && m_DoubleTapForward < 2 ||
                m_DoubleTapTimer > m_DoubleTapCooldown && m_DoubleTapLeft < 2 ||
                m_DoubleTapTimer > m_DoubleTapCooldown && m_DoubleTapRight < 2)
            {
                m_DoubleTapBackward = 0;
                m_DoubleTapForward = 0;
                m_DoubleTapLeft = 0;
                m_DoubleTapRight = 0;

                m_HasDodgeBackward = false;
                m_HasDodgeForward = false;
                m_HasDodgeLeft = false;
                m_HasDodgeRight = false;

                m_DoubleTapTimer = 0.0f;
            }
        }


        if (m_HasDodgeBackward ||
            m_HasDodgeForward  ||
            m_HasDodgeLeft     ||
            m_HasDodgeRight)
        {
            m_ForceTimer += Time.deltaTime;

            if (m_ForceTimer > 0.7f)
            {
                m_HasDodgeBackward = false;
                m_HasDodgeForward = false;
                m_HasDodgeLeft = false;
                m_HasDodgeRight = false;
                m_ForceTimer = 0.0f;
            }
        }

        if (!m_IsReadyToDodge)
        {
            m_DodgeTimer += Time.deltaTime;
            if (m_DodgeTimer > m_DodgeCooldown)
            {
                m_IsReadyToDodge = true;
                m_DodgeTimer = 0.0f;
            }
        }

        GameObject go = GameObject.Find("CharacterModel");
        if (go != null)
            go.transform.localPosition = new Vector3Int(0, -1, 0);
    }

    public bool HasDodgeForward => m_HasDodgeForward;
    public bool HasDodgeBackward => m_HasDodgeBackward;
    public bool HasDodgeLeft => m_HasDodgeLeft;
    public bool HasDodgeRight => m_HasDodgeRight;
}
