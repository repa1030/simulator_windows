// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNightScene : MonoBehaviour
{
    [Header("Real-Time/Sim-Time Ratio ----------")]
    public float minutesPerHour = 5.0f;
    [Header("Day and Night Cycle Configuration ----------")]
    public float sunZenithTime = 13.0f;
    public float sunZenithAngle = 60.0f;
    public float dayLength = 12.0f;
    public float timeOfDay = 12.0f;
    [Header("Scene Type Selection ----------")]
    public bool dayAndNightCycle = false;
    public bool dayScene = false;
    public bool nightScene = false;
    [Header("Skybox Materials----------")]
    public Material skyboxDay;
    public Material skyboxNight;
    public Material skyboxCycle;
    [Header("Sun Light ----------")]
    public Light sceneLight;
    public Transform sunTransform;
    public Transform moonTransform;
    [Header("Day and Night Cycle Configuration ----------")]
    public float defaultEnvironmentIntensity = 0.8f;
    public float darkEnvironmentIntensity = 0.4f;
    public float skyboxRotationDarkEnvironment = -25f;
    // Members
    private float m_deltaHours;
    private enum sceneMode
    {
        cycle = 1,
        day = 2,
        night = 3
    }
    private sceneMode m_sceneMode;
    private enum cycleStatus
    {
        dayRise = 1,
        dayFall = 2,
        nightRise = 3,
        nightFall = 4
    }
    private cycleStatus m_cycleStatus;
    private float m_sunRotateRight;
    private float m_sunRotateUp;
    private float m_upRotateRatio;
    private float m_sunZenithAngle;
    private float m_sunRotateDay;
    private float m_sunRotateNight;
    private float m_defaultEnvironmentIntensity;
    private float m_darkEnvironmentIntensity;
    private float m_skyboxRotationDarkEnvironment;
    private float m_intensity_slope;
    private float m_intensity_offset;
    // Start is called before the first frame update
    void Start()
    {
        // Init
        minutesPerHour = PlayerPrefs.GetFloat("ratio", 5.0f);
        m_sunRotateUp = 0.0f;
        m_deltaHours = 0.0f;
        m_upRotateRatio = 360f / 24f; // 360° in 24h
        m_sunZenithAngle = sunZenithAngle;
        m_sunRotateDay = 0.5f * (sunZenithAngle / dayLength);
        m_sunRotateNight = 0.5f * (sunZenithAngle / (24f - dayLength));
        m_cycleStatus = cycleStatus.dayFall;

        // Parameters (set once):
        m_defaultEnvironmentIntensity = defaultEnvironmentIntensity;
        m_darkEnvironmentIntensity = darkEnvironmentIntensity;
        m_skyboxRotationDarkEnvironment = skyboxRotationDarkEnvironment;
        m_intensity_slope = (m_defaultEnvironmentIntensity - m_darkEnvironmentIntensity) / (Mathf.Abs(m_sunZenithAngle) + m_skyboxRotationDarkEnvironment); // Adjustment Factor for Environment Intensity
        m_intensity_offset = m_defaultEnvironmentIntensity - m_intensity_slope * m_skyboxRotationDarkEnvironment; // Adjustment Offset for Calculation of Environement Intensity

        string inputTime = PlayerPrefs.GetString("time", "12:00");
        string[] timeStrArr = inputTime.Split(':');
        float hour = float.Parse(timeStrArr[0]);
        float minutes = float.Parse(timeStrArr[1])/60;
        timeOfDay = hour+minutes;

        m_sceneMode = (sceneMode)PlayerPrefs.GetInt("time_mode", 2);
        switch (m_sceneMode)
        {
            case sceneMode.day:
                dayScene = true;
                setupDayScene();
                break;
            case sceneMode.night:
                setupNightScene();
                nightScene = true;
                break;
            case sceneMode.cycle:
                setUpCycle();
                dayAndNightCycle = true;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        changeScene();
    }

    void changeScene()
    {
        switch(m_sceneMode)
        {
            case sceneMode.day:
                if (nightScene)
                {
                    setupNightScene();
                    m_sceneMode = sceneMode.night;
                    dayAndNightCycle = false;
                    dayScene = false;
                }
                else if (dayAndNightCycle)
                {
                    m_sceneMode = sceneMode.cycle;
                    timeOfDay = sunZenithTime;
                    setUpCycle();
                    nightScene = false;
                    dayScene = false;
                }
                else
                {
                    dayScene = true;
                }
                break;
            case sceneMode.night:
                if (dayScene)
                {
                    setupDayScene();
                    m_sceneMode = sceneMode.day;
                    nightScene = false;
                    dayAndNightCycle = false;
                }
                else if (dayAndNightCycle)
                {
                    m_sceneMode = sceneMode.cycle;
                    timeOfDay = calcDayClockTime(sunZenithTime-12);
                    setUpCycle();
                    nightScene = false;
                    dayScene = false;
                }
                else
                {
                    nightScene = true;
                }
                break;
            case sceneMode.cycle:
                sceneCycle();
                if (dayScene)
                {
                    setupDayScene();
                    m_sceneMode = sceneMode.day;
                    dayAndNightCycle = false;
                    nightScene = false;
                }
                else if (nightScene)
                {
                    setupNightScene();
                    m_sceneMode = sceneMode.night;
                    dayAndNightCycle = false;
                    dayScene = false;
                }
                else
                {
                    dayAndNightCycle = true;
                }
                break;
        }
    }

    void setupDayScene()
    {
        RenderSettings.skybox = skyboxDay;
        m_sunRotateRight = 90f;
        sunTransform.rotation = Quaternion.Euler(m_sunRotateRight, m_sunRotateUp, 0);
        moonTransform.rotation = Quaternion.Euler(-m_sunRotateRight, -m_sunRotateUp, 0);
        sceneLight.intensity = 1f;
        m_defaultEnvironmentIntensity = defaultEnvironmentIntensity;
        RenderSettings.ambientIntensity = m_defaultEnvironmentIntensity;
    }

    void setupNightScene()
    {
        RenderSettings.skybox = skyboxNight;
        m_sunRotateRight = -90f;
        moonTransform.rotation = Quaternion.Euler(-m_sunRotateRight, -m_sunRotateUp, 0);
        sunTransform.rotation = Quaternion.Euler(m_sunRotateRight, m_sunRotateUp, 0);
        sceneLight.intensity = 0.0f;
        m_darkEnvironmentIntensity = darkEnvironmentIntensity;
        RenderSettings.ambientIntensity = m_darkEnvironmentIntensity;
    }

    public void setUpCycle()
    {
        
        m_sunZenithAngle = sunZenithAngle;
        m_sunRotateDay = 2f * (sunZenithAngle / dayLength);
        m_sunRotateNight = 2f * (sunZenithAngle / (24f - dayLength));
        // Cycle Start State and Conditions
        m_sunRotateRight = getSunRotate();
        m_defaultEnvironmentIntensity = defaultEnvironmentIntensity;
        m_darkEnvironmentIntensity = darkEnvironmentIntensity;
        m_skyboxRotationDarkEnvironment = skyboxRotationDarkEnvironment;
        rotateSunUp();
        adjustSunIntensity();
        RenderSettings.skybox = skyboxCycle;
        sceneLight.intensity = 1f;
        sunTransform.rotation = Quaternion.Euler(m_sunRotateRight, m_sunRotateUp, 0);
        moonTransform.rotation = Quaternion.Euler(-m_sunRotateRight, -m_sunRotateUp, 0);
        if (m_sunRotateRight <= m_skyboxRotationDarkEnvironment) // Skybox Dark below -25° SunRightRotation --> Reducing Environment Intensity
        {
            RenderSettings.ambientIntensity = m_intensity_slope * m_sunRotateRight + m_intensity_offset;
        }
        else
        {
            RenderSettings.ambientIntensity = m_defaultEnvironmentIntensity;
        }
    }

    float getSunRotate()
    {
        float time = timeOfDay;
        float sunRotate = 60f;
        // Convert 24h-Clock Format to 24+x Format
        if (timeOfDay < sunZenithTime)
        {
            time = timeOfDay + 24f;
        }
        // Calculate SunRotateRight
        if ((sunZenithTime <= time) && (time < (sunZenithTime + 0.5f * dayLength)))
        {
            sunRotate = -m_sunRotateDay * time + sunZenithAngle + m_sunRotateDay * sunZenithTime;
            m_cycleStatus = cycleStatus.dayFall;
        }
        else if (((sunZenithTime + 0.5f * dayLength) <= time) && (time < (sunZenithTime + 12f)))
        {
            sunRotate = -m_sunRotateNight * time + m_sunRotateNight * (sunZenithTime + 0.5f * dayLength);
            m_cycleStatus = cycleStatus.nightFall;
        }
        else if (((sunZenithTime + 12f) <= time) && (time < (sunZenithTime + 24f - 0.5f * dayLength)))
        {
            sunRotate = m_sunRotateNight * time - (sunZenithAngle + m_sunRotateNight * (sunZenithTime + 12f));
            m_cycleStatus = cycleStatus.nightRise;
        }
        else if (((sunZenithTime + 24f - 0.5f * dayLength) <= time) && (time < (sunZenithTime + 24f)))
        {
            sunRotate = m_sunRotateDay * time + sunZenithAngle - m_sunRotateDay * (sunZenithTime + 24f);
            m_cycleStatus = cycleStatus.dayRise;
        }
        return sunRotate;
    }

    bool checkCycleState(float lowerBorder, float upperBorder)
    {
        bool retBool = false;
        if (lowerBorder < upperBorder)
        {
            retBool = timeOfDay >= upperBorder;
        }
        else
        {
            retBool = (timeOfDay < lowerBorder) && (timeOfDay >= upperBorder);
        }
        return retBool;
    }

    void sceneCycle()
    {
        m_deltaHours = (Time.deltaTime / 60f) / minutesPerHour;
        timeOfDay += m_deltaHours;
        timeOfDay = calcDayClockTime(timeOfDay);
        rotateSunUp();
        rotateSunRight();
        adjustSunIntensity();
        sunTransform.rotation = Quaternion.Euler(m_sunRotateRight, m_sunRotateUp, 0);
        moonTransform.rotation = Quaternion.Euler(-m_sunRotateRight, -m_sunRotateUp, 0);
    }

    void rotateSunUp()
    {
        m_sunRotateUp = (m_upRotateRatio * (timeOfDay - sunZenithTime)) % 360f;
    }

    void rotateSunRight()
    {
        switch (m_cycleStatus)
        {
            case cycleStatus.dayRise:
                m_sunRotateRight += m_sunRotateDay * m_deltaHours;
                if (m_sunRotateRight >= m_sunZenithAngle)
                {
                    m_cycleStatus = cycleStatus.dayFall;
                }
                break;
            case cycleStatus.dayFall:
                m_sunRotateRight -= m_sunRotateDay * m_deltaHours;
                if (m_sunRotateRight <= 0f)
                {
                    m_cycleStatus = cycleStatus.nightFall;
                }
                break;
            case cycleStatus.nightFall:
                m_sunRotateRight -= m_sunRotateNight * m_deltaHours;
                if (m_sunRotateRight <= m_skyboxRotationDarkEnvironment) // Skybox Dark below -25° SunRightRotation --> Reducing Environment Intensity
                {
                    RenderSettings.ambientIntensity = m_intensity_slope * m_sunRotateRight + m_intensity_offset;
                }
                else
                {
                    RenderSettings.ambientIntensity = m_defaultEnvironmentIntensity;
                }
                if (m_sunRotateRight <= -m_sunZenithAngle)
                {
                    m_cycleStatus = cycleStatus.nightRise;
                    RenderSettings.ambientIntensity = m_darkEnvironmentIntensity; // Reset to defined Dark Environment Intensity
                }
                break;
            case cycleStatus.nightRise:
                m_sunRotateRight += m_sunRotateNight * m_deltaHours;
                if (m_sunRotateRight <= m_skyboxRotationDarkEnvironment) // Skybox Dark below -25° SunRightRotation --> Reducing Environment Intensity
                {
                    RenderSettings.ambientIntensity = m_intensity_slope * m_sunRotateRight + m_intensity_offset;
                }
                else
                {
                    RenderSettings.ambientIntensity = m_defaultEnvironmentIntensity;
                }
                if (m_sunRotateRight >= 0f)
                {
                    m_cycleStatus = cycleStatus.dayRise;
                }
                break;
        }
    }

    void adjustSunIntensity()
    {
        if (m_sunRotateRight >= 10f)
        {
            sceneLight.intensity = 1.0f;
        }
        else if ((m_sunRotateRight >= -1f) && (m_sunRotateRight < 10f))
        {
            sceneLight.intensity = 0.09f * m_sunRotateRight + 0.1f;
        }
        else
        {
            sceneLight.intensity = 0.0f;
        }
    }

    float calcDayClockTime(float dayTime)
    {
        float retTime = 0f;
        if (dayTime < 0f)
        {
            retTime = 24f + dayTime;
        }
        else if (dayTime >= 24f)
        {
            retTime = dayTime - 24f;
        }
        else
        {
            retTime = dayTime;
        }
        return retTime;
    }


    void OnApplicationQuit()
    {
        RenderSettings.skybox = skyboxDay;
    }
}
