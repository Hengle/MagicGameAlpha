using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateSceneTemplate : ScriptableWizard
{
    public Texture2D HeightMap;
    public Texture2D GroundMap;
    public Texture2D DocMap;
    public string GroundType;
    public string TemplateName;

    protected class CDecorateInfo
    {
        public int m_iCh;
        public Vector3 m_vMid;
        public int m_iSize;
    }

    [MenuItem("MGA/Create/Scene/Create Template")]
    static void CreateWizard()
    {
        DisplayWizard<CreateSceneTemplate>("Create Scene Template", "Create");
    }

    void OnWizardCreate()
    {
        #region Step1: 检查文件是否存在

        Texture2D heighttx = (Texture2D)EditorCommonFunctions.GetReadWritable(HeightMap);
        if (null == heighttx || 33 != heighttx.width || 33 != heighttx.height)
        {
            CRuntimeLogger.LogError("必须是33x33的高度贴图");
            return;
        }
        Texture2D groundtx = (Texture2D)EditorCommonFunctions.GetReadWritable(GroundMap);
        if (null == groundtx || 33 != groundtx.width || 33 != groundtx.height)
        {
            CRuntimeLogger.LogError("必须是33x33的地皮贴图");
            return;
        }
        Texture2D doctx = (Texture2D)EditorCommonFunctions.GetReadWritable(DocMap);
        if (null == doctx || 32 != doctx.width || 32 != doctx.height)
        {
            CRuntimeLogger.LogError("必须是32x32的装饰贴图");
            return;
        }
        CSceneTexture sceneTx = new CSceneTexture {m_sSceneType = GroundType};
        if (!sceneTx.Load() || 81 != sceneTx.m_pElement.Count)
        {
            CRuntimeLogger.LogError(GroundType + "贴图配置文件有问题");
            return;
        }

        CSceneGroudTemplate groundTemplate = new CSceneGroudTemplate {m_sSceneType = GroundType};
        if (!groundTemplate.Load())
        {
            CRuntimeLogger.LogError(GroundType + "贴图配置文件有问题");
            return;
        }

        CSceneType type = new CSceneType();
        type.Load();
        if (null == type[GroundType])
        {
            CRuntimeLogger.LogError(GroundType + "的配置找不到？");
            return;
        }

        #endregion

        #region 读颜色并且生成悬崖

        Vector3[,] verts = new Vector3[33, 33];
        int[,] gtypes = new int[33, 33];
        for (int i = 0; i < 33; ++i)
        {
            for (int j = 0; j < 33; ++j)
            {
                Color h = heighttx.GetPixel(i, j);
                Color g = groundtx.GetPixel(i, j);
                verts[i, j] = new Vector3(i - 16, (h.r - 0.5f) * 6.0f, j - 16);
                gtypes[i, j] = CompareRGB(g.r, g.g, g.b);
            }
        }

        //Make Clifs
        for (int i = 0; i < 32; ++i)
        {
            for (int j = 0; j < 32; ++j)
            {
                if (Mathf.Max(verts[i, j].y, verts[i + 1, j].y, verts[i + 1, j + 1].y, verts[i, j + 1].y)
                  - Mathf.Min(verts[i, j].y, verts[i + 1, j].y, verts[i + 1, j + 1].y, verts[i, j + 1].y) > 2.0f)
                {
                    gtypes[i, j] = type[GroundType].m_iCliffType;
                    gtypes[i + 1, j] = type[GroundType].m_iCliffType;
                    gtypes[i + 1, j + 1] = type[GroundType].m_iCliffType;
                    gtypes[i, j + 1] = type[GroundType].m_iCliffType;
                }
            }
        }

        #endregion

        #region 生成地板模型

        Vector3[] poses = new Vector3[4 * 32 * 32];
        Vector2[] theuvs = new Vector2[4 * 32 * 32];
        int[] trangles = new int[6 * 32 * 32];

        //Create Mesh
        for (int i = 0; i < 32; ++i)
        {
            for (int j = 0; j < 32; ++j)
            {
                Vector2[] uvs = GetUVs(gtypes[i, j], gtypes[i + 1, j], gtypes[i + 1, j + 1], gtypes[i, j + 1], sceneTx, groundTemplate, type[GroundType].m_bCanRot);

                poses[(i * 32 + j) * 4 + 0] = verts[i, j];
                poses[(i * 32 + j) * 4 + 1] = verts[i + 1, j];
                poses[(i * 32 + j) * 4 + 2] = verts[i + 1, j + 1];
                poses[(i * 32 + j) * 4 + 3] = verts[i, j + 1];

                theuvs[(i * 32 + j) * 4 + 0] = uvs[0];
                theuvs[(i * 32 + j) * 4 + 1] = uvs[1];
                theuvs[(i * 32 + j) * 4 + 2] = uvs[2];
                theuvs[(i * 32 + j) * 4 + 3] = uvs[3];

                trangles[(i * 32 + j) * 6 + 0] = (i * 32 + j) * 4 + 0;
                trangles[(i * 32 + j) * 6 + 1] = (i * 32 + j) * 4 + 3;
                trangles[(i * 32 + j) * 6 + 2] = (i * 32 + j) * 4 + 1;

                trangles[(i * 32 + j) * 6 + 3] = (i * 32 + j) * 4 + 1;
                trangles[(i * 32 + j) * 6 + 4] = (i * 32 + j) * 4 + 3;
                trangles[(i * 32 + j) * 6 + 5] = (i * 32 + j) * 4 + 2;
            }            
        }

        Mesh theMesh = new Mesh {vertices = poses, uv = theuvs, triangles = trangles};
        theMesh.RecalculateNormals();
        Unwrapping.GenerateSecondaryUVSet(theMesh);
        theMesh.Optimize();
        AssetDatabase.CreateAsset(theMesh, "Assets/" + SceneConstant.m_sArtworkPath + "Templates/" + GroundType + "/GenerateMesh/" + TemplateName + ".asset");
        AssetDatabase.Refresh();

        GameObject newGround = new GameObject();
        newGround.transform.position = Vector3.zero;
        newGround.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/" + SceneConstant.m_sArtworkPath + GroundType + SceneConstant.m_sSceneTexturesPath + ".mat");
        newGround.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/" + SceneConstant.m_sArtworkPath + "Templates/" + GroundType + "/GenerateMesh/" + TemplateName + ".asset");

        #endregion

        #region 生成装饰物

        CSceneDecorate doc = new CSceneDecorate{m_sSceneType = GroundType};
        doc.Load();

        bool[,,] chs = new bool[3,32,32];
        for (int i = 0; i < 32; ++i)
        {
            for (int j = 0; j < 32; ++j)
            {
                Color h = doctx.GetPixel(i, j);
                //Vector3 vPos = new Vector3(i - 15.5f, 0.25f * verts[i,j].y, j - 15.5f);
                chs[0, i, j] = h.r > 0.5f;
                chs[1, i, j] = h.g > 0.5f;
                chs[2, i, j] = h.b > 0.5f;
            }
        }
        List<CDecorateInfo> addDocs = new List<CDecorateInfo>();

        for (int i = 0; i < 32; ++i)
        {
            for (int j = 0; j < 32; ++j)
            {
                for (int k = 0; k < 3; ++k)
                {
                    if (null != doc[0])
                    {
                        if (chs[k, i, j])
                        {
                            if (2 == doc[k].m_iDecrateSize
                                && i < 31 && j < 31
                                && chs[k, i + 1, j]
                                && chs[k, i, j + 1]
                                && chs[k, i + 1, j + 1])
                            {
                                addDocs.Add(new CDecorateInfo
                                {
                                    m_iCh = k,
                                    m_vMid = new Vector3(i - 15, verts[i + 1, j + 1].y, j - 15),
                                    m_iSize = 2
                                });
                                chs[k, i + 1, j] = false;
                                chs[k, i, j + 1] = false;
                                chs[k, i + 1, j + 1] = false;
                            }
                            else
                            {
                                addDocs.Add(new CDecorateInfo
                                {
                                    m_iCh = k,
                                    m_vMid = new Vector3(i - 15.5f,
                                        0.25f * (verts[i, j].y + verts[i + 1, j].y + verts[i, j + 1].y + verts[i + 1, j + 1].y), 
                                        j - 15.5f),
                                    m_iSize = 1
                                });
                            }
                        }
                    }
                }
            }
        }

        foreach (CDecorateInfo dc in addDocs)
        {
            GameObject newDec = new GameObject();
            newDec.transform.position = dc.m_vMid + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            if (doc[dc.m_iCh].m_bOnlyRotateY)
            {
                newDec.transform.eulerAngles = new Vector3(0.0f, Random.Range(-360.0f, 360.0f), 0.0f);
            }
            else
            {
                newDec.transform.eulerAngles = new Vector3(Random.Range(-360.0f, 360.0f), Random.Range(-360.0f, 360.0f), Random.Range(-360.0f, 360.0f));
            }

            if (2 == dc.m_iSize)
            {
                if (doc[dc.m_iCh].m_bResizeHeight)
                {
                    newDec.transform.localScale = new Vector3(
                        Random.Range(1.6f, 2.4f),
                        Random.Range(1.6f, 2.4f),
                        Random.Range(1.6f, 2.4f)
                        );
                }
                else
                {
                    newDec.transform.localScale = new Vector3(
                        Random.Range(1.6f, 2.4f),
                        Random.Range(0.8f, 1.2f),
                        Random.Range(1.6f, 2.4f)
                        );                    
                }
            }
            else
            {
                newDec.transform.localScale = new Vector3(
                    Random.Range(0.8f, 1.2f),
                    Random.Range(0.8f, 1.2f),
                    Random.Range(0.8f, 1.2f)
                    );                   
            }
            string sMeshName = doc[dc.m_iCh].m_sElementName + "_" + Random.Range(1, doc[dc.m_iCh].m_iDecrateRepeat);
            newDec.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/" + SceneConstant.m_sArtworkPath + GroundType + SceneConstant.m_sSceneTexturesPath + ".mat");
            newDec.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/" + SceneConstant.m_sArtworkPath + GroundType + SceneConstant.m_sDecoratesPath + "/Generate/" + sMeshName + ".asset");            
        }

        #endregion

    }

    private static int CompareRGB(float r, float g, float b)
    {
        return r > g ? (b > r ? 2 : 0) : (b > g ? 2 : 1);
    }

    private static Vector2[] GetUVs(int lt, int rt, int rb, int lb, CSceneTexture tx, CSceneGroudTemplate tem, bool bCanRot)
    {
        int iCode2 = lt * CommonFunctions.IntPow(3, 0)
                   + rt * CommonFunctions.IntPow(3, 1)
                   + rb * CommonFunctions.IntPow(3, 2)
                   + lb * CommonFunctions.IntPow(3, 3);

        CSceneTextureElement txelement = tx[iCode2];
        if (null == txelement)
        {
            CRuntimeLogger.LogError("贴图配置文件有问题");
            return null;
        }

        string sTxName = string.Format("T{0}_{1}", txelement.m_iTemplate, Random.Range(0, txelement.m_iTextureCount) + 1);
        Rect uv = CommonFunctions.V42Rect(tem[sTxName].m_vUV);

        int iRot = txelement.m_iRotNumber;
        bool bRot = lt == rt && lt == rb && lt == lb;
        if (bRot && bCanRot)
        {
            iRot += Random.Range(0, 4);
        }

        Vector2[] ret =
        {
            new Vector2(uv.xMin, uv.yMin + uv.height), 
            new Vector2(uv.xMin + uv.width, uv.yMin + uv.height), 
            new Vector2(uv.xMin + uv.width, uv.yMin), 
            new Vector2(uv.xMin, uv.yMin)
        };

        for (int i = 0; i < iRot; ++i)
        {
            Vector2 last = ret[0];
            ret[0] = ret[1];
            ret[1] = ret[2];
            ret[2] = ret[3];
            ret[3] = last;

            //Vector2 last = ret[3];
            //ret[3] = ret[2];
            //ret[2] = ret[1];
            //ret[1] = ret[0];
            //ret[0] = last;
        }

        return ret;
    }
}
