// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public class SumoMain : MonoBehaviour
{
    // Enums
    public enum UpdateBehaviour
    {
        Update,
        FixedUpdate
    }

    // public variables
    [Header("*** Vehicle Types Settings")]
    public string egoVehicleID = "ego_vehicle";
    public Transform vehicleTypes;
    [Header("*** Traffic Settings")]
    public Transform traffic;
    [Header("*** Map Settings")]
    public float positionOffsetX = 0.0f;
    public float positionOffsetY = 0.0f;
    public float sunDawnAngle = 25.0f;
    [Header("*** Visualization Settings")]
    public bool useVehicleAltitude = false;
    public UpdateBehaviour transformUpdate = UpdateBehaviour.FixedUpdate;
    [Tooltip("Warning: This might cause transform delays between Unity and Sumo")]
    public bool smoothTransform = false;
    [Range(0.0f, 20.0f)]
    public float smoothFactor = 10.0f;
 
    // private member variables
    private SumoSocketInterface m_socketInterface;
    private SumoTimeInfo m_timeInformation;
    private HashSet<string> m_trafficList;
    private HashSet<string> m_trafficListOld;
    private Dictionary<string, CarInfo> m_carInfoDict;
    private Dictionary<string, GameObject> m_carObjectDictionary;
    private Dictionary<string, Transform> m_vehTypes;
    private CultureInfo m_ci;

    private void Start()
    {
        m_socketInterface = GetComponent<SumoSocketInterface>();
        m_timeInformation = new SumoTimeInfo(0.0f, 0.0f, 0.0f);
        m_trafficList = new HashSet<string>();
        m_trafficListOld = new HashSet<string>();
        m_carInfoDict = new Dictionary<string, CarInfo>();
        m_carObjectDictionary = new Dictionary<string, GameObject>();
        m_vehTypes = new Dictionary<string, Transform>();
        m_ci = new CultureInfo("en-US");
        smoothTransform = (PlayerPrefs.GetInt("smooth", 1) != 0);
        foreach (Transform car in vehicleTypes)
        {
            m_vehTypes.Add(car.gameObject.name, car);
        }
    }

    private void FixedUpdate()
    {
        if (transformUpdate == UpdateBehaviour.Update) return;
        // get the incoming data from websocket script
        string rx = m_socketInterface.RxMsg(ref m_timeInformation);
        if (rx != null)
        {
            ProcessData(rx);
        }
        if (!m_socketInterface.ConnectionClosed)
            ApplyData(Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (transformUpdate == UpdateBehaviour.FixedUpdate) return;
        // get the incoming data from websocket script
        string rx = m_socketInterface.RxMsg(ref m_timeInformation);
        if (rx != null)
        {
            ProcessData(rx);
        }
        if (!m_socketInterface.ConnectionClosed)
            ApplyData(Time.deltaTime);
    }

    // Split incoming string per vehicle and process
    private void ProcessData(string message)
    {
        // @ is the separator between vehicles
        if (!message.Contains("@")) return;

        message = message.Remove(message.Length - 1);
        m_trafficList.Clear();
        string[] dataPerVehicle = message.Split('@');
        // Go through all received data
        foreach (string carInfo in dataPerVehicle)
        {
            // Creating a CarInfo class with name car
            CarInfo car = new CarInfo(carInfo, m_ci);
            m_trafficList.Add(car.vehid);
            // Vehicle is new and has to be instantiated
            if (!m_carInfoDict.ContainsKey(car.vehid) && car.vehid != egoVehicleID)
            {
                // Fill up dictionary, create Gameobject
                Transform vehTypTF = m_vehTypes[car.vehid.Split('.')[0]];
                SumoVehicleHandler sumoVehicleHandler = vehTypTF.GetComponent<SumoVehicleHandler>();
                // Convert to Unity coordinate frame
                car.pos_target.x = (float)(car.pos_target.x - (Math.Sin(car.heading*Mathf.Deg2Rad) * (car.length/2-sumoVehicleHandler.TransformPointOffset.z)) - positionOffsetX);
                car.pos_target.z = (float)(car.pos_target.z - (Math.Cos(car.heading*Mathf.Deg2Rad) * (car.length/2-sumoVehicleHandler.TransformPointOffset.z)) - positionOffsetY);
                if (useVehicleAltitude)
                    car.pos_target.y = (float)(car.pos_target.y + sumoVehicleHandler.TransformPointOffset.y);
                else
                    car.pos_target.y = 0.0f;
                car.pos_origin = car.pos_target;
                // Time information for interpolation
                car.step = m_timeInformation.current_step_size;
                // Instantiate new car of requested type
                Transform newCar = Instantiate(vehTypTF, car.pos_origin, car.rot_origin, traffic);
                newCar.name = car.vehid;
                // Add car for tracking to dictionaries
                m_carInfoDict.Add(car.vehid, car);
                m_carObjectDictionary.Add(car.vehid, newCar.gameObject);
                sumoVehicleHandler.lightOnSunAngle = sunDawnAngle;
            }
            // Vehicle already exists and has to be updated
            else if (m_carInfoDict.ContainsKey(car.vehid) && car.vehid != null)
            {
                GameObject tmpCar;
                if(!m_carObjectDictionary.TryGetValue(car.vehid, out tmpCar)) continue;
                SumoVehicleHandler sumoVehicleHandler = tmpCar.GetComponent<SumoVehicleHandler>();
                // Convert to Unity coordinate frame
                car.pos_target.x = (float)(car.pos_target.x - (Math.Sin(car.heading*Mathf.Deg2Rad) * (car.length/2-sumoVehicleHandler.TransformPointOffset.z)) - positionOffsetX);
                car.pos_target.z = (float)(car.pos_target.z - (Math.Cos(car.heading*Mathf.Deg2Rad) * (car.length/2-sumoVehicleHandler.TransformPointOffset.z)) - positionOffsetY);
                if (useVehicleAltitude)
                    car.pos_target.y = (float)(car.pos_target.y + sumoVehicleHandler.TransformPointOffset.y);
                car.pos_origin = m_carObjectDictionary[car.vehid].transform.position;
                car.rot_origin = m_carObjectDictionary[car.vehid].transform.rotation;
                // Time information for interpolation
                car.step = m_timeInformation.current_step_size;
                // Update dicitonary
                m_carInfoDict[car.vehid] = car;
            }
        }
        // Delete despawned vehicles
        foreach (string vehicle in m_trafficListOld)
        {
            // Vehicle despawned
            if (!m_trafficList.Contains(vehicle))
            {
                Destroy(GameObject.Find(traffic.name + "/" + vehicle));
                m_carObjectDictionary.Remove(vehicle);
                m_carInfoDict.Remove(vehicle);
            }
        }
        // Copy trafficList to trafficListOld
        m_trafficListOld.Clear();
        foreach (string vehicle in m_trafficList)
        {
            m_trafficListOld.Add(vehicle);
        }
    }

    // Apply data and interpolate the vehicle locations
    private void ApplyData(float delta)
    {
        foreach (CarInfo vehObj in m_carInfoDict.Values) //running through all vehicle
        {
            // Try to get gameObject of specific vehicle
            GameObject tmpCar;
            if (!m_carObjectDictionary.TryGetValue(vehObj.vehid, out tmpCar)) continue;

            // Get Sumo vehicle handler
            SumoVehicleHandler sumoVehicleHandler = tmpCar.GetComponent<SumoVehicleHandler>();

            // Position and rotation lerp
            vehObj.lerp += delta / vehObj.step;
            Vector3 current = tmpCar.transform.position;

            // Apply position and rotation by lerping
            if (smoothTransform)
            {
                tmpCar.transform.position = Vector3.MoveTowards(current, Vector3.Lerp(current, vehObj.pos_target, smoothFactor * delta), 1000f);
                tmpCar.transform.rotation = Quaternion.Lerp(tmpCar.transform.rotation, vehObj.rot_target, smoothFactor * delta);
            }
            else
            {
                tmpCar.transform.position = Vector3.MoveTowards(current, Vector3.Lerp(vehObj.pos_origin, vehObj.pos_target, vehObj.lerp), 1000f);
                tmpCar.transform.rotation = Quaternion.Lerp(vehObj.rot_origin, vehObj.rot_target, vehObj.lerp);
            }
            
            float speed = (float)Math.Round(vehObj.speed, 2);

            // Set Sumo vehicle handler data
            sumoVehicleHandler.Velocity = speed;
            sumoVehicleHandler.CalculateSteering(vehObj.heading, m_timeInformation.current_step_size);
            sumoVehicleHandler.BrakeLightSwitch(vehObj.brakestate);
            sumoVehicleHandler.DirectionIndicatorSwitch(vehObj.blinkerstate);
        }
    }

    // emergency brake if connection lost
    public void EmergencyBrakeVehicles()
    {
        foreach (CarInfo vehObj in m_carInfoDict.Values) // running through all vehicle
        {
            GameObject tmpCar;
            if (!m_carObjectDictionary.TryGetValue(vehObj.vehid, out tmpCar)) continue;
            SumoVehicleHandler sumoVehicleHandler = tmpCar.GetComponent<SumoVehicleHandler>();
            if (sumoVehicleHandler != null)
            {
                sumoVehicleHandler.BrakeLightSwitch(true);
                sumoVehicleHandler.DirectionIndicatorSwitch(3);
                sumoVehicleHandler.Velocity = 0.0f;
            }
        }
    }
}