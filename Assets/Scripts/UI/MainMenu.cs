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
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private GameObject popup;
    private GameObject cycleSettings;
    private GameObject warning;

    private Toggle setCycle;
    
    private GameObject optionsROS;
    private Slider imgFreq;
    private Text imgValue;
    private Slider otherFreq;
    private Text otherValue;

    void Start()
    {
        Button startDayButton = GameObject.Find("Start").GetComponent<Button>();
        startDayButton.onClick.AddListener(StartSimulator);

        Button resetButton = GameObject.Find("Reset").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSettings);

        Button closeButton = GameObject.Find("Close").GetComponent<Button>();
        closeButton.onClick.AddListener(ClosePopup);

        // ROS settings
        InputField ipInput = GameObject.Find("IP").GetComponent<InputField>();
        ipInput.onEndEdit.AddListener(delegate{CheckIP();});
        InputField portInput = GameObject.Find("Port").GetComponent<InputField>();

        imgFreq = GameObject.Find("ImgSlider").GetComponent<Slider>();
        imgValue = GameObject.Find("ImgValue").GetComponent<Text>();
        otherFreq = GameObject.Find("OtherSlider").GetComponent<Slider>();
        otherValue = GameObject.Find("OtherValue").GetComponent<Text>();

        imgFreq.value = (float)PlayerPrefs.GetInt("imgfrq", 20);
        otherFreq.value = (float)PlayerPrefs.GetInt("frq", 30);
        ipInput.text = PlayerPrefs.GetString("ip", "192.168.206.3");
        portInput.text = PlayerPrefs.GetString("port", "9090");
        Toggle pub = GameObject.Find("PublisherToggle").GetComponent<Toggle>();
        pub.isOn = (PlayerPrefs.GetInt("publishers", 1) != 0);
        Dropdown serializer = GameObject.Find("SerializerSelect").GetComponent<Dropdown>();
        serializer.value = PlayerPrefs.GetInt("serializer", 0);

        // Scene settings
        Toggle mode;
        switch(PlayerPrefs.GetInt("time_mode", 2))
        {
            case 1:
                mode = GameObject.Find("CycleToggle").GetComponent<Toggle>();
                mode.isOn = true;
                break;
            case 2:
                mode = GameObject.Find("DayToggle").GetComponent<Toggle>();
                mode.isOn = true;
                break;
            case 3:
                mode = GameObject.Find("NightToggle").GetComponent<Toggle>();
                mode.isOn = true;
                break;
            default:
                break;
        }
        setCycle = GameObject.Find("CycleToggle").GetComponent<Toggle>();
        InputField setTime = GameObject.Find("TimeInput").GetComponent<InputField>();     
		setTime.onEndEdit.AddListener(delegate{CheckTime();});
        setTime.text = PlayerPrefs.GetString("time", "12:00");
        InputField setRatio = GameObject.Find("TimeRatio").GetComponent<InputField>();     
		setRatio.onEndEdit.AddListener(delegate{ChangeRatio();});
        setRatio.text = PlayerPrefs.GetFloat("ratio", 5.0f).ToString(new CultureInfo("en-US"));
        Toggle trees = GameObject.Find("TreeToggle").GetComponent<Toggle>();
        trees.isOn = (PlayerPrefs.GetInt("trees", 0) != 0);

        // Traffic settings
        Toggle traffic = GameObject.Find("TrafficToggle").GetComponent<Toggle>();
        traffic.onValueChanged.AddListener(delegate{CheckForSUMO();});
        traffic.isOn = (PlayerPrefs.GetInt("traffic", 0) != 0);
        warning = GameObject.Find("Warning");
        warning.SetActive(false);
        Toggle smoothMotion = GameObject.Find("SmoothToggle").GetComponent<Toggle>();
        smoothMotion.onValueChanged.AddListener(delegate{ShowWarning();});
        smoothMotion.isOn = (PlayerPrefs.GetInt("smooth", 0) != 0);

        popup = GameObject.Find("PopUp");
        popup.SetActive(false);
        cycleSettings = GameObject.Find("CycleSettings");
        cycleSettings.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {      
        imgValue.text = imgFreq.value.ToString() + " Hz";
        otherValue.text = otherFreq.value.ToString() + " Hz";

        if (setCycle.isOn)
        {
            cycleSettings.SetActive(true);
        } else {
            cycleSettings.SetActive(false);
        }
    }

    void CheckTime()
    {
        InputField setTime = GameObject.Find("TimeInput").GetComponent<InputField>();
        string[] timeStrArr = setTime.text.Split(':');
        int[] timeIntArr = new int[timeStrArr.Length];
        Text timeText = GameObject.Find("TimeInput/Text").GetComponent<Text>();
        if (timeIntArr.Length != 2)
        {
            timeText.color = Color.red;
        } else {
            if (int.TryParse(timeStrArr[0], out timeIntArr[0]) && int.TryParse(timeStrArr[1], out timeIntArr[1]))
            {
                if (timeIntArr[0] < 0) timeIntArr[0] = 0;
                else if (timeIntArr[0] > 24) timeIntArr[0] = 24;
                if (timeIntArr[1] < 0) timeIntArr[1] = 0;
                else if (timeIntArr[1] > 59) timeIntArr[1] = 59;
                setTime.text = timeIntArr[0].ToString("D2") + ":" + timeIntArr[1].ToString("D2");
                timeText.color = Color.black;
            } else {
                Debug.Log("time error!");
                timeText.color = Color.red;
            }
        }
    }

    void ChangeRatio()
    {
        InputField setRatio = GameObject.Find("TimeRatio").GetComponent<InputField>();
        setRatio.text = setRatio.text.Replace(",", ".");
        if (float.Parse(setRatio.text, new CultureInfo("en-US")) < 0)
            setRatio.text = "0.0";
    }

    void CheckIP()
    {
        InputField ipInput = GameObject.Find("IP").GetComponent<InputField>();
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
                if (int.TryParse(ipStringArr[i], out ipIntArr[i]))
                {
                    foreach (char c in ipStringArr[i])
                    {
                        if (!Char.IsDigit(c))
                        {
                            ipText.color = Color.red;
                            return;
                        }
                    }
                    if (ipIntArr[i] < 0 || ipIntArr[i] > 255)
                    {
                        ipText.color = Color.red;
                        return;
                    } else {
                        ipText.color = Color.black;
                    }
                } else {
                    ipText.color = Color.red;
                    return;
                }
            }
        }
    }

    void ResetSettings()
    {
        InputField ipInput = GameObject.Find("IP").GetComponent<InputField>();
        ipInput.text = PlayerPrefs.GetString("ip", "192.168.206.3");
        Text ipText = GameObject.Find("IP/Text").GetComponent<Text>();
        ipText.color = Color.black;
        InputField portInput = GameObject.Find("Port").GetComponent<InputField>();
        portInput.text = PlayerPrefs.GetString("port", "9090");
        imgFreq.value = PlayerPrefs.GetInt("imgfrq", 20);
        otherFreq.value = PlayerPrefs.GetInt("frq", 30);
        Toggle pub = GameObject.Find("PublisherToggle").GetComponent<Toggle>();
        pub.isOn = (PlayerPrefs.GetInt("publishers", 1) != 0);
        Dropdown serializer = GameObject.Find("SerializerSelect").GetComponent<Dropdown>();
        serializer.value = PlayerPrefs.GetInt("serializer", 0);

        cycleSettings.SetActive(true);
        InputField setTime = GameObject.Find("TimeInput").GetComponent<InputField>();
        setTime.text = PlayerPrefs.GetString("time", "12:00");
        Text timeText = GameObject.Find("TimeInput/Text").GetComponent<Text>();
        timeText.color = Color.black;
        InputField setRatio = GameObject.Find("TimeRatio").GetComponent<InputField>();
        setRatio.text = PlayerPrefs.GetFloat("ratio", 5.0f).ToString();
        
        Toggle mode;
        switch(PlayerPrefs.GetInt("time_mode", 2))
        {
            case 1:
                mode = GameObject.Find("CycleToggle").GetComponent<Toggle>();
                mode.isOn = true;
                break;
            case 2:
                mode = GameObject.Find("DayToggle").GetComponent<Toggle>();
                mode.isOn = true;
                cycleSettings.SetActive(false);
                break;
            case 3:
                mode = GameObject.Find("NightToggle").GetComponent<Toggle>();
                mode.isOn = true;
                cycleSettings.SetActive(false);
                break;
            default:
                break;
        }
        Toggle trees = GameObject.Find("TreeToggle").GetComponent<Toggle>();
        trees.isOn = (PlayerPrefs.GetInt("trees", 0) != 0);

        Toggle traffic = GameObject.Find("TrafficToggle").GetComponent<Toggle>();
        traffic.isOn = (PlayerPrefs.GetInt("traffic", 0) != 0);
        Toggle smoothMotion = GameObject.Find("SmoothToggle").GetComponent<Toggle>();
        smoothMotion.isOn = (PlayerPrefs.GetInt("smooth", 0) != 0);
    }

    void CheckForSUMO()
    {
        Toggle traffic = GameObject.Find("TrafficToggle").GetComponent<Toggle>();
        if (!traffic.isOn)
            return;

        string env_var = Environment.GetEnvironmentVariable("Path");
        if (!env_var.Contains("Sumo") && !env_var.Contains("sumo") && !env_var.Contains("SUMO"))
        {
            popup.SetActive(true);
            CanvasGroup canvas = GameObject.Find("MainMenu").GetComponent<CanvasGroup>();
            canvas.interactable = false;
        }
    }

    void ShowWarning()
    {
        Toggle smoothMotion = GameObject.Find("SmoothToggle").GetComponent<Toggle>();
        if (smoothMotion.isOn)
            warning.SetActive(true);
        else
            warning.SetActive(false);
    }

    void ClosePopup()
    {
        Toggle traffic = GameObject.Find("TrafficToggle").GetComponent<Toggle>();
        traffic.isOn = false;
        popup.SetActive(false);
        CanvasGroup canvas = GameObject.Find("MainMenu").GetComponent<CanvasGroup>();
        canvas.interactable = true;
    }

    void StartSimulator()
    {
        // save settings
        Text ipText = GameObject.Find("IP/Text").GetComponent<Text>();
        if (ipText.color == Color.red)
        {
            Debug.LogWarning("Errors have to be resolved to save Settings");
            return;
        }
        PlayerPrefs.SetString("ip", ipText.text);
        InputField portInput = GameObject.Find("Port").GetComponent<InputField>();
        PlayerPrefs.SetString("port", portInput.text);
        PlayerPrefs.SetInt("imgfrq", (int)imgFreq.value);
        PlayerPrefs.SetInt("frq", (int)otherFreq.value);
        Toggle pub = GameObject.Find("PublisherToggle").GetComponent<Toggle>();
        PlayerPrefs.SetInt("publishers", pub.isOn ? 1 : 0);
        Dropdown serializer = GameObject.Find("SerializerSelect").GetComponent<Dropdown>();
        PlayerPrefs.SetInt("serializer", serializer.value);

        ToggleGroup setTimeMode = GameObject.Find("TimeModeSelection").GetComponent<ToggleGroup>();
        Toggle setMode = setTimeMode.ActiveToggles().FirstOrDefault();
        switch(setMode.name)
        {
            case "CycleToggle":
                PlayerPrefs.SetInt("time_mode", 1);
                break;
            case "DayToggle":
                PlayerPrefs.SetInt("time_mode", 2);
                break;
            case "NightToggle":
                PlayerPrefs.SetInt("time_mode", 3);
                break;
            default:
                Debug.Log("error reading toggle group");
                break;
        }
        if (setCycle.isOn){
            Text timeText = GameObject.Find("TimeInput/Text").GetComponent<Text>();
            if (timeText.color == Color.red)
            {
                Debug.LogWarning("Set a valid start time for cycle");
                return;
            }
            PlayerPrefs.SetString("time", timeText.text);
            InputField setRatio = GameObject.Find("TimeRatio").GetComponent<InputField>();
            float ratio = float.Parse(setRatio.text, new CultureInfo("en-US"));
            PlayerPrefs.SetFloat("ratio", ratio);
        }
        Toggle trees = GameObject.Find("TreeToggle").GetComponent<Toggle>();
        PlayerPrefs.SetInt("trees", trees.isOn ? 1 : 0);

        Toggle traffic = GameObject.Find("TrafficToggle").GetComponent<Toggle>();
        PlayerPrefs.SetInt("traffic", traffic.isOn ? 1 : 0);
        Toggle smoothMotion = GameObject.Find("SmoothToggle").GetComponent<Toggle>();
        PlayerPrefs.SetInt("smooth", smoothMotion.isOn ? 1 : 0);
        
        SceneManager.LoadScene("Circuit");
    }
}
