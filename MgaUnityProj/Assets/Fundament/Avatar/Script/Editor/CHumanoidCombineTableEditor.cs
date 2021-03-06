using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CHumanoidCombineTableEditor : TMGTextDataEditor<CHumanoidCombineTableElement>
{

    [MenuItem("MGA/Editor/Charactor/Humanoid Combine Editor")]
    public static void ShowWindow()
    {
        CHumanoidCombineTableEditor pEditor = (CHumanoidCombineTableEditor)GetWindow(typeof(CHumanoidCombineTableEditor));
        pEditor.InitEditor();
    }

    #region Override Functions

    protected override void OnGUI()
    {
        base.OnGUI();
        if (GUILayout.Button("Create Default"))
        {
            List<string[]> ret = new List<string[]>();
            string sP1 = "Body/Head/C_GH_Head_F";
            string[] sP1G1 = 
            {
                "C_GH_Head_F1_Hair1",
                "C_GH_Head_F2_Hair1",
                "C_GH_Head_F3_Hair1",
                "C_GH_Head_F4_Hair1",
                "C_GH_Head_F5_Hair1",
                "C_GH_Head_F6_Hair1",
                "C_GH_Head_F7_Hair1",
                "C_GH_Head_F8_Hair1",
                "C_GH_Head_F9_Hair1",
                "C_GH_Head_F10_Hair1",
            };
            string[] sP1G2 = 
            {
                "",
                "Hat1_1",
                "Hat2_1",
                "Hat3_1",
                "Hat4_1",
                "Hat5_1",
            };
            string[] sP1G3 = 
            {
                "",
                "Glass_1",
                "Glass_2",
            };
            for (int i = 0; i < sP1G1.Length; ++i)
            {
                for (int j = 0; j < sP1G2.Length; ++j)
                {
                    for (int k = 0; k < sP1G3.Length; ++k)
                    {
                        List<string> list = new List<string>();
                        list.Add(sP1);
                        list.Add(sP1 + "/" + sP1G1[i]);
                        if (!string.IsNullOrEmpty(sP1G2[j]))
                        {
                            list.Add(sP1 + "/" + sP1G2[j]);
                        }
                        if (!string.IsNullOrEmpty(sP1G3[k]))
                        {
                            list.Add(sP1 + "/" + sP1G3[k]);
                        }
                        ret.Add(list.ToArray());
                    }
                }
            }

            ret.Add(new[] { "Body/Head/C_GH_Head_M5", "Body/Head/C_GH_Head_M5/C_GH_Head_M5Hair1" });
            ret.Add(new[] { "Body/Head/C_GH_Head_M6", "Body/Head/C_GH_Head_M6/C_GH_Head_M6Face" });

            m_pEditingData = new CHumanoidCombineTable();
            foreach (string[] cbs in ret)
            {
                CHumanoidCombineTableElement one = m_pEditingData.CreateElement();
                one.m_sCombine = cbs;
            }

            m_pEditingData.Save();
            RefreshData();
        }
    }

    public override void InitEditor()
    {
        base.InitEditor();

        //load data
        m_pEditingData = new CHumanoidCombineTable();
        m_pEditingData.Load();
    }

    [MGADataEditor(typeof(CHumanoidCombineTableElement))]
    protected static void EditOneElement(CHumanoidCombineTableElement element, int iLineStart)
    {
        bool bBeginLine = false;
        BeginLine(iLineStart, ref bBeginLine);
        element.m_sCombine = (string[])EditorField("Combine", element.m_sCombine, iLineStart, ref bBeginLine);
        EndLine(ref bBeginLine);
    }

    #endregion
}
