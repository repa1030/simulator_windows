// =================================================================
// Copyright (C) 2020 Hochschule Karlsruhe - Technik und Wirtschaft
// This program and the accompanying materials
// are made available under the terms of the MIT license.
// =================================================================
// Authors: Anna-Lena Marmein, Magdalena Kugler, Yannick Rauch,
//          Patrick Rebling
// =================================================================

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class IconManager
{
    public enum Icon
    {
        None = -1,
        CircleGray,
        CircleBlue,
        CircleTeal,
        CircleGreen,
        CircleYellow,
        CircleOrange,
        CircleRed,
        CirclePurple,
        DiamondGray,
        DiamondBlue,
        DiamondTeal,
        DiamondGreen,
        DiamondYellow,
        DiamondOrange,
        DiamondRed,
        DiamondPurple
    }

    private static GUIContent[] largeIcons;

    public static void SetIcon(GameObject gObj, Icon icon)
    {
        if (largeIcons == null)
        {
            largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
        }
        if (icon == Icon.None)
            Internal_SetIcon(gObj, null);
        else
            Internal_SetIcon(gObj, largeIcons[(int)icon].image as Texture2D);
    }

    private static void Internal_SetIcon(GameObject gObj, Texture2D texture)
    {
#if UNITY_EDITOR
        var ty = typeof(EditorGUIUtility);
        var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
        mi.Invoke(null, new object[] { gObj, texture });
#endif
    }

    private static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
    {
        GUIContent[] guiContentArray = new GUIContent[count];
#if UNITY_EDITOR
        for (int index = 0; index < count; index++)
        {
            guiContentArray[index] = EditorGUIUtility.IconContent(baseName + (startIndex + index) + postFix);
        }
#endif
        return guiContentArray;
    }
}