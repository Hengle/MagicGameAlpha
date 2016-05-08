using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

    public int m_iCurrentSpace = 0;
    private bool m_bInBeginLine = false;
    public void BeginLine()
    {
        if (m_bInBeginLine)
        {
            return;
        }
        m_bInBeginLine = true;
        EditorGUILayout.BeginHorizontal();
        SpaceH(m_iCurrentSpace);
    }

    public void EndLine()
    {
        if (!m_bInBeginLine)
        {
            return;
        }
        m_bInBeginLine = false;
        EditorGUILayout.EndHorizontal();
    }

    public void SpaceH(int iWidth)
    {
        GUILayout.Space(20.0f * iWidth);
    }

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

        if (current is bool)
        {
            return EditorGUILayout.Toggle(sDesc, (bool)current);
        }
        if (current is sbool)
        {
            return (sbool)EditorGUILayout.Toggle(sDesc, (sbool)current);
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
        Enum eCurrent = current as Enum;
        if (null != eCurrent)
        {
            return EditorGUILayout.EnumPopup(sDesc, eCurrent);
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

        int[] iCurrentList = current as int[];
        if (null != iCurrentList)
        {
            BeginLine();
            int length = iCurrentList.Length;
            length = EditorGUILayout.IntField("长度", length);
            EndLine();
            BeginLine();
            List<int> res = new List<int>();
            for (int i = 0; i < length; ++i)
            {
                if (0 != i && 0 == (i % 4))
                {
                    EndLine();
                    BeginLine();
                }
                int ioneres = EditorGUILayout.IntField(i + ":", i < iCurrentList.Length ? iCurrentList[i] : 0);
                res.Add(ioneres);
            }
            EndLine();
            return res.ToArray();
        }

        float[] fCurrentList = current as float[];
        if (null != fCurrentList)
        {
            BeginLine();
            int length = fCurrentList.Length;
            length = EditorGUILayout.IntField("长度", length);
            EndLine();
            BeginLine();
            List<float> res = new List<float>();
            for (int i = 0; i < length; ++i)
            {
                if (0 != i && 0 == (i % 4))
                {
                    EndLine();
                    BeginLine();
                }
                float foneres = EditorGUILayout.FloatField(i + ":", i < fCurrentList.Length ? fCurrentList[i] : 0.0f);
                res.Add(foneres);
            }
            EndLine();
            return res.ToArray();
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
        m_pMainEditor.m_iCurrentSpace = 2;
        m_pMainEditor.BeginLine();

        int iID = EditorGUILayout.IntField("ID", element.m_iID);
        if (iID >= 0 && iID != element.m_iID)
        {
            m_pEditingData.ChangeId(element.m_iID, iID);    
        }

        GUI.SetNextControlName("FocusTextField" + element.m_iID);
        string sName = EditorGUILayout.TextField("名字", element.m_sElementName);
        if (!string.IsNullOrEmpty(sName) && !sName.Equals(element.m_sElementName))
        {
            m_pEditingData.ChangeName(element.m_sElementName, sName);
        }

        if (bFocus)
        {
            GUI.FocusControl("FocusTextField" + element.m_iID);
            EditorGUI.FocusTextInControl("FocusTextField" + element.m_iID);
            m_iNeedFocuce = -1;
        }

        m_pMainEditor.EndLine();

        m_pMainEditor.BeginLine();

        element.m_sTags = (string[])EditorField("Tags", element.m_sTags);

        m_pMainEditor.EndLine();
    }

    #endregion

    protected enum ETextEditorOrder
    {
        ETEO_ID,
        ETEO_Name,
        ETEO_Tag,
    }

    protected CMGSubDataEditor m_pMainEditor;
    protected Dictionary<string, CMGSubDataEditor> m_pSubElements;
    protected TMGData<T> m_pEditingData;

    protected Dictionary<string, List<int>> m_pOrder;
    protected Dictionary<string, bool> m_pOrderFold;
    protected Dictionary<string, int> m_pSearchDic;
    protected List<string> m_pSearchMatches;
    protected Dictionary<int, bool> m_pFold;

    protected ETextEditorOrder m_eOrder = ETextEditorOrder.ETEO_ID;
    protected int m_iNeedFocuce = -1;
    protected string m_sSearching = "";
    protected int m_iDeleteIndex = -1;
    protected Vector2 m_vScrollMove = new Vector2(0.0f, 0.0f);

    protected void EditElements()
    {
        m_vScrollMove = EditorGUILayout.BeginScrollView(m_vScrollMove, false, true, GUILayout.ExpandHeight(true));

        //Reload
        if (GUILayout.Button("重新加载"))
        {
            InitEditor();
            RefreshData();
            EditorGUILayout.EndScrollView();
            return;
        }

        if (null == m_pEditingData)
        {
            EditorGUILayout.EndScrollView();
            return;
        }

        ETextEditorOrder eOrder = m_eOrder;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ID排序"))
        {
            eOrder = ETextEditorOrder.ETEO_ID;
        }
        if (GUILayout.Button("名称排序"))
        {
            eOrder = ETextEditorOrder.ETEO_Name;
        }
        if (GUILayout.Button("Tag排序"))
        {
            eOrder = ETextEditorOrder.ETEO_Tag;
        }
        if (GUILayout.Button("刷新"))
        {
            RefreshData();
        }
        EditorGUILayout.EndHorizontal();

        if (eOrder != m_eOrder || null == m_pOrder)
        {
            m_eOrder = eOrder;
            switch (m_eOrder)
            {
                case ETextEditorOrder.ETEO_ID:
                    OrderById();
                    break;
                case ETextEditorOrder.ETEO_Name:
                    OrderByName();
                    break;
                case ETextEditorOrder.ETEO_Tag:
                    OrderByTag();
                    break;
            }
            m_pOrderFold = new Dictionary<string, bool>();
            Debug.Assert(m_pOrder != null, "m_pOrder != null");
            foreach (KeyValuePair<string, List<int>> kvp in m_pOrder)
            {
                m_pOrderFold.Add(kvp.Key, false);
            } 
            m_pFold = new Dictionary<int, bool>(new IntEqualityComparer());
            m_pSearchDic = new Dictionary<string, int>();
            foreach (T element in m_pEditingData.m_pElement)
            {
                m_pFold.Add(element.m_iIndex, false);
                if (!m_pSearchDic.ContainsKey(element.m_sElementName))
                {
                    m_pSearchDic.Add(element.m_sElementName, element.m_iIndex);
                }
                if (!m_pSearchDic.ContainsKey(element.m_iID.ToString()))
                {
                    m_pSearchDic.Add(element.m_iID.ToString(), element.m_iIndex);
                }
            }
        }

        EditorGUILayout.BeginHorizontal();

        if (m_iNeedFocuce != -1)
        {
            FocusToIndex(m_iNeedFocuce);
        }
        m_sSearching = EditorGUILayout.TextField("搜索", m_sSearching);
        if (m_sSearching.Length >= 3)
        {
            if (GUILayout.Button("搜索"))
            {
                m_pSearchMatches = CommonFunctions.GuessWord(m_pSearchDic.Keys.ToList(), m_sSearching, 0.2f, 4);
            }
        }

        EditorGUILayout.EndHorizontal();

        if (null != m_pSearchMatches && m_pSearchMatches.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();

            foreach (string t in m_pSearchMatches)
            {
                if (GUILayout.Button(t))
                {
                    FocusToIndex(m_pSearchDic[t]);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        foreach (KeyValuePair<string, List<int>> kvp in m_pOrder)
        {
            bool bFold = m_pOrderFold[kvp.Key];
            bFold = EditorGUILayout.Foldout(bFold, kvp.Key);
            m_pOrderFold[kvp.Key] = bFold;
            if (!bFold)
            {
                continue;
            }
            foreach (int indexes in kvp.Value)
            {
                EditorGUILayout.BeginHorizontal();
                m_pMainEditor.SpaceH(1);
                bool bElementFold = m_pFold[indexes];
                bElementFold = EditorGUILayout.Foldout(bElementFold, string.Format("{0}-{1}", m_pEditingData.m_pElement[indexes].m_iID, m_pEditingData.m_pElement[indexes].m_sElementName));
                EditorGUILayout.EndHorizontal();
                m_pFold[indexes] = bElementFold;
                if (!bElementFold)
                {
                    continue;
                }
                EditorOneElement(m_pEditingData.m_pElement[indexes], m_iNeedFocuce == indexes);
                ShowDeleteMe(m_pEditingData.m_pElement[indexes]);
            }
        }
        m_iNeedFocuce = -1;

        if (m_iDeleteIndex >= 0)
        {
            m_pEditingData.DeleteElement(m_iDeleteIndex);
            m_iDeleteIndex = -1;
            RefreshData();
            EditorGUILayout.EndScrollView();
            return;
        }

        if (GUILayout.Button("添加一个元素"))
        {
            m_pEditingData.CreateElement();
            m_iNeedFocuce = m_pEditingData.m_pElement.Count - 1;
            RefreshData();
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

    protected void RefreshData()
    {
        m_pOrder = null;
    }

    protected void OrderByName()
    {
        Dictionary<string, List<int>>  pOrder = new Dictionary<string, List<int>>();
        foreach (T t in m_pEditingData.m_pElement)
        {
            string sHead = new string((char)CommonFunctions.GetAlphaBeta(t.m_sElementName), 1);
            if (pOrder.ContainsKey(sHead))
            {
                List<int> list = pOrder[sHead];
                bool bInserted = false;
                for (int j = 0; j < list.Count; ++j)
                {
                    if (String.Compare(m_pEditingData.m_pElement[list[j]].m_sElementName, t.m_sElementName, StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }
                    list.Insert(j, t.m_iIndex);
                    bInserted = true;
                    break;
                }
                if (!bInserted)
                {
                    list.Add(t.m_iIndex);
                }
            }
            else
            {
                pOrder.Add(sHead, new List<int>());
                pOrder[sHead].Add(t.m_iIndex);
            }
        }

        List<string> finalOrder = pOrder.Keys.ToList();
        finalOrder.Sort(SortFunction);
        m_pOrder = new Dictionary<string, List<int>>();
        foreach (string keys in finalOrder)
        {
            m_pOrder.Add(keys, pOrder[keys]);
        }
    }

    protected void OrderById()
    {
        Dictionary<string, List<int>> pOrder = new Dictionary<string, List<int>>();
        foreach (T t in m_pEditingData.m_pElement)
        {
            string sHead = t.m_iID.ToString().Substring(0, 1);
            if (pOrder.ContainsKey(sHead))
            {
                List<int> list = pOrder[sHead];
                bool bInserted = false;
                for (int j = 0; j < list.Count; ++j)
                {
                    if (m_pEditingData.m_pElement[list[j]].m_iID >= t.m_iID)
                    {
                        continue;
                    }
                    list.Insert(j, t.m_iIndex);
                    bInserted = true;
                    break;
                }
                if (!bInserted)
                {
                    list.Add(t.m_iIndex);
                }
            }
            else
            {
                pOrder.Add(sHead, new List<int>());
                pOrder[sHead].Add(t.m_iIndex);
            }
        }

        List<string> finalOrder = pOrder.Keys.ToList();
        finalOrder.Sort(SortFunction);
        m_pOrder = new Dictionary<string, List<int>>();
        foreach (string keys in finalOrder)
        {
            m_pOrder.Add(keys, pOrder[keys]);
        }
    }

    protected void OrderByTag()
    {
        Dictionary<string, List<int>>  pOrder = new Dictionary<string, List<int>>();
        foreach (T t in m_pEditingData.m_pElement)
        {
            List<string> sTags = new List<string>();
            if (null != t.m_sTags && t.m_sTags.Length > 0)
            {
                foreach (string oneTag in t.m_sTags)
                {
                    if (!string.IsNullOrEmpty(oneTag))
                    {
                        sTags.Add(oneTag);
                    }
                }
            }
            if (0 == sTags.Count)
            {
                sTags.Add("No Tag");
            }
            foreach (string sHead in sTags)
            {
                if (pOrder.ContainsKey(sHead))
                {
                    List<int> list = pOrder[sHead];
                    bool bInserted = false;
                    for (int j = 0; j < list.Count; ++j)
                    {
                        if (m_pEditingData.m_pElement[list[j]].m_iID >= t.m_iID)
                        {
                            continue;
                        }
                        list.Insert(j, t.m_iIndex);
                        bInserted = true;
                        break;
                    }
                    if (!bInserted)
                    {
                        list.Add(t.m_iIndex);
                    }
                }
                else
                {
                    pOrder.Add(sHead, new List<int>());
                    pOrder[sHead].Add(t.m_iIndex);
                }                
            }
        }

        List<string> finalOrder = pOrder.Keys.ToList();
        finalOrder.Sort(SortFunction);
        m_pOrder = new Dictionary<string, List<int>>();
        foreach (string keys in finalOrder)
        {
            m_pOrder.Add(keys, pOrder[keys]);
        }
    }

    protected static int SortFunction(string s1, string s2)
    {
        return s1[0] > s2[0] ? 1 : -1;
    }

    protected void ShowDeleteMe(T element)
    {
        if (GUILayout.Button("删除-" + element.m_sElementName))
        {
            m_iDeleteIndex = element.m_iIndex;
        }
    }

    protected void FocusToIndex(int iIndex)
    {
        //Close and open folds
        if (null != m_pFold)
        {
            List<int> allKeys = m_pFold.Keys.ToList();
            foreach (int key in allKeys)
            {
                if (key == iIndex)
                {
                    m_pFold[key] = true;
                }
                else
                {
                    m_pFold[key] = false;
                }
            }
        }

        if (null != m_pOrder && null != m_pOrderFold)
        {
            foreach (KeyValuePair<string, List<int>> kvp in m_pOrder)
            {
                List<int> indexes = kvp.Value;
                if (null != indexes && indexes.Contains(iIndex))
                {
                    m_pOrderFold[kvp.Key] = true;
                }
                else
                {
                    m_pOrderFold[kvp.Key] = false;
                }
            }
        }

        //Set focus Index
        m_iNeedFocuce = iIndex;
    }

    protected object EditorField(string sDesc, object current)
    {
        if (current is CMGDataSubElement)
        {
            if (null != m_pSubElements && m_pSubElements.ContainsKey(sDesc))
            {
                m_pSubElements[sDesc].EditorOneElement((CMGDataSubElement) current);
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
