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

    [Header("UI Settings")]
    public GameObject ccData;
    public GameObject lkasData;
    public GameObject ADASScreen;

    [Header("Camera Settings")]
    public Camera firstPerson, thirdPerson;

    [Header("Cruise Controller Settings")]
    public int minSpeed = 10;
    public int maxSpeed = 140;

    private bool switchPOV = false;
	
    private RectTransform speedo_needle;
    private Toggle cc;
    private Toggle lkas;
    private InputField setSpeed;
    
    private bool rosActive;
    private RosConnector ros;
    private Text connectionStatus;

    public bool LKASActive { set { lkas.isOn = value; } get { return lkas.isOn; } }
    public bool CCActive { set { cc.isOn = value; } get { return cc.isOn; } }
	
    // Start is called before the first frame update
    void Start()
    {
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

        speedo_needle = GameObject.Find("Speedo_needle").GetComponent<RectTransform>();

        cc = GameObject.Find("CC Toggle").GetComponent<Toggle>();
        cc.onValueChanged.AddListener(delegate{ToggleValueChanged();});
        lkas = GameObject.Find("LKAS Toggle").GetComponent<Toggle>();
        setSpeed = GameObject.Find("speedInput").GetComponent<InputField>();        
		setSpeed.onEndEdit.AddListener(delegate{CheckInput();});
        setSpeed.text = ccSpeed.ToString();

        rosActive = GameObject.Find("RosConnector");
        if (rosActive)
        {
            ros = GameObject.Find("RosConnector").GetComponent<RosConnector>();
        }
        connectionStatus = GameObject.Find("status").GetComponent<Text>();
        
        ADASScreen.SetActive(false);
        ccData.SetActive(false);
        lkasData.SetActive(false);

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
