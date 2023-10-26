using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using UnityEngine;

public class AOE_Spawner : MonoBehaviour
{
    public GameObject[] m_makeObjs;
    public Transform m_movePos;

    public GameObject prefabToSpawn;
    public int maxSpawnedObjects = 10;
    public float spawnInterval = 2.0f; // Time interval between spawns
    public float spawnDuration = 10.0f; // Duration of spawning
    public float radius = 5;

    private float timeSinceLastSpawn = 0.0f;
    private float spawnTimer = 0.0f;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentSpawnedIndex = 0;

    public float m_startDelay;
    public int m_makeCount;
    public float m_makeDelay;
    public Vector3 m_randomPos;
    public Vector3 m_randomRot;
    float m_Time;
    float m_Time2;
    float m_delayTime;
    float m_count;

    void Start()
    {
        m_Time = m_Time2 = Time.time;
    }

    void Update()
    {
        if (Time.time > m_Time + m_startDelay)
        {
            if (Time.time > m_Time2 + m_makeDelay && m_count < m_makeCount)
            {
                Vector3 m_pos = transform.position + GetRandomVector(m_randomPos);
                Quaternion m_rot = transform.rotation * Quaternion.Euler(GetRandomVector(m_randomRot));
                m_pos.y = transform.position.y + 10.0f;
                for (int i = 0; i < m_makeObjs.Length; i++)
                {
                    GameObject m_obj = Instantiate(m_makeObjs[i], m_pos, m_rot);
                    m_obj.transform.parent = this.transform;

                    if (m_movePos)
                    {
                        if (m_obj.GetComponent<MoveToObject>())
                        {
                            MoveToObject m_script = m_obj.GetComponent<MoveToObject>();
                            m_script.m_movePos = m_movePos;
                        }
                    }
                }

                m_Time2 = Time.time;
                m_count++;
            }
        }
    }

    public Vector3 GetRandomVector(Vector3 value)
    {
        Vector3 result;
        result.x = GetRandomValue(value.x);
        result.y = GetRandomValue(value.y);
        result.z = GetRandomValue(value.z);
        return result;
    }

    public float GetRandomValue(float value)
    {
        return Random.Range(-value, value);
    }
}
