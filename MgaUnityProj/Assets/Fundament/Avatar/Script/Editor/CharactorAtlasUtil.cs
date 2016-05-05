using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;

//This class is for atlas the charactor
public class CharactorAtlasUtil
{
    #region New Version

    public class SCharRendererFullInfo
    {
        public string m_sFullTexName;

        public string m_sMeshFilterName;
        public string m_sMeshName;
        public string m_sObjectPath;
        public GameObject m_pObj;

        public Texture2D m_pMainTexture;
        public Texture2D m_pColorTexture;
        public Texture2D m_pShaTexture;
        public Mesh m_pMesh;

        public Mesh[] m_pCombinedMesh;
        public string[] m_sTextureNames;
        public GameObject[] m_pCombinedMeshObj;
        public bool m_bDiscard;
        public bool m_bCombine;
        public GameObject m_pTransfParent;
    }

    private static List<string[]> GetHumanoidCombineList()
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
        return ret;
    }

    public static void MakeAtlasReplaceWithCloth(GameObject pObj, string sParentFolder)
    {
        #region Step0: Count Progress

        List<string[]> combineList = GetHumanoidCombineList();
        int iProgressCount = pObj.GetComponentsInChildren<MeshRenderer>(true).Length + combineList.Count;
        int iProgressFull = iProgressCount * 56;
        int iProgressNow = 0;

        #endregion

        #region Step1: make gameobject table and make dirs (1x)

        CommonFunctions.CreateFolder(Application.dataPath + "/" + sParentFolder + "/Textures/?");
        for (int i = 0; i < (int) ECharactorMainColor.ECMC_Max; ++i)
        {
            for (int j = 0; j < (int) ECharactorSubColor.ECSC_Max; ++j)
            {
                CommonFunctions.CreateFolder(Application.dataPath + "/" + sParentFolder + "/Resources/CharMesh/" + string.Format("M{0}S{1}/?", i, j));
                CommonFunctions.CreateFolder(Application.dataPath + "/" + sParentFolder + "/Resources/CharMesh/" + string.Format("M{0}S{1}I/?", i, j));
            }
        }

        List<SCharRendererFullInfo> info = new List<SCharRendererFullInfo>();
        Dictionary<string, Texture2D[]> contextureCombine = new Dictionary<string, Texture2D[]>();
        Dictionary<string, int> infoIndex = new Dictionary<string, int>();
        foreach (MeshRenderer renderer in pObj.GetComponentsInChildren<MeshRenderer>(true))
        {
            ++iProgressNow;
            EditorUtility.DisplayProgressBar("正在收集Renderer信息", string.Format("{0}/{1}", iProgressNow, iProgressFull),
                iProgressNow / (float)iProgressFull);

            Texture2D mainTex = renderer.sharedMaterial.GetTexture("_MainTex") as Texture2D;
            Texture2D colorTex = renderer.sharedMaterial.GetTexture("_ColorTex") as Texture2D;
            Texture2D shaTex = renderer.sharedMaterial.GetTexture("_ShadowTex") as Texture2D;

            if (null == mainTex || null == colorTex || null == shaTex)
            {
                CRuntimeLogger.LogError("有的材质没有贴图: " + CommonFunctions.FindFullName(pObj, renderer.gameObject));
                EditorUtility.ClearProgressBar();
                return;
            }

            string sTextureName = CommonFunctions.BuildStringOrder(new[] {mainTex.name, colorTex.name, shaTex.name});
            Mesh mesh = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh;

            info.Add(new SCharRendererFullInfo
            {
                m_sFullTexName = sTextureName,
                m_pMesh = mesh,
                m_sMeshFilterName = AssetDatabase.GetAssetPath(mesh),
                m_sMeshName = mesh.name,
                m_pObj = renderer.gameObject,
                m_sObjectPath = CommonFunctions.FindFullName(pObj, renderer.gameObject),
                m_pMainTexture = mainTex,
                m_pColorTexture = colorTex,
                m_pShaTexture = shaTex,
                m_pCombinedMesh = new Mesh[0],
                m_bDiscard = false,
                m_pCombinedMeshObj = null,
                m_bCombine = false,
            });
            infoIndex.Add(CommonFunctions.FindFullName(pObj, renderer.gameObject), info.Count - 1);
            if (!contextureCombine.ContainsKey(sTextureName))
            {
                contextureCombine.Add(sTextureName, new[] { mainTex, colorTex, shaTex });
            }
        }

        iProgressNow = iProgressCount;

        #endregion

        #region Step2: make colored textures (16x)

        CCharactorColor colors = new CCharactorColor();
        colors.Load();
        Dictionary<string, Texture2D> outTexture = new Dictionary<string, Texture2D>();
        for (int i = 0; i < (int)ECharactorMainColor.ECMC_Max; ++i)
        {
            for (int j = 0; j < (int)ECharactorSubColor.ECSC_Max; ++j)
            {
                Color mainC = colors[string.Format("M{0}", i + 1)].m_cColor;
                Color subC = colors[string.Format("S{0}", i + 1)].m_cColor;
                
                foreach (KeyValuePair<string, Texture2D[]> kvp in contextureCombine)
                {
                    ++iProgressNow;
                    EditorUtility.DisplayProgressBar("正在阵营着色", string.Format("{0}/{1}", iProgressNow, iProgressFull),
                        iProgressNow / (float)iProgressFull);
                    Texture2D[] outPut = GetTexture2DWithColor(kvp.Value[0], kvp.Value[1], kvp.Value[2], mainC, subC);
                    outTexture.Add(string.Format("{0}_M{1}S{2}", kvp.Key, i, j), outPut[0]);
                    outTexture.Add(string.Format("{0}_M{1}S{2}I", kvp.Key, i, j), outPut[1]);
                }
                iProgressNow = (i*(int) ECharactorSubColor.ECSC_Max + j + 2)*iProgressCount;
            }            
        }

        Shader unlit = Shader.Find("Unlit/Texture");
        string sPackTextureName = sParentFolder + "/Textures/" + unlit.name.Replace("/", "_").Replace(".", "_");
        Dictionary<string, Rect> packed = EditorCommonFunctions.PackTexture(outTexture, sPackTextureName, unlit);

        #endregion

        #region Step3: Combine Batch (1x)

        List<string> sDisableList = new List<string>();
        foreach (string[] combines in combineList)
        {
            ++iProgressNow;
            EditorUtility.DisplayProgressBar("正在收集合并Batch信息", string.Format("{0}/{1}", iProgressNow, iProgressFull),
                iProgressNow / (float)iProgressFull);

            GameObject parent = info[infoIndex[pObj.name + "/" + combines[0]]].m_pObj;
            SCharRendererFullInfo cmb = new SCharRendererFullInfo
            {
                m_sObjectPath = CommonFunctions.FindFullName(pObj, parent.transform.parent.gameObject) + "/",
                m_bDiscard = false,
                m_pObj = null,
                m_pTransfParent = parent,
                m_bCombine = true,
            };
            string sMeshFilterName = "";
            List<GameObject> cmbobjs = new List<GameObject>();
            List<Mesh> cmbmeshes = new List<Mesh>();
            List<string> cmbtextures = new List<string>();

            foreach (string t in combines)
            {
                if (!sDisableList.Contains(pObj.name + "/" + t))
                {
                    sDisableList.Add(pObj.name + "/" + t);
                }
                GameObject theObj = info[infoIndex[pObj.name + "/" + t]].m_pObj;
                cmb.m_sObjectPath += theObj.name;
                sMeshFilterName += theObj.name;
                cmbobjs.Add(theObj);
                cmbmeshes.Add(info[infoIndex[pObj.name + "/" + t]].m_pMesh);
                cmbtextures.Add(info[infoIndex[pObj.name + "/" + t]].m_sFullTexName);
            }
            cmb.m_pCombinedMeshObj = cmbobjs.ToArray();
            cmb.m_pCombinedMesh = cmbmeshes.ToArray();
            cmb.m_sTextureNames = cmbtextures.ToArray();
            cmb.m_sMeshFilterName = sMeshFilterName;
            info.Add(cmb);
        }

        foreach (string namecmbdisable in sDisableList)
        {
            info[infoIndex[namecmbdisable]].m_bDiscard = true;
        }

        iProgressNow = 18*iProgressCount;

        #endregion

        #region Step4: Create Meshes (16x)

        Dictionary<string, string> meshDic = CreateMeshes(info.ToArray(), packed, sParentFolder + "/Resources/CharMesh/", iProgressNow, iProgressFull);
        AssetDatabase.Refresh();
        iProgressNow = 34 * iProgressCount;

        #endregion

        #region Step5: Assemble Game Objects (32x)

        //Create the matrix
        for (int i = 0; i < (int) ECharactorMainColor.ECMC_Max; ++i)
        {
            for (int j = 0; j < (int) ECharactorSubColor.ECSC_Max; ++j)
            {
                CreateObj(i, j, false, info, meshDic, "Assets/" + sPackTextureName + ".mat", pObj, iProgressNow, iProgressFull);
                CreateObj(i, j, true, info, meshDic, "Assets/" + sPackTextureName + ".mat", pObj, iProgressNow, iProgressFull);
                iProgressNow = (34 + ((i * (int)ECharactorSubColor.ECSC_Max) + j + 1) * 2 ) * iProgressCount;
            }
        }

        #endregion

        EditorUtility.ClearProgressBar();
    }

    public static Texture2D[] GetTexture2DWithColor(Texture2D pMain, Texture2D pColor, Texture2D pShadow, Color cMain, Color cSub)
    {
        Texture2D pMainR = (Texture2D) EditorCommonFunctions.GetReadWritable(pMain);
        Texture2D pColorR = (Texture2D)EditorCommonFunctions.GetReadWritable(pColor);
        Texture2D pShadowR = (Texture2D)EditorCommonFunctions.GetReadWritable(pShadow);
        int iWidth = Mathf.Max(pMainR.width, pColorR.width);
        int iHeight = Mathf.Max(pMainR.height, pColorR.height);
        Texture2D ret1 = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, false);
        Texture2D ret2 = new Texture2D(iWidth, iHeight, TextureFormat.RGB24, false);

        for (int i = 0; i < iWidth; ++i)
        {
            for (int j = 0; j < iHeight; ++j)
            {
                Color cMainTex = pMainR.GetPixelBilinear(i / (iWidth - 1.0f), j / (iHeight - 1.0f));
                Color cColorTex = pColorR.GetPixelBilinear(i / (iWidth - 1.0f), j / (iHeight - 1.0f));
                Color cShadowTex = pShadowR.GetPixelBilinear(i / (iWidth - 1.0f), j / (iHeight - 1.0f));

				float shadowcol = Mathf.Clamp01((cShadowTex.r + 0.2f) * (1.0f + cColorTex.b * cColorTex.r));
                float rate = cColorTex.b;
                Color cor = cSub * cColorTex.g + cMain * cColorTex.r;
                Color realC = cMainTex * (1.0f - rate) + cor * rate;
                realC = 1.5f * shadowcol * realC;
                realC.a = 1.0f;

                Color invC = new Color(realC.r - 0.5f, realC.g - 0.5f, realC.b - 0.5f) * 0.25f + 0.15f * cMain + new Color(0.2f, 0.2f, 0.2f);

                ret1.SetPixel(i, j, realC);
                ret2.SetPixel(i, j, invC);
            }
        }

        return new[] {ret1, ret2};
    }

    public static Dictionary<string, string> CreateMeshes(SCharRendererFullInfo[] mesh, Dictionary<string, Rect> rectList, string sFileName, int iProgress, int iProgressFull)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        //Load Mesh
        foreach (SCharRendererFullInfo info in mesh)
        {
            if (!info.m_bDiscard)
            {
                for (int i = 0; i < (int)ECharactorMainColor.ECMC_Max; ++i)
                {
                    for (int j = 0; j < (int)ECharactorSubColor.ECSC_Max; ++j)
                    {
                        ++iProgress;
                        EditorUtility.DisplayProgressBar("正在生成Atlas模型", string.Format("{0}/{1}", iProgress, iProgressFull),
                            iProgress / (float)iProgressFull);


                        if (!info.m_bCombine)
                        {
                            #region Not Combine

                            string sTexture1 = string.Format("{0}_M{1}S{2}", info.m_sFullTexName, i, j);
                            string sTexture2 = string.Format("{0}_M{1}S{2}I", info.m_sFullTexName, i, j);

                            Mesh theMesh = (Mesh) EditorCommonFunctions.GetReadWritable(info.m_pMesh);
                            Mesh theMesh1 = new Mesh();
                            Mesh theMesh2 = new Mesh();

                            theMesh1.SetVertices(theMesh.vertices.ToList());
                            theMesh2.SetVertices(theMesh.vertices.ToList());
                            theMesh1.SetTriangles(theMesh.triangles.ToList(), 0);
                            theMesh2.SetTriangles(theMesh.triangles.ToList(), 0);

                            List<Vector2> list1 = new List<Vector2>();
                            List<Vector2> list2 = new List<Vector2>();
                            for (int k = 0; k < theMesh.uv.Length; ++k)
                            {
                                Vector2 vUV = theMesh.uv[k];
                                if (theMesh.uv[k].x < -0.01f || theMesh.uv[k].y < -0.01f || theMesh.uv[k].x > 1.01f ||
                                    theMesh.uv[k].y > 1.01f)
                                {
                                    CRuntimeLogger.LogWarning("UV is not in 0 - 1, clampled!: FBX:" +
                                                              CommonFunctions.GetLastName(info.m_sMeshFilterName)
                                                              + " Model:" + info.m_pMesh.name);
                                }

                                vUV.x = Mathf.Clamp01(theMesh.uv[k].x);
                                vUV.y = Mathf.Clamp01(theMesh.uv[k].y);

                                Rect rect1 = rectList[sTexture1];
                                Rect rect2 = rectList[sTexture2];
                                list1.Add(new Vector2(vUV.x*rect1.width + rect1.xMin, vUV.y*rect1.height + rect1.yMin));
                                list2.Add(new Vector2(vUV.x*rect2.width + rect2.xMin, vUV.y*rect2.height + rect2.yMin));
                            }
                            theMesh1.uv = list1.ToArray();
                            theMesh2.uv = list2.ToArray();

                            theMesh1.uv2 = null;
                            theMesh2.uv2 = null;
                            theMesh1.normals = null;
                            theMesh2.normals = null;
                            theMesh1.colors = null;
                            theMesh2.colors = null;
                            theMesh1.tangents = null;
                            theMesh2.tangents = null;

                            theMesh2.Optimize();

                            //Create Mesh
                            string sName1 = string.Format("M{0}S{1}/", i, j) +
                                            CommonFunctions.GetLastName(info.m_sMeshFilterName)
                                            + "_" + info.m_pMesh.name
                                            + "_" + info.m_sFullTexName;
                            string sName2 = string.Format("M{0}S{1}I/", i, j) +
                                            CommonFunctions.GetLastName(info.m_sMeshFilterName)
                                            + "_" + info.m_pMesh.name
                                            + "_" + info.m_sFullTexName;

                            AssetDatabase.CreateAsset(theMesh1, "Assets/" + sFileName + sName1 + ".asset");
                            AssetDatabase.CreateAsset(theMesh2, "Assets/" + sFileName + sName2 + ".asset");
                            //AssetDatabase.Refresh();
                            if (ret.ContainsKey(sName1))
                            {
                                CRuntimeLogger.LogWarning("有mesh的FBX文件重名！" + sName1);
                            }
                            else
                            {
                                ret.Add(sName1, "Assets/" + sFileName + sName1 + ".asset");
                            }
                            if (ret.ContainsKey(sName2))
                            {
                                CRuntimeLogger.LogWarning("有mesh的FBX文件重名！" + sName2);
                            }
                            else
                            {
                                ret.Add(sName2, "Assets/" + sFileName + sName1 + ".asset");
                            }
                            #endregion
                        }
                        else
                        {
                            #region Combine

                            List<Vector3> poses = new List<Vector3>();
                            List<int> indexes = new List<int>();
                            List<Vector2> uvs1 = new List<Vector2>();
                            List<Vector2> uvs2 = new List<Vector2>();

                            for (int objIndex = 0; objIndex < info.m_pCombinedMeshObj.Length; ++objIndex)
                            {
                                string sTexture1 = string.Format("{0}_M{1}S{2}", info.m_sTextureNames[objIndex], i, j);
                                string sTexture2 = string.Format("{0}_M{1}S{2}I", info.m_sTextureNames[objIndex], i, j);
                                Mesh theMesh = (Mesh)EditorCommonFunctions.GetReadWritable(info.m_pCombinedMesh[objIndex]);

                                if (0 == objIndex)
                                {
                                    poses = theMesh.vertices.ToList();
                                }
                                else
                                {
                                    foreach (Vector3 vets in theMesh.vertices)
                                    {
                                        Vector3 worldPos = info.m_pCombinedMeshObj[objIndex].transform.localToWorldMatrix*vets;
                                        poses.Add(info.m_pCombinedMeshObj[0].transform.worldToLocalMatrix * worldPos);
                                    }
                                }

                                if (0 == objIndex)
                                {
                                    indexes = theMesh.triangles.ToList();
                                }
                                else
                                {
                                    int iIndexNow = indexes.Count;
                                    foreach (int oneind in theMesh.triangles)
                                    {
                                        indexes.Add(oneind + iIndexNow);
                                    }
                                }

                                for (int k = 0; k < theMesh.uv.Length; ++k)
                                {
                                    Vector2 vUV = theMesh.uv[k];
                                    vUV.x = Mathf.Clamp01(theMesh.uv[k].x);
                                    vUV.y = Mathf.Clamp01(theMesh.uv[k].y);
                                    Rect rect1 = rectList[sTexture1];
                                    Rect rect2 = rectList[sTexture2];

                                    uvs1.Add(new Vector2(vUV.x * rect1.width + rect1.xMin, vUV.y * rect1.height + rect1.yMin));
                                    uvs2.Add(new Vector2(vUV.x * rect2.width + rect2.xMin, vUV.y * rect2.height + rect2.yMin));
                                }
                            }

                            Mesh theMesh1 = new Mesh();
                            Mesh theMesh2 = new Mesh();

                            theMesh1.SetVertices(poses);
                            theMesh2.SetVertices(poses);
                            theMesh1.SetTriangles(indexes, 0);
                            theMesh2.SetTriangles(indexes, 0);
                            theMesh1.uv = uvs1.ToArray();
                            theMesh2.uv = uvs2.ToArray();

                            theMesh1.uv2 = null;
                            theMesh2.uv2 = null;
                            theMesh1.normals = null;
                            theMesh2.normals = null;
                            theMesh1.colors = null;
                            theMesh2.colors = null;
                            theMesh1.tangents = null;
                            theMesh2.tangents = null;

                            theMesh2.Optimize();

                            //Create Mesh
                            string sName1 = string.Format("M{0}S{1}/cmb_", i, j) +
                                            CommonFunctions.GetLastName(info.m_sMeshFilterName);
                            string sName2 = string.Format("M{0}S{1}I/cmb_", i, j) +
                                            CommonFunctions.GetLastName(info.m_sMeshFilterName);

                            AssetDatabase.CreateAsset(theMesh1, "Assets/" + sFileName + sName1 + ".asset");
                            AssetDatabase.CreateAsset(theMesh2, "Assets/" + sFileName + sName2 + ".asset");
                            //AssetDatabase.Refresh();
                            if (ret.ContainsKey(sName1))
                            {
                                CRuntimeLogger.LogWarning("有mesh的FBX文件重名！" + sName1);
                            }
                            else
                            {
                                ret.Add(sName1, "Assets/" + sFileName + sName1 + ".asset");
                            }
                            if (ret.ContainsKey(sName2))
                            {
                                CRuntimeLogger.LogWarning("有mesh的FBX文件重名！" + sName2);
                            }
                            else
                            {
                                ret.Add(sName2, "Assets/" + sFileName + sName1 + ".asset");
                            }

                            #endregion
                        }
                    }
                }                
            }
        }

        return ret;
    }

    public static void CreateObj(int i, int j, bool bInv, List<SCharRendererFullInfo> info, Dictionary<string, string> meshDic,
        string sMatName, GameObject pObj, int iProgressNow, int iProgressFull)
    {
        GameObject copy = Object.Instantiate(pObj, pObj.transform.position + Vector3.left * 3.0f * (i + 1) + Vector3.back * 3.0f * j, pObj.transform.rotation) as GameObject;
        Debug.Assert(copy != null, "copy != null");
        string sTypeName = string.Format("M{0}S{1}{2}", i, j, bInv ? "I" : "");
        string sObjName = pObj.name + sTypeName;
        copy.name = sObjName;
        
        foreach (SCharRendererFullInfo oneinfo in info)
        {
            ++iProgressNow;
            EditorUtility.DisplayProgressBar("正在组装", string.Format("{0}/{1}", iProgressNow, iProgressFull),
                iProgressNow / (float)iProgressFull);

            if (!oneinfo.m_bDiscard)
            {
                if (oneinfo.m_bCombine)
                {
                    //Create the new obj
                    GameObject newObj = new GameObject(CommonFunctions.GetLastName(oneinfo.m_sMeshFilterName));
                    newObj.transform.parent = oneinfo.m_pTransfParent.transform.parent;
                    newObj.transform.localPosition = oneinfo.m_pTransfParent.transform.localPosition;
                    newObj.transform.localRotation = oneinfo.m_pTransfParent.transform.localRotation;
                    newObj.transform.localScale = oneinfo.m_pTransfParent.transform.localScale;

                    MeshFilter filter = newObj.AddComponent<MeshFilter>();
                    string sName = sTypeName + "/cmb_"  + CommonFunctions.GetLastName(oneinfo.m_sMeshFilterName);
                    string sPath = meshDic[sName];
                    filter.sharedMesh = AssetDatabase.LoadAssetAtPath(sPath, typeof(Mesh)) as Mesh;

                    MeshRenderer renderer = newObj.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath(sMatName, typeof(Material)) as Material;
                    renderer.useLightProbes = false;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                }
                else
                {
                    string sName = sTypeName + "/" + CommonFunctions.GetLastName(oneinfo.m_sMeshFilterName)
                                            + "_" + oneinfo.m_pMesh.name
                                            + "_" + oneinfo.m_sFullTexName;
                    string sPath = meshDic[sName];
                    GameObject toReplace = GameObject.Find(oneinfo.m_sObjectPath.Replace(pObj.name, sObjName));
                    toReplace.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath(sMatName, typeof(Material)) as Material;
                    toReplace.GetComponent<Renderer>().useLightProbes = false;
                    toReplace.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                    toReplace.GetComponent<Renderer>().receiveShadows = false;
                    toReplace.GetComponent<Renderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
                    toReplace.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath(sPath, typeof(Mesh)) as Mesh;
                }
            }
        }

        foreach (SCharRendererFullInfo oneinfo in info)
        {
            if (oneinfo.m_bDiscard)
            {
                GameObject toReplace = GameObject.Find(oneinfo.m_sObjectPath.Replace(pObj.name, sObjName));
                Object.DestroyImmediate(toReplace);
            }
        }
    }

    #endregion

    #region Old Version

    public struct SRendererInfo
    {
        public string m_sTextureName;
        public string m_sMeshFilterName;
        public Shader m_pShader;
        public string m_sObjectPath;

        public Texture2D m_pTexture;
        public Mesh m_pMesh;
        public GameObject m_pObj;
    }

    public static void MakeAtlasReplace(GameObject pObj, string sParentFolder, bool bForceTransparent = false)
    {
        int iProgressFull = pObj.GetComponentsInChildren<MeshRenderer>(true).Length;
        iProgressFull = iProgressFull*3;
        int iProgressNow = 0;

        //========================================
        //Step 1, find all renderes need to atlas
        Dictionary<Shader, List<SRendererInfo>> typedRenderers = new Dictionary<Shader, List<SRendererInfo>>();
        foreach (MeshRenderer renderer in pObj.GetComponentsInChildren<MeshRenderer>(true))
        {
            ++iProgressNow;
            EditorUtility.DisplayProgressBar("正在收集Renderer信息", string.Format("{0}/{1}", iProgressNow, iProgressFull),
                iProgressNow/(float) iProgressFull);
            if (!typedRenderers.ContainsKey(renderer.sharedMaterial.shader))
            {
                typedRenderers.Add(renderer.sharedMaterial.shader, new List<SRendererInfo>());
            }
            typedRenderers[renderer.sharedMaterial.shader].Add(new SRendererInfo
            {
                m_sTextureName = AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture),
                m_sMeshFilterName = AssetDatabase.GetAssetPath(renderer.gameObject.GetComponent<MeshFilter>().sharedMesh),
                m_pShader = renderer.sharedMaterial.shader,
                m_sObjectPath = CommonFunctions.FindFullName(pObj, renderer.gameObject),
                m_pTexture = (Texture2D)renderer.sharedMaterial.mainTexture,
                m_pMesh = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh,
                m_pObj = renderer.gameObject,
            });

            if (null == renderer.sharedMaterial.mainTexture)
            {
                CRuntimeLogger.LogError("有的材质没有贴图: " + CommonFunctions.FindFullName(pObj, renderer.gameObject));
                EditorUtility.ClearProgressBar();
                return;
            }
        }

        CommonFunctions.CreateFolder(Application.dataPath + "/" + sParentFolder + "/Textures/?");
        CommonFunctions.CreateFolder(Application.dataPath + "/" + sParentFolder + "/Meshes/?");
        GameObject copy = Object.Instantiate(pObj, pObj.transform.position + Vector3.left * 3.0f, pObj.transform.rotation) as GameObject;
        Debug.Assert(copy != null, "copy != null");
        copy.name = pObj.name + "_Atlas";

        foreach (KeyValuePair<Shader, List<SRendererInfo>> kvp in typedRenderers)
        {
            //========================================
            //Create Texture and Material
            Dictionary<string, Texture2D> uniqueTextures = new Dictionary<string, Texture2D>();
            foreach (SRendererInfo info in kvp.Value)
            {
                if (!uniqueTextures.ContainsKey(info.m_sTextureName))
                {
                    uniqueTextures.Add(info.m_sTextureName, info.m_pTexture);
                }
            }
            Texture2D[] allTextures = uniqueTextures.Values.ToArray();
            string sTextureFileName = sParentFolder + "/Textures/" + kvp.Key.name.Replace("/", "_").Replace(".", "_");
            Dictionary<string, Rect> textureDic = EditorCommonFunctions.PackTextureInDataBase(allTextures,
                sTextureFileName,
                kvp.Key,
                1);

            //========================================
            //Create Mesh with new UV
            Dictionary<string, SRendererInfo> uniqueMeshes = new Dictionary<string, SRendererInfo>();
            foreach (SRendererInfo info in kvp.Value)
            {
                string sName = "f_" + CommonFunctions.GetLastName(info.m_sMeshFilterName) + "_m_" + info.m_pMesh.name + "_t_" + info.m_pTexture.name;
                if (!uniqueMeshes.ContainsKey(sName))
                {
                    uniqueMeshes.Add(sName, info);
                }
            }

            Dictionary<string, string> oldNewMeshDic = CreateMeshes(
                uniqueMeshes.Values.ToArray(), 
                textureDic, 
                sParentFolder + "/Meshes/",
                iProgressNow,
                iProgressFull);

            iProgressNow += oldNewMeshDic.Count;

            //========================================
            //Create a new gameobject and replace the material and mesh filter
            foreach (SRendererInfo info in kvp.Value)
            {
                ++iProgressNow;
                EditorUtility.DisplayProgressBar("正在组装", string.Format("{0}/{1}", iProgressNow, iProgressFull),
                    iProgressNow / (float)iProgressFull);
                string sName = "f_" + CommonFunctions.GetLastName(info.m_sMeshFilterName) + "_m_" + info.m_pMesh.name + "_t_" + info.m_pTexture.name;
                string sPath = oldNewMeshDic[sName];
                GameObject toReplace = GameObject.Find(info.m_sObjectPath.Replace(pObj.name, pObj.name + "_Atlas"));
                toReplace.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath("Assets/" + sTextureFileName + ".mat", typeof(Material)) as Material;
                toReplace.GetComponent<Renderer>().useLightProbes = false;
                toReplace.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                toReplace.GetComponent<Renderer>().receiveShadows = false;
                toReplace.GetComponent<Renderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
                toReplace.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath(sPath, typeof(Mesh)) as Mesh;
            }

            iProgressNow += oldNewMeshDic.Count;
        }
        EditorUtility.ClearProgressBar();
    }

    public static Dictionary<string, string> CreateMeshes(SRendererInfo[] mesh, Dictionary<string, Rect> rectList, string sFileName, int iProgress, int iProgressFull)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        //Load Mesh
        foreach (SRendererInfo info in mesh)
        {
            Mesh theMesh = EditorCommonFunctions.LoadMeshAtPathWithName(info.m_sMeshFilterName, info.m_pMesh.name);
            Mesh theMesh2 = new Mesh();
            ++iProgress;
            EditorUtility.DisplayProgressBar("正在生成Atlas模型", string.Format("{0}/{1}", iProgress, iProgressFull),
                iProgress / (float)iProgressFull);
            List<Vector3> list1 = new List<Vector3>();
            for (int i = 0; i < theMesh.vertices.Length; ++i)
            {
                list1.Add(theMesh.vertices[i]);
            }
            theMesh2.vertices = list1.ToArray();

            List<int> list2 = new List<int>();
            for (int i = 0; i < theMesh.triangles.Length; ++i)
            {
                list2.Add(theMesh.triangles[i]);
            }
            theMesh2.triangles = list2.ToArray();

            List<Vector2> list3 = new List<Vector2>();
            for (int i = 0; i < theMesh.uv.Length; ++i)
            {
                Vector2 vUV = theMesh.uv[i];
                if (theMesh.uv[i].x < -0.01f || theMesh.uv[i].y < -0.01f || theMesh.uv[i].x > 1.01f || theMesh.uv[i].y > 1.01f)
                {
                    CRuntimeLogger.LogWarning("UV is not in 0 - 1, clampled!: FBX:" +
                                              CommonFunctions.GetLastName(info.m_sMeshFilterName) 
                                              + " Model:" + info.m_pMesh.name);
                    vUV.x = Mathf.Repeat(theMesh.uv[i].x, 1.0f);
                    vUV.y = Mathf.Repeat(theMesh.uv[i].y, 1.0f);
                }
                else
                {
                    vUV.x = Mathf.Clamp01(theMesh.uv[i].x);
                    vUV.y = Mathf.Clamp01(theMesh.uv[i].y);
                }
                Rect rect = rectList[info.m_sTextureName];
                list3.Add(new Vector2(vUV.x * rect.width + rect.xMin, vUV.y * rect.height + rect.yMin));
            }
            theMesh2.uv = list3.ToArray();

            List<Vector2> list4 = new List<Vector2>();
            for (int i = 0; i < theMesh.uv2.Length; ++i)
            {
                list4.Add(theMesh.uv2[i]);
            }
            theMesh2.uv2 = list4.ToArray();

            List<Vector3> list5 = new List<Vector3>();
            for (int i = 0; i < theMesh.normals.Length; ++i)
            {
                list5.Add(theMesh.normals[i]);
            }
            theMesh2.normals = list5.ToArray();

            List<Color> list6 = new List<Color>();
            for (int i = 0; i < theMesh.colors.Length; ++i)
            {
                list6.Add(theMesh.colors[i]);
            }
            theMesh2.colors = list6.ToArray();

            List<Vector4> list7 = new List<Vector4>();
            for (int i = 0; i < theMesh.tangents.Length; ++i)
            {
                list7.Add(theMesh.tangents[i]);
            }
            theMesh2.tangents = list7.ToArray();

            theMesh2.Optimize();

            //Create Mesh
            string sName = "f_" + CommonFunctions.GetLastName(info.m_sMeshFilterName) + "_m_" + info.m_pMesh.name + "_t_" + info.m_pTexture.name;
            string thePath = "Assets/" + sFileName + sName + ".asset";
            AssetDatabase.CreateAsset(theMesh2, thePath);
            AssetDatabase.Refresh();
            if (ret.ContainsKey(sName))
            {
                CRuntimeLogger.LogError("有mesh的FBX文件重名！");
            }
            else
            {
                ret.Add(sName, thePath);
            }
        }

        return ret;
    }

    #endregion
}
