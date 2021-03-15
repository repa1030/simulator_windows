using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LightInterface : MonoBehaviour
{
    public abstract void LeftIndicator(bool state);
    public abstract void RightIndicator(bool state);
    public abstract void BrakeLight(bool state);
    public abstract void GeneralLight(bool state);
    public bool leftIndicatorActive;
    public bool rightIndicatorActive;
}
