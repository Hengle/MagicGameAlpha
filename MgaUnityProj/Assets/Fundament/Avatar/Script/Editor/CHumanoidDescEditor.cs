using UnityEngine;
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

    protected GameObject m_pGO;

    protected override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.BeginHorizontal();

        m_pGO = (GameObject)EditorGUILayout.ObjectField("把角色拖入", m_pGO, typeof(GameObject), true);

        if (GUILayout.Button("Create Default"))
        {
            BuildDesc(m_pGO, false);
        }
        if (GUILayout.Button("Merge Current"))
        {
            BuildDesc(m_pGO, true);
        }

        EditorGUILayout.EndHorizontal();
    }

    public override void InitEditor()
    {
        base.InitEditor();

        //load data
        m_pEditingData = new CHumanoidDesc();
        m_pEditingData.Load();
    }

    [MGADataEditor(typeof(CHumanoidDescElement))]
    protected static void EditOneElement(CHumanoidDescElement element, int iLineStart)
    {
        bool bBeginLine = false;
        BeginLine(iLineStart, ref bBeginLine);

        element.m_sObjectPath = (string)EditorField("ObjectPath", element.m_sObjectPath, iLineStart, ref bBeginLine);

        EndLine(ref bBeginLine);

        BeginLine(iLineStart, ref bBeginLine);
        element.m_ePos = (EHumanoidComponentPos)EditorField("Pos", element.m_ePos, iLineStart, ref bBeginLine);
        element.m_eHumanType = (EHumanoidType)EditorField("Type", element.m_eHumanType, iLineStart, ref bBeginLine);
        element.m_eHumanSide = (EHumanoidSide)EditorField("Side", element.m_eHumanSide, iLineStart, ref bBeginLine);
        EndLine(ref bBeginLine);

        BeginLine(iLineStart, ref bBeginLine);
        element.m_eHumanWeapon = (EHumanoidWeapon)EditorField("Weapon", element.m_eHumanWeapon, iLineStart, ref bBeginLine);
        element.m_iWeight = (int)EditorField("Weight", element.m_iWeight, iLineStart, ref bBeginLine);
        EndLine(ref bBeginLine);
    }

    #endregion

    protected void BuildDesc(GameObject pEditing, bool bMerge)
    {
        CHumanoidDesc newDesc = new CHumanoidDesc();

        CHumanoidDesc oldDesc = null;
        if (bMerge)
        {
            oldDesc = new CHumanoidDesc();
            oldDesc.Load();
        }

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

            if (null != oldDesc && null != oldDesc[element.m_sElementName])
            {
                element.m_sTags = oldDesc[element.m_sElementName].m_sTags;
                element.m_iWeight = oldDesc[element.m_sElementName].m_iWeight;
                element.m_ePos = oldDesc[element.m_sElementName].m_ePos;
                element.m_eHumanSide = oldDesc[element.m_sElementName].m_eHumanSide;
                element.m_eHumanType = oldDesc[element.m_sElementName].m_eHumanType;
                element.m_eHumanWeapon = oldDesc[element.m_sElementName].m_eHumanWeapon;
            }
            else
            {
                element.m_iWeight = 1;

                #region Tags By Names

                #region Body

                if (element.m_sElementName.Contains("Body_F"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Body;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] { "Body", "Female" };
                }
                if (element.m_sElementName.Contains("Body_M"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Body;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_sTags = new[] { "Body", "Male" };
                }

                #endregion

                #region Head

                if (element.m_sElementName.Contains("Head_F"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Head;
                    element.m_eHumanType = EHumanoidType.EHT_Female;
                    element.m_sTags = new[] { "Head", "Female" };

                    if (!element.m_sElementName.Contains("Hat") && !element.m_sElementName.Contains("Glass"))
                    {
                        element.m_iWeight = 30;
                    }
                    else if (element.m_sElementName.Contains("Hat") && !element.m_sElementName.Contains("Glass"))
                    {
                        element.m_iWeight = 3;
                    }
                    else if (!element.m_sElementName.Contains("Hat") && element.m_sElementName.Contains("Glass"))
                    {
                        element.m_iWeight = 2;
                    }
                    else if (element.m_sElementName.Contains("Hat") && element.m_sElementName.Contains("Glass"))
                    {
                        element.m_iWeight = 1;
                    }                    
                }

                if (element.m_sElementName.Contains("Head_M"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Head;
                    element.m_eHumanType = EHumanoidType.EHT_Male;
                    element.m_sTags = new[] { "Head", "Male" };
                }

                #endregion

                #region back, hand, foot, wing

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
                    element.m_sTags = new[] { "Foot", "Female", "Left" };
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

                #endregion

                #region Weapon

                //=====================================================
                //weapon
                if (element.m_sElementName.Contains("SHandWeapon_R"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Right;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_SingleHand_Fight;
                    if (element.m_sElementName.Contains("SHandWeapon_R_W"))
                    {
                        element.m_sTags = new[] { "Weapon", "SHand", "Wizard", "Female", "Male", "Right" };
                    }
                    else
                    {
                        element.m_sTags = new[] { "Weapon", "SHand", "Fight", "Female", "Male", "Right" };
                    }
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
                    element.m_sTags = new[] { "Weapon", "SHand", "Fight", "Wizard", "Female", "Male", "Left" };
                }

                if (element.m_sElementName.Contains("SHandWeapon_L"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Weapon;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_eHumanWeapon = EHumanoidWeapon.EHT_SingleHand_Fight;
                    element.m_sTags = new[] { "Weapon", "SHand", "Fight", "Female", "Male", "Left" };
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

                #region etc

                if (element.m_sElementName.Contains("Shadow"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_Shadow;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_sTags = new[] { "Shadow" };
                }
                if (element.m_sElementName.Contains("BloodLineFront"))
                {
                    element.m_ePos = EHumanoidComponentPos.ECP_BloodLineFront;
                    element.m_eHumanType = EHumanoidType.EHT_Both;
                    element.m_eHumanSide = EHumanoidSide.EHT_Left;
                    element.m_sTags = new[] { "BloodLineFront" };
                }

                #endregion

                #endregion
            }

        }
        newDesc.Save();

        m_pEditingData = newDesc;
        RefreshData();
    }
}
