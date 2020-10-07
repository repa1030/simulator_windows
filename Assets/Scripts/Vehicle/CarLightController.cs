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

public class CarLightController : MonoBehaviour
{
    public CarController carController;
    [Header("Main Light Settings ----------")]
    public Light mainLightLeft;
    public Light mainLightRight;
    public Light licensePlateLightLeft;
    public Light licensePlateLightRight;
    public float highBeamRange = 200.0f;
    public float lowBeamRangeLeft = 60.0f;
    public float lowBeamRangeRight = 80.0f;
    [Header("Turn Signal Settings ----------")]
    public float turnSignalFrequency = 1.0f;
    [Header("Light Objects ----------")]
    public GameObject mainLights;
    public GameObject frontLights;
    public GameObject rearLights;
    public GameObject brakeLights;
    public GameObject turnLightFrontLeft;
    public GameObject turnLightFrontRight;
    public GameObject turnLightRearLeft;
    public GameObject turnLightRearRight;
    [Header("Light Materials ----------")]
    // General ON & OFF
    public Material lightOff;
    public Material lightOn;
    public Material dayLightOn;
    // Turn Lights Only (On)
    public Material turnLightFrontOn;
    // Turn Light Full Size (Object: RevereseLight)
    public Material rearTurnLightOn;
    // Brake Light
    public Material brakeLightOn;
    // Fog Light (Object: RevereseLight)
    public Material fogLightOn;
    public Material fogAndTurnLightOn;
    // Reverse Light (Object: RevereseLight)
    public Material reverseLightOn;
    public Material reverseAndTurnLightOn;
    // Setter for Light Control Interface
    public bool lightStatusIn { set { m_lightStatusIn = value; } }
    public bool fogLightIn { set { m_fogLightIn = value; } }
    public turnlights turnLightsIn { set { m_turnLights = value; } 
                                     get { return m_turnLights; } }
    public bool highBeamIn { set { m_highBeamIn = value; } }
    public bool flashLightIn { set { m_flashLightIn = value; } }
    // Public Enum for Setter
    public enum turnlights
    {
        Off = 0,
        Left = 1,
        Right = 2,
        All = 3
    }
    // Private Member for Light Control Interface
    private bool m_lightStatusIn;
    private bool m_fogLightIn;
    private bool m_allTurnLightsIn;
    private bool m_leftTurnLightsIn;
    private bool m_rightTurnLightsIn;
    private bool m_highBeamIn;
    private bool m_flashLightIn;
    // Private Light Flags
    private bool m_lightStatus;
    private bool m_fogLight;
    private turnlights m_turnLights;
    private bool m_turnLightsAll;
    private bool m_highBeam;
    private bool m_flashLight;
    // Private MeshRenderer of Light Objects
    private MeshRenderer m_mainLights;
    private MeshRenderer m_frontLights;
    private MeshRenderer m_rearLights;
    private MeshRenderer m_brakeLights;
    private MeshRenderer m_turnLightFrontLeft;
    private MeshRenderer m_turnLightFrontRight;
    private MeshRenderer m_turnLightRearLeft;
    private MeshRenderer m_turnLightRearRight;
    // Private Parameters for Turn Frequency Generator
    private bool m_turnFreqState;
    private bool m_turnFreqActive;
    private float m_turnFreqTimer;
    private float m_turnFreqMaxTime; // 1 blink signal per second
    // Private Members for Rear Light State Management
    private enum vehicleLightState
    {
        Off = 0,  
        HighBeam = 1,
        LowBeam = 2
    }
    private vehicleLightState m_vehicleLightState;
    private enum rearLightState
    {
        Forward = 1,
        Fog = 2,
        Reverse = 3
    }
    private rearLightState m_rearLightState;
    private enum turnLightState
    {
        Off = 0,
        BrightAll = 1,
        DarkAll = 2,
        BrightRight = 3,
        DarkRight = 4,
        BrightLeft = 5,
        DarkLeft = 6
    }
    private turnLightState m_turnLightState;
    private bool m_brakeSignal;
    private bool m_reverseSignal;
    // Private Mebers for RearLight Turns
    private Material m_rearTurnOff;
    private Material m_rearTurnOn;
    // Start is called before the first frame update
    void Start()
    {
        // get MeshRenderer of the Light Objects
        m_mainLights = mainLights.GetComponent<MeshRenderer>();
        m_frontLights = frontLights.GetComponent<MeshRenderer>();
        m_rearLights = rearLights.GetComponent<MeshRenderer>();
        m_brakeLights = brakeLights.GetComponent<MeshRenderer>();       
        m_turnLightFrontLeft = turnLightFrontLeft.GetComponent<MeshRenderer>();
        m_turnLightFrontRight = turnLightFrontRight.GetComponent<MeshRenderer>();
        m_turnLightRearLeft = turnLightRearLeft.GetComponent<MeshRenderer>();
        m_turnLightRearRight = turnLightRearRight.GetComponent<MeshRenderer>();
        // inits:
        m_turnLights = turnlights.Off;
        m_rearLightState = rearLightState.Forward; // initalize rear light state
        m_vehicleLightState = vehicleLightState.Off;
        m_turnLightState = turnLightState.Off;
        m_turnLightFrontLeft.material = dayLightOn;
        m_turnLightFrontRight.material = dayLightOn;
        m_turnFreqMaxTime = 1 / turnSignalFrequency;
        mainLightLeft.enabled = mainLightRight.enabled = false;
        licensePlateLightLeft.enabled = licensePlateLightRight.enabled = false;
        mainLightLeft.range = lowBeamRangeLeft;
        mainLightRight.range = lowBeamRangeRight;
        m_lightStatus = false;
        m_fogLight = false;
        m_turnLightsAll = false;
        m_highBeam = false;
        m_flashLight = false;
        m_turnFreqState = false;
        m_turnFreqActive = false;
        m_turnFreqTimer = 0.0f;
        m_brakeSignal = false;
        m_reverseSignal = false;
        // init light control interface
        m_lightStatusIn = false;
        m_fogLightIn = false;
        m_allTurnLightsIn = false;
        m_leftTurnLightsIn = false;
        m_rightTurnLightsIn = false;
        m_highBeamIn = false;
        m_flashLightIn = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        GeneralLightFunctions();
        RearTurnLightStates();
        TurnLightFunctions();
        BrakeLightFunction();
    }

    private void GetInput()
    {
        // General Car Light - State Chart
        switch (m_lightStatus)
        {
            case true:
                if (Input.GetKeyDown("1") || m_lightStatusIn)
                {
                    m_lightStatus = false;
                }
                break;

            case false:
                if (Input.GetKeyDown("1") || m_lightStatusIn)
                {
                    m_lightStatus = true;
                }
                break;
        }
        // High Beam - State Chart
        switch (m_highBeam)
        {
            case true:
                if ((Input.GetKeyDown("2") || m_highBeamIn) || !m_lightStatus)
                {
                    m_highBeam = false;
                }
                break;
            case false:
                if ((Input.GetKeyDown("2") || m_highBeamIn) && m_lightStatus)
                {
                    m_highBeam = true;
                }
                break;
        }
        // Flash Light - State Chart
        switch (m_flashLight)
        {
            case true:
                if (m_highBeam || (Input.GetKeyUp("tab") || m_flashLightIn))
                {
                    m_flashLight = false;
                }
                break;
            case false:
                if(!m_highBeam && (Input.GetKeyDown("tab") || m_flashLightIn))
                {
                    m_flashLight = true;
                }
                break;
        }
        // Turn Lights  - State Chart
        switch (m_turnLights)
        {
            case turnlights.Off:
                if (Input.GetKeyDown("5") || m_allTurnLightsIn || m_turnLightsAll)
                {
                    m_turnLights = turnlights.All;
                    m_turnLightsAll = true;
                }
                else if (Input.GetKeyDown("3") || m_leftTurnLightsIn)
                {
                    m_turnLights = turnlights.Left;
                }
                else if (Input.GetKeyDown("4") || m_rightTurnLightsIn)
                {
                    m_turnLights = turnlights.Right;
                }
                break;
            case turnlights.Left:
                if (Input.GetKeyDown("4") || m_rightTurnLightsIn)
                {
                    m_turnLights = turnlights.Right;
                }
                else if (Input.GetKeyDown("5") || m_allTurnLightsIn)
                {
                    if (m_turnLightsAll)
                    {
                        m_turnLightsAll = false;
                    }
                    else
                    {
                        m_turnLightsAll = true;
                    }
                }
                else if ((Input.GetKeyDown("3") || m_leftTurnLightsIn) && m_turnLightsAll)
                {
                    m_turnLights = turnlights.All;
                }
                else if (Input.GetKeyDown("3") || m_leftTurnLightsIn)
                {
                    m_turnLights = turnlights.Off;
                }
                break;
            case turnlights.Right:
                if (Input.GetKeyDown("3") || m_leftTurnLightsIn)
                {
                    m_turnLights = turnlights.Left;
                }
                else if (Input.GetKeyDown("5") || m_allTurnLightsIn)
                {
                    if (m_turnLightsAll)
                    {
                        m_turnLightsAll = false;
                    }
                    else
                    {
                        m_turnLightsAll = true;
                    }  
                }
                else if ((Input.GetKeyDown("4") || m_rightTurnLightsIn) && m_turnLightsAll)
                {
                    m_turnLights = turnlights.All;
                }
                else if (Input.GetKeyDown("4") || m_rightTurnLightsIn)
                {
                    m_turnLights = turnlights.Off;
                }
                break;
            case turnlights.All:
                if (Input.GetKeyDown("3") || m_leftTurnLightsIn)
                {
                    m_turnLights = turnlights.Left;
                }
                else if (Input.GetKeyDown("4") || m_rightTurnLightsIn)
                {
                    m_turnLights = turnlights.Right;
                }
                else if (Input.GetKeyDown("5") || m_allTurnLightsIn)
                {
                    m_turnLights = turnlights.Off;
                    m_turnLightsAll = false;
                }
                break;
        }
        // Fog Light Signal - State Chart
        switch (m_fogLight)
        {
            case true:
                if ((Input.GetKeyDown("6") || m_fogLightIn) || !m_lightStatus)
                {
                    m_fogLight = false;
                }
                break;
            case false:
                if ((Input.GetKeyDown("6") || m_fogLightIn) && m_lightStatus)
                {
                    m_fogLight = true;
                }
                break;
        }
        m_brakeSignal = carController.brake_signal;
        m_reverseSignal = carController.reverse_signal;
    }

    private void GeneralLightFunctions()
    {
         switch (m_vehicleLightState)
         {
            case vehicleLightState.Off:
                if (m_flashLight)
                {
                    // Vehicle Lights
                    m_frontLights.material = lightOn;
                    m_mainLights.material = lightOn;
                    // Spot Lights
                    mainLightLeft.enabled = mainLightRight.enabled = true;
                    mainLightLeft.range = mainLightRight.range = highBeamRange;
                    // State Change
                    m_vehicleLightState = vehicleLightState.HighBeam;
                }
                else if (m_lightStatus)
                {
                    // Vehicle Lights
                    m_rearLights.material = lightOn;
                    m_mainLights.material = lightOn;
                    // Spot Lights
                    mainLightLeft.enabled = mainLightRight.enabled = true;
                    licensePlateLightLeft.enabled = licensePlateLightRight.enabled = true;
                    mainLightLeft.range = lowBeamRangeLeft;
                    mainLightRight.range = lowBeamRangeRight;
                    // State Change
                    m_vehicleLightState = vehicleLightState.LowBeam;
                }
                break;
            case vehicleLightState.LowBeam:
                if (m_highBeam || m_flashLight)
                {
                    // Vehicle Lights
                    m_frontLights.material = lightOn;
                    // Spot Lights
                    mainLightLeft.range = mainLightRight.range = highBeamRange;
                    // State Change
                    m_vehicleLightState = vehicleLightState.HighBeam;
                }
                else if (!m_lightStatus)
                {
                    // Vehicle Lights
                    m_rearLights.material = lightOff;
                    m_mainLights.material = lightOff;
                    // Spot Lights
                    mainLightLeft.enabled = mainLightRight.enabled = false;
                    licensePlateLightLeft.enabled = licensePlateLightRight.enabled = false;
                    mainLightLeft.range = lowBeamRangeLeft;
                    mainLightRight.range = lowBeamRangeRight;
                    // State Change
                    m_vehicleLightState = vehicleLightState.Off;
                }
                break;
            case vehicleLightState.HighBeam:
                if (!m_lightStatus && !m_flashLight)
                {
                    // Vehicle Lights
                    m_rearLights.material = lightOff;
                    m_mainLights.material = lightOff;
                    m_frontLights.material = lightOff;
                    // Spot Lights
                    mainLightLeft.enabled = mainLightRight.enabled = false;
                    licensePlateLightLeft.enabled = licensePlateLightRight.enabled = false;
                    // State-Change
                    m_vehicleLightState = vehicleLightState.Off;
                }
                else if (!m_highBeam && !m_flashLight)
                {
                    // Vehicle Lights
                    m_frontLights.material = lightOff;
                    // Spot Lights
                    mainLightLeft.range = lowBeamRangeLeft;
                    mainLightRight.range = lowBeamRangeRight;
                    // State Change
                    m_vehicleLightState = vehicleLightState.LowBeam;
                }
                break;
        }
    }

    private void BrakeLightFunction()
    {
        switch (m_brakeSignal)
        {
            case true:
                m_brakeLights.material = brakeLightOn;
                break;
            case false:
                m_brakeLights.material = lightOff;
                break;
        }
    }
    
    private void TurnLightFunctions()
    {
        switch (m_turnLightState)
        {
            case turnLightState.Off:
                if(m_turnLights == turnlights.All)
                {
                    m_turnFreqMaxTime = 1 / turnSignalFrequency;
                    m_turnFreqActive = true;
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightAll;
                }
                else if (m_turnLights == turnlights.Left)
                {
                    m_turnFreqMaxTime = 1 / turnSignalFrequency;
                    m_turnFreqActive = true;
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.BrightLeft;
                }
                else if (m_turnLights == turnlights.Right)
                {
                    m_turnFreqMaxTime = 1 / turnSignalFrequency;
                    m_turnFreqActive = true;
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightRight;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                }
                break;
            case turnLightState.BrightAll:
                if (!m_turnFreqState)
                {
                    m_turnLightFrontLeft.material = lightOff;
                    m_turnLightFrontRight.material = lightOff;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.DarkAll;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOn;
                }
                break;
            case turnLightState.DarkAll:
                if (m_turnFreqState && m_turnLights == turnlights.Off)
                {
                    m_turnFreqActive = false;
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.Off;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.All)
                {
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightAll;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.Left)
                {
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.BrightLeft;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.Right)
                {
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightRight;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                }
                break;
            case turnLightState.BrightLeft:
                if (!m_turnFreqState)
                {
                    m_turnLightFrontLeft.material = lightOff;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.DarkLeft;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOff;
                }
                break;
            case turnLightState.DarkLeft:
                if (m_turnFreqState && m_turnLights == turnlights.Off)
                {
                    m_turnFreqActive = false;
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.Off;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.All)
                {
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightAll;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.Left)
                {
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.BrightLeft;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.Right)
                {
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightRight;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                }
                break;
            case turnLightState.BrightRight:
                if (!m_turnFreqState)
                {
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = lightOff;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.DarkRight;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOn;
                }
                break;
            case turnLightState.DarkRight:
                if (m_turnFreqState && m_turnLights == turnlights.Off)
                {
                    m_turnFreqActive = false;
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.Off;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.All)
                {
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightAll;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.Left)
                {
                    m_turnLightFrontLeft.material = turnLightFrontOn;
                    m_turnLightFrontRight.material = dayLightOn;
                    m_turnLightRearLeft.material = m_rearTurnOn;
                    m_turnLightRearRight.material = m_rearTurnOff;
                    m_turnLightState = turnLightState.BrightLeft;
                }
                else if (m_turnFreqState && m_turnLights == turnlights.Right)
                {
                    m_turnLightFrontLeft.material = dayLightOn;
                    m_turnLightFrontRight.material = turnLightFrontOn;
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOn;
                    m_turnLightState = turnLightState.BrightRight;
                }
                else
                {
                    m_turnLightRearLeft.material = m_rearTurnOff;
                    m_turnLightRearRight.material = m_rearTurnOff;
                }
                break;
        }
    }

    private void RearTurnLightStates()
    {
        switch (m_rearLightState)
        {
            case rearLightState.Forward:
                m_rearTurnOn = rearTurnLightOn;
                m_rearTurnOff = lightOff;
                // Check State Change
                if (m_reverseSignal)
                {
                    m_rearLightState = rearLightState.Reverse;
                }
                else if (m_fogLight)
                {
                    m_rearLightState = rearLightState.Fog;
                }
                break;
            case rearLightState.Fog:
                m_rearTurnOn = fogAndTurnLightOn;
                m_rearTurnOff = fogLightOn;
                // Check State Change
                if (m_reverseSignal)
                {
                    m_rearLightState = rearLightState.Reverse;
                }
                else if (!m_fogLight)
                {
                    m_rearLightState = rearLightState.Forward;
                }
                break;
            case rearLightState.Reverse:
                m_rearTurnOn = reverseAndTurnLightOn;
                m_rearTurnOff = reverseLightOn;
                // Check State Change
                if (!m_reverseSignal && !m_fogLight)
                {
                    m_rearLightState = rearLightState.Forward;
                }
                else if (!m_reverseSignal && m_fogLight)
                {
                    m_rearLightState = rearLightState.Fog;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        // functions with time depency need to be calculated in FixedUpdate
        FreqGenerator(ref m_turnFreqState, ref m_turnFreqActive, ref m_turnFreqMaxTime, ref m_turnFreqTimer);
    }

    private void FreqGenerator(ref bool freqGenState, ref bool freqActive, ref float maxTime, ref float timer)
    {
        switch (freqGenState)
        {
            case false:
                if (freqActive && (timer >= maxTime))
                {
                    timer = 0.0f;
                    freqGenState = true;
                }
                else if (freqActive && (timer >= maxTime / 2))
                {
                    timer = timer + Time.fixedDeltaTime;
                }
                else if (freqActive)
                {
                    freqGenState = true;
                }
                else
                {
                    timer = 0.0f;
                }
                break;
            case true:
                timer = timer + Time.fixedDeltaTime;
                if (timer >= maxTime / 2)
                {
                    freqGenState = false;
                }
                else if (!freqActive)
                {
                    timer = 0.0f;
                }
                break;
        }
    }
}