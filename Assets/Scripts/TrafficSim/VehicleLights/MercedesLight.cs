using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercedesLight : LightInterface
{
    [Header("Light Objects ----------")]
    public GameObject mainLights;
    public GameObject rearLights;
    public GameObject brakeLights;
    public GameObject turnLightFrontLeft;
    public GameObject turnLightFrontRight;
    public GameObject turnLightRearLeft;
    public GameObject turnLightRearRight;
    [Header("Light Materials ----------")]
    // General ON & OFF
    public Material lightOff;
    public Material lightOn;
    public Material dayLightOn;
    // Brake Light
    public Material brakeLightOn;
    // Turn Light Full Size (Object: RevereseLight)
    public Material turnLightFrontOn;
    public Material rearTurnLightOn;

    // Private MeshRenderer of Light Objects
    private MeshRenderer m_mainLights;
    private MeshRenderer m_rearLights;
    private MeshRenderer m_brakeLights;
    private MeshRenderer m_turnLightFrontLeft;
    private MeshRenderer m_turnLightFrontRight;
    private MeshRenderer m_turnLightRearLeft;
    private MeshRenderer m_turnLightRearRight;
    // Private Materials of LightObjects
    private Material m_lightOff;
    private Material m_lightOn;
// Start is called before the first frame update
    void Awake()
    {
        // get MeshRenderer of the Light Objects
        m_mainLights = mainLights.GetComponent<MeshRenderer>();
        m_rearLights = rearLights.GetComponent<MeshRenderer>();
        m_brakeLights = brakeLights.GetComponent<MeshRenderer>();       
        m_turnLightFrontLeft = turnLightFrontLeft.GetComponent<MeshRenderer>();
        m_turnLightFrontRight = turnLightFrontRight.GetComponent<MeshRenderer>();
        m_turnLightRearLeft = turnLightRearLeft.GetComponent<MeshRenderer>();
        m_turnLightRearRight = turnLightRearRight.GetComponent<MeshRenderer>();
        m_lightOff = lightOff;
        m_lightOn = lightOn;
    }

    public override void LeftIndicator(bool state)
    {
        if (state)
        {
            m_turnLightFrontLeft.material = turnLightFrontOn;
            m_turnLightRearLeft.material = rearTurnLightOn;
            leftIndicatorActive = true;
        }
        else
        {
            m_turnLightFrontLeft.material = dayLightOn;
            m_turnLightRearLeft.material = lightOff;
            leftIndicatorActive = false;
        }
    }
    public override void RightIndicator(bool state)
    {
        if (state)
        {
            m_turnLightFrontRight.material = turnLightFrontOn;
            m_turnLightRearRight.material = rearTurnLightOn;
            rightIndicatorActive = true;
        }
        else
        {
            m_turnLightFrontRight.material = dayLightOn;
            m_turnLightRearRight.material = lightOff;
            rightIndicatorActive = false;
        }
    }
    public override void BrakeLight(bool state)
    {
        if(state)
        {
            m_brakeLights.material = brakeLightOn;
        }
        else
        {
            m_brakeLights.material = m_lightOff;
        }
    }
    public override void GeneralLight(bool state)
    {
        if(state)
        {
            m_mainLights.material = lightOn;
            m_rearLights.material = lightOn;
        }
        else
        {
            m_mainLights.material = lightOff;
            m_rearLights.material = lightOff;
        }
    }
}
