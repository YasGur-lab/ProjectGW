using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using GW.Attributes;
using GW.Combat;
using GW.Core;
using GW.Movement;
using GW.Statistics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GW.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] private float m_ChaseDistance = 5.0f;
        [SerializeField] private float m_SuspicionTime = 2.0f;
        [SerializeField] private float m_WaypointTolerance = 0.5f;
        [SerializeField] private float m_WaypointDwellTime = 3.0f;
        [Range(0,1)][SerializeField] private float m_PatrolSpeed = 0.2f;

        [SerializeField] private Transform m_AllyPosition;

        [SerializeField] PatrolPath m_PatrolPath;

        private Fighter m_Fighter;
        private Health m_Health;
        private GameObject m_Player;
        private Mover m_Mover;

        private LazyValue<Vector3> m_GuardPosition;
        
        private float m_TimeSinceLastSawPlayer = Mathf.Infinity;
        private float m_TimeSinceArrivedAtWaypoint = Mathf.Infinity;

        private int m_CurrentWaypointIndex = 0;

        [SerializeField] private bool m_FollowLeader = false;

        void Awake()
        {
            m_Fighter = GetComponent<Fighter>();
            m_Health = GetComponent<Health>();
            m_Player = GameObject.FindWithTag("Player");
            m_Mover = GetComponent<Mover>();
            m_GuardPosition = new LazyValue<Vector3>(GuardPosition);
        }

        private Vector3 GuardPosition()
        {
            if (!m_FollowLeader) return transform.position;
            else return m_AllyPosition.position;
        }

        // Start is called before the first frame update
        void Start()
        {
            m_GuardPosition.ForceInit();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_FollowLeader)
                m_GuardPosition.value = m_AllyPosition.position;

            if (m_Health.IsDead()) return;
            if (InAttackRangeOfPlayer() && m_Fighter.CanAttack(m_Player))
            {
                m_TimeSinceLastSawPlayer = 0.0f;
                GetComponent<Animator>().SetBool("IsRunning", true);
                GetComponent<Animator>().SetBool("IsWalking", false);
                AttackBehavior();
            }
            else if (m_TimeSinceLastSawPlayer < m_SuspicionTime)
            {
                SuspicionBehavior();
            }
            else
            {
                PatrolBehavior();
            }

            UpdateTimers();
        }

        private void AttackBehavior()
        {
            //m_Fighter.Attack(m_Player);
        }

        private void SuspicionBehavior()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
            GetComponent<Animator>().SetBool("IsRunning", false);
            GetComponent<Animator>().SetBool("IsWalking", false);
        }

        private void PatrolBehavior()
        {
            Vector3 nextPos = m_GuardPosition.value;

            if (m_PatrolPath)
            {
                if (AtWaypoint())
                {
                    m_TimeSinceArrivedAtWaypoint = 0.0f;
                    GetComponent<Animator>().SetBool("IsWalking", false);
                    CycleWaypoint();
                }
                nextPos = GetCurrentWaypoint();
            }

            if (m_TimeSinceArrivedAtWaypoint > m_WaypointDwellTime)
            {
                transform.LookAt(GetVectorLookAt(nextPos));
                m_Mover.StartMoveAction(nextPos, true, m_PatrolSpeed);
            }

            if (!m_PatrolPath)
            {
                transform.LookAt(GetVectorLookAt(m_GuardPosition.value));
                if (IsNearFromGuardPosition())
                    GetComponent<Animator>().SetBool("IsWalking", false);
            }
        }

        private Vector3 GetCurrentWaypoint()
        {
            return m_PatrolPath.GetWaypoint(m_CurrentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            m_CurrentWaypointIndex = m_PatrolPath.GetNextWaypoint(m_CurrentWaypointIndex);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(Vector3XZ(transform.position), Vector3XZ(GetCurrentWaypoint()));
            return distanceToWaypoint < m_WaypointTolerance;
        }

        private bool InAttackRangeOfPlayer()
        {
            float distanceToPlayer = Vector3.Distance(m_Player.transform.position, transform.position);
            return distanceToPlayer < m_ChaseDistance;
        }

        private bool IsNearFromGuardPosition()
        {
            Vector3 GuardPosNoY = new Vector3(m_GuardPosition.value.x, 0.0f, m_GuardPosition.value.z);
            Vector3 TransPosNoY = new Vector3(transform.position.x, 0.0f, transform.position.z);
            float distanceToGuardPosition = Vector3.Distance(GuardPosNoY, TransPosNoY);
            return distanceToGuardPosition < 0.5f;
        }

        private Vector3 Vector3XZ(Vector3 vectorToChange)
        {
            Vector3 VectorXZ = new Vector3(vectorToChange.x, 0.0f, vectorToChange.z);
            return VectorXZ;
        }

        private Vector3 GetVectorLookAt(Vector3 vectorToChange)
        {
            Vector3 lookPos = vectorToChange;
            lookPos.y = transform.position.y;
            return lookPos;
        }

        private void UpdateTimers()
        {
            m_TimeSinceLastSawPlayer += Time.deltaTime;
            m_TimeSinceArrivedAtWaypoint += Time.deltaTime;
        }

        //Called by unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_ChaseDistance);
        }
    }
}
