// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

[CustomEditor(typeof(WaypointsExport))]
public class WaypointsExportEditor : Editor
{
    private Dictionary<WaypointsManager, string> m_lane_info = new Dictionary<WaypointsManager, string>();

    public override void OnInspectorGUI()
    {
        // Help box strings
        string sumo_error_str = "Export as SUMO Network requires installation of SUMO! " +
            "Export was aborted.";
        string optimize_warn = "This might cause trouble with geometries in SUMO " +
            "but will increase performance of vehicle visualization.";
        string lane_option_info = "The entry \"3.00, 1.0; 2.00, 2.0\"\nwill define a road with 2 lanes where " +
           "the left lane is 3.00 m wide with a speedlimit of 1.0 m/s and the right lane is 2.00 m wide with " +
           "a limit of 2.0 m/s. There is currently no limit of the number of lanes.";

        // Get WaypointsExport instance
        WaypointsExport wp_export = (WaypointsExport)target;

        // File export options
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("File Export Options", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        wp_export.FilePath = EditorGUILayout.TextField("Export Path Name", wp_export.FilePath);
        wp_export.FileName = EditorGUILayout.TextField("File Name Prefix", wp_export.FileName);
        wp_export.ClosedCircuit = EditorGUILayout.Toggle("Closed Circuit", wp_export.ClosedCircuit);
        wp_export.Optimize = EditorGUILayout.Toggle("Optimize Network", wp_export.Optimize);
        if (wp_export.Optimize)
            EditorGUILayout.HelpBox(optimize_warn, MessageType.Warning, true);

        // Lane export information
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lane Export Information", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lane_option_info, MessageType.Info, true);
        m_lane_info = wp_export.LaneInfo;
        foreach (WaypointsManager wpm in wp_export.WPManagers)
        {
            EditorGUILayout.LabelField("Lane Info of " + wpm.gameObject.name);
            m_lane_info[wpm] = EditorGUILayout.TextField(m_lane_info[wpm]);
            if (GUILayout.Button("Change Driving Direction of " + wpm.gameObject.name))
            {
                ChangeDrivingDirection(wp_export, wpm);
                Debug.Log("Waypoint Export: Changed Driving Direction of " + wpm.gameObject.name);
            }
        }
        wp_export.LaneInfo = m_lane_info;

        // Utilities of waypoints export
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (GUILayout.Button("Clean Waypoint Coordinates"))
        {
            ResetCoordinateFrames(wp_export);
            CleanWaypointNaming(wp_export);
            Debug.Log("Waypoint Export: Reset Coordinate Frames And Cleaned Waypoint Naming");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Set File Path to Project Directory"))
        {
            wp_export.FilePath = Application.dataPath.ToString();
            wp_export.FilePath = wp_export.FilePath.Replace("/", "\\");
            // Remove "Assets"
            wp_export.FilePath = wp_export.FilePath.Substring(0, wp_export.FilePath.Length - 6);
            Debug.Log("Waypoint Export: File Path Set to " + wp_export.FilePath);
        }
        if (GUILayout.Button("Set File Name to Default"))
        {
            wp_export.FileName = "Waypoints";
            Debug.Log("Waypoint Export: File Name Prefix Set to " + wp_export.FileName);
        }
        if (GUILayout.Button("Set All Options to Default"))
        {
            wp_export.SetAllToDefault();
            Debug.Log("Waypoint Export: Options Set to Default");
        }

        // Export buttons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
        if (wp_export.SumoError)
            EditorGUILayout.HelpBox(sumo_error_str, MessageType.Error, true);
        EditorGUILayout.Space();
        if (GUILayout.Button("Save Waypoints to Text Files"))
        {
            ExportAsFile(wp_export);
            Debug.Log("Waypoint Export: File(s) Successfully Exported to " + wp_export.FilePath);
        }
        if (GUILayout.Button("Save Waypoints to SUMO Network"))
        {
            // Get environment PATH variable and search for sumo entry
            string env_var = Environment.GetEnvironmentVariable("Path");
            if (!env_var.Contains("Sumo") && !env_var.Contains("sumo") && !env_var.Contains("SUMO"))
            {
                wp_export.SumoError = true;
            }
            else
            {
                wp_export.SumoError = false;
                ExportAsSumoNetwork(wp_export);
                Debug.Log("Waypoint Export: SUMO Network Successfully Exported to " + wp_export.FilePath);
            }
        }
    }

    private void ChangeDrivingDirection(WaypointsExport wp_export, WaypointsManager wpm)
    {
        int length = wpm.Waypoints.Count;
        int sibling_idx = 0;
        List<Transform> new_wps = new List<Transform>();
        for (int i = length-1; i > 0; i--)
        {
            new_wps.Add(wpm.Waypoints[i]);
            wpm.Waypoints[i].SetSiblingIndex(sibling_idx);
            sibling_idx++;
        }
        wpm.Waypoints = new_wps;
        CleanWaypointNaming(wp_export);
    }

    private void ResetCoordinateFrames(WaypointsExport wp_export)
    {
        foreach (WaypointsManager wpm in wp_export.WPManagers)
        {
            // Get current transform
            Vector3 tf_parent = wpm.transform.localPosition;

            // Set position of parent to 0, 0, 0
            wpm.transform.localPosition = new Vector3(0f, 0f, 0f);
            wpm.transform.localRotation = Quaternion.identity;

            // Move every child relativ to parents origin position
            foreach (Transform child in wpm.transform)
            {
                // Ignore the parent transform
                if (child == wpm.transform) continue;

                // Set new positions and rotations
                child.localPosition = child.localPosition + tf_parent;
                child.localRotation = Quaternion.identity;
            }
        }
    }

    private void CleanWaypointNaming(WaypointsExport wp_export)
    {
        foreach (WaypointsManager wpm in wp_export.WPManagers)
        {
            // Get current transform
            Transform tf_parent = wpm.transform;

            // Move every child relativ to parents origin position
            int i = 0;
            foreach (Transform child in tf_parent)
            {
                // Ignore the parent transform
                if (child == wpm.transform) continue;

                // Set new positions and rotations
                string wp_prefix = child.gameObject.name;
                int index = wp_prefix.IndexOf("_");
                if (index < 0) continue;
                wp_prefix = wp_prefix.Substring(0, index);
                child.gameObject.name = wp_prefix + "_" + i;

                i += 1;
            }
        }
    }

    private void ExportAsFile(WaypointsExport wp_export)
    {
        // Todo: save waypoints in seperate .csv or text files
        foreach (WaypointsManager wpm in wp_export.WPManagers)
        {
            StringBuilder sbOutput_csv = new StringBuilder();

            // Add "/" to file path if missing
            string last_char = wp_export.FilePath.Substring(wp_export.FilePath.Length - 1);
            if (last_char != "/" && last_char != "\\")
            wp_export.FilePath += "/";
            wp_export.FilePath = wp_export.FilePath.Replace("/", "\\");

            // Filepath
            string strFilePath_csv =  wp_export.FilePath + wp_export.FileName + "_" + wpm.gameObject.name + ".csv";
            sbOutput_csv.AppendLine("x,y,z");

            // Strings of x, y and z positions
            string wpx, wpy, wpz;

            for (int i = 0; i < wpm.Waypoints.Count; i++)
            {
                // positions
                wpx = Convert.ToString(wpm.Waypoints[i].position.x).Replace(",", ".");
                wpy = Convert.ToString(wpm.Waypoints[i].position.y).Replace(",", ".");
                wpz = Convert.ToString(wpm.Waypoints[i].position.z).Replace(",", ".");

                sbOutput_csv.AppendLine(wpx + "," + wpy + "," + wpz);
            }

            File.WriteAllText(strFilePath_csv, sbOutput_csv.ToString());

        }
    }

    private void ExportAsSumoNetwork(WaypointsExport wp_export)
    {
        // Strings of x and y positions
        string wpx, wpy, wpz;

        // Add "/" to file path if missing
        string last_char = wp_export.FilePath.Substring(wp_export.FilePath.Length - 1);
        if (last_char != "/" && last_char != "\\")
            wp_export.FilePath += "/";
        wp_export.FilePath = wp_export.FilePath.Replace("/", "\\");

        // Initialize nodes file
        string strFilePath_nodes = wp_export.FilePath + wp_export.FileName + ".nod.xml";
        StringBuilder sbOutput_nodes = new StringBuilder();
        sbOutput_nodes.AppendLine("<nodes>");

        // Counting variable for WaypointsManager instances
        int wpm_instance = 0;

        foreach (WaypointsManager wpm in wp_export.WPManagers)
        {
            // Adding waypoints to node file
            for (int i = 0; i < wpm.Waypoints.Count; i++)
            {
                // positions converted to sumo frame
                wpx = Convert.ToString(wpm.Waypoints[i].position.x).Replace(",", ".");
                wpy = Convert.ToString(wpm.Waypoints[i].position.z).Replace(",", ".");
                wpz = Convert.ToString(wpm.Waypoints[i].position.y).Replace(",", ".");

                // <node id="WPMjNi" x="x.xx" y="y.yy"/>
                sbOutput_nodes.AppendLine(
                    "<node id=\"WPM" + wpm_instance + "N" + i + 
                    "\" x=\"" + wpx + 
                    "\" y=\"" + wpy + 
                    "\" z=\"" + wpz + "\"/>"
                );
            }
            // Next WaypointsManager
            wpm_instance += 1;
        }

        // Add end line to node file
        sbOutput_nodes.AppendLine("</nodes>");

        // Create and write the csv file
        File.WriteAllText(strFilePath_nodes, sbOutput_nodes.ToString());

        // Initialize edge file
        string strFilePath_edges = wp_export.FilePath + wp_export.FileName + ".edg.xml";
        StringBuilder sbOutput_edges = new StringBuilder();
        sbOutput_edges.AppendLine("<edges>");

        // Reset of counting variable for WaypointsManager instances
        wpm_instance = 0;

        foreach (WaypointsManager wpm in wp_export.WPManagers)
        {
            // Extract lane number and width information
            string buf = m_lane_info[wpm].Replace(" ", "");
            string[] lane_infos = buf.Split(';');

            // Adding connections to edge file
            int i;
            for (i = 0; i < (wpm.Waypoints.Count - 1); i++)
            {
                // Format for each edge:
                // <edge id="WPMjEi" from="WPMjNi" to="WPMjN(i+1)" numLanes="k">
                // <lane index="l" width="x.xx" />
                // </edge>
                sbOutput_edges.AppendLine(
                    "<edge id=\"WPM" + wpm_instance + "E" + i + 
                    "\" from=\"WPM" + wpm_instance + "N" + i + 
                    "\" to=\"WPM" + wpm_instance + "N" + (i + 1).ToString() + 
                    "\" numLanes=\"" + lane_infos.Length + "\">"
                );
                for (int j = 0; j < lane_infos.Length; j++)
                {
                    string[] single_lane_info = lane_infos[j].Split(',');
                    sbOutput_edges.AppendLine(
                        "\t<lane index=\"" + j + 
                        "\" width=\"" + single_lane_info[0] + 
                        "\" speed=\"" + single_lane_info[1] + "\"/>"
                    );
                }
                sbOutput_edges.AppendLine("</edge>");
            }
            if (wp_export.ClosedCircuit)
            {
                // Form last to first node
                sbOutput_edges.AppendLine(
                    "<edge id=\"WPM" + wpm_instance + "E" + i + 
                    "\" from=\"WPM" + wpm_instance + "N" + i + 
                    "\" to=\"WPM" + wpm_instance + "N" + 0 + 
                    "\" numLanes=\"" + lane_infos.Length + "\">"
                );
                for (int j = 0; j < lane_infos.Length; j++)
                {
                    string[] single_lane_info = lane_infos[j].Split(',');
                    sbOutput_edges.AppendLine(
                        "\t<lane index=\"" + j +
                        "\" width=\"" + single_lane_info[0] +
                        "\" speed=\"" + single_lane_info[1] + "\"/>"
                    );
                }
                sbOutput_edges.AppendLine("</edge>");
            }
            wpm_instance += 1;
        }

        sbOutput_edges.AppendLine("</edges>");

        // Create and write the csv file
        File.WriteAllText(strFilePath_edges, sbOutput_edges.ToString());

        // Sumo conversions (merge to net file) and delete edge and node files at the end
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        string file = wp_export.FilePath + wp_export.FileName;
        startInfo.Arguments = "/c netconvert --node-files=\"" + file + ".nod.xml\" " + 
            "--edge-files=\"" + file + ".edg.xml\" --output-file=\"" + file + ".net.xml\"";
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        startInfo.Arguments = "/c del \"" + file + ".nod.xml\" & del \"" + file + ".edg.xml\"";
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        
        // Optimization of Sumo network
        if (wp_export.Optimize)
        {
            startInfo.Arguments = "/c  netconvert -s \"" + file + ".net.xml\" " +
                "--geometry.min-dist 5 -o \"" + file + ".net.xml\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            startInfo.Arguments = "/c  netconvert -s \"" + file + ".net.xml\" -R -o \"" + file + ".net.xml\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
