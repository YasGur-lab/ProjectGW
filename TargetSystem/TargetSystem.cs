using System;
using System.Collections.Generic;
using GW.Combat;
using UnityEngine;
using UnityEngine.UI;

public class TargetChangedEventArgs : EventArgs
{
    public GameObject NewTarget { get; }
    public bool Ally { get; }

    public TargetChangedEventArgs(GameObject newTarget, bool ally)
    {
        NewTarget = newTarget;
        Ally = ally;
    }
}

public delegate void TargetChangedEventHandler(object sender, TargetChangedEventArgs e);


public class TargetSystem : MonoBehaviour
{
    private GameObject m_Target;
    public event TargetChangedEventHandler TargetChangedEvent;
    TerrainMaterialUpdater m_TerrainMaterialUpdater;
    private bool m_TargetIsAnEnemy;
    private void Start()
    {
        m_TerrainMaterialUpdater = FindAnyObjectByType<TerrainMaterialUpdater>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ResetTargetVisuals();
        }
    }

    public GameObject GetTarget()
    {
        if (m_Target == null)
            return null;
        return m_Target;
    }

    public void SetCombatTarget(GameObject target)
    {
        if (m_Target) ResetTargetVisuals();
        m_Target = target;
        m_TargetIsAnEnemy = true;
        m_TerrainMaterialUpdater.AddEnemySkillTransformToArray(m_Target.transform, 0.5f);
        TargetChangedEvent?.Invoke(this, new TargetChangedEventArgs(target, m_TargetIsAnEnemy));

    }

    public void SetAllyTarget(GameObject target)
    {
        if(m_Target) ResetTargetVisuals();
        m_Target = target;
        m_TargetIsAnEnemy = false;
        m_TerrainMaterialUpdater.AddAllySkillTransformToArray(m_Target.transform, 0.5f);
        TargetChangedEvent?.Invoke(this, new TargetChangedEventArgs(target, m_TargetIsAnEnemy));

    }

    public void ResetTargetVisuals()
    {
        if (m_Target)
        {
            if (m_TargetIsAnEnemy)
            {
                m_TerrainMaterialUpdater.RemoveEnemySkillTransformFromArray(m_Target.transform, 0.5f);
            }
            else
            {
                m_TerrainMaterialUpdater.RemoveAllySkillTransformFromArray(m_Target.transform, 0.5f);
            }
        }

        m_Target = null;
        GetComponent<Fighter>().SetTarget(null);

        TargetChangedEvent?.Invoke(this, new TargetChangedEventArgs(null, m_TargetIsAnEnemy));
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    if (m_Target)
    //        Gizmos.DrawSphere(m_Target.transform.position + Vector3.up, 0.2f);
    //}
    
    public bool IsGameObjectInTagList(GameObject go, List<string> tagList)
    {
        return tagList.Contains(go.tag);
    }
}
