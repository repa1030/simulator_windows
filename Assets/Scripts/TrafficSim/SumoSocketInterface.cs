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
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class SumoSocketInterface : MonoBehaviour
{
    // public variables
    [Header("*** General Settings")]
    public float sendingFrequency = 25;
    public float sumoTimeStep = 0.04f;
    public bool useSumoAutoConnection = true;
    [Tooltip("Name of the SUMO network located in StreamingAssets/TrafficSim/SumoNetworks")]
    public string sumoNetworkName = "circuit";
    [Header("*** Ego Vehicle Settings")]
    public Transform egoVehicle;
    public float egoVehicleLength = 4.63f;
    [Header("*** WebSocket Settings")]
    public string uri = "localhost";
    public int port = 4042;

    // private members
    private WebSocket m_socket;
    private Thread m_client_thread;
    private ConcurrentQueue<string> m_socket_recv_queue;
    private ConcurrentQueue<string> m_socket_send_queue;
    private float m_next_send;
    private bool m_close_connection;
    private SumoMain m_sumo_main;
    private CarController m_car_controller;
    private CarLightController m_light_controller;
    private System.Diagnostics.Process m_python_process;
    private CultureInfo m_ci;

    // Getter
    public bool ConnectionClosed { 
        get { return m_close_connection; }
    }

    // start sumo
    private void Awake()
    {
        if (useSumoAutoConnection)
            StartSumoSimulation();
    }

    // init
    private void Start()
    {
        // connect to server
        ConnectToServer();
        // initialize variables
        m_socket_recv_queue = new ConcurrentQueue<string>();
        m_socket_send_queue = new ConcurrentQueue<string>();
        m_next_send = Time.time + (1 / sendingFrequency);
        m_close_connection = false;
        m_sumo_main = GetComponent<SumoMain>();
        m_car_controller = egoVehicle.GetComponent<CarController>();
        m_light_controller = egoVehicle.GetComponent<CarLightController>();
        m_ci = new CultureInfo("en-US");
    }

    private void Update()
    {
        // update ego vehicle position every 1/frequency seconds if m_egoData is empty
        if (Time.time >= m_next_send)
        {
            float heading = egoVehicle.rotation.eulerAngles.y;
            float posXSumo = (egoVehicle.position.x + (float)(Math.Sin(heading * Mathf.Deg2Rad) * (egoVehicleLength / 2))) + m_sumo_main.positionOffsetX;
            float posYSumo = (egoVehicle.position.z + (float)(Math.Cos(heading * Mathf.Deg2Rad) * (egoVehicleLength / 2))) + m_sumo_main.positionOffsetY;
            float currentVelo = Mathf.Abs((egoVehicle.InverseTransformDirection(egoVehicle.GetComponent<Rigidbody>().velocity)).z);   // in m/s
            string brakeState = m_car_controller.brake_signal ? "1" : "0";
            string backwards = m_car_controller.reverse_signal ? "1" : "0";
            string signals = m_light_controller.turnLightsIn.ToString();
            string data = "START@"
                    + posXSumo.ToString("0.00") + ";"
                    + posYSumo.ToString("0.00") + ";"
                    + currentVelo.ToString("0.00") + ";"
                    + heading.ToString("0.00") + ";"
                    + brakeState + ";"
                    + backwards + ";"
                    + signals + "@END";
            m_socket_send_queue.Enqueue(data);
            m_next_send = Time.time + (1 / sendingFrequency);
        }
        // Emergency brake if currently no connection to sumo
        if (m_close_connection)
            m_sumo_main.EmergencyBrakeVehicles();
    }

    // Unity simulation stop
    private void OnDestroy()
    {
        m_close_connection = true;
        if (useSumoAutoConnection) KillSumoProcess();
    }

    // starting the socket interface
    private void ConnectToServer()
    {
        m_client_thread = new Thread(() => SocketThread());
        m_client_thread.IsBackground = true;
        m_client_thread.Start();
    }

    // WebSocket client for receiving sumo data and sending ego vehicle data
    private void SocketThread()
    {
        string data;
        // Create new websocket instance
        m_socket = new WebSocket("ws://" + uri + ":" + port);
        // Add event for successfully connected socket
        m_socket.OnOpen += (sender, e) =>
        {
            Debug.Log("SUMO: Connection Established.");
        };
        // Add event for closing connection (SUMO emergency brake)
        m_socket.OnClose += (sender, e) => {
            m_close_connection = true;
            Debug.Log("SUMO: Connection Closed. Traffic Simulation Aborted.");
        };
        // Add event for incoming messages
        m_socket.OnMessage += (sender, e) =>
        {
            // Clear queue to avoid filling up
            string buf;
            m_socket_recv_queue.TryDequeue(out buf);
            // Add new element
            m_socket_recv_queue.Enqueue(e.Data);
        };
        m_socket.ConnectAsync();
        // Thread loop (sending messages)
        while (true)
        {
            if (m_close_connection) break;
            if (m_socket_send_queue.TryDequeue(out data))
            {
                m_socket.Send(data);
            }
            Thread.Sleep(5);
        }
        CloseThread();
    }

    // Closing of socket and exit thread
    private void CloseThread()
    {
        Debug.Log("SUMO: Socket Thread Destroyed.");
        m_socket.CloseAsync();
        m_client_thread.Abort();
    }

    // Function to start sumo by starting the unity simulation
    private void StartSumoSimulation()
    {
        try
        {
            // Get environment PATH variable and search for python entry
            string env_var = Environment.GetEnvironmentVariable("Path");
            if (!env_var.Contains("Python") && !env_var.Contains("python") && !env_var.Contains("PYTHON"))
                Debug.LogWarning("SUMO: No Path to Python Folder Found in 'Path' System Environment Variable. Traffic Simulation Aborted.");

            // Prepare executable and arguments for process start
            string pythonExe = "python.exe";
            string file = Application.streamingAssetsPath + "/TrafficSim/TraCI/Main.py";
            string time_step = sumoTimeStep.ToString().Replace(",", ".");
            string argument = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\"", file, uri, port, sumoNetworkName, time_step);
            // Generate windows process with start info
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(pythonExe);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = argument;

            // Start process and get process id
            m_python_process = System.Diagnostics.Process.Start(startInfo);
            Debug.Log("SUMO: TraCI Started Successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("SUMO: Unable to launch Sumo: " + e.Message + ". Traffic Simulation Aborted.");
        }
    }

    // Kill the sumo process when stopping unity simulation
    private void KillSumoProcess()
    {
        // kill python script
        if (!m_python_process.HasExited)
        {
            m_python_process.Kill();
            m_python_process.WaitForExit();
            m_python_process.Close();
        }

        // kill sumo instances
        System.Diagnostics.Process[] running = System.Diagnostics.Process.GetProcesses();
        foreach (System.Diagnostics.Process process in running)
        {
            try
            {
                if (!process.HasExited && process.ProcessName == "sumo-gui")
                {
                    process.Kill();
                    process.WaitForExit();
                    process.Close();
                }
            }
            catch (System.InvalidOperationException)
            {
                //do nothing
                Debug.LogWarning("SUMO: InvalidOperationException was caught!");
            }
        }
    }

    // Function for SumoMain to get Rx message
    public string RxMsg(ref SumoTimeInfo ref_times)
    {
        string rx;
        string incoming;
        // read the receive queue and save in m_Rx for SumoMain
        if (m_socket_recv_queue.TryDequeue(out incoming))
        {
            // check message: OG1...&
            if (incoming != null && incoming.Contains("O1G") && incoming.Contains("&"))
            {
                // delete "O1G"
                incoming = incoming.Substring(3);
                // check if message is not empty
                if (incoming.IndexOf("&") <= 0) return null;
                // split message: time_stamp#sumo_step_target#sumo_step_real#data
                string[] msgs = incoming.Split('#');
                // vehicle data string
                ref_times.time_stamp = (float)Convert.ToDouble(msgs[0], m_ci);
                ref_times.target_step_size = (float)Convert.ToDouble(msgs[1], m_ci);
                ref_times.current_step_size = (float)Convert.ToDouble(msgs[2], m_ci);
                rx = msgs[3].Substring(0, msgs[3].IndexOf("&"));
            }
            else
            {
                rx = null;
            }
        }
        else
        {
            rx = null;
        }
        return rx;
    }
}