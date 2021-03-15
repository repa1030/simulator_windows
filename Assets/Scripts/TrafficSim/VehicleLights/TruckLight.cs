using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckLight : LightInterface
{
    [Header("Light Objects ----------")]
    public GameObject mainLights;
    public GameObject positionLights;
    public GameObject rearLights;
    public GameObject brakeLights;
    public GameObject turnLightLeft;
    public GameObject turnLightRight;
    [Header("Light Materials ----------")]
    public Material lightOff;
    public Material lightOn;
    // Private MeshRenderer of Light Objects
    private MeshRenderer m_mainLights;
    private MeshRenderer m_positionLights;
    private MeshRenderer m_rearLights;
    private MeshRenderer m_brakeLights;
    private MeshRenderer m_turnLightRight;
    private MeshRenderer m_turnLightLeft;
    // Start is called before the first frame update
    void Awake()
    {
       // get MeshRenderer of the Light Objects
        m_mainLights = mainLights.GetComponent<MeshRenderer>();
        m_positionLights = positionLights.GetComponent<MeshRenderer>();
        m_rearLights = rearLights.GetComponent<MeshRenderer>();
        m_brakeLights = brakeLights.GetComponent<MeshRenderer>();       
        m_turnLightLeft = turnLightLeft.GetComponent<MeshRenderer>();
        m_turnLightRight = turnLightRight.GetComponent<MeshRenderer>(); 
    }

    public override void LeftIndicator(bool state)
    {
        if (state)
        {
            m_turnLightLeft.material = lightOn;
            leftIndicatorActive = true;
        }
        else
        {
            m_turnLightLeft.material = lightOff;
            leftIndicatorActive = false;
        }
    }
    public override void RightIndicator(bool state)
    {
        if (state)
        {
            m_turnLightRight.material = lightOn;
            rightIndicatorActive = true;
        }
        else
        {
            m_turnLightRight.material = lightOff;
            rightIndicatorActive = false;
        }
    }
    public override void BrakeLight(bool state)
    {
        if(state)
        {
            m_brakeLights.material = lightOn;
        }
        else
        {
            m_brakeLights.material = lightOff;
        }
    }
    public override void GeneralLight(bool state)
    {
        if(state)
        {
            m_mainLights.material = lightOn;
            m_rearLights.material = lightOn;
            m_positionLights.material = lightOn;
        }
        else
        {
            m_mainLights.material = lightOff;
            m_rearLights.material = lightOff;
            m_positionLights.material = lightOff;
        }
    }
}
