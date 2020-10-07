using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int targetFPS = 150;
    
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }
    
    void Update()
    {
        if(Application.targetFrameRate != targetFPS)
            Application.targetFrameRate = targetFPS;
    }
}
