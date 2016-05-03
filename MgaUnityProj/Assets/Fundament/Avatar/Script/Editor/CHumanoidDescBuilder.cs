using UnityEngine;
using UnityEditor;

public class CHumanoidDescBuilder : EditorWindow
{
    [MenuItem("MGA/Editor/Charactor/Humanoid Desc Builder")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CHumanoidDescBuilder));
    }

    protected GameObject m_pEditing;
	public void OnGUI()
	{
        m_pEditing = (GameObject)EditorGUILayout.ObjectField("把角色拖入", m_pEditing, typeof(GameObject), true);
        if (null != m_pEditing && GUILayout.Button("生成Atlas"))
        {
            CharactorAtlasUtil.MakeAtlasReplace(m_pEditing, "Fundament/Avatar/Artwork/Generated/Humanoid");
        }
        if (null != m_pEditing && GUILayout.Button("生成Desc"))
        {
            BuildDesc(m_pEditing);
        }
        if (null != m_pEditing && GUILayout.Button("根据Desc填写脚本"))
        {
        }
	}

    protected void BuildDesc(GameObject pEditing)
    {
        CHumanoidDesc oldDesc = new CHumanoidDesc();
        oldDesc.Load();
        CHumanoidDesc newDesc = new CHumanoidDesc();
        foreach (MeshRenderer mesh in pEditing.GetComponentsInChildren<MeshRenderer>())
        {
            CHumanoidDescElement element = newDesc.CreateElement();
            if (!newDesc.ChangeName(element.m_sElementName, mesh.gameObject.name))
            {
                CRuntimeLogger.LogError("名称重复了:" + mesh.gameObject.name);
                return;
            }
            element.m_sObjectPath = CommonFunctions.FindFullName(pEditing.gameObject, mesh.gameObject);
            element.m_sObjectPath = element.m_sObjectPath.Replace(pEditing.gameObject.name + "/", "");

            if (null != oldDesc[element.m_sElementName])
            {
                element.m_sDependency = oldDesc[element.m_sElementName].m_sDependency;
                element.m_sTags = oldDesc[element.m_sElementName].m_sTags;

                element.m_ePos = oldDesc[element.m_sElementName].m_ePos;
                element.m_eHumanSide = oldDesc[element.m_sElementName].m_eHumanSide;
                element.m_eHumanType = oldDesc[element.m_sElementName].m_eHumanType;
                element.m_eHumanWeapon = oldDesc[element.m_sElementName].m_eHumanWeapon;
                element.m_eHumanDocPos = oldDesc[element.m_sElementName].m_eHumanDocPos;
            }
            else
            {
                if (mesh.transform.parent != pEditing.transform)
                {
                    element.m_sDependency = new[] { mesh.transform.parent.gameObject.name };
                }

                #region Tags By Names

                if (element.m_sElementName.Contains("Body_F"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Body;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] {"Body", "Female"};
                }
                if (element.m_sElementName.Contains("Body_M"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Body;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_sTags = new[] { "Body", "Male" };
                }
                if (element.m_sElementName.Contains("Head_F") && !element.m_sElementName.Contains("Hair"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Head;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] { "Head", "Female" };
                }
                if (element.m_sElementName.Contains("Head_F") && element.m_sElementName.Contains("Hair"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_HeadWear;
                    element.m_eHumanDocPos = EHumanoidDoc.EHD_Hair;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] { "Hair", "Female" };
                }
                if (element.m_sElementName.Contains("Glass"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_HeadWear;
                    element.m_eHumanDocPos = EHumanoidDoc.EHD_Glass;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] { "Glass", "Female" };
                }
                if (element.m_sElementName.Contains("Hat"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_HeadWear;
                    element.m_eHumanDocPos = EHumanoidDoc.EHD_Hat;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] { "Hat", "Female" };
                }
                if (element.m_sElementName.Contains("Head_M") && !element.m_sElementName.Contains("Hair"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Head;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_sTags = new[] { "Head", "Male" };
                }
                if (element.m_sElementName.Contains("Head_M") && element.m_sElementName.Contains("Hair"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_HeadWear;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_eHumanDocPos = EHumanoidDoc.EHD_Hair;
                    element.m_sTags = new[] { "Hair", "Male" };
                }

                if (element.m_sElementName.Contains("_Back"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Back;
                    element.m_sTags = new[] { "Back", "Female", "Male" };
                }

                if (element.m_sElementName.Contains("HandL"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Hand;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_sTags = new[] { "Hand", "Female", "Male" };
                }
                if (element.m_sElementName.Contains("HandR"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Hand;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right; 
                    element.m_sTags = new[] { "Hand", "Female", "Male", "Right" };
                }
                if (element.m_sElementName.Contains("HandL"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Hand;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_sTags = new[] { "Hand", "Female", "Male", "Left" };
                }
                if (element.m_sElementName.Contains("Feet_F") && element.m_sElementName.Contains("L"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Feet;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left; 
                    element.m_sTags = new[] { "Foot", "Female", "Left"};
                }
                if (element.m_sElementName.Contains("Feet_F") && element.m_sElementName.Contains("R"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Feet;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_sTags = new[] { "Foot", "Female", "Right" };
                }
                if (element.m_sElementName.Contains("Feet_M") && element.m_sElementName.Contains("L"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Feet;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_sTags = new[] { "Foot", "Male", "Left" };
                }
                if (element.m_sElementName.Contains("Feet_M") && element.m_sElementName.Contains("R"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Feet;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_sTags = new[] { "Foot", "Male", "Right" };
                }

                if (element.m_sElementName.Contains("LWing"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Wing;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_sTags = new[] { "Wing", "Female", "Male", "Left" };
                }
                if (element.m_sElementName.Contains("RWing"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Wing;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_sTags = new[] { "Wing", "Female", "Male", "Right" };
                }


                //=====================================================
                //weapon
                if (element.m_sElementName.Contains("SHandWeapon_R"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_SingleHand_Fight;
                    element.m_sTags = new[] { "Weapon", "SHand", "Female", "Male", "Right" };
                }

                if (element.m_sElementName.Contains("Shoot_R"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_Shoot;
                    element.m_sTags = new[] { "Weapon", "Shoot", "Female", "Male", "Right" };
                }

                if (element.m_sElementName.Contains("DHandWeapon_Sword"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_DoubleHand_Fight;
                    element.m_sTags = new[] { "Weapon", "DHand", "Fight", "Female", "Male", "Right" };
                }

                if (element.m_sElementName.Contains("DHandWeapon_Stick"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_DoubleHand_Wizard;
                    element.m_sTags = new[] { "Weapon", "DHand", "Wizard", "Female", "Male", "Right" };
                }

                if (element.m_sElementName.Contains("SHandWeapon_Shield"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_SingleHand_Fight;
                    element.m_sTags = new[] { "Weapon", "SHand", "Fight", "Wizard", "Wizard", "Female", "Male", "Left" };
                }

                if (element.m_sElementName.Contains("SHandWeapon_L"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_SingleHand_Fight;
                    element.m_sTags = new[] { "Weapon", "SHand", "Fight", "Wizard", "Female", "Male", "Left" };
                }

                if (element.m_sElementName.Contains("Weapon_Shoot_L"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_Shoot;
                    element.m_sTags = new[] { "Weapon", "Shoot", "Female", "Male", "Left" };
                }

                #endregion
            }

        }
        newDesc.Save();
    }
}
