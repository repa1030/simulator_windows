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
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RosSharp.RosBridgeClient;

public class UIFunctions : MonoBehaviour
{
    public static int ccSpeed = 70;

    [Header("Vehicle Settings")]
	public Rigidbody vehicle;
    public Transform vehicleTransform;

    [Header("Camera Settings")]
    public Camera firstPerson;
    public Camera thirdPerson;

    [Header("Cruise Controller Settings")]
    public int minSpeed = 10;
    public int maxSpeed = 140;

    // global variables Shortcuts
    private GameObject short_max;
    private GameObject short_min;
    private bool switchPOV = false;

    // global variables Time
    private DayAndNightScene dayNightScene;
    private GameObject time_max;
    private GameObject time_min;
    private GameObject current_time;
    private InputField setTime;
    private Text daytime;
    private bool timeActive = true;

    // global variables ROS status
    private bool rosActive;
    private RosConnector ros;
    private Text connectionStatus;

    // global variables ADAS
    private GameObject ccData;
    private GameObject lkasData;
    private GameObject ADASScreen;
    private Toggle cc;
    private Toggle lkas;
    private InputField setSpeed;
    public bool LKASActive { set { lkas.isOn = value; } get { return lkas.isOn; } }
    public bool CCActive { set { cc.isOn = value; } get { return cc.isOn; } }

	
    void Awake()    // set global variables
    {
        // Shortcuts
        short_max = GameObject.Find("Shortcuts_max");
        short_min = GameObject.Find("Shortcuts_min");

        // Time
        dayNightScene = GameObject.Find("World").GetComponent<DayAndNightScene>();
        time_max = GameObject.Find("Time_max");
        time_min = GameObject.Find("Time_min");
        current_time = GameObject.Find("CurrentTime");
        setTime = GameObject.Find("timeInput").GetComponent<InputField>();       
		setTime.onEndEdit.AddListener(delegate{ChangeTime();});
        setTime.text = PlayerPrefs.GetString("time", "12:00");
        daytime = GameObject.Find("Daytime").GetComponent<Text>();

        // ROS status
        rosActive = GameObject.Find("RosConnector");
        if (rosActive)
        {
            ros = GameObject.Find("RosConnector").GetComponent<RosConnector>();
        }
        connectionStatus = GameObject.Find("status").GetComponent<Text>();

        // ADAS info
        cc = GameObject.Find("CC Toggle").GetComponent<Toggle>();
        cc.onValueChanged.AddListener(delegate{ToggleValueChanged();});
        lkas = GameObject.Find("LKAS Toggle").GetComponent<Toggle>();
        setSpeed = GameObject.Find("speedInput").GetComponent<InputField>();        
		setSpeed.onEndEdit.AddListener(delegate{CheckInput();});
        setSpeed.text = ccSpeed.ToString();

        // ADAS selection
        ADASScreen = GameObject.Find("ADAS Selection");
        ccData = GameObject.Find("CC Data");
        lkasData = GameObject.Find("LKAS Data");
    }

    // Start is called before the first frame update
    void Start() 
    {   
        // connect buttons to functions
        Button minimise_short = short_max.transform.Find("Minimise").GetComponent<Button>();
        minimise_short.onClick.AddListener(MinMaxShortcuts);
        Button maximise_short = short_min.transform.Find("Maximise").GetComponent<Button>();
        maximise_short.onClick.AddListener(MinMaxShortcuts);

        Button minimise_time = time_max.transform.Find("Minimise").GetComponent<Button>();
        minimise_time.onClick.AddListener(MinMaxTime);
        Button maximise_time = time_min.transform.Find("Maximise").GetComponent<Button>();
        maximise_time.onClick.AddListener(MinMaxTime);

        Button time_select = GameObject.Find("ModeSelect").GetComponent<Button>();
        time_select.onClick.AddListener(ChangeTimeMode);
        Text select_text = GameObject.Find("TimeText").GetComponent<Text>();
        switch(PlayerPrefs.GetInt("time_mode", 2))
        {
            case 1:
                dayNightScene.dayAndNightCycle = true;
                select_text.text = "Cycle";
                current_time.gameObject.SetActive(true);
                timeActive = true;
                break;
            case 2:
                dayNightScene.dayScene = true;
                select_text.text = "Day";
                current_time.gameObject.SetActive(false);
                break;
            case 3:
                dayNightScene.nightScene = true;
                select_text.text = "Night";
                current_time.gameObject.SetActive(false);
                break;
            default:
                break;
        }

        Button reset = GameObject.Find("Reset").GetComponent<Button>();
		reset.onClick.AddListener(ResetVehicle);
        Button menu = GameObject.Find("MenuButton").GetComponent<Button>();
        menu.onClick.AddListener(OpenMainMenu);
        Button adas = GameObject.Find("Show ADAS").GetComponent<Button>();
		adas.onClick.AddListener(ToggleDisplay);

        Button plus = GameObject.Find("Speed+").GetComponent<Button>();
        plus.onClick.AddListener(IncreaseSpeed);
        Button minus = GameObject.Find("Speed-").GetComponent<Button>();
        minus.onClick.AddListener(DecreaseSpeed);

        // disable UI Elements
        short_max.SetActive(false);
        time_max.SetActive(false);
        ccData.SetActive(false);
        lkasData.SetActive(false);
        ADASScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            switchPOV = !switchPOV;
            firstPerson.enabled = switchPOV;
            thirdPerson.enabled = !switchPOV;
        }

        float velocity_kmh = vehicle.velocity.magnitude * 60 * 60 / 1000;
        float velocity_deg = velocity_kmh * -135.0f/120.0f;
        RectTransform speedo_needle = GameObject.Find("Speedo_needle").GetComponent<RectTransform>();
        speedo_needle.transform.eulerAngles = new Vector3(0f, 0f, velocity_deg - 45);

        if (cc.isOn)
        {
            ccData.SetActive(true);
        } else {
            ccData.SetActive(false);
        }

        if (lkas.isOn)
        {
            lkasData.SetActive(true);
        } else {
            lkasData.SetActive(false);
        }

        if (rosActive) 
        {
            if (ros.IsConnected.WaitOne(0) == true)
            {
                connectionStatus.text = "ROS Connected";
                connectionStatus.color = Color.green;
            } else {
                connectionStatus.text = "ROS Not Connected";
                connectionStatus.color = Color.red;
            }
        }

        if (timeActive)
        {
            int hours = (int)dayNightScene.timeOfDay;
            int minutes = (int)((dayNightScene.timeOfDay - hours) * 60);
            daytime.text = hours.ToString("D2") + ":" + minutes.ToString("D2");
        }
    }

    void MinMaxShortcuts()
    {
        if(short_max.activeSelf)
        {
            short_max.SetActive(false);
            short_min.SetActive(true);
        } else {
            short_max.SetActive(true);
            short_min.SetActive(false);
        }
    }

    void MinMaxTime()
    {
        if(time_max.activeSelf)
        {
            time_max.SetActive(false);
            time_min.SetActive(true);
        } else {
            time_max.SetActive(true);
            time_min.SetActive(false);
        }
    }

    void ChangeTimeMode()
    {
        Text select_text = GameObject.Find("TimeText").GetComponent<Text>();
        switch(select_text.text)
        {
            case "Day":
                dayNightScene.dayScene = false;
                dayNightScene.nightScene = true;
                select_text.text = "Night";
                current_time.gameObject.SetActive(false);
                break;
            case "Night":
                dayNightScene.nightScene = false;
                dayNightScene.dayAndNightCycle = true;
                select_text.text = "Cycle";
                current_time.gameObject.SetActive(true);
                timeActive = true;
                break;
            case "Cycle":
                dayNightScene.dayAndNightCycle = false;
                dayNightScene.dayScene = true;
                select_text.text = "Day";
                current_time.gameObject.SetActive(false);
                timeActive = false;
                break;
            default:
                break;
        }
    }

    void ChangeTime()
    {        
        string[] timeStrArr = setTime.text.Split(':');
        int[] timeIntArr = new int[timeStrArr.Length];
        if (timeIntArr.Length != 2)
        {
            setTime.text = "";
            return;
        } else {
            if (int.TryParse(timeStrArr[0], out timeIntArr[0]) && int.TryParse(timeStrArr[1], out timeIntArr[1]))
            {
                if (timeIntArr[0] < 0) timeIntArr[0] = 0;
                else if (timeIntArr[0] > 24) timeIntArr[0] = 24;
                if (timeIntArr[1] < 0) timeIntArr[1] = 0;
                else if (timeIntArr[1] > 59) timeIntArr[1] = 59;
                setTime.text = timeIntArr[0].ToString("D2") + ":" + timeIntArr[1].ToString("D2");
                dayNightScene.timeOfDay = timeIntArr[0] + (float)(timeIntArr[1]/60.0);
                dayNightScene.setUpCycle();
            } else {
                setTime.text = "";
                return;
            }
        }
    }

	void ResetVehicle()
	{
		vehicleTransform.eulerAngles = new Vector3(0f, vehicleTransform.eulerAngles.y, 0f); 
	}

    void OpenMainMenu()
    {        
        SceneManager.LoadScene("MainMenu");
    }

    void ToggleDisplay()
    {        
        if (ADASScreen.activeSelf)
        {
            ADASScreen.SetActive(false);
        } else {
            ADASScreen.SetActive(true);
        }
    }

    void ToggleValueChanged()
    {
        if (cc.isOn){
            ccSpeed = (int)vehicle.velocity.magnitude * 60 * 60 / 1000;
            // Round to nearest 5
            ccSpeed = 5 * (int)Math.Round(ccSpeed / 5.0);
            if (ccSpeed < minSpeed)
            {
                ccSpeed = minSpeed;
            } else if (ccSpeed > maxSpeed) {
                ccSpeed = maxSpeed;
            }
            setSpeed.text = ccSpeed.ToString();
        }
    }

    void CheckInput()
    {
        ccSpeed = int.Parse(setSpeed.text);
        if (ccSpeed < minSpeed)
        {
            ccSpeed = minSpeed;
            setSpeed.text = ccSpeed.ToString();
        } else if (ccSpeed > maxSpeed) {
            ccSpeed = maxSpeed;
            setSpeed.text = ccSpeed.ToString();
        }
    }

    void IncreaseSpeed()
    {
        ccSpeed += 5;
        if (ccSpeed > maxSpeed)
            ccSpeed = maxSpeed;

        setSpeed.text = ccSpeed.ToString();
    }

    void DecreaseSpeed()
    {
        ccSpeed -= 5;
        if (ccSpeed < minSpeed)
            ccSpeed = minSpeed;
            
        setSpeed.text = ccSpeed.ToString();
    }
}
