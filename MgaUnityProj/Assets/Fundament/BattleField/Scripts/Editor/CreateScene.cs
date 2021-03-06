using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class CreateScene : EditorWindow
{
    private CSceneType m_pSceneType;
    private CSceneTemplate m_pAllTemplates;
    private string[] m_sTemplateChooses;

    protected string m_sT1;
    protected string m_sT2;
    protected int m_iRot1 = 0;
    protected int m_iRot2 = 0;
    protected bool m_bFlip1 = false;
    protected bool m_bFlip2 = false;

    protected string m_sBattleFieldName = "";
    protected string m_sGroundType = "";
    protected ESceneEdgeType m_eSceneEdge = ESceneEdgeType.ESET_Water;

    protected Texture2D m_pHTexture = null;
    protected Texture2D m_pGTexture = null;
    protected Texture2D m_pDTexture = null;

    protected Texture2D m_pWTexture = null;
    protected Texture2D m_pNTexture = null;

    [MenuItem("MGA/Create/Scene/Battle Filed")]
    public static void ShowWindow()
    {
        CreateScene pEditor = (CreateScene)GetWindow(typeof(CreateScene));
        pEditor.Initial();
    }

    protected void Initial()
    {
        m_pAllTemplates = new CSceneTemplate();
        m_pAllTemplates.Load();
        List<string> sAllTemplates = new List<string>();
        foreach (CSceneTemplateElement element in m_pAllTemplates.m_pElement)
        {
            sAllTemplates.Add(element.m_sElementName);
        }
        m_sTemplateChooses = sAllTemplates.ToArray();

        m_pSceneType = new CSceneType();
        m_pSceneType.Load();
    }

    protected void OnGUI()
    {
        if (null == m_pAllTemplates || null == m_sTemplateChooses || 0 == m_sTemplateChooses.Length)
        {
            EditorGUILayout.HelpBox("创建的模板并没有找到！", MessageType.Error);
            return;
        }

        int iIndex1 = 0;
        int iIndex2 = 0;
        for (int i = 0; i < m_sTemplateChooses.Length; ++i)
        {
            if (!string.IsNullOrEmpty(m_sT1) && m_sT1.Equals(m_sTemplateChooses[i]))
            {
                iIndex1 = i;
            }
            if (!string.IsNullOrEmpty(m_sT2) && m_sT2.Equals(m_sTemplateChooses[i]))
            {
                iIndex2 = i;
            }
        }

        //Choose Template 1
        EditorGUILayout.BeginHorizontal();
        int iChoose1 = EditorGUILayout.Popup("选择模板1", iIndex1, m_sTemplateChooses);
        m_sT1 = m_sTemplateChooses[iChoose1];
        m_iRot1 = EditorGUILayout.IntField("旋转次数", m_iRot1);
        m_bFlip1 = EditorGUILayout.Toggle("是否翻转", m_bFlip1);
        EditorGUILayout.EndHorizontal();

        //Choose Template 2
        EditorGUILayout.BeginHorizontal();
        int iChoose2 = EditorGUILayout.Popup("选择模板2", iIndex2, m_sTemplateChooses);
        m_sT2 = m_sTemplateChooses[iChoose2];
        m_iRot2 = EditorGUILayout.IntField("旋转次数", m_iRot2);
        m_bFlip2 = EditorGUILayout.Toggle("是否翻转", m_bFlip2);
        EditorGUILayout.EndHorizontal();

        bool bGenerateLittleMap = false;
        if (GUILayout.Button("随机生成"))
        {
            bGenerateLittleMap = true;
            m_sT1 = m_sTemplateChooses[Random.Range(0, m_sTemplateChooses.Length)];
            m_sT2 = m_sTemplateChooses[Random.Range(0, m_sTemplateChooses.Length)];
            m_iRot1 = Random.Range(0, 4);
            m_iRot2 = Random.Range(0, 4);
            m_bFlip1 = Random.Range(0.0f, 1.0f) > 0.5f;
            m_bFlip2 = Random.Range(0.0f, 1.0f) > 0.5f;
        }

        if (GUILayout.Button("看看小地图") || bGenerateLittleMap)
        {
            m_pHTexture = new Texture2D(SceneConstant.m_iSceneHeightMapSize, SceneConstant.m_iSceneHeightMapSize, TextureFormat.RGB565, false);
            m_pGTexture = new Texture2D(SceneConstant.m_iSceneHeightMapSize, SceneConstant.m_iSceneHeightMapSize, TextureFormat.RGB565, false);
            m_pDTexture = new Texture2D(SceneConstant.m_iSceneSize, SceneConstant.m_iSceneSize, TextureFormat.RGB565, false);

            Texture2D ht1 = (Texture2D)EditorCommonFunctions.GetReadWritable(
                AssetDatabase.LoadAssetAtPath<Texture2D>(m_pAllTemplates[m_sT1].m_sHeightPath));
            Texture2D ht2 = (Texture2D)EditorCommonFunctions.GetReadWritable(
                AssetDatabase.LoadAssetAtPath<Texture2D>(m_pAllTemplates[m_sT2].m_sHeightPath));

            Texture2D gt1 = (Texture2D)EditorCommonFunctions.GetReadWritable(
                AssetDatabase.LoadAssetAtPath<Texture2D>(m_pAllTemplates[m_sT1].m_sGroundPath));
            Texture2D gt2 = (Texture2D)EditorCommonFunctions.GetReadWritable(
                AssetDatabase.LoadAssetAtPath<Texture2D>(m_pAllTemplates[m_sT2].m_sGroundPath));

            Texture2D dt1 = (Texture2D)EditorCommonFunctions.GetReadWritable(
                AssetDatabase.LoadAssetAtPath<Texture2D>(m_pAllTemplates[m_sT1].m_sDecoratePath));
            Texture2D dt2 = (Texture2D)EditorCommonFunctions.GetReadWritable(
                AssetDatabase.LoadAssetAtPath<Texture2D>(m_pAllTemplates[m_sT2].m_sDecoratePath));

            for (int i = 0; i < 2 * SceneConstant.m_iHightmapSize - 1; ++i)
            {
                for (int j = 0; j < 2 * SceneConstant.m_iHightmapSize - 1; ++j) //0-64
                {
                    //0-31, 0-31
                    if (i < SceneConstant.m_iHightmapSize - 1 && j < SceneConstant.m_iHightmapSize - 1)
                    {
                        m_pHTexture.SetPixel(i, j, GetPixel(ht2, i, j, m_iRot2 + 2, m_bFlip2));
                        m_pGTexture.SetPixel(i, j, GetPixel(gt2, i, j, m_iRot2 + 2, m_bFlip2));
                    }
                    //33-64, 0-31
                    else if (i > SceneConstant.m_iHightmapSize - 1 && j < SceneConstant.m_iHightmapSize - 1)
                    {
                        m_pHTexture.SetPixel(i, j, GetPixel(ht1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1));
                        m_pGTexture.SetPixel(i, j, GetPixel(gt1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1));
                    }
                    else if (i > SceneConstant.m_iHightmapSize - 1 && j > SceneConstant.m_iHightmapSize - 1)
                    {
                        m_pHTexture.SetPixel(i, j, GetPixel(ht2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2));
                        m_pGTexture.SetPixel(i, j, GetPixel(gt2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2));
                    }
                    else if (i < SceneConstant.m_iHightmapSize - 1 && j > SceneConstant.m_iHightmapSize - 1)
                    {
                        m_pHTexture.SetPixel(i, j, GetPixel(ht1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1));
                        m_pGTexture.SetPixel(i, j, GetPixel(gt1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1));
                    }

                    else if (i == SceneConstant.m_iHightmapSize - 1 && j < SceneConstant.m_iHightmapSize - 1)
                    {
                        Color hc1 = GetPixel(ht2, i, j, m_iRot2 + 2, m_bFlip2);
                        Color hc2 = GetPixel(ht1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1);
                        m_pHTexture.SetPixel(i, j, hc1 * 0.5f + hc2 * 0.5f);
                        Color gc1 = GetPixel(gt2, i, j, m_iRot2 + 2, m_bFlip2);
                        Color gc2 = GetPixel(gt1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1);
                        m_pGTexture.SetPixel(i, j, gc1 * 0.5f + gc2 * 0.5f);
                    }
                    else if (i == SceneConstant.m_iHightmapSize - 1 && j > SceneConstant.m_iHightmapSize - 1)
                    {
                        Color hc1 = GetPixel(ht2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2);
                        Color hc2 = GetPixel(ht1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1);
                        m_pHTexture.SetPixel(i, j, hc1 * 0.5f + hc2 * 0.5f);
                        Color gc1 = GetPixel(gt2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2);
                        Color gc2 = GetPixel(gt1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1);
                        m_pGTexture.SetPixel(i, j, gc1 * 0.5f + gc2 * 0.5f);
                    }
                    else if (i > SceneConstant.m_iHightmapSize - 1 && j == SceneConstant.m_iHightmapSize - 1)
                    {
                        Color hc1 = GetPixel(ht1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1);
                        Color hc2 = GetPixel(ht2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2);
                        m_pHTexture.SetPixel(i, j, hc1 * 0.5f + hc2 * 0.5f);
                        Color gc1 = GetPixel(gt1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1);
                        Color gc2 = GetPixel(gt2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2);
                        m_pGTexture.SetPixel(i, j, gc1 * 0.5f + gc2 * 0.5f);
                    }
                    else if (i < SceneConstant.m_iHightmapSize - 1 && j == SceneConstant.m_iHightmapSize - 1)
                    {
                        Color hc1 = GetPixel(ht2, i, j, m_iRot2 + 2, m_bFlip2);
                        Color hc2 = GetPixel(ht1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1);
                        m_pHTexture.SetPixel(i, j, hc1 * 0.5f + hc2 * 0.5f);
                        Color gc1 = GetPixel(gt2, i, j, m_iRot2 + 2, m_bFlip2);
                        Color gc2 = GetPixel(gt1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1);
                        m_pGTexture.SetPixel(i, j, gc1 * 0.5f + gc2 * 0.5f);
                    }

                    else if (i == SceneConstant.m_iHightmapSize - 1 && j == SceneConstant.m_iHightmapSize - 1)
                    {
                        Color hc1 = GetPixel(ht2, i, j, m_iRot2 + 2, m_bFlip2);
                        Color hc2 = GetPixel(ht1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1);
                        Color hc3 = GetPixel(ht2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2);
                        Color hc4 = GetPixel(ht1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1);
                        m_pHTexture.SetPixel(i, j, hc1 * 0.25f + hc2 * 0.25f + hc3 * 0.25f + hc4 * 0.25f);

                        Color gc1 = GetPixel(gt2, i, j, m_iRot2 + 2, m_bFlip2);
                        Color gc2 = GetPixel(gt1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1);
                        Color gc3 = GetPixel(gt2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2);
                        Color gc4 = GetPixel(gt1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1);
                        m_pGTexture.SetPixel(i, j, gc1 * 0.25f + gc2 * 0.25f + gc3 * 0.25f + gc4 * 0.25f);
                    }

                    if (i < 2*SceneConstant.m_iHightmapSize - 2 && j < 2*SceneConstant.m_iHightmapSize - 2)
                    {
                        //0-31, 0-31
                        if (i < SceneConstant.m_iHightmapSize - 1 && j < SceneConstant.m_iHightmapSize - 1)
                        {
                            m_pDTexture.SetPixel(i, j, GetPixel(dt2, i, j, m_iRot2 + 2, m_bFlip2));
                        }
                        //32-63, 0-31
                        else if (i >= SceneConstant.m_iHightmapSize - 1 && j < SceneConstant.m_iHightmapSize - 1)
                        {
                            m_pDTexture.SetPixel(i, j, GetPixel(dt1, i - SceneConstant.m_iHightmapSize + 1, j, m_iRot1 + 2, m_bFlip1));
                        }
                        else if (i >= SceneConstant.m_iHightmapSize - 1 && j >= SceneConstant.m_iHightmapSize - 1)
                        {
                            m_pDTexture.SetPixel(i, j, GetPixel(dt2, i - SceneConstant.m_iHightmapSize + 1, j - SceneConstant.m_iHightmapSize + 1, m_iRot2, m_bFlip2));
                        }
                        else if (i < SceneConstant.m_iHightmapSize - 1 && j >= SceneConstant.m_iHightmapSize - 1)
                        {
                            m_pDTexture.SetPixel(i, j, GetPixel(dt1, i, j - SceneConstant.m_iHightmapSize + 1, m_iRot1, m_bFlip1));
                        }                        
                    }
                }                
            }

            m_pHTexture.Apply();
            m_pGTexture.Apply();
            m_pDTexture.Apply();
        }

        EditorGUILayout.BeginHorizontal();

        if (null != m_pHTexture)
        {
            GUILayout.Box(m_pHTexture);
        }
        if (null != m_pGTexture)
        {
            GUILayout.Box(m_pGTexture);
        }
        if (null != m_pDTexture)
        {
            GUILayout.Box(m_pDTexture);
        }

        EditorGUILayout.EndHorizontal();

        if (null != m_pHTexture && null != m_pGTexture && null != m_pDTexture)
        {
            m_sGroundType = EditorGUILayout.TextField("地图类型:", m_sGroundType);
            m_eSceneEdge = (ESceneEdgeType)EditorGUILayout.EnumPopup("包边", m_eSceneEdge);
            m_sBattleFieldName = EditorGUILayout.TextField("战场文件名称:", m_sBattleFieldName);
            if (!string.IsNullOrEmpty(m_sBattleFieldName) && null != m_pSceneType[m_sGroundType])
            {
                if (GUILayout.Button("生成场景"))
                {
                    BakeEdge(m_pHTexture, m_pGTexture, m_pDTexture, m_eSceneEdge);

                    m_pWTexture = new Texture2D(SceneConstant.m_iSceneSize, SceneConstant.m_iSceneSize, TextureFormat.RGB565, false);
                    m_pNTexture = new Texture2D(SceneConstant.m_iSceneSize, SceneConstant.m_iSceneSize, TextureFormat.RGB565, false);
                    
                    //Save textures
                    CommonFunctions.CreateFolder(Application.dataPath + "/" + SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + "/?");
                    File.WriteAllBytes(Application.dataPath + "/" + SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + "/Heightmap.png", m_pHTexture.EncodeToPNG());
                    File.WriteAllBytes(Application.dataPath + "/" + SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + "/Griundmap.png", m_pGTexture.EncodeToPNG());
                    File.WriteAllBytes(Application.dataPath + "/" + SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + "/Decoratemap.png", m_pDTexture.EncodeToPNG());

                    Texture2D wmap, nmap;
                    SceneEditorUtility.CreateGround(m_pHTexture, m_pGTexture, m_pDTexture, SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + ".asset", m_sGroundType, true, out wmap, out nmap);

                    File.WriteAllBytes(Application.dataPath + "/" + SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + "/Walkmap.png", wmap.EncodeToPNG());
                    File.WriteAllBytes(Application.dataPath + "/" + SceneConstant.m_sArtworkPath + "BattleFields/" + m_sBattleFieldName + "/Normalmap.png", nmap.EncodeToPNG());
                }
            }
        }
    }

    private Color GetPixel(Texture2D tx, int i, int j, int iRot, bool bFlip)
    {
        int iRealRot = iRot%4;
        if (bFlip)
        {
            i = tx.width - 1 - i;
        }
        int iRealI = i;
        int iRealJ = j;
        switch (iRealRot)
        {
            case 1:
                iRealI = j;
                iRealJ = tx.width - 1 - i;
                break;
            case 2:
                iRealI = tx.width - 1 - i;
                iRealJ = tx.height - 1 - j;
                break;
            case 3:
                iRealI = tx.height - 1 - j;
                iRealJ = i;
                break;
        }

        return tx.GetPixel(iRealI, iRealJ);
    }

    public void BakeEdge(Texture2D hmap, Texture2D gmap, Texture2D decmap, ESceneEdgeType eSceneEdge)
    {
        switch (eSceneEdge)
        {
            case ESceneEdgeType.ESET_Water:
                SceneEditorUtility.BakeEdgeWater(hmap, gmap, decmap);
                break;
        }
    }
}
