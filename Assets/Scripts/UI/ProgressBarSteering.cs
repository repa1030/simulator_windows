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

[ExecuteInEditMode]

public class ProgressBarSteering : MonoBehaviour
{

    [Header("Title Setting")]
    public string Title;
    public Color TitleColor;
    public Font TitleFont;
    public int TitleFontSize = 10;

    [Header("Bar Setting")]
    public Color TopbarColor; 
    public Color BottombarColor;  
    public Color BarBackgroundColor;
    public Sprite BarBackgroundSprite;

    private Image topbar, bottombar, barBackground;
    private Text txtTitle;
    private float barValue;
    public float BarValue
    {
        get { return barValue; }

        set
        {
            value = Mathf.Clamp(value, 0, 100);
            barValue = value;
            UpdateValue(barValue);
        }
    }

    private void Awake()
    {
        topbar = transform.Find("Topbar").GetComponent<Image>();
        bottombar = transform.Find("Bottombar").GetComponent<Image>();
        barBackground = GetComponent<Image>();
        txtTitle = transform.Find("Text").GetComponent<Text>();
        barBackground = transform.Find("BarBackground").GetComponent<Image>();
    }

    private void Start()
    {
        txtTitle.text = Title;
        txtTitle.color = TitleColor;
        txtTitle.font = TitleFont;
        txtTitle.fontSize = TitleFontSize;

        bottombar.color = BottombarColor;
        topbar.color = TopbarColor;
        barBackground.color = BarBackgroundColor; 
        barBackground.sprite = BarBackgroundSprite;

        UpdateValue(barValue);
    }

    void UpdateValue(float val)
    {
        if (val >= 50)
        {
            topbar.fillAmount = 0.5f;
            bottombar.fillAmount = val / 100;
            float degree = (val-50)*30/50;
            degree = (float)System.Math.Round(degree,2);
            txtTitle.text = Title + " " + degree + "°";
        } else {            
            topbar.fillAmount = val / 100;
            bottombar.fillAmount = 0.5f;
            float degree = (val-50)*30/50;
            degree = (float)System.Math.Round(degree,2);
            txtTitle.text = Title + " " + degree + "°";
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {           
            UpdateValue(50);
            txtTitle.color = TitleColor;
            txtTitle.font = TitleFont;
            txtTitle.fontSize = TitleFontSize;

            topbar.color = TopbarColor;
            bottombar.color = BottombarColor;
            barBackground.color = BarBackgroundColor;

            barBackground.sprite = BarBackgroundSprite;           
        }
    }
}
