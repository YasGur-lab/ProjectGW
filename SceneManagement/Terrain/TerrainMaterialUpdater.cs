using System;
using System.Collections;
using System.Collections.Generic;
using GW.Combat;
using UnityEngine;
using static Unity.VisualScripting.StickyNote;

public enum VisualShapes
{
    Circle,
    Triangle
}

public class TerrainMaterialUpdater : MonoBehaviour
{
    Terrain m_Terrain;
    Material m_TerrainMat;
    MaterialPropertyBlock m_Block;

    List<Vector4> m_EnemySkillPos = new List<Vector4>();
    List<float> m_EnemySkillRadius = new List<float>();
    List<Transform> m_EnemySkillTransforms = new List<Transform>();
    List<float> m_EnemySkillRadiusFromShader = new List<float>();
    List<Vector4> m_AllySkillPos = new List<Vector4>();
    List<Transform> m_AllySkillTransforms = new List<Transform>();
    List<float> m_AllySkillRadius = new List<float>();
    List<float> m_AllySkillRadiusFromShader = new List<float>();

    private float m_MouseIdicatorRadius;
    private Vector4 m_MousePos;
    private Color m_MouseColor;

    int m_Shape;
    Transform m_Player;
    Vector4 m_AimDirection;
    private float m_AimSpreadAngle;

    //--RESETS
    private float[] m_EmptyFloats = new float[100];
    private Vector4[] m_EmptyVectors = new Vector4[100];
    private Color m_EmptyColor = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player").transform;
        m_Terrain = GetComponent<Terrain>();
        m_TerrainMat = m_Terrain.materialTemplate;
        m_Block = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (m_TerrainMat)
        {
            UpdateSkillPositionsAndRadius(m_EnemySkillPos, m_EnemySkillRadius, m_EnemySkillTransforms, m_EnemySkillRadiusFromShader);
            UpdateSkillPositionsAndRadius(m_AllySkillPos, m_AllySkillRadius, m_AllySkillTransforms, m_AllySkillRadiusFromShader);
        }

    }

    private void UpdateSkillPositionsAndRadius(List<Vector4> posList, List<float> radiusList, List<Transform> transformList, List<float> radiusFromShaderList)
    {
        posList.Clear();
        radiusList.Clear();

        for (int i = 0; i < transformList.Count; i++)
        {
            posList.Add(transformList[i].transform.position);
            radiusList.Add(radiusFromShaderList[i]);
        }

        // Fill remaining positions and radii with default values
        while (posList.Count < 100)
        {
            posList.Add(Vector4.zero);
            radiusList.Add(0.0f);
        }

        ApplyMaterialProperties();
    }
    
    private void ApplyMaterialProperties()
    {

        if (m_EnemySkillTransforms.Count > 0)
        {
            m_Block.SetVectorArray("_EnemySkillPositions", m_EnemySkillPos);
            m_Block.SetInt("_NumEnemySkillPositions", m_EnemySkillTransforms.Count);
            m_Block.SetColor("_FOVColor", Color.red);
            m_Block.SetFloatArray("_EnemySkillRadius", m_EnemySkillRadius);
        }
        else
        {
            m_Block.SetVectorArray("_EnemySkillPositions", m_EmptyVectors);
            m_Block.SetInt("_NumEnemySkillPositions", 0);
            m_Block.SetColor("_FOVColor", m_EmptyColor);
            m_Block.SetFloatArray("_EnemySkillRadius", m_EmptyFloats);
        }

        if (m_AllySkillTransforms.Count > 0)
        {
            m_Block.SetVectorArray("_AllySkillPositions", m_AllySkillPos);
            m_Block.SetInt("_NumAllySkillPositions", m_AllySkillTransforms.Count);
            m_Block.SetColor("_AllyColor", Color.green);
            m_Block.SetFloatArray("_AllySkillRadius", m_AllySkillRadius);
        }
        else
        {
            m_Block.SetVectorArray("_AllySkillPositions", m_EmptyVectors);
            m_Block.SetInt("_NumAllySkillPositions", 0);
            m_Block.SetColor("_AllyColor", m_EmptyColor);
            m_Block.SetFloatArray("_AllySkillRadius", m_EmptyFloats);
        }

        //bool _IsCircleShape;
        //float4 _PlayerPosition;
        //float3 _AimDirection;

        if (m_MousePos != Vector4.zero)
        {
            m_Block.SetVector("_MousePosition", m_MousePos);
            m_Block.SetFloat("_MouseRadius", m_MouseIdicatorRadius);
            m_Block.SetColor("_MouseColor", m_MouseColor);
            m_Block.SetInt("_Shape", m_Shape);
            m_Block.SetColor("_MouseColor", m_MouseColor);
            m_Block.SetVector("_PlayerPosition", m_Player.position);
            m_Block.SetVector("_AimDirection", m_AimDirection);
            m_Block.SetFloat("_AimSpreadAngle", m_AimSpreadAngle);
        }
        else
        {
            m_Block.SetVector("_MousePosition", m_MousePos);
            m_Block.SetFloat("_MouseRadius", m_MouseIdicatorRadius);
            m_Block.SetColor("_MouseColor", m_EmptyColor);
            m_Block.SetInt("_Shape", m_Shape);
        }


        m_Terrain.SetSplatMaterialPropertyBlock(m_Block);
    }

    // Method to add a transform to the array
    public void AddEnemySkillTransformToArray(Transform newTransform, float radius)
    {
        List<Transform> tempList = new List<Transform>(m_EnemySkillTransforms);
        tempList.Add(newTransform);
        m_EnemySkillTransforms = tempList;
        List<float> tempRadius = new List<float>(m_EnemySkillRadiusFromShader);
        tempRadius.Add(radius);
        m_EnemySkillRadiusFromShader = tempRadius;
    }

    // Method to remove a specific transform from the array
    public void RemoveEnemySkillTransformFromArray(Transform transformToRemove, float radius)
    {
        List<Transform> tempList = new List<Transform>(m_EnemySkillTransforms);
        tempList.Remove(transformToRemove);
        m_EnemySkillTransforms = tempList;
        List<float> tempRadius = new List<float>(m_EnemySkillRadiusFromShader);
        tempRadius.Remove(radius);
        m_EnemySkillRadiusFromShader = tempRadius;
    }

    // Method to add a transform to the array
    public void AddAllySkillTransformToArray(Transform newTransform, float radius)
    {
        List<Transform> tempList = new List<Transform>(m_AllySkillTransforms);
        tempList.Add(newTransform);
        m_AllySkillTransforms = tempList;
        List<float> tempRadius = new List<float>(m_AllySkillRadiusFromShader);
        tempRadius.Add(radius);
        m_AllySkillRadiusFromShader = tempRadius;
    }

    // Method to remove a specific transform from the array
    public void RemoveAllySkillTransformFromArray(Transform transformToRemove, float radius)
    {
        List<Transform> tempList = new List<Transform>(m_AllySkillTransforms);
        tempList.Remove(transformToRemove);
        m_AllySkillTransforms = tempList;
        List<float> tempRadius = new List<float>(m_AllySkillRadiusFromShader);
        tempRadius.Remove(radius);
        m_AllySkillRadiusFromShader = tempRadius;
    }

    public void SetMouse(Vector3 pos, float radius, Color color, VisualShapes shape, Vector3 aimDirection, float aimSpreadAngle)
    {
        m_Shape = GetShapeIndex(shape);
        m_AimDirection = aimDirection;
        m_AimSpreadAngle = aimSpreadAngle;
        m_MousePos = pos;
        m_MouseIdicatorRadius = radius;
        m_MouseColor = color;
    }
    public void ResetMouse()
    {
        m_Shape = -1;
        m_MousePos = Vector4.zero;
        m_MouseIdicatorRadius = 0;
        m_MouseColor = m_EmptyColor;
    }

    public int GetShapeIndex(VisualShapes shape)
    {
        VisualShapes[] allShapes = (VisualShapes[])Enum.GetValues(typeof(VisualShapes));
        int index = System.Array.IndexOf(allShapes, shape);
        return index;
    }
}
