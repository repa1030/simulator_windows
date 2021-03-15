// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumoInit : MonoBehaviour
{
    // Start is called before the first frame update
    // Initalize Sumo
    void Awake()
    {
        bool traffic = (PlayerPrefs.GetInt("traffic", 1) != 0);
        if (!traffic)
        {
            this.gameObject.SetActive(false);
        }
    }
}
