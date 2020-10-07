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
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject options;

    private Slider imgFreq;
    private Text imgValue;
    private Slider otherFreq;
    private Text otherValue;
    private bool isOptionsOpen;

    private InputField ipInput;
    private InputField portInput;

    void Start()
    {
        Button startDayButton = GameObject.Find("StartDay").GetComponent<Button>();
        startDayButton.onClick.AddListener(StartSimulatorDay);

        Button startNightButton = GameObject.Find("StartNight").GetComponent<Button>();
        startNightButton.onClick.AddListener(StartSimulatorNight);

        Button optionsButton = GameObject.Find("Options").GetComponent<Button>();
        optionsButton.onClick.AddListener(OpenOptions);

        Button saveButton = GameObject.Find("Save").GetComponent<Button>();
        saveButton.onClick.AddListener(SaveOptions);

        imgFreq = GameObject.Find("ImgSlider").GetComponent<Slider>();
        imgValue = GameObject.Find("ImgValue").GetComponent<Text>();
        otherFreq = GameObject.Find("OtherSlider").GetComponent<Slider>();
        otherValue = GameObject.Find("OtherValue").GetComponent<Text>();

        ipInput = GameObject.Find("IP").GetComponent<InputField>();
        portInput = GameObject.Find("Port").GetComponent<InputField>();

        imgFreq.value = (float)PlayerPrefs.GetInt("imgfrq", 20);
        otherFreq.value = (float)PlayerPrefs.GetInt("frq", 30);
        ipInput.text = PlayerPrefs.GetString("ip", "192.168.206.3");
        portInput.text = PlayerPrefs.GetString("port", "9090");
        Toggle pub = GameObject.Find("PublisherToggle").GetComponent<Toggle>();
        pub.isOn = (PlayerPrefs.GetInt("publishers", 1) != 0);
        
        options.SetActive(false);
        isOptionsOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        imgValue.text = imgFreq.value.ToString() + " Hz";
        otherValue.text = otherFreq.value.ToString() + " Hz";
    }

    void StartSimulatorDay()
    {
        SceneManager.LoadScene("Circuit");
    }

    void StartSimulatorNight()
    {
        SceneManager.LoadScene("Circuit_Night");
    }

    void OpenOptions()
    {
        if (isOptionsOpen) 
        {
            options.SetActive(false);
            isOptionsOpen = false;
        }
        else
        {
            options.SetActive(true);
            isOptionsOpen = true;
        }
    }

    void SaveOptions()
    {
        string ip = ipInput.text;
        string[] ipStringArr = ip.Split('.');
        int[] ipIntArr = new int[ipStringArr.Length];
        Text ipText = GameObject.Find("IP/Text").GetComponent<Text>();
        if (ipIntArr.Length != 4)
        {
            ipText.color = Color.red;
        } else {
            for (int i = 0; i < ipStringArr.Length; i++)
            {
                ipIntArr[i] = int.Parse(ipStringArr[i]);
                if (ipIntArr[i] < 0 || ipIntArr[i] > 255)
                {
                    ipText.color = Color.red;
                    break;
                } else {
                    ipText.color = Color.black;
                    PlayerPrefs.SetString("ip", ipInput.text);
                }
            }
        }

        PlayerPrefs.SetString("port", portInput.text);

        PlayerPrefs.SetInt("imgfrq", (int)imgFreq.value);
        PlayerPrefs.SetInt("frq", (int)otherFreq.value);

        Toggle pub = GameObject.Find("PublisherToggle").GetComponent<Toggle>();
        bool val = pub.isOn;
        PlayerPrefs.SetInt("publishers", val ? 1 : 0);

        options.SetActive(false);
        isOptionsOpen = false;
    }
}
