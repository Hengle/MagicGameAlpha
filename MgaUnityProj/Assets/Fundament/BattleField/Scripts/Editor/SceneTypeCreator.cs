using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class SceneTypeCreator : ScriptableWizard
{
    public string TypeName = "";
    public bool CanRotate = false;
    public bool HasGroundOffset = true;
    public int CliffType = 0;
    public ESceneEdgeType EdgeType = ESceneEdgeType.ESET_Water;

    [MenuItem("MGA/Create/Scene/Create Type")]
    public static void CreateWizard()
    {
        DisplayWizard<SceneTypeCreator>("Create Scene Type", "Create");
    }

    public void OnWizardCreate()
    {
        if (string.IsNullOrEmpty(TypeName))
        {
            CRuntimeLogger.LogWarning("你需要输入一个地形类型名称!");
            return;
        }

        BuildTextureDataWithFolder();
    }

    protected void BuildTextureDataWithFolder()
    {
        #region Step1: Find all files

        string sFolderTx = Application.dataPath + "/" 
            + SceneConstant.m_sArtworkPath 
            + TypeName 
            + SceneConstant.m_sSceneTexturesPath;
        string sFolderDoc = Application.dataPath + "/"
            + SceneConstant.m_sArtworkPath
            + TypeName
            + SceneConstant.m_sDecoratesPath;

        CRuntimeLogger.Log("将从" + sFolderTx + "和" + sFolderDoc + "收集信息");
        DirectoryInfo dirtx = new DirectoryInfo(sFolderTx);
        DirectoryInfo dirdoc = new DirectoryInfo(sFolderDoc);
        if (!dirtx.Exists || !dirdoc.Exists)
        {
            CRuntimeLogger.LogWarning("你选择目录内容不太对吧!");
            return;
        }

        List<FileInfo> allTextures = new List<FileInfo>();
        allTextures.AddRange(dirtx.GetFiles("*.png", SearchOption.AllDirectories));
        allTextures.AddRange(dirdoc.GetFiles("*.png", SearchOption.AllDirectories));
        List<FileInfo> allModels = dirdoc.GetFiles("*.fbx", SearchOption.AllDirectories).ToList();

        #endregion

        #region Step2: Create SceneType

        CSceneType scenetypes = new CSceneType();
        scenetypes.Load();
        if (null == scenetypes[TypeName])
        {
            CSceneTypeElement newType = scenetypes.CreateElement(TypeName);
            newType.m_bCanRot = CanRotate;
            newType.m_eEdgeType = EdgeType;
            newType.m_iCliffType = CliffType;
            newType.m_bHasGroundOffset = HasGroundOffset;
        }
        else
        {
            scenetypes[TypeName].m_bCanRot = CanRotate;
            scenetypes[TypeName].m_eEdgeType = EdgeType;
            scenetypes[TypeName].m_iCliffType = CliffType;
            scenetypes[TypeName].m_bHasGroundOffset = HasGroundOffset;
        }
        scenetypes.Save();

        #endregion

        #region Step3: Bake Texture Atlas

        Dictionary<int, int> codes = new Dictionary<int, int>(new IntEqualityComparer());
        Dictionary<string, Texture2D> txDic = new Dictionary<string, Texture2D>();
        foreach (FileInfo fileInfo in allTextures)
        {
            string sFileName = CommonFunctions.GetLastName(fileInfo.FullName);
            string[] twoparts = sFileName.Split('_');
            string sAssetPath = "Assets" + fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), "");
            Texture2D t1 = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(Texture2D)) as Texture2D;
            txDic.Add(sFileName, (Texture2D)EditorCommonFunctions.GetReadWritable(t1));

            //Check whether is a template texture
            int iCode, iRepeat;
            if (2 == twoparts.Length 
             && !string.IsNullOrEmpty(twoparts[0])
             && int.TryParse(twoparts[0].Substring(1, twoparts[0].Length - 1), out iCode)
             && int.TryParse(twoparts[1], out iRepeat))
            {
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

        Dictionary<string, Rect> textureDic = EditorCommonFunctions.PackTexture(
            txDic, 
            SceneConstant.m_sArtworkPath 
            + TypeName
            + SceneConstant.m_sSceneTexturesPath,
            Shader.Find("Mobile/Unlit (Supports Lightmap)"), EditorCommonFunctions.EPackColorFormat.ForcePng);

        #endregion

        #region Step4: Make template

        CSceneGroudTemplate tamplate = new CSceneGroudTemplate { m_sSceneType = TypeName };

        foreach (KeyValuePair<string, Rect> kvp in textureDic)
        {
            CSceneGroudTemplateElement newGT = tamplate.CreateElement();
            tamplate.ChangeName(newGT.m_sElementName, kvp.Key);
            newGT.m_vUV = CommonFunctions.Rect2V4(kvp.Value);
        }
        tamplate.Save();

        #endregion

        #region Step5: Make Scene Texture

        CSceneTexture newT = new CSceneTexture { m_sSceneType = TypeName };

        for (int lt = 0; lt < 3; ++lt)
        {
            for (int rt = 0; rt < 3; ++rt)
            {
                for (int rb = 0; rb < 3; ++rb)
                {
                    for (int lb = 0; lb < 3; ++lb)
                    {
                        int iCode, iRots;
                        bool bFlip;
                        FindCodeAndRot(codes, lt, rt, rb, lb, out iCode, out iRots, out bFlip);
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
                        newE.m_iRotNumber = iRots;
                        newE.m_iTextureCount = codes[iCode];
                        newE.m_bReflect = bFlip;
                    }
                }
            }
        }

        newT.Save();

        #endregion

        #region Step6: Make Decorate

        Dictionary<int, string> modelTemplates = new Dictionary<int, string>(new IntEqualityComparer());
        Dictionary<int, int> modelRepeates = new Dictionary<int, int>(new IntEqualityComparer());
        foreach (FileInfo fileInfo in allModels)
        {
            string sFileName = CommonFunctions.GetLastName(fileInfo.FullName);
            string[] twoparts = sFileName.Split('_');
            string sAssetPath = "Assets" + fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), "");

            //Check whether is a template texture
            int iCode, iRepeat;
            if (3 == twoparts.Length
                && int.TryParse(twoparts[1], out iCode)
                && int.TryParse(twoparts[2], out iRepeat)
                && iCode < 3 && iCode >= 0)
            {
                //Make template data
                if (!modelTemplates.ContainsKey(iCode))
                {
                    modelTemplates.Add(iCode, twoparts[0] + "_" + iCode);
                }

                if (!modelRepeates.ContainsKey(iCode))
                {
                    modelRepeates.Add(iCode, iRepeat);
                }
                else if (modelRepeates[iCode] < iRepeat)
                {
                    modelRepeates[iCode] = iRepeat;
                }

                //Make the mesh
                List<Vector3> verts = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<int> trangles = new List<int>();

                GameObject go = AssetDatabase.LoadMainAssetAtPath(sAssetPath) as GameObject;
                if (null != go)
                {
                    foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>(true))
                    {
                        Mesh editablemesh = (Mesh) EditorCommonFunctions.GetReadWritable(mf.sharedMesh);
                        int iVertCount = verts.Count;
                        Rect rct;

                        MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
                        if (null != mr && null != mr.sharedMaterial && null != mr.sharedMaterial.mainTexture &&
                            textureDic.ContainsKey(mr.sharedMaterial.mainTexture.name))
                        {
                            rct = textureDic[mr.sharedMaterial.mainTexture.name];
                        }
                        else if (textureDic.ContainsKey(editablemesh.name))
                        {
                            rct = textureDic[editablemesh.name];
                        }
                        else
                        {
                            CRuntimeLogger.LogWarning("Mesh找不到贴图：" + sAssetPath);
                            return;
                        }

                        if (mf.transform == go.transform)
                        {
                            verts.AddRange(editablemesh.vertices);
                        }
                        else
                        {
                            for (int i = 0; i < editablemesh.vertices.Length; ++i)
                            {
                                Vector3 worldPos = mf.transform.localToWorldMatrix.MultiplyVector(editablemesh.vertices[i]) + mf.transform.position;
                                verts.Add(go.transform.worldToLocalMatrix.MultiplyVector(worldPos - go.transform.position));
                            }
                        }
                        for (int i = 0; i < editablemesh.uv.Length; ++i)
                        {
                            float fX = Mathf.Clamp01(editablemesh.uv[i].x);
                            float fY = Mathf.Clamp01(editablemesh.uv[i].y);
                            uvs.Add(new Vector2(rct.xMin + rct.width * fX, rct.yMin + rct.height * fY));
                        }
                        for (int i = 0; i < editablemesh.triangles.Length; ++i)
                        {
                            trangles.Add(iVertCount + editablemesh.triangles[i]);
                        }
                    }
                }
                Mesh theMesh = new Mesh { vertices = verts.ToArray(), uv = uvs.ToArray(), triangles = trangles.ToArray() };
                theMesh.RecalculateNormals();
                Unwrapping.GenerateSecondaryUVSet(theMesh);
                ;
                AssetDatabase.CreateAsset(theMesh, "Assets/"
                    + SceneConstant.m_sArtworkPath + TypeName
                    + SceneConstant.m_sDecoratesPath
                    + "/Generate/" + sFileName + ".asset");
            }
        }

        CSceneDecorate doc = new CSceneDecorate { m_sSceneType = TypeName };
        CSceneDecorate olddoc = new CSceneDecorate { m_sSceneType = TypeName };
        olddoc.Load();
        foreach (KeyValuePair<int, string> kvp in modelTemplates)
        {
            CSceneDecorateElement element = doc.CreateElement(kvp.Value);
            if (element.m_iID != kvp.Key)
            {
                doc.ChangeId(element.m_iID, kvp.Key);
            }
            element.m_iDecrateRepeat = modelRepeates[kvp.Key];
            if (null != olddoc[kvp.Key])
            {
                element.m_bBlockPathfinding = olddoc[kvp.Key].m_bBlockPathfinding;
                element.m_iDecrateSize = olddoc[kvp.Key].m_iDecrateSize;
                element.m_bOnlyRotateY = olddoc[kvp.Key].m_bOnlyRotateY;
            }
        }
        doc.Save();
        AssetDatabase.Refresh();

        #endregion

    }

    static public void FindCodeAndRot(Dictionary<int, int> dics, int lt, int rt, int rb, int lb, out int codes, out int rots, out bool bFlip)
    {
        rots = -1;
        codes = -1;
        bFlip = false;

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
