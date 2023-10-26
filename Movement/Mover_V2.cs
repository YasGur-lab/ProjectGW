using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using GW.Attributes;
using GW.Combat;
using GW.Core;
using UnityEngine;
using UnityEngine.AI;
using GW.Saving;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using UnityEngine.Windows;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace GW.Movement
{
    public class Mover_V2 : MonoBehaviour
    {
        //[SerializeField] private float m_MaxSpeed = 7.0f;
        private float m_InitialMaxSpeed;
        private NavMeshAgent m_NavMeshAgent;
        private Health m_Health;
        //[SerializeField] public Animator m_Animator;
        private Vector3 m_InitialPostion;
        private float currentSpeedMultiplier = 1f;
        private Vector3 currentDestination; // Store the current destination for FixedUpdate
        private bool moveToDestination; // Flag to indicate if the agent should move towards the destination
        private Vector3 m_Velocity;






        /// <summary>
        /// //////////////////
        /// </summary>
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float InCombatSpeed = 2.0f;
        [SerializeField] private GameObject pathFinderPrefab;
        private GameObject pathFinder;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;


        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        // player
        private float _speed;
        private float _animationBlendX;
        private float _animationBlendY;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private Vector3 m_MoveDirection;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDSpeedVertical;

        private Animator _animator;
        private CharacterController _controller;
        private Fighter m_Fighter;
        private bool _hasAnimator;
        private bool m_AtDestination;
        private Vector3[] pathPoints;
        private int currentPathIndex = 0;
        private bool m_HasResetController;

        private static LTDescr m_Delay;

        bool m_Leader;
        void Awake()
        {
            //m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_Health = GetComponent<Health>();
            m_Fighter = GetComponent<Fighter>();
            m_InitialPostion = transform.position;
            //_Animator = GetComponent<Animator>();
        }

        void Start()
        {
            pathFinder = Instantiate(pathFinderPrefab, transform);
            //pathFinder.transform.position = m_InitialPostion;

            m_NavMeshAgent = pathFinder.GetComponent<NavMeshAgent>();
            SetNavMeshPoint();
            pathFinder.transform.parent = null;
            transform.position = pathFinder.transform.position;

            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            //m_Animator = GetComponent<Animator>();
            
            if (GetComponentInParent<PartySystem>().GetLeader() == gameObject)
                m_Leader = true;
            //StartCoroutine(DelayNavMeshUpdate());

            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            _animIDSpeedVertical = Animator.StringToHash("Vertical");
        }

        private void FixedUpdate()
        {
            Move();
            JumpAndGravity();
        }

        private void Move()
        {
            float targetSpeed;
            if(m_Fighter.IsInCombat()) targetSpeed = SprintSpeed;
            else targetSpeed = SprintSpeed;

            targetSpeed *= currentSpeedMultiplier;

            float inputMagnitude = 1f;
            float movementThreshold = 1.1f;

            if (!m_AtDestination)
            {
                // Check if there are valid path points
                if (pathPoints != null && currentPathIndex < pathPoints.Length)
                {
                    Vector3 currentPathPoint = pathPoints[currentPathIndex];
                    currentPathPoint.y = 0.0f;
                    Vector3 currentPosition = transform.position;
                    currentPosition.y = 0.0f;

                    // Calculate distance to the current path point
                    float distanceToPathPoint = Vector3.Distance(currentPosition, currentPathPoint);
                    
                    _speed = targetSpeed;

                    _animationBlendY = Mathf.Lerp(_animationBlendY, _speed, Time.fixedDeltaTime * SpeedChangeRate);
                    if (_animationBlendY < 0.01f && _animationBlendY > -0.01f) _animationBlendY = 0f;

                    if (targetSpeed > 0.0f)
                    {
                        // Look towards next destination
                        Vector3 lookAt = new Vector3(currentPathPoint.x, transform.position.y, currentPathPoint.z);
                        if(lookAt - transform.position != Vector3.zero)
                            transform.rotation = Quaternion.LookRotation(lookAt - transform.position);
                    }

                    // Move towards the current path point
                    Vector3 moveDirection = (currentPathPoint - currentPosition).normalized;
                    _controller.Move((moveDirection * _speed * Time.fixedDeltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.fixedDeltaTime);


                    // Check if close enough to the current path point
                    if (distanceToPathPoint < movementThreshold)
                    {
                        currentPathIndex++;

                        // Check if there are more path points

                        if (currentPathIndex >= pathPoints.Length)
                        {
                            m_AtDestination = true;
                        }
                    }
                }
            }
            else
            {
                if (!m_HasResetController)
                {
                    m_HasResetController = true;
                    _controller.Move(Vector3.zero);
                }

                if (_animationBlendY != 0f)
                {
                    _animationBlendY = Mathf.Lerp(_animationBlendY, 0.0f, Time.fixedDeltaTime * SpeedChangeRate);
                    if (_animationBlendY < 0.01f && _animationBlendY > -0.01f) _animationBlendY = 0f;
                }
            }

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlendX);
                _animator.SetFloat(_animIDSpeedVertical, _animationBlendY);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.fixedDeltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.fixedDeltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
        
        public void ModifyMovementSpeed(float speedMultiplier)
        {
            currentSpeedMultiplier *= speedMultiplier;
            //m_MaxSpeed *= currentSpeedMultiplier;
        }

        public void ResetMovementSpeed()
        {
            currentSpeedMultiplier = 1f;
            //m_MaxSpeed = m_InitialMaxSpeed;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public Vector3 GetInitialPosition() => m_InitialPostion;

        public NavMeshAgent GetNavMeshAgent() { return m_NavMeshAgent; }

        public void SetCurrentDestination(Vector3 destination)
        {
            SetNavMeshPoint();
            m_MoveDirection = destination;
            EnableNavMeshAgent();
        }
        
        void EnableNavMeshAgent()
        {
            m_NavMeshAgent.SetDestination(m_MoveDirection);

            NavMeshPath navMeshPath = new NavMeshPath();
            if (m_NavMeshAgent.CalculatePath(m_MoveDirection, navMeshPath))
            {
                pathPoints = navMeshPath.corners;
                currentPathIndex = 0;
                m_NavMeshAgent.enabled = false;
                m_AtDestination = false;
                m_HasResetController = false;
            }
            else
            {
                Debug.LogWarning("Path calculation failed.");
            }
        }

        public void SetNavMeshPoint()
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 100.0f, NavMesh.AllAreas))
            {
                pathFinder.transform.position = hit.position;
                m_NavMeshAgent.enabled = true;
            }
        }
    }
}
