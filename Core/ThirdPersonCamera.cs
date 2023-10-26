using Cinemachine;
using GW.Attributes;
using GW.Movement;
using GW.Saving;
using System;
using System.Xml.Serialization;
using StarterAssets;
using UnityEngine;

namespace GW.Core
{
    public class ThirdPersonCamera : MonoBehaviour, ISaveable
    {
        [SerializeField] private Transform m_Camera;
        [SerializeField] private GameObject m_Player;
        [SerializeField] private float m_CameraRotationSpeedOnNoMouseClick = 76.0f;
        [SerializeField] private float m_ScrollingSpeed = 7.0f;
        [SerializeField] private float m_XRotationSpeed = 500.0f;
        private CursorSettings m_CursorSettings;

        void Awake()
        {
            m_Camera.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = true;
            m_CursorSettings = GetComponent<CursorSettings>();
        }

        public void UpdateCamera()
        {
            CameraOrientation();
            CameraZoom();
        }

        private void CameraZoom()
        {
            m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[0].m_Height =
                Mathf.Clamp(m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[0].m_Height -= Input.GetAxis("Mouse ScrollWheel") * m_ScrollingSpeed, 4.0f, 10.0f);

            m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[1].m_Radius =
                Mathf.Clamp(m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[1].m_Radius -= Input.GetAxis("Mouse ScrollWheel") * m_ScrollingSpeed, 4.0f, 10.0f);
        }

        private void CameraOrientation()
        {
            if (m_Player.GetComponentInChildren<ThirdPersonController>().FixedCamera)
            {
                CameraSettings();
            }
            else if (m_Player.GetComponentInChildren<ThirdPersonController>().MoveCameraWithMouse)
            {
                if (m_CursorSettings && m_CursorSettings.InteractWithUI()) return;
                CameraSettings();
            }
            else
            {
                m_Camera.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = m_CameraRotationSpeedOnNoMouseClick;
                m_Camera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisValue = 0.0f;
                m_Camera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = "Horizontal";
                m_Camera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = "";
            }
        }

        private void CameraSettings()
        {
            m_Camera.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = false;
            m_Camera.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = m_XRotationSpeed;
            m_Camera.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisName = "Mouse X";
            m_Camera.GetComponent<CinemachineFreeLook>().m_YAxis.m_InputAxisName = "Mouse Y";
        }

        public object CaptureState()
        {
            return new SaveData()
            {
                m_TopHeightOrbit = m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[0].m_Height,
                m_MiddleRadiusOrbit = m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[1].m_Radius,
                m_DirectionX = m_Camera.GetComponent<CinemachineFreeLook>().m_XAxis.Value,
                m_DirectionY = m_Camera.GetComponent<CinemachineFreeLook>().m_YAxis.Value,
            };
        }

        public void RestoreState(object state)
        {
            SaveData saveData = (SaveData)state;
            m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[0].m_Height = saveData.m_TopHeightOrbit;
            m_Camera.GetComponent<CinemachineFreeLook>().m_Orbits[1].m_Radius = saveData.m_MiddleRadiusOrbit;
            m_Camera.GetComponent<CinemachineFreeLook>().m_XAxis.Value = saveData.m_DirectionX;
            m_Camera.GetComponent<CinemachineFreeLook>().m_YAxis.Value = saveData.m_DirectionY;
        }

        [Serializable]
        private struct SaveData
        {
            public float m_TopHeightOrbit;
            public float m_MiddleRadiusOrbit;
            public float m_DirectionX;
            public float m_DirectionY;
        }
    }
}
