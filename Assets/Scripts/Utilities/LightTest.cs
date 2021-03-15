using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTest : MonoBehaviour
{
    public LightInterface carLight;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("y"))
        {
            carLight.GeneralLight(true);
        }
        else
        {
            carLight.GeneralLight(false);
        }
        if (Input.GetKey("x"))
        {
            carLight.BrakeLight(true);
        }
        else
        {
            carLight.BrakeLight(false);
        }
        if (Input.GetKey("m"))
        {
            carLight.LeftIndicator(true);
        }
        else
        {
            carLight.LeftIndicator(false);
        }
        if (Input.GetKey("n"))
        {
            carLight.RightIndicator(true);
        }
        else
        {
            carLight.RightIndicator(false);
        }
    }
}
