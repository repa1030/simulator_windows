// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System;
using System.Globalization;
using UnityEngine;

// Sumo time info class
public class SumoTimeInfo
{
    public float time_stamp;
    public float target_step_size;
    public float current_step_size;

    public SumoTimeInfo(float x, float y, float z)
    {
        time_stamp = x;
        target_step_size = y;
        current_step_size = z;
    }

    public float GetSingleStepDelay()
    {
        return target_step_size - current_step_size;
    }
}

// Sumo car info class (incoming string)
public class CarInfo
{
    public string vehid;            // Sumo vehicle id
    public float lerp;
    public float step;              // Time for interpolation from pos_origin to pos_target
    public Vector3 pos_target;      // Target Position after step time 
    public Vector3 pos_origin;      // Origin Position at beginning
    public Quaternion rot_target;   // Target rotation after step time
    public Quaternion rot_origin;   // Origin rotation at beginning
    public float heading;           // Heading for steering angle
    public float speed;             // Vehicle speed from sumo (might differ to Unity Speed)
    public float length;            // Vehicle length (for coord transform)
    public int blinkerstate;        // 0->none, 1->right, 2->left
    public bool brakestate;         // bool for brake


    public CarInfo(string txt, CultureInfo ci)
    {
        if (!txt.Contains(";")) return;
        string[] a = txt.Split(';');
        if (a.Length >= 9)
        {
            vehid = a[0];
            lerp = 0.0f;
            step = 0.0f;
            pos_target.x = (float)Convert.ToDouble(a[1], ci);
            pos_target.z = (float)Convert.ToDouble(a[2], ci);
            pos_target.y = (float)Convert.ToDouble(a[3], ci);
            pos_origin = pos_target;
            speed = (float)Convert.ToDouble(a[4], ci);
            heading = (float)Convert.ToDouble(a[5], ci);
            rot_target = Quaternion.AngleAxis(heading, new Vector3(0, 1, 0));
            rot_origin = rot_target;
            length = (float)Convert.ToDouble(a[6], ci);
            if (Int32.Parse(a[7]) == 1)
                brakestate = true;
            else
                brakestate = false;
            blinkerstate = Int32.Parse(a[8]);  
        }
        else
        {
            Debug.Log("SUMO: Incorrect Incoming Message Format");
        }
    }
}