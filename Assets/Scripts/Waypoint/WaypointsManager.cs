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
public class WaypointsManager : MonoBehaviour
{
    // Enums
    public enum Mode
    {
        EnableEdit,
        DisableEdit
    }

    // Member variables
    private List<Transform> m_waypoints;
    private Mode m_operation_mode;
    private string m_name_prefix;
    private IconManager.Icon m_selected_icon;
    private Vector3 m_position_adder;
    private int m_count;

    // Setter and getter methods
    public Mode OperationMode
    {
        get { return m_operation_mode; }
        set { m_operation_mode = value; }
    }
    public string NamePrefix
    {
        get { return m_name_prefix; }
        set { m_name_prefix = value; }
    }
    public IconManager.Icon SelectedIcon
    {
        get { return m_selected_icon; }
        set { m_selected_icon = value; }
    }
    public int Count
    {
        get { return m_count; }
        set { m_count = value; }
    }
    public List<Transform> Waypoints
    {
        get { return m_waypoints; }
        set { m_waypoints = value; }
    }

    void Awake()
    {
        m_operation_mode = Mode.DisableEdit;
        m_name_prefix = "WP";
        m_selected_icon = IconManager.Icon.CircleGray;
        m_position_adder = Vector3.zero;
        m_count = 0;
        m_waypoints = new List<Transform>();
        ResetWaypoints();
    }

    void Update()
    {
        if (m_waypoints == null) this.Awake();
        ResetWaypoints();
    }

    void ResetWaypoints()
    {
        m_waypoints.Clear();
        foreach (Transform waypoint in transform)
            if (waypoint != transform)
                m_waypoints.Add(waypoint);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < m_waypoints.Count - 1; i++)
            Gizmos.DrawLine(m_waypoints[i].position, m_waypoints[i + 1].position);
    }
}
