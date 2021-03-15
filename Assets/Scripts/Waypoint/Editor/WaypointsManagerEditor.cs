// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointsManager))]
public class WaypointsManagerEditor : Editor
{
    void OnSceneGUI()
    {
        WaypointsManager wpm = (WaypointsManager)target;
        // Stop if disabled
        if (wpm.OperationMode == WaypointsManager.Mode.DisableEdit)
            return;
        // Check for left click and get raycast hit
        Event e = Event.current;
        RaycastHit hitInfo;

        if (e.type == EventType.MouseUp && e.button == 1 )
        {
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(worldRay, out hitInfo))
                AddWaypoint(wpm, hitInfo);
        }
    }

    public override void OnInspectorGUI()
    {
        WaypointsManager wpm = (WaypointsManager)target;

        // Enabled editing
        if (wpm.OperationMode == WaypointsManager.Mode.EnableEdit)
        {
            // Enable/Disable
            if (GUILayout.Button("Disable Editing"))
            {
                wpm.OperationMode = WaypointsManager.Mode.DisableEdit;
            }
            // Prefix settings
            wpm.NamePrefix = EditorGUILayout.TextField("Name Prefix", wpm.NamePrefix);
            // Icon settings
            wpm.SelectedIcon = (IconManager.Icon)EditorGUILayout.EnumPopup("Icons", wpm.SelectedIcon);
            // Reset count button
            if (GUILayout.Button("Reset Count"))
                wpm.Count = 0;
            // Set icon button
            if (GUILayout.Button("Set All Icons to Current Selection"))
                SetAllIconsToCurrent(wpm);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(">> Right Click in Scene to Add Waypoint <<");
        }
        // Disabled editing
        else if (wpm.OperationMode == WaypointsManager.Mode.DisableEdit)
        {
            if (GUILayout.Button("Enable Editing"))
            {
                wpm.OperationMode = WaypointsManager.Mode.EnableEdit;
            }
        }
    }

    private void AddWaypoint(WaypointsManager wpm, RaycastHit hitInfo)
    {
        // Generate new waypoint
        GameObject waypointInstance = new GameObject();
        waypointInstance.transform.position = hitInfo.point;
        waypointInstance.transform.parent = Selection.activeGameObject.transform;
        waypointInstance.name = wpm.NamePrefix + "_" + wpm.Count;
        wpm.Count++;
        IconManager.SetIcon(waypointInstance, wpm.SelectedIcon);
    }

    private void SetAllIconsToCurrent(WaypointsManager wpm)
    {
        for (int i = 0; i < wpm.Waypoints.Count; i++)
        {
            IconManager.SetIcon(wpm.Waypoints[i].gameObject, wpm.SelectedIcon);
        }
    }
}