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

public class SensorVisualInterface : MonoBehaviour
{
    // script for test of sensor functions
    public WheelSpeedSensor wheelSpeedSensor;
    public IMUUnit imuUnit;
    public GPSUnit gPSUnit;
    private GUIStyle m_display_style;
    // Start is called before the first frame update
    void Start()
    {
        m_display_style = new GUIStyle();
        m_display_style.fontSize = 18;
        m_display_style.fontStyle = FontStyle.Bold;
        m_display_style.normal.textColor = new Color(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnGUI()
    {
        string driving_data;
        driving_data = "Wheel Speed FL: " + (wheelSpeedSensor.wheel_fl_rps*60).ToString("0.00") + "U/min\n";
        driving_data += "Wheel Speed FR: " + (wheelSpeedSensor.wheel_fr_rps*60).ToString("0.00") + "U/min\n";
        driving_data += "Wheel Speed RL: " + (wheelSpeedSensor.wheel_rl_rps*60).ToString("0.00") + "U/min\n";
        driving_data += "Wheel Speed RR: " + (wheelSpeedSensor.wheel_rr_rps*60).ToString("0.00") + "U/min\n";
        driving_data += "Gierrate: " + (imuUnit.yaw_rate).ToString("0.00") + "rad/s\n";
        driving_data += "Wankrate: " + (imuUnit.wank_rate).ToString("0.00") + "rad/s\n";
        driving_data += "Nickrate: " + (imuUnit.nick_rate).ToString("0.00") + "rad/s\n";
        driving_data += "Längsbeschleunigung: " + (imuUnit.acc_lon).ToString("0.00") + "m/s^2\n";
        driving_data += "Querbeschleunigung: " + (imuUnit.acc_lat).ToString("0.00") + "m/s^2\n";
        driving_data += "Vertikalbeschleunigung: " + (imuUnit.acc_lat).ToString("0.00") + "m/s^2\n";
        driving_data += "LON-Position: " + (gPSUnit.lon).ToString("0.000000") + "°\n";
        driving_data += "LAT-Position: " + (gPSUnit.lat).ToString("0.000000") + "°\n";
        driving_data += "ALT-Position: " + (gPSUnit.alt).ToString("0.00") + "m\n";
        //driving_data += "X-Position: " + (gPSUnit.pos_x).ToString("0.00") + "m\n";
        //driving_data += "Z-Position: " + (gPSUnit.pos_z).ToString("0.00") + "m\n";
        //driving_data += "Y-Position: " + (gPSUnit.pos_y).ToString("0.00") + "m\n";
        GUI.Label(new Rect(800, 10, 100, 20), driving_data, m_display_style);
    }
}
