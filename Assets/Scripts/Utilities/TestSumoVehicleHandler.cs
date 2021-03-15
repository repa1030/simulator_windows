using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSumoVehicleHandler : MonoBehaviour
{
    public SumoVehicleHandler sumoVehicle = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("x"))
        {
            sumoVehicle.BrakeLightSwitch(true);
        }
        else
        {
            sumoVehicle.BrakeLightSwitch(false);
        }
        
        
        if (Input.GetKey("m"))
        {
            sumoVehicle.DirectionIndicatorSwitch(1);
        }
        else if (Input.GetKey("n"))
        {
            sumoVehicle.DirectionIndicatorSwitch(2);
        }
        else
        {
             sumoVehicle.DirectionIndicatorSwitch(0);
        }
    }

}
