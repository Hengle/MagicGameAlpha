﻿using UnityEngine;

public enum EHumanoidType : byte
{
    EHT_Male,
    EHT_Female,
    EHT_Both,
}

public enum EHumanoidSide : byte
{
    EHT_Left,
    EHT_Right,
}

public enum EHumanoidWeapon : byte
{
    EHW_BareHand,
    EHT_SingleHand_Wizard,
    EHT_SingleHand_Fight,
    EHT_DoubleHand_Wizard,
    EHT_DoubleHand_Fight,
    EHT_Shoot,
}

public enum EHumanoidDoc : byte
{
    EHD_Hair,
    EHD_Hat,
    EHD_Glass,
}

public enum EHumanoidComponentPos
{
    ECP_Body,
    ECP_Back,
    ECP_Head,
    ECP_Hair,
    ECP_HeadWear,
    ECP_Feet,
    ECP_Hand,
    ECP_Weapon,
    ECP_Wing,
    ECP_BloodLineBack,
    ECP_BloodLineFront,
    ECP_Shadow,
}

public class CHumanoidDescElement : CMGDataElement
{
    public string m_sObjectPath = "";
    public string[] m_sDependency = new string[0];

    public EHumanoidComponentPos m_ePos = EHumanoidComponentPos.ECP_Body;
    public EHumanoidType m_eHumanType = EHumanoidType.EHT_Both;
    public EHumanoidSide m_eHumanSide = EHumanoidSide.EHT_Left;
    public EHumanoidWeapon m_eHumanWeapon = EHumanoidWeapon.EHW_BareHand;
    public EHumanoidDoc m_eHumanDocPos = EHumanoidDoc.EHD_Glass;

    override public string GetString()
    {
        string sRet = base.GetString();

        sRet += Write("ObjectPath", m_sObjectPath);
        sRet += Write("Dependency", m_sDependency);
        sRet += Write("Pos", m_ePos);
        sRet += Write("HumanType", m_eHumanType);
        sRet += Write("HumanSide", m_eHumanSide);
        sRet += Write("HumanWeapon", m_eHumanWeapon);
        sRet += Write("HumanDocPos", m_eHumanDocPos);

        return sRet;
    }

    override public void LoadData(string sTextToParse)
    {
        base.LoadData(sTextToParse);
        m_sObjectPath = (string)GetElementValue(sTextToParse, "ObjectPath", m_sObjectPath);
        m_sDependency = (string[])GetElementValue(sTextToParse, "Dependency", m_sDependency);
        m_ePos = (EHumanoidComponentPos)GetElementValue(sTextToParse, "Pos", m_ePos);
        m_eHumanType = (EHumanoidType)GetElementValue(sTextToParse, "HumanType", m_eHumanType);
        m_eHumanSide = (EHumanoidSide)GetElementValue(sTextToParse, "HumanSide", m_eHumanSide);
        m_eHumanWeapon = (EHumanoidWeapon)GetElementValue(sTextToParse, "HumanWeapon", m_eHumanWeapon);
        m_eHumanDocPos = (EHumanoidDoc)GetElementValue(sTextToParse, "HumanDocPos", m_eHumanDocPos);
    }
}

public class CHumanoidDesc : TMGData<CHumanoidDescElement>
{
    public override string GetDefaultSavePath()
    {
        return Application.dataPath + "/Fundament/Avatar/Resources/HumanoidDesc";
    }

    public override string GetDefaultLoadPath()
    {
        return "HumanoidDesc";
    }
}
