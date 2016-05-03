﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class CHumanoidDescEditor : TMGTextDataEditor<CHumanoidDescElement>
{

    [MenuItem("MGA/Editor/Charactor/Humanoid Desc Editor")]
    public static void ShowWindow()
    {
        CHumanoidDescEditor pEditor = (CHumanoidDescEditor)GetWindow(typeof(CHumanoidDescEditor));
        pEditor.InitEditor();
    }

    #region Override Functions

    public override void InitEditor()
    {
        base.InitEditor();

        //load data
        m_pEditingData = new CHumanoidDesc();
        m_pEditingData.Load();
    }

    protected override void EditorOneElement(CHumanoidDescElement element, bool bFocus)
    {
        base.EditorOneElement(element, bFocus);

        element.m_sObjectPath = (string)EditorField("ObjectPath", element.m_sObjectPath);
        element.m_sDependency = (string[])EditorField("Dependency", element.m_sDependency);

        GUILayout.BeginHorizontal();
        element.m_ePos = (EHumanoidComponentPos)EditorField("Pos", element.m_ePos);
        element.m_eHumanType = (EHumanoidType)EditorField("Type", element.m_eHumanType);
        element.m_eHumanSide = (EHumanoidSide)EditorField("Side", element.m_eHumanSide);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        element.m_eHumanWeapon = (EHumanoidWeapon)EditorField("Weapon", element.m_eHumanWeapon);
        element.m_eHumanDocPos = (EHumanoidDoc)EditorField("Doc Pos", element.m_eHumanDocPos);
        GUILayout.EndHorizontal();
    }

    #endregion
}
