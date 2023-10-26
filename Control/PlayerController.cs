using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GW.Attributes;
using GW.Combat;
using GW.Movement;
using UnityEngine;

namespace GW.Control
{
    public class PlayerController : MonoBehaviour
    {
        //--MOVEMENTS--
        [Range(0, 1000)] public float m_Speed = 7.0f;
        [Range(0, 10)][SerializeField] private float m_RotationSpeed = 3.0f;
        [Range(0, 10)] [SerializeField] private float m_JumpForce = 5.0f;
        [Range(-10, 10)] public float m_Gravity = 1.0f;

        //[SerializeField] private GameObject m_TargetPC;



        private Health m_Health;
        private Vector3 m_MoveDirection;
        private float m_Horizontal;

        [SerializeField] private bool m_PlayerHasJumped = false;
        private float m_JumpTimer;
        [SerializeField] private CharacterController m_CharacterController;

        float m_VerticalVelocity;

        //--Gravity--


        //--CURSOR--
        CursorSettings m_CursorSettings;

        //--MOUNT--
        private MountController m_MountController;

        //--PARENT--
        private Transform m_OriginalParent;

        //--CAMERA--
        private GameObject m_Camera;

        //--FIGHTER--
        private Fighter m_Fighter;
        private Dictionary<GameObject, int> initialLayers = new Dictionary<GameObject, int>();

        public enum ControllerState
        {
            Moving,
            Attacking
        }

        // Start is called before the first frame update
        void Awake()
        {
            m_Health = GetComponent<Health>();
            m_CursorSettings = GetComponent<CursorSettings>();
            m_OriginalParent = transform.parent;
            m_Camera = GetComponentInChildren<CinemachineFreeLook>().gameObject;
            m_Fighter = GetComponent<Fighter>();
        }

        void Start()
        {
            initialLayers.Clear();

            foreach (var tag in m_Fighter.m_AllyTags.Concat(m_Fighter.m_EnemyTags))
            {
                GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
                foreach (var go in gos)
                {
                    initialLayers[go] = go.layer;
                }
            }

           // float cooldownUpdateInterval = 0.5f; // Update every 0.1 seconds (adjust as needed)
        //    InvokeRepeating("UpdateOnDelay", cooldownUpdateInterval, cooldownUpdateInterval);
        }


        // Update is called once per frame
        void Update()
        {
            //Debug.Log(GetComponent<CharacterController>().isGrounded);

            //if (m_CharacterController.isGrounded)
            //    m_Grounded = true;

            //if (m_PlayerHasJumped)
            //{
            //    m_JumpTimer += Time.deltaTime;
            //    if (m_JumpTimer > m_JumpCooldown)
            //    {
            //        m_PlayerHasJumped = false;
            //        m_JumpTimer = 0.0f;
            //    }
            //}

            if (m_CursorSettings.InteractWithUI() && !GetComponent<Mover>().CurrentlyMoving)
            {
                return;
            }

            if (m_Health.IsDead())
            {
                m_CursorSettings.SetCursor(CursorSettings.CursorType.Default);
                return;
            }

            //InteractWithMount();
            //InteractWithComponent();
            //if (!IsHoveringOverValidTarget())
            //{
            //    m_CursorSettings.ResetCursor();
            //    //m_TargetSystem.m_Outline.ToggleOutline(false);
            //    //m_TargetSystem.m_Outline.outlineColor = UnityEngine.Color.clear;
            //    ResetTargetLayer();
            //}
            //InteractWithMovement();
            //InteractWithMovement();
        }

        private void FixedUpdate()
        {
          InteractWithMovement();
        }

        public void UpdateOnDelay()
        {
            if (m_CursorSettings.InteractWithUI() && !GetComponent<Mover>().CurrentlyMoving)
            {
                return;
            }

            if (m_Health.IsDead())
            {
                m_CursorSettings.SetCursor(CursorSettings.CursorType.Default);
                return;
            }

            if (!IsHoveringOverValidTarget())
            {
                m_CursorSettings.ResetCursor();
                ResetTargetLayer();
            }
        }

        //private void InteractWithComponent()
        //{
        //    RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

        //    foreach (RaycastHit hit in hits)
        //    {
        //        IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();

        //        Debug.Log(raycastables.Length);

        //        if (raycastables.Length == 0)
        //        {
        //            m_CursorSettings.ResetCursor();
        //        }

        //        foreach (var raycastable in raycastables)
        //        {
        //            raycastable.HandleRaycast(GetComponent<Fighter>());
        //        }
        //    }
        //}

        private bool IsHoveringOverValidTarget()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();

                foreach (var raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ResetTargetLayer()
        {
            foreach (var kvp in initialLayers)
            {
                GameObject npc = kvp.Key;
                int initialLayer = kvp.Value;

                if (npc.layer == 8)
                {
                    SetLayerRecursively(npc, initialLayer);
                }
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        private void InteractWithMovement()
        {
            if (IsMounted()) return;
            //InteractWithJump();
            MoveWithWASD();
        }

        private void InteractWithJump()
        {
            bool groundedPlayer = m_CharacterController.isGrounded;
            
            if (groundedPlayer)
            {
                // cooldown interval to allow reliable jumping even whem coming down ramps
                m_JumpTimer = 0.2f;
            }
            if (m_JumpTimer > 0)
            {
                m_JumpTimer -= Time.deltaTime;
            }

            // slam into the ground
            if (groundedPlayer && m_VerticalVelocity < 0)
            {
                // hit ground
                m_VerticalVelocity = 0f;
            }

            // apply gravity always, to let us track down ramps properly
            m_VerticalVelocity -= m_Gravity * Time.deltaTime;

            // gather lateral input control
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // scale by speed
            move *= m_Speed;

            // only align to motion if we are providing enough input
            if (move.magnitude > 0.05f)
            {
                gameObject.transform.forward = move;
            }

            // allow jump as long as the player is on the ground
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // must have been grounded recently to allow jump
                if (m_JumpTimer > 0)
                {
                    // no more until we recontact ground
                    m_JumpTimer = 0;

                    // Physics dynamics formula for calculating jump up velocity based on height and gravity
                    m_VerticalVelocity += Mathf.Sqrt(m_JumpForce * 2 * m_Gravity);
                }
            }

            // inject Y velocity before we use it
            move.y = m_VerticalVelocity;

            // call .Move() once only
            m_CharacterController.Move(move * Time.deltaTime);

        }

        private void InteractWithMount()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                MountController mount = GetMountInRange();
                if (mount != null)
                {
                    Dismount(mount);
                    mount.Mount(this);

                    transform.position = mount.transform.position;
                    transform.parent = mount.transform;
                }
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                MountController mount = GetMountInRange();
                if (mount != null)
                {
                    Dismount(mount);
                }
            }
        }

        private MountController GetMountInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Mount"));
            float closestDistance = Mathf.Infinity;
            MountController closestMount = null;

            foreach (Collider collider in colliders)
            {
                MountController mount = collider.GetComponent<MountController>();
                if (mount != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestMount = mount;
                    }
                }
            }

            return closestMount;
        }

        public void SetMount(MountController newMount)
        {
            m_MountController = newMount;

            if (m_MountController != null) m_Camera.SetActive(false);       
            else m_Camera.SetActive(true);
        }

        public bool IsMounted()
        {
            return m_MountController != null;
        }

        public void Dismount(MountController mount)
        {
            if (m_MountController != null)
            {
                m_MountController.Dismount();
                SetMount(null);
                
                ResetParentTransform();
                Vector3 dismountPosition = mount.DismountPosition.position;
                dismountPosition += Vector3.up * 0.5f; // Add an upward offset to avoid collisions
                transform.position = dismountPosition;
                transform.rotation = mount.DismountPosition.rotation;
            }
        }

        private void ResetParentTransform()
        {
            transform.parent = m_OriginalParent;
        }

        private IEnumerator OffGround()
        {
            yield return new WaitForSeconds(0.2f);
            //m_Grounded = false;
        }

        private void MoveWithWASD()
        {
            bool groundedPlayer = m_CharacterController.isGrounded;
            if (groundedPlayer)
            {
                m_VerticalVelocity = -m_Gravity * Time.fixedDeltaTime; // Reset vertical velocity on ground
                m_JumpTimer = 0.2f;
            }

            if (m_JumpTimer > 0)
            {
                m_JumpTimer -= Time.fixedDeltaTime;
            }

            if (GetComponent<Mover>().HorizontalMovement)
            {
                m_Horizontal = Input.GetAxisRaw("Horizontal");
            }
            else
            {
                m_Horizontal = 0.0f;
                if (!GetComponent<Mover>().FixedCamera)
                {
                    transform.Rotate(0, Input.GetAxis("Horizontal") * m_RotationSpeed, 0);
                }
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
            Vector3 flatMovement = transformDirection * m_Speed * Time.fixedDeltaTime;

            m_MoveDirection = new Vector3(flatMovement.x, m_VerticalVelocity, flatMovement.z);

            if (Input.GetKeyDown(KeyCode.Space) && m_JumpTimer > 0)
            {
                m_VerticalVelocity = Mathf.Sqrt(2 * m_JumpForce * Mathf.Abs(m_Gravity)); // Calculate jump velocity
                m_JumpTimer = 0;
            }

            // Apply gravity
            m_VerticalVelocity -= m_Gravity * Time.fixedDeltaTime;

            m_MoveDirection.y = m_VerticalVelocity;
            GetComponent<CharacterController>().Move(m_MoveDirection * Time.fixedDeltaTime);
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        public bool IsJumping()
        {
            return m_PlayerHasJumped;
        }
    }
}
