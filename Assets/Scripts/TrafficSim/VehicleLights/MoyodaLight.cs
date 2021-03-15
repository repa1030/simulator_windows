using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoyodaLight : LightInterface
{
    [Header("Light Objects ----------")]
    public GameObject mainLightLeft;
    public GameObject mainLightRight;
    public GameObject rearLights;
    public GameObject brakeLights;
    public GameObject turnLightLeft;
    public GameObject turnLightRight;
    [Header("Light Materials ----------")]
    public Material lightOff;
    public Material lightOn;
    [Header("Head Lights ----------")]
    public GameObject headLightLeft;
    public GameObject headLightRight;
    // Private MeshRenderer of Light Objects
    private MeshRenderer m_mainLightLeft;
    private MeshRenderer m_mainLightRight;
    private MeshRenderer m_rearLights;
    private MeshRenderer m_brakeLights;
    private MeshRenderer m_turnLightRight;
    private MeshRenderer m_turnLightLeft;
    // Start is called before the first frame update
    void Awake()
    {
        // get MeshRenderer of the Light Objects
        m_mainLightLeft = mainLightLeft.GetComponent<MeshRenderer>();
        m_mainLightRight = mainLightRight.GetComponent<MeshRenderer>();
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
        int headLightRotation = 0;
        if(state)
        {
            m_mainLightLeft.material = lightOn;
            m_mainLightRight.material = lightOn;
            m_rearLights.material = lightOn;
            headLightRotation = -155;
        }
        else
        {
            m_mainLightLeft.material = lightOff;
            m_mainLightRight.material = lightOff;
            m_rearLights.material = lightOff;
            headLightRotation = -90;
        }
        headLightLeft.transform.localRotation = Quaternion.Euler(headLightRotation,0,3);
        headLightRight.transform.localRotation = Quaternion.Euler(headLightRotation,0,-3);
    }
}
