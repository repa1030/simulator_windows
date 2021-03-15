// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System;
using UnityEngine;

public class SumoVehicleHandler : MonoBehaviour
{
    [Header("Wheels Meshes")]
    public GameObject[] wheelsFront = new GameObject[2];
    public GameObject[] wheelsRear = new GameObject[2];

    [Header("Transform Target (The Vehicle)")]
    public Transform target;

    [Header("Steering Parameters")]
    public float steerRatio = 45;
    public float steeringDamping = 4;

    [Header("Vehicle Parameters")]
    public Vector3 transformPointOffset = new Vector3(0.0f, 0.0f, 0.0f);
    public float wheelRadius = 0.0f;

    [Header("Vehicle Light Interface")]
    public LightInterface carLight = null;
    public float flashFrequency = 1.0f;

    // private members
    private float m_last_rot;
    private float m_rotation;
    private float m_heading_diff;
    private float m_last_heading;
    private float m_steer_last;
    private float m_steer_calc;
    private float m_velocity;
    private float m_di_timer;
    private float m_lightOnSunAngle;

    // Getter and setter
    public float lightOnSunAngle { set { m_lightOnSunAngle = value; } }
    public float Velocity { get => m_velocity; set => m_velocity = value; }
    public Vector3 TransformPointOffset { get => transformPointOffset; }

    private void Start()
    {
        m_steer_last = 0.0f;
        m_last_heading = 0.0f;
        m_rotation = 0.0f;
        m_last_rot = 0.0f;
        m_di_timer = 0.0f;
    }

    private void Update()
    {
        VehicleLightSwitch();
    }

    public void CalculateSteering(float heading, float timer)
    {
        // Difference between the current heading and the weighted last 3 heading
        m_heading_diff = (heading - m_last_heading);
        m_last_heading = heading;
        m_steer_calc = Mathf.LerpAngle(m_steer_last, m_heading_diff * steerRatio, steeringDamping * Time.deltaTime);
        m_steer_last = m_steer_calc;
        WheelHandler(heading, timer);
    }

    public void WheelHandler(float heading, float timer)
    {
        // Calculating the rotation of the wheels based on the speed and the time
        m_rotation = m_last_rot + ( 180.0f / (float)Math.PI / wheelRadius / 3.6f * m_velocity * timer );
        if (m_rotation > 360)
        {
            m_rotation = m_rotation - 360;
        }
        else if (m_rotation < 0)
        {
            m_rotation = m_rotation + 360;
        }
        foreach (GameObject wheel in wheelsFront)
        {
            wheel.transform.rotation = Quaternion.Euler((m_rotation), m_steer_calc + heading, 0f);
        }
        foreach (GameObject wheel in wheelsRear)
        {
            wheel.transform.rotation = Quaternion.Euler((m_rotation), heading, 0f);
        }
        m_last_rot = m_rotation;
    }

    public void BrakeLightSwitch(bool state)
    {
        carLight.BrakeLight(state);
    }

    public void DirectionIndicatorSwitch(int _value)
    {
        switch(_value)
        {
            // No turn
            case 0:
                carLight.LeftIndicator(false);
                carLight.RightIndicator(false);
                break;
            // Turn right
            case 1:
                m_di_timer += Time.deltaTime;
                if (m_di_timer >= (1.0f / flashFrequency) * 0.5f)
                {
                    carLight.RightIndicator(!carLight.rightIndicatorActive);
                    m_di_timer = 0.0f;
                }
                break;
            // Turn left
            case 2:
                m_di_timer += Time.deltaTime;
                if (m_di_timer >= (1.0f / flashFrequency) * 0.5f)
                {
                    carLight.LeftIndicator(!carLight.leftIndicatorActive);
                    m_di_timer = 0.0f;
                }
                break;
            // Hazard lights
            case 3:
                m_di_timer += Time.deltaTime;
                // Clear signal states
                if (carLight.leftIndicatorActive != carLight.rightIndicatorActive)
                {
                    carLight.LeftIndicator(false);
                    carLight.RightIndicator(false);
                }
                if (m_di_timer >= (1.0f / flashFrequency) * 0.5f)
                {
                    carLight.LeftIndicator(!carLight.leftIndicatorActive);
                    carLight.RightIndicator(!carLight.rightIndicatorActive);
                    m_di_timer = 0.0f;
                }
                break;
            // No valid state
            default:
                Debug.LogError("SUMO: No Valid State For Direction Indicator of SUMO Car");
                break;
        }
    }

    private void VehicleLightSwitch()
    {
        Transform sunTransform = RenderSettings.sun.transform;
        float sunRotation = sunTransform.rotation.eulerAngles.x;
        if (sunRotation < m_lightOnSunAngle || sunRotation >= 270f)
        {
            carLight.GeneralLight(true);
        }
        else
        {
            carLight.GeneralLight(false);
        }
    }
}