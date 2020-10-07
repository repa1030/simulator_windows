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

public class SetBarValues : MonoBehaviour
{
    public ProgressBar gas;
    public ProgressBar brake;
    public ProgressBarSteering steering;

    private CarController car;

    // Start is called before the first frame update
    void Start()
    {
        car = GameObject.Find("base_link").GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        gas.BarValue = Mathf.Round(car.gas_ctrl * 100);
        brake.BarValue = Mathf.Round(car.brake_ctrl * 100);
        steering.BarValue = (float)Math.Round(car.steering_angle * 50/30 + 50, 2);
    }
}
