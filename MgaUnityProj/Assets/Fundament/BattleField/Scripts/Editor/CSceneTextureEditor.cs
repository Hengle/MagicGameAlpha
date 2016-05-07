using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CSceneTextureEditor : TMGTextDataEditor<CSceneTextureElement>
{

    [MenuItem("MGA/Editor/Scene/Scene Texture Editor")]
    public static void ShowWindow()
    {
        CSceneTextureEditor pEditor = (CSceneTextureEditor)GetWindow(typeof(CSceneTextureEditor));
        pEditor.InitEditor();
    }

    #region Override Functions

    protected override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Create Default"))
        {
            BuildTextureDataWithFolder();
        }

        EditorGUILayout.EndHorizontal();
    }

    public override void InitEditor()
    {
        base.InitEditor();

        //load data
        m_pEditingData = new CSceneTexture();
        m_pEditingData.Load();
    }

    protected override void EditorOneElement(CSceneTextureElement element, bool bFocus)
    {
        base.EditorOneElement(element, bFocus);

        m_pMainEditor.BeginLine();

        element.m_iTemplate = (int)EditorField("Template", element.m_iTemplate);
        element.m_iTextureCount = (int)EditorField("TextureCount", element.m_iTextureCount);
        element.m_iRotNumber = (int)EditorField("Rots", element.m_iRotNumber);
        element.m_bCanRot = (bool)EditorField("CanRot", element.m_bCanRot);

        m_pMainEditor.EndLine();

        m_pMainEditor.BeginLine();

        element.m_rcUVRect = (Vector4)EditorField("UVRect", element.m_rcUVRect);

        m_pMainEditor.EndLine();
    }

    #endregion

    protected void BuildTextureDataWithFolder()
    {
        string sFolder = EditorUtility.OpenFolderPanel("Select texture folder", "", "");
        if (string.IsNullOrEmpty(sFolder))
        {
            CRuntimeLogger.LogWarning("你需要选择一个目录!");
            return;
        }

        Dictionary<int, int> codes = new Dictionary<int, int>(new IntEqualityComparer());
        DirectoryInfo dir = new DirectoryInfo(sFolder);
        bool bHasT0 = false;
        if (dir.Exists)
        {
            foreach (FileInfo fileInfo in dir.GetFiles("*.png", SearchOption.AllDirectories))
            {
                string sFileName = CommonFunctions.GetLastName(fileInfo.FullName);
                string[] twoparts = sFileName.Split('_');
                if (2 == twoparts.Length && !string.IsNullOrEmpty(twoparts[0]))
                {
                    int iCode, iRepeat;
                    if (int.TryParse(twoparts[0].Substring(1, twoparts[0].Length - 1), out iCode) 
                     && int.TryParse(twoparts[1], out iRepeat))
                    {
                        bHasT0 = true;
                        if (codes.ContainsKey(iCode))
                        {
                            if (codes[iCode] < iRepeat)
                            {
                                codes[iCode] = iRepeat;    
                            }
                        }
                        else
                        {
                            codes.Add(iCode, iRepeat);
                        }
                    }
                }
            }
        }
        else
        {
            CRuntimeLogger.LogWarning("你需要选择一个目录!");
            return;
        }


        if (!bHasT0)
        {
            CRuntimeLogger.LogWarning("你需要选择一个含有T0_1,T0_2,T1_1...的目录!");
            return;
        }

        CSceneTexture newT =  new CSceneTexture();

        for (int lt = 0; lt < 3; ++lt)
        {
            for (int rt = 0; rt < 3; ++rt)
            {
                for (int rb = 0; rb < 3; ++rb)
                {
                    for (int lb = 0; lb < 3; ++lb)
                    {
                        int iCode, iRots;
                        bool bRot = lt == rt && lt == rb && lt == lb;
                        FindCodeAndRot(codes, lt, rt, rb, lb, out iCode, out iRots);
                        if (iCode < 0)
                        {
                            CRuntimeLogger.LogError(string.Format("组合没有找到{0}-{1}-{2}-{3}", lt, rt, rb, lb));
                            return;
                        }

                        CSceneTextureElement newE = newT.CreateElement();

                        int iCode2 = lt * CommonFunctions.IntPow(3, 0) 
                                   + rt * CommonFunctions.IntPow(3, 1)
                                   + rb * CommonFunctions.IntPow(3, 2)
                                   + lb * CommonFunctions.IntPow(3, 3);
                        newT.ChangeName(newE.m_sElementName, "T" + iCode2);
                        if (iCode2 != newE.m_iID)
                        {
                            newT.ChangeId(newE.m_iID, iCode2);
                        }

                        newE.m_iTemplate = iCode;
                        newE.m_iTextureCount = codes[iCode];
                        newE.m_bCanRot = bRot;
                        newE.m_iRotNumber = iRots;
                    }
                }
            }            
        }

        newT.Save();
        m_pEditingData = newT;
        RefreshData();
    }

    static public void FindCodeAndRot(Dictionary<int, int> dics, int lt, int rt, int rb, int lb, out int codes, out int rots)
    {
        rots = -1;
        codes = -1;

        for (int i = 0; i < 4; ++i)
        {
            int iReallt = i;
            int iRealrt = (i + 1) > 3 ? (i + 1 - 4) : (i + 1);
            int iRealrb = (i + 2) > 3 ? (i + 2 - 4) : (i + 2);
            int iReallb = (i + 3) > 3 ? (i + 3 - 4) : (i + 3);

            int iCode = lt * CommonFunctions.IntPow(3, iReallt) 
                      + rt * CommonFunctions.IntPow(3, iRealrt)
                      + rb * CommonFunctions.IntPow(3, iRealrb)
                      + lb * CommonFunctions.IntPow(3, iReallb);

            if (dics.ContainsKey(iCode))
            {
                codes = iCode;
                rots = i;
                return;
            }
        }
    }
}
