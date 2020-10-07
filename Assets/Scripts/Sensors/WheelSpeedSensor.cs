// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWH.WheelController3D;

public class WheelSpeedSensor : MonoBehaviour
{
    // Wheel Speed Sensor Control
    [Header("Wheel Speed Sensor Signal Control ----------")]
    public bool enableSensorFL = true;
    public bool enableSensorFR = true;
    public bool enableSensorRL = true;
    public bool enableSensorRR = true;
    // Wheel Speed Sensor Results (Sensor-Signals)
    public float wheel_fl_rps { get { return m_wheel_fl_rps; } }
    public float wheel_fr_rps { get { return m_wheel_fr_rps; } }
    public float wheel_rl_rps { get { return m_wheel_rl_rps; } }
    public float wheel_rr_rps { get { return m_wheel_rr_rps; } }
    // Private Wheel Speed Sensor Members for Signal Calculation
    private float m_wheel_fl_rps;
    private float m_wheel_fr_rps;
    private float m_wheel_rl_rps;
    private float m_wheel_rr_rps;
    // Wheel Controllers of Vehicle 
    [Header("Wheel Controller ----------")]
    public WheelController wheelcontrollerFL;
    public WheelController wheelcontrollerFR;
    public WheelController wheelcontrollerRL;
    public WheelController wheelcontrollerRR;

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        // read wheel-rps from WheelController Scripts
        m_wheel_fl_rps = getWheelRPS(wheelcontrollerFL,enableSensorFL);
        m_wheel_fr_rps = getWheelRPS(wheelcontrollerFR,enableSensorFR);
        m_wheel_rl_rps = getWheelRPS(wheelcontrollerRL,enableSensorRL);
        m_wheel_rr_rps = getWheelRPS(wheelcontrollerRR,enableSensorRR);
    }

    // Update is called once per frame
    void Update()
    {
        // Everything Sensor-Related is done in Fixed-Update
    }

    private float getWheelRPS(WheelController wheelController,bool enable)
    {
        // transforms wheel-rpm to wheel-rps
        float wheel_rps = 0.0f;
        if (enable) // ENABLED: get wheel speed from WheelController
        {
            wheel_rps = wheelController.wheel.rpm / 60;
        }
        else // DISABLED: set wheel speed to 0
        {
            wheel_rps = 0;
        }
        return wheel_rps;
    }
}
