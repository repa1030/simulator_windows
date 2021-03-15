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

            int serializer = PlayerPrefs.GetInt("serializer", 0);
            switch (serializer)
            {
                case 0:
                    ros.Serializer = RosSharp.RosBridgeClient.RosSocket.SerializerEnum.Newtonsoft_BSON;
                    break;
                case 1:
                    ros.Serializer = RosSharp.RosBridgeClient.RosSocket.SerializerEnum.Newtonsoft_JSON;
                    break;
                case 2:
                    ros.Serializer = RosSharp.RosBridgeClient.RosSocket.SerializerEnum.Microsoft;
                    break;
                default:
                    ros.Serializer = RosSharp.RosBridgeClient.RosSocket.SerializerEnum.Newtonsoft_BSON;
                    break;
            }
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
