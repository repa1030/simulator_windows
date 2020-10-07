// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMUUnit : MonoBehaviour
{
    // Reference GameObject with Vehicle Rigidbody
    [Header("Reference Rigidbody ----------")]
    public GameObject baseLink;
    private Rigidbody m_rigidbody;
    // IMU Control
    [Header("IMU Control ----------")]
    public bool enableIMU = true;
    [Header("IMU Signal Control ----------")]
    public bool enableTurnRates = true;
    public bool enableAccelerations = true;
    public bool enableOrientations = false;
    [Header("IMU Covariance ----------")]
    private const int COVAR_SIZE = 9;
    [SerializeField] private double[] orientationCovariance = new double[COVAR_SIZE];
    [SerializeField] private double[] angularVelocityCovariance = new double[COVAR_SIZE];
    [SerializeField] private double[] linearAccelerationCovariance = new double[COVAR_SIZE];
    
    // IMU Results (Sensor-Signals)
    public bool enable_orient { get { return enableOrientations; } }
    public bool enable_velo { get { return enableTurnRates; } }
    public bool enable_accel { get { return enableAccelerations; } }
    public float yaw_rate { get { return m_yaw_rate; } }
    public float wank_rate { get { return m_wank_rate; } }
    public float nick_rate { get { return m_nick_rate; } }
    public float acc_lon { get { return m_acc_lon; } }
    public float acc_lat { get { return m_acc_lat; } }
    public float acc_alt { get { return m_acc_alt; } }
    public float orient_x { get { return m_orient_x; } }
    public float orient_y { get { return m_orient_y; } }
    public float orient_z { get { return m_orient_z; } }
    public double[] covar_accel { get { return linearAccelerationCovariance; } }
    public double[] covar_velo { get { return angularVelocityCovariance; } }
    public double[] covar_orient { get { return orientationCovariance; } }
    // Private IMU Members for Signal Calculation
    private float m_acc_lon;
    private float m_acc_lat;
    private float m_acc_alt;
    private float m_yaw_rate;
    private float m_wank_rate;
    private float m_nick_rate;
    private float m_orient_x;
    private float m_orient_y;
    private float m_orient_z;
    private float m_prev_velocity_lon = 0.0f;
    private float m_prev_velocity_lat = 0.0f;
    private float m_prev_velocity_alt = 0.0f;
    private Vector3 m_velocity;
    private Vector3 m_angularVelocity;

    // Validate the covariance matrices
    private void OnValidate()
    {
        // Check array sizes (should be COVAR_SIZE)
        if (orientationCovariance.Length != COVAR_SIZE)
        {
            Debug.LogWarning("Don't change the 'Orientation Covariance' field's array size!");
            Array.Resize(ref orientationCovariance, COVAR_SIZE);
        }
        if (angularVelocityCovariance.Length != COVAR_SIZE)
        {
            Debug.LogWarning("Don't change the 'Angular Velocity Covariance' field's array size!");
            Array.Resize(ref angularVelocityCovariance, COVAR_SIZE);
        }
        if (linearAccelerationCovariance.Length != COVAR_SIZE)
        {
            Debug.LogWarning("Don't change the 'Linear Acceleration Covariance' field's array size!");
            Array.Resize(ref linearAccelerationCovariance, COVAR_SIZE);
        }
        // Check enabling of data measuring
        if (!enableOrientations)
            orientationCovariance[0] = -1;
        else 
            orientationCovariance[0] = 0;
        if (!enableTurnRates)
            angularVelocityCovariance[0] = -1;
        else
            angularVelocityCovariance[0] = 0;
        if (!enableAccelerations)
            linearAccelerationCovariance[0] = -1;
        else
            linearAccelerationCovariance[0] = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody = baseLink.GetComponent<Rigidbody>(); // get vehicle rigidbody
        // Orientation prediction is not in use -> set default 0
        m_orient_x = 0.0f; m_orient_y = 0.0f; m_orient_z = 0.0f;
    }

    void FixedUpdate()
    {
        // transform rigidbody-velocity/turnrates into local coordinates
        m_velocity = this.transform.InverseTransformDirection(m_rigidbody.velocity); // m/s
        m_angularVelocity = this.transform.InverseTransformDirection(m_rigidbody.angularVelocity);
        // yaw-Rates ind rad/s
        if (enableIMU && enableTurnRates) // ENABLED: get turn rates from rigidbody
        {
            m_yaw_rate = m_angularVelocity.y;
            m_wank_rate = m_angularVelocity.z;
            m_nick_rate = m_angularVelocity.x;
        }
        else   // DISABLED: set all turn rate values to 0
        {
            m_yaw_rate = 0;
            m_wank_rate = 0;
            m_nick_rate = 0;
        }
        // Longitudinal and Lateral Acceleration in m/s^2
        // required to be calculated in FixedUpdate - DOES NOT WORK in Update
        if (enableIMU && enableAccelerations) // ENABLED: get velocities from rigidbody
        {
            m_acc_lon = (m_velocity.z - m_prev_velocity_lon) / Time.fixedDeltaTime;
            m_acc_lat = (m_velocity.x - m_prev_velocity_lat) / Time.fixedDeltaTime;
            m_acc_alt = (m_velocity.y - m_prev_velocity_alt) / Time.fixedDeltaTime;
        }
        else // DISABLED: set all acceleration values to 0
        {
            m_acc_lon = 0;
            m_acc_lat = 0;
            m_acc_alt = 0;
        }
        // save actual velocities as pervious values for next call
        m_prev_velocity_lon = m_velocity.z;
        m_prev_velocity_lat = m_velocity.x;
        m_prev_velocity_alt = m_velocity.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Everything Sensor-Related is done in Fixed-Update
    }

}
