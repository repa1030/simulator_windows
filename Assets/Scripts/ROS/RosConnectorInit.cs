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

public class RosConnectorInit : MonoBehaviour
{
    // Start is called before the first frame update
    // Initalize RosConnector
    void Awake()
    {
        bool publishing =  (PlayerPrefs.GetInt("publishers", 1) != 0);
        if (publishing)
        {
            RosSharp.RosBridgeClient.RosConnector ros = GetComponent<RosSharp.RosBridgeClient.RosConnector>();
            string IP = PlayerPrefs.GetString("ip", "192.168.0.3");
            string Port = PlayerPrefs.GetString("port", "9090");
            ros.RosBridgeServerUrl = "ws://" + IP + ":" + Port;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
