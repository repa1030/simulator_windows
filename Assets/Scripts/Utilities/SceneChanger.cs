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
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public GameObject globalLight;
    public float fogDensity = 0.05f;
    public float ambientLight = 0.8f;
    private Camera[] m_cameras;
    private int m_current_light;
    private int m_fog_state;
    private Material m_skybox;

    // Start is called before the first frame update
    void Start()
    {
        m_cameras = Camera.allCameras;
        m_skybox = RenderSettings.skybox;
        m_current_light = 0;
        m_fog_state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Change Scene
        if (Input.GetKeyDown("m"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        // Change Light
        if (Input.GetKeyDown("n"))
        {
            if (m_current_light == 0)
            {
                globalLight.GetComponent<Light>().enabled = false;
                RenderSettings.ambientIntensity = 0.0f;
                RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.03f);
                RenderSettings.skybox = null;
                foreach (Camera cam in m_cameras) 
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0.03f, 0.03f, 0.03f);
                }
                m_current_light = 1;
                DynamicGI.UpdateEnvironment();
            }
            else 
            {
                globalLight.GetComponent<Light>().enabled = true;
                RenderSettings.skybox = m_skybox;
                RenderSettings.ambientIntensity = ambientLight;
                foreach (Camera cam in m_cameras) 
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                }
                m_current_light = 0;
                DynamicGI.UpdateEnvironment();
            }
        }
        // Change Fog
        if (Input.GetKeyDown("y"))
        {
            if (m_fog_state == 0)
            {
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fog = true;
                m_fog_state = 1;
            }
            else
            {
                RenderSettings.fog = false;
                m_fog_state = 0;
            }
        }
    }
}
