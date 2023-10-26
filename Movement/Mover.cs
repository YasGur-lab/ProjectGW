using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using GW.Attributes;
using GW.Core;
using UnityEngine;
using UnityEngine.AI;
using GW.Saving;
using UnityEngine.UIElements;

namespace GW.Movement
{
    public class Mover : MonoBehaviour, IAction
    {
        [SerializeField] private float m_MaxSpeed = 7.0f;
        private float m_InitialMaxSpeed;
        private NavMeshAgent m_NavMeshAgent;
        private Health m_Health;
        [SerializeField] public Animator m_Animator;
        private Vector3 m_InitialPostion;
        private float currentSpeedMultiplier = 1f;
        private Vector3 currentDestination; // Store the current destination for FixedUpdate
        private bool moveToDestination; // Flag to indicate if the agent should move towards the destination
        private Vector3 m_Velocity;
        bool m_Leader;
        void Awake()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Health = GetComponent<Health>();
            m_InitialPostion = transform.position;
            m_Animator = GetComponent<Animator>();
        }

        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_InitialMaxSpeed = m_MaxSpeed;
            if (GetComponentInParent<PartySystem>().GetLeader() == gameObject)
                m_Leader = true;
            StartCoroutine(DelayNavMeshUpdate());
        }

        private IEnumerator DelayNavMeshUpdate()
        {
            yield return new WaitForSeconds(0.5f);
            if (m_NavMeshAgent)
                m_NavMeshAgent.updatePosition = false;
        }

        private void FixedUpdate()
        {
            if (m_NavMeshAgent && !m_Health.IsDead())
            {
                MoveTo(currentDestination, moveToDestination, 1.0f);
                Vector3 currentSteeringTarget = m_NavMeshAgent.steeringTarget;
                Vector3 currentVelocity = m_NavMeshAgent.velocity;
                if (!IsSameDirection(currentSteeringTarget - transform.position, currentVelocity))
                {
                    Quaternion lookRotation = Quaternion.LookRotation(currentVelocity.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
                }
                //if(!m_Leader)
                if (m_NavMeshAgent.velocity == Vector3.zero && m_Animator.GetBool("IsRunning"))
                {
                    //Debug.Log(gameObject.name + " " + m_Velocity);
                    CancelMovementAnimations();
                }
            }
        }

        public void StartMoveAction(Vector3 destination, bool toDestination, float speedFraction)
        {
            //if(!m_Leader) 
            //    Debug.Log("StartMoveAction");
            m_Animator.SetBool("IsRunning", true);
            m_Animator.SetBool("IsWalking", false);
            currentDestination = destination;
            moveToDestination = toDestination;
            m_NavMeshAgent.speed = m_MaxSpeed * Mathf.Clamp01(speedFraction);
            m_NavMeshAgent.isStopped = false;
        }

        public void MoveTo(Vector3 destination, bool toDestination, float speedFraction)
        {
            if (m_NavMeshAgent.enabled == false) return;
            if (!toDestination)
            {
                if (m_NavMeshAgent.enabled)
                    m_NavMeshAgent.Move(destination);
            }
            else
            {
                if (m_Animator.GetBool("IsRunning") == false)
                    m_Animator.SetBool("IsRunning", true);
                
                if (m_NavMeshAgent.destination != destination)
                    m_NavMeshAgent.destination = destination;

                if(Math.Abs(m_NavMeshAgent.speed - m_MaxSpeed * Mathf.Clamp01(speedFraction)) > 0.1f)
                    m_NavMeshAgent.speed = m_MaxSpeed * Mathf.Clamp01(speedFraction);

                if (m_NavMeshAgent.isStopped)
                    m_NavMeshAgent.isStopped = false;

                transform.position = Vector3.SmoothDamp(transform.position, m_NavMeshAgent.nextPosition,
                    ref m_Velocity, 0.1f);

            }
        }

        private bool IsSameDirection(Vector3 dir1, Vector3 dir2)
        {
            // Check if the angle between the two directions is within a tolerance
            return Vector3.Angle(dir1, dir2) < 5f;
        }

        public void Cancel()
        {
            if (gameObject.tag != "Player")
            {
                if (m_NavMeshAgent.enabled == false) return;

                //Debug.Log("Stopped");
                m_NavMeshAgent.isStopped = true;
                //if (m_Animator == null) m_Animator = GetComponent<Animator>();
                m_Animator.SetBool("IsRunning", false);
                m_Animator.SetBool("IsWalking", false);
            }
        }

        public void CancelNavMesh()
        {
            m_NavMeshAgent.isStopped = true;
        }

        public void CancelMovementAnimations()
        {
            //Debug.Log(gameObject.name + " " + "CancelMovementAnimations");
            m_Animator.SetBool("IsRunning", false);
            m_Animator.SetBool("IsWalking", false);
        }

        public void ModifyMovementSpeed(float speedMultiplier)
        {
            currentSpeedMultiplier *= speedMultiplier;
            m_MaxSpeed *= currentSpeedMultiplier;
        }

        public void ResetMovementSpeed()
        {
            currentSpeedMultiplier = 1f;
            m_MaxSpeed = m_InitialMaxSpeed;
        }

        //public object CaptureState()
        //{
        //    return new SerializableVector3(transform.position);
        //}

        //public void RestoreState(object state)
        //{
        //    SerializableVector3 position = (SerializableVector3)state;
        //    GetComponent<NavMeshAgent>().enabled = false;
        //    transform.position = position.ToVector();
        //    GetComponent<NavMeshAgent>().enabled = true;
        //}

        //public bool SetLeader(bool b) => m_Leader = b;

        public Vector3 GetInitialPosition() => m_InitialPostion;

        public NavMeshAgent GetNavMeshAgent() { return m_NavMeshAgent; }

        public bool MoveCharacterWASD => Input.GetKey(KeyCode.W) ||
                                         Input.GetKey(KeyCode.A) ||
                                         Input.GetKey(KeyCode.S) ||
                                         Input.GetKey(KeyCode.D);

        public bool MoveCharacterWASDKeyDown => Input.GetKeyDown(KeyCode.W) ||
                                         Input.GetKeyDown(KeyCode.A) ||
                                         Input.GetKeyDown(KeyCode.S) ||
                                         Input.GetKeyDown(KeyCode.D);

        public bool HorizontalMovement => Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.Mouse1) ||
                                          Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Mouse1) ||
                                          Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.Mouse0) &&
                                          Input.GetKey(KeyCode.Mouse1) ||
                                          Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Mouse0) &&
                                          Input.GetKey(KeyCode.Mouse1);

        public bool MoveCharacterWithMouse => Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1);
        public bool FixedCamera => Input.GetKey(KeyCode.Mouse1);
        public bool MoveCameraWithMouse => Input.GetKey(KeyCode.Mouse0);

        public bool CurrentlyMoving => MoveCharacterWASD || MoveCharacterWASDKeyDown || HorizontalMovement ||
                                       MoveCharacterWithMouse;
    }
}
