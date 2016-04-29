using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

//This editor is only editor sub element
public class CMGSubDataEditor
{
    public virtual void InitEditor()
    {

    }

    public virtual void EditorOneElement(CMGDataSubElement element)
    {

    }
    
    public void ShowSepB() { GUILayout.Label("=============================================================================="); }

    public void ShowSepS() { GUILayout.Label("---------------------------------------------------"); }

    protected Dictionary<string, string[]> m_pBitFiledDesc = null;

    public void PutBitField(string sId, string[] field)
    {
        if (null == m_pBitFiledDesc)
        {
            m_pBitFiledDesc = new Dictionary<string, string[]>();
        }
        if (!m_pBitFiledDesc.ContainsKey(sId))
        {
            m_pBitFiledDesc.Add(sId, field);
        }
        else
        {
            m_pBitFiledDesc[sId] = field;
        }
    }

    public object EditorField(string sDesc, object current)
    {
        if (current is int)
        {
            return EditorGUILayout.IntField(sDesc, (int)current);
        }
        if (current is sint)
        {
            return (sint)EditorGUILayout.IntField(sDesc, (sint)current);
        }
        if (current is short)
        {
            return (short)EditorGUILayout.IntField(sDesc, (short)current);
        }
        if (current is ushort)
        {
            return (ushort)EditorGUILayout.IntField(sDesc, (ushort)current);
        }
        if (current is byte)
        {
            return (byte)EditorGUILayout.IntField(sDesc, (byte)current);
        }
        if (current is long)
        {
            return EditorGUILayout.LongField(sDesc, (long)current);
        }

        if (current is float)
        {
            return EditorGUILayout.FloatField(sDesc, (float)current);
        }
        if (current is sfloat)
        {
            return (sfloat)EditorGUILayout.FloatField(sDesc, (sfloat)current);
        }

        string sCurrent = current as string;
        if (null != sCurrent)
        {
            return EditorGUILayout.TextField(sDesc, sCurrent);
        }

        //=====================================================================
        //Bit Field
        if (current is uint)
        {
            return (uint)EditorGUILayout.LongField(sDesc, (uint)current);
        }
        if (current is ulong)
        {
            return (ulong)EditorGUILayout.LongField(sDesc, (long)(ulong)current);
        }

        //=====================================================================
        //Struct Field
        if (current is Vector2)
        {
            return EditorGUILayout.Vector2Field(sDesc, (Vector2)current);
        }
        if (current is Vector3)
        {
            return EditorGUILayout.Vector3Field(sDesc, (Vector3)current);
        }
        if (current is Vector4)
        {
            return EditorGUILayout.Vector4Field(sDesc, (Vector4)current);
        }
        if (current is Color)
        {
            return EditorGUILayout.ColorField(sDesc, (Color)current);
        }
        if (current is Enum)
        {
            return EditorGUILayout.EnumPopup(sDesc, (Enum)current);
        }

        //=====================================================================
        //String List Field
        string[] sCurrentList = current as string[];
        if (null != sCurrentList)
        {
            string sCurrentV = sCurrentList.Aggregate("", (current1, eachone) => current1 + (string.IsNullOrEmpty(current1) ? eachone : (";" + eachone)));
            sCurrentV = EditorGUILayout.TextField(sDesc, sCurrentV);
            return sCurrentV.Split(';');
        }

        return null;
    }

}

public class TMGTextDataEditor<T> : EditorWindow where T : CMGDataElement, new()
{
    #region Override Functions

    protected virtual void OnGUI()
    {
        EditElements();
    }

    public virtual void InitEditor()
    {
        m_pMainEditor = new CMGSubDataEditor();
        m_pSubElements = new Dictionary<string, CMGSubDataEditor>();

        //load data
        //add bit fields
        //add sub editors
    }

    protected virtual void EditorOneElement(T element, bool bFocus)
    {
        //Edit the ID and Name
        EditorGUILayout.BeginHorizontal();

        int iID = EditorGUILayout.IntField("ID", element.m_iID);
        m_pEditingData.ChangeId(element.m_iID, iID);

        if (bFocus)
        {
            GUI.SetNextControlName("FocusTextField");
        }

        string sName = EditorGUILayout.TextField("名字", element.m_sElementName);
        m_pEditingData.ChangeName(element.m_sElementName, sName);

        if (bFocus)
        {
            EditorGUI.FocusTextInControl("FocusTextField");
            m_iNeedFocuce = -1;
        }

        EditorGUILayout.EndHorizontal();

        element.m_sTags = (string[])EditorField("Tags", element.m_sTags);
    }

    #endregion

    protected CMGSubDataEditor m_pMainEditor;
    protected Dictionary<string, CMGSubDataEditor> m_pSubElements;
    protected TMGData<T> m_pEditingData;

    protected Dictionary<int, List<int>> m_pOrder;
    protected Dictionary<string, int> m_pSearchDic;
    protected Dictionary<int, bool> m_pFold;

    protected bool m_bOrderByIndex = true;
    protected int m_iNeedFocuce = -1;
    protected string m_sSearching = "";
    protected int m_iDeleteIndex = -1;
    protected Vector2 m_vScrollMove = new Vector2(0.0f, 0.0f);

    protected void EditElements()
    {
        EditorGUILayout.BeginScrollView(m_vScrollMove, false, true, GUILayout.ExpandHeight(true));

        //Reload
        if (GUILayout.Button("重新加载"))
        {
            InitEditor();
            EditorGUILayout.EndScrollView();
            return;
        }

        if (null == m_pEditingData)
        {
            EditorGUILayout.EndScrollView();
            return;
        }

        //Editing

        //Order By Index
        //Order By Name
        bool bOrder = m_bOrderByIndex;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ID排序"))
        {
            bOrder = true;
        }
        if (GUILayout.Button("名称排序"))
        {
            bOrder = false;
        }
        EditorGUILayout.EndHorizontal();

        if (bOrder != m_bOrderByIndex || null == m_pOrder)
        {
            m_bOrderByIndex = bOrder;
            //Order
        }

        EditorGUILayout.BeginHorizontal();

        m_sSearching = EditorGUILayout.TextField("搜索", m_sSearching);
        if (m_sSearching.Length >= 3)
        {
            if (GUILayout.Button("搜索"))
            {
                
            }
        }

        EditorGUILayout.EndHorizontal();

        foreach (T t in m_pEditingData.m_pElement)
        {
            m_pMainEditor.ShowSepS();
            EditorOneElement(t, m_iNeedFocuce == t.m_iIndex);
            ShowDeleteMe(t);
        }
        m_iNeedFocuce = -1;

        if (m_iDeleteIndex >= 0)
        {
            m_pEditingData.DeleteElement(m_iDeleteIndex);
            m_iDeleteIndex = -1;
            m_pOrder = null;
            EditorGUILayout.EndScrollView();
            return;
        }

        if (GUILayout.Button("添加一个元素"))
        {
            m_pEditingData.CreateElement();
            m_iNeedFocuce = m_pEditingData.m_pElement.Count;
            m_pOrder = null;
            EditorGUILayout.EndScrollView();
            return;
        }

        m_pMainEditor.ShowSepB();
        if (GUILayout.Button("保存"))
        {
            m_pEditingData.Save();
        }

        EditorGUILayout.EndScrollView();
    }

    protected void ShowDeleteMe(T element)
    {
        if (GUILayout.Button("删除-" + element.m_sElementName))
        {
            m_iDeleteIndex = element.m_iIndex;
        }
    }

    protected object EditorField(string sDesc, object current)
    {
        if (current is CMGDataSubElement)
        {
            if (null != m_pSubElements && m_pSubElements.ContainsKey(sDesc))
            {
                m_pSubElements[sDesc].EditorOneElement(current as CMGDataSubElement);
            }
        }

        if (current is CMGDataSubElement[])
        {
            if (null != m_pSubElements && m_pSubElements.ContainsKey(sDesc))
            {

            }
        }

        return m_pMainEditor.EditorField(sDesc, current);
    }
}
