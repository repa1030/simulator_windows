// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointsExport : MonoBehaviour
{
    // private members
    private string m_file_path;
    private string m_file_name;
    private bool m_closed_circuit;
    private bool m_optimize;
    private bool m_sumo_error;
    private List<WaypointsManager> m_wp_managers;
    private Dictionary<WaypointsManager, string> m_lane_info;

    // Getter and setter for private members
    public string FilePath
    {
        get { return m_file_path; }
        set { m_file_path = value; }
    }
    public string FileName
    {
        get { return m_file_name; }
        set { m_file_name = value; }
    }
    public bool ClosedCircuit
    {
        get { return m_closed_circuit; }
        set { m_closed_circuit = value; }
    }
    public bool Optimize
    {
        get { return m_optimize; }
        set { m_optimize = value; }
    }
    public bool SumoError
    {
        get { return m_sumo_error; }
        set { m_sumo_error = value; }
    }
    public List<WaypointsManager> WPManagers
    {
        get { return m_wp_managers; }
    }
    public Dictionary<WaypointsManager, string> LaneInfo
    {
        get { return m_lane_info; }
        set { m_lane_info = value; }
    }

    void Awake()
    {
        // Initialize with default values
        string s = Application.dataPath.ToString();
        m_file_path = s.Substring(0, s.Length - 6);
        m_file_name = "Waypoints";
        m_closed_circuit = true;
        m_optimize = false;

        // Get current WaypointsManagers
        m_wp_managers = new List<WaypointsManager>();
        m_lane_info = new Dictionary<WaypointsManager, string>();
        ReadWaypointManagers();
    }

    void Update()
    {
        // Reinitialize if list or dictionary is null
        if (m_lane_info == null || m_wp_managers == null)
            this.Awake();
        ReadWaypointManagers();
    }

    private void ReadWaypointManagers()
    {
        m_wp_managers.Clear();
        foreach (Transform child in transform)
        {
            if (child != transform)
            {
                WaypointsManager wpm = child.GetComponent<WaypointsManager>();
                if (wpm == null)
                {
                    Debug.LogWarning("Waypoint Export: There is child in \"Waypoints\" without WaypointsManager!");
                    continue;
                }
                m_wp_managers.Add(wpm);
                if (m_lane_info.ContainsKey(wpm)) continue;
                m_lane_info[wpm] = "3.83, 13.89; 3.75, 13.89; 3.40, 13.89";
            }
        }
    }

    public void SetAllToDefault()
    {
        this.Awake();
    }
}
