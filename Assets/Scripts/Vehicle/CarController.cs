// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Markus Zimmermann, Patrick Rebling
// =================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using NWH.WheelController3D;

[System.Serializable]
public class NewAxis : System.Object
{
    public WheelController controllerLeft;
    public WheelController controllerRight;
    public bool enableMotor;
    public bool enableSteer;
    public enum axisType 
    {
        Front = -1, 
        Rear = 1,
        NotAssigned = 0
    }
    public axisType axisMode;
    public float brakeForceDistribution;
}

public class CarController : MonoBehaviour
{
    // Member variables
    private float m_manual_steer;
    private float m_manual_speed_ctrl;
    private float m_gas_ctrl;
    public float gas_ctrl
    {
        get {return m_gas_ctrl;}
    }
    private float m_brake_ctrl;
    public float brake_ctrl
    {
        get {return m_brake_ctrl;}
    }
    private float m_steerWheel_angle;
    private float m_max_steerWheel_angle;
    private float m_min_steerWheel_angle;
    private float m_steering_angle;
    private float m_last_steering_anlge;
    public float steering_angle
    {
        get {return m_steering_angle;}
    }
    private bool m_brake_signal;
    public bool brake_signal { get { return m_brake_signal; } }
    private bool m_driveReverse;
    private bool m_reverse_singal;
    public bool reverse_signal { get { return m_reverse_singal; } }
    private float m_axle_width;
    private float m_wheelbase;
    private Vector3 m_velocity;
    private bool m_accelerate;
    private bool m_brake;
    private bool m_handbrake;
    private bool m_lights_on;
    private bool m_lkas_active;
    private bool m_cc_active;
    private Vector3 m_centerOfMass;
    private Vector3 m_prevOffset = Vector3.zero;
    private float m_steerMin_velocity;
    private float m_A_x_cW;
    private float m_rhoAir;
    private float m_airResist;
    private float m_gravity;
    private float m_fR;
    private float m_rollResist;

    // Public variables
    [Header("Vehicle and Axis Settings ----------")]
    public GameObject steeringWheel;
    public List<NewAxis> VehicleAxis;
    public Vector3 centerOfMassOffset = Vector3.zero;
    public UIFunctions ADASControl;
    public float drag = 0.22f;
    public float rollResistFactor = 0.014f;
    [Header("Physics Configuration --------------")]
    public bool velocitySteerLimiting = true;
    public float maxSteerAngle = 29.0f; // Wendekreis 11m: ~29°
    public float minSteerAngle = 15.0f;
    public float minSteerAngleVelocity = 70.0f; //km/h
    public bool steerAngleChangeLimiting = true;
    public float maxSteerAngleChange = 1.0f;
    public float maxSteeringWheelTurns = 1.5f;
    public float maxVelocity = 100.0f;
    public float brakeForce = 1500.0f;
    public float motorForce = 1000.0f;
    public float downforce = 100.0f;
    public float antiRollBarForce = 13000.0f;


    // Getter and setter methods
    public bool LKASFlag { set { m_lkas_active = value; } }
    public bool CCFlag { set { m_cc_active = value; } }
    public float SteeringAngle { set { m_steering_angle = value; } }
    public float BrakePedal { set { m_brake_ctrl = value; } }
    public float GasPedal { set { m_gas_ctrl = value; } }
    
    // On start
    private void Start()
    {
        VehicleGeometry();
        m_centerOfMass = this.GetComponent<Rigidbody>().centerOfMass;
        m_steerWheel_angle = 0.0f;
        m_last_steering_anlge = 0.0f;
        m_steering_angle = 0.0f;
        m_min_steerWheel_angle = maxSteeringWheelTurns * 360.0f;
        m_max_steerWheel_angle = -maxSteeringWheelTurns * 360.0f;
        m_brake_signal = false;
        m_reverse_singal = false;
        m_rhoAir = 1.3f;
        m_A_x_cW = drag;
        m_airResist = 0.0f;
        m_fR = rollResistFactor;
        m_gravity = 9.81f;
        m_rollResist = 0.0f;
        m_driveReverse = false;
        m_steerMin_velocity = 1 / minSteerAngleVelocity;
    }

    // On frame update
    private void Update()
    {
        GetInput();
        if (!m_cc_active) DrivingAnalysis();
        //SetLights();
        foreach (NewAxis axis in VehicleAxis)
        {
            DrivingCommand(axis);
        }
        CenterOfMass();
    }

    private void FixedUpdate()
    {
        foreach (NewAxis axis in VehicleAxis)
        {
            Steer(axis);
            ApplyAntiRollBar(axis.controllerLeft, axis.controllerRight);
        }
        AddDownForce();
        AddDriveResist();
        SteeringWheelRotation();
    }

    private void AddDriveResist()
    {
        m_A_x_cW = drag;
        m_fR = rollResistFactor;
        m_airResist = 0.5f * m_rhoAir * m_A_x_cW * m_velocity.z * m_velocity.z * Mathf.Sign(m_velocity.z);
        if (m_velocity.z <= -0.1 || m_velocity.z >= 0.1)
        {
            m_rollResist = this.GetComponent<Rigidbody>().mass * m_fR * m_gravity * Mathf.Sign(m_velocity.z);
        }
        else
        {
            m_rollResist = 0;
        }
        this.GetComponent<Rigidbody>().AddForce(-transform.forward * (m_airResist + m_rollResist));
    }
   
    // Get input data from keyboard and from unity
    private void GetInput()
    {
        // Disable adas functions if manual input exist
        if (Input.GetAxis("Horizontal") != 0.0 && m_lkas_active)
        {
            m_lkas_active = false;
            ADASControl.LKASActive = false;
        }
        if ((Input.GetAxis("Vertical") != 0.0 || Input.GetKey("b")) && m_cc_active)
        {
            m_cc_active = false;
            ADASControl.CCActive = false;
        }
            
        // Check LKAS Bool
        if (m_lkas_active) 
        {
            // Limit Steer Angle Rate will be done in Steer()
            m_steering_angle = (m_steering_angle > maxSteerAngle) ? maxSteerAngle : m_steering_angle;
        }
        else
        {
            m_manual_steer = Input.GetAxis("Horizontal");
        }

        // Check CC Bool
        if (m_cc_active)
        {
            // Set bools based on pedal positions
            m_brake = (m_brake_ctrl > 0.0) ? true : false;
            m_accelerate = (m_gas_ctrl > 0.0) ? true : false;
        }
        else
        {
            // Get manual input
            m_manual_speed_ctrl = Input.GetAxis("Vertical");
            m_handbrake = Input.GetKey("b");
        }

        // Read velocity and convert to km/h
        m_velocity = this.transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity) * 60 * 60 / 1000;
        if (m_velocity.z < -1/3.6 && m_driveReverse)
        {
            m_reverse_singal = true;
        }
        else
        {
            m_reverse_singal = false;
        }
    }

    // Analyse the actual driving behavior if cruise control is inactive
    private void DrivingAnalysis()
    {
        // Brake
        if ((m_manual_speed_ctrl > 0 && m_velocity.z < -1.0f) || (m_manual_speed_ctrl < 0 && m_velocity.z > 1.0f))
        {
            m_accelerate = false;
            m_brake = true;
        }
        // Accelerate forwards or backwards
        else if (((m_manual_speed_ctrl > 0 && m_velocity.z >= -1.0f) || (m_manual_speed_ctrl < 0 && m_velocity.z <= 1.0f)) && m_velocity.z < maxVelocity)
        {
            m_accelerate = true;
            m_brake = false;
        }
        // Roll
        else
        {
            m_accelerate = false;
            m_brake = false;
        }
    }

    // Set steering of vehicle axis
    private void Steer(NewAxis axis)
    {
        if (axis.enableSteer)
        {
            m_steerMin_velocity = 1 / minSteerAngleVelocity; 
            if(!m_lkas_active)            
            {
                if(velocitySteerLimiting)
                {
                    m_steering_angle = Mathf.Lerp(maxSteerAngle, minSteerAngle, Mathf.Abs(m_velocity.z) * m_steerMin_velocity) * m_manual_steer;
                }
                else
                {
                    m_steering_angle = maxSteerAngle * m_manual_steer;
                }
            }
            if (steerAngleChangeLimiting)
            {
                LimitSteerAngleChange();
            }
            if (axis.axisMode == (NewAxis.axisType.Front)) // Ackermann-Steering for front axis
            {
                Ackermann(axis);
            }
            else // Default Steering for other axis - REQUIRES SPECIAL IMPLEMENTATION -
            {
                axis.controllerLeft.steerAngle = m_steering_angle;
                axis.controllerRight.steerAngle = m_steering_angle;
            }
        }
    }
    
    // Rotation of Steering Wheel Visual
    private void SteeringWheelRotation()
    {
        Vector3 actualRotation = Vector3.zero;
        m_steerWheel_angle = Mathf.Lerp(m_min_steerWheel_angle, m_max_steerWheel_angle, (m_steering_angle / maxSteerAngle) * 0.5f + 0.5f);
        actualRotation = steeringWheel.transform.localRotation.eulerAngles;
        Quaternion steerQuaternion = Quaternion.Euler(actualRotation.x, actualRotation.y, m_steerWheel_angle);
        steeringWheel.transform.localRotation = steerQuaternion;
    }

    // Steer-Angle-Change Limitation 
    private void LimitSteerAngleChange()
    {
        if ((m_steering_angle - m_last_steering_anlge) > maxSteerAngleChange)
        {
            m_steering_angle = m_last_steering_anlge + maxSteerAngleChange;
        }
        else if ((m_steering_angle - m_last_steering_anlge) < (-maxSteerAngleChange))
        {
            m_steering_angle = m_last_steering_anlge - maxSteerAngleChange;
        }
        m_last_steering_anlge = m_steering_angle;
    }
    // Ackermann-Steering for front axis
    private void Ackermann(NewAxis axis)
    {
        float steer_angle_inverse = 1 / (Mathf.Tan(Mathf.Abs(m_steering_angle) * Mathf.Deg2Rad));
        float ackermann_offset = m_axle_width / (2 * m_wheelbase);
        float steer_angle_close = Mathf.Atan(1 / (steer_angle_inverse - ackermann_offset)) * Mathf.Rad2Deg;
        float steer_angle_far = Mathf.Atan(1 / (steer_angle_inverse + ackermann_offset)) * Mathf.Rad2Deg;
        if (m_steering_angle <= 0) // Steer Left
        {
            axis.controllerLeft.steerAngle = - steer_angle_close;
            axis.controllerRight.steerAngle = - steer_angle_far;
        }
        else if (m_steering_angle > 0) // Steer Right
        {
            axis.controllerLeft.steerAngle = steer_angle_far;
            axis.controllerRight.steerAngle = steer_angle_close;
        }
    }

    private void VehicleGeometry()
    {
        Vector3 front_position = Vector3.zero;
        foreach (NewAxis axis in VehicleAxis)
        {
            if(axis.axisMode == NewAxis.axisType.Front)
            {
                front_position = axis.controllerLeft.transform.position;
                m_axle_width = (axis.controllerRight.transform.position - axis.controllerLeft.transform.position).magnitude;
            }
            else // correct for assuming that last axis of vehicle is last defined in VehicleAxis
            {
                m_wheelbase = (front_position - axis.controllerLeft.transform.position).magnitude;
            }
        }
    }
    
    // Driving command (acceleration, braking,...)
    private void DrivingCommand(NewAxis axis)
    {
        // Brake
        float brakeTorque_left = 0;
        float brakeTorque_right = 0;
        float motorTorque_left = 0;
        float motorTorque_right = 0;
        if (m_brake)
        {
            motorTorque_left = 0;
            motorTorque_right = 0;
            brakeTorque_left = (m_cc_active) ? m_brake_ctrl * brakeForce : Math.Abs(m_manual_speed_ctrl) * brakeForce;
            brakeTorque_right = (m_cc_active) ? m_brake_ctrl * brakeForce : Math.Abs(m_manual_speed_ctrl) * brakeForce;
            if (axis.axisMode == NewAxis.axisType.Front)
            {
                brakeTorque_left = brakeTorque_left * VehicleAxis.Capacity * axis.brakeForceDistribution;
                brakeTorque_right = brakeTorque_right * VehicleAxis.Capacity * axis.brakeForceDistribution;
            }
            else if (axis.axisMode == NewAxis.axisType.Rear)
            {
                brakeTorque_left = brakeTorque_left * VehicleAxis.Capacity * axis.brakeForceDistribution;
                brakeTorque_right = brakeTorque_right * VehicleAxis.Capacity * axis.brakeForceDistribution;
            }         
        }
        // Accelerate
        else if (m_accelerate && axis.enableMotor)
        {
            motorTorque_left = (m_cc_active) ? m_gas_ctrl * motorForce : m_manual_speed_ctrl * motorForce;
            motorTorque_right = (m_cc_active) ? m_gas_ctrl * motorForce : m_manual_speed_ctrl * motorForce;
        }
        // Handbrake
        if(m_handbrake)
        {
            if (axis.axisMode == NewAxis.axisType.Front)
            {
                brakeTorque_left = brakeForce * VehicleAxis.Capacity * axis.brakeForceDistribution;
                brakeTorque_right = brakeForce * VehicleAxis.Capacity * axis.brakeForceDistribution;
            }
            else if (axis.axisMode == NewAxis.axisType.Rear)
            {
                brakeTorque_left = brakeForce * VehicleAxis.Capacity * axis.brakeForceDistribution;
                brakeTorque_right = brakeForce * VehicleAxis.Capacity * axis.brakeForceDistribution;
            }            
        }
        axis.controllerLeft.brakeTorque = brakeTorque_left;
        axis.controllerRight.brakeTorque = brakeTorque_right;
        axis.controllerLeft.motorTorque = motorTorque_left;
        axis.controllerRight.motorTorque = motorTorque_right;
        if (Mathf.Abs(brakeTorque_left) + Mathf.Abs(brakeTorque_right) > 0)
        {
            m_brake_signal = true;
        }
        else
        {
            m_brake_signal = false;
        }
        if (((motorTorque_left + motorTorque_right) <= -0.02f * motorForce))
        {
            m_driveReverse = true;
        }
        else
        {
            m_driveReverse = false;
        }

    }


    private void ApplyAntiRollBar(WheelController wheelControllerLeft, WheelController wheelControllerRight)
    {
        if (!wheelControllerLeft.springOverExtended &&
            !wheelControllerLeft.springBottomedOut &&
            !wheelControllerRight.springOverExtended &&
            !wheelControllerRight.springBottomedOut)
        {
            float driverSideTravel = wheelControllerLeft.springTravel;
            float passengerSideTravel = wheelControllerRight.springTravel;
            float arf = (driverSideTravel - passengerSideTravel) * antiRollBarForce;

            if (wheelControllerLeft.isGrounded)
                wheelControllerLeft.parent.GetComponent<Rigidbody>().AddForceAtPosition(wheelControllerLeft.wheel.up * -arf, wheelControllerLeft.wheel.worldPosition);

            if (wheelControllerRight.isGrounded)
                wheelControllerRight.parent.GetComponent<Rigidbody>().AddForceAtPosition(wheelControllerRight.wheel.up * arf, wheelControllerRight.wheel.worldPosition);
        }
    }

    private void AddDownForce()
    {
        this.GetComponent<Rigidbody>().AddForce(-transform.up * downforce * this.GetComponent<Rigidbody>().velocity.magnitude);
    }

    private void CenterOfMass()
    {
        if (centerOfMassOffset != m_prevOffset) // when didnt changed no action required
        {
            this.GetComponent<Rigidbody>().centerOfMass = m_centerOfMass + centerOfMassOffset;
        }
        m_prevOffset = centerOfMassOffset;
    }
}
