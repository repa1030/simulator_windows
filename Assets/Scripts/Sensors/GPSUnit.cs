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

public class GPSUnit : MonoBehaviour
{
    public float lat { get { return m_lat; } }
    public float lon { get { return m_lon; } }
    public float alt { get { return m_alt; } }
    public float pos_x { get { return m_pos_x; } }
    public float pos_y { get { return m_pos_y; } }
    public float pos_z { get { return m_pos_z; } }
    public double[] pos_covar { get { return positionCovariance; } }
    // Reference GameObject with Vehicle Rigidbody
    [Header("Reference Rigidbody ----------")]
    public GameObject baseLink;
    private Rigidbody m_rigidbody;
    // GPS Unit Control
    [Header("GPS Control ----------")]
    public bool enableGPSUnit = true;
    [Header("GPS Noise Settings ----------")]
    public bool enableGPSNoise = false;
    public float upperAccuracy = 25.0f;
    public float lowerAccuracy = 5.0f;
    public float noiseUpdateIntervall = 1.0f;
    [Header("GPS Base Offset ----------")]
    public float baseLonCoordinate = 9.0f;
    public float baseLatCoordinate = 48.0f;
    public float baseAltitude = 120.0f;
    [Header("GPS Covariance ----------")]
    private const int COVAR_SIZE = 9;
    [SerializeField] private double[] positionCovariance = new double[COVAR_SIZE];

    // Private GPS Unit Members for Signal Calculation
    private float m_accuracy;
    private Vector3 m_position_xyz;
    private float m_pos_x;
    private float m_pos_y;
    private float m_pos_z;
    private float m_deviation_x = 0.0f;
    private float m_deviation_y = 0.0f;
    private float m_deviation_z = 0.0f;
    private float m_lat;
    private float m_lon;
    private float m_alt;
    private int m_noise_counter = 0;
    private int m_noiseUpdateValue;
    // WGS-84 Parameters:
    private const float m_eq_radius = 6378137.0f;
    private const float m_eq_f = 1 / 298.257223563f;
    private const float m_orientation = 0.0f;

    // Validate the covariance matrix
    private void OnValidate()
    {
        if (positionCovariance.Length != COVAR_SIZE)
        {
            Debug.LogWarning("Don't change the 'Position Covariance' field's array size!");
            Array.Resize(ref positionCovariance, COVAR_SIZE);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_noiseUpdateValue = (int)(noiseUpdateIntervall / Time.fixedDeltaTime);
        m_rigidbody = baseLink.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        m_position_xyz = m_rigidbody.position;
        if (enableGPSNoise && enableGPSUnit) // (real) GPS Unit with noise
        {
            m_noise_counter++;
            if (m_noise_counter == m_noiseUpdateValue)
            {
                m_noise_counter = 0;
                generateGPSNoise();
            }
            m_pos_x = m_position_xyz.x + m_deviation_x;
            m_pos_y = m_position_xyz.y + m_deviation_y;
            m_pos_z = m_position_xyz.z + m_deviation_z;
        }
        else if(enableGPSUnit)  // (ideal) GPS Unit
        {
            m_pos_x = m_position_xyz.x;
            m_pos_y = m_position_xyz.y;
            m_pos_z = m_position_xyz.z;
        }
        else // DISABLED: set positions to 0
        {
            m_pos_x = 0;
            m_pos_y = 0;
            m_pos_z = 0;
            m_lon = 0;
            m_lat = 0;
            m_alt = 0;
        }
        calcCoordinates(); // transform x,y,z to lon,lat,alt
    }

    void generateGPSNoise()
    {
        // double random noise for gps signal
        // noise1: accuracy value out of specified range
        float factor_accuracy = UnityEngine.Random.value;
        m_accuracy = Mathf.Lerp(lowerAccuracy, upperAccuracy, factor_accuracy);
        // noise2: multiply accuracy for each direction with different random value  
        m_deviation_x = (2 * UnityEngine.Random.value - 1) * m_accuracy;
        m_deviation_y = (2 * UnityEngine.Random.value - 1) * m_accuracy;
        m_deviation_z = (2 * UnityEngine.Random.value - 1) * m_accuracy;
    }

    void calcCoordinates()
    {
        // transformation of x,y,z-coordinates to geographical coordinates (lon, lat, alt)
        float dLon = Mathf.Cos(m_orientation) * m_pos_z - Mathf.Sin(m_orientation) * m_pos_x;
        float dLat = Mathf.Cos(m_orientation) * m_pos_x - Mathf.Sin(m_orientation) * m_pos_z;
        // f1, f2 are factors for calculation of Rn, Rm
        float f1 = (2 * m_eq_f - m_eq_f * m_eq_f);
        float f2 = Mathf.Sin(baseLatCoordinate * Mathf.Deg2Rad) * Mathf.Sin(baseLatCoordinate * Mathf.Deg2Rad);
        float Rn = m_eq_radius / Mathf.Sqrt(1 - f1 * f2);
        float Rm = Rn * (1 - f1 / (1 - f1 * f2));
        float deltaLat = dLon * Mathf.Atan2(1, Rm);
        float deltaLon = dLat * Mathf.Atan2(1, Rn * Mathf.Cos(baseLatCoordinate * Mathf.Deg2Rad));
        m_lon = deltaLon * Mathf.Rad2Deg + baseLonCoordinate;
        m_lat = deltaLat * Mathf.Rad2Deg + baseLatCoordinate;
        m_alt = m_pos_y;
    }

    // Update is called once per frame
    void Update()
    {
        // Everything Sensor-Related is done in Fixed-Update
    }
}
