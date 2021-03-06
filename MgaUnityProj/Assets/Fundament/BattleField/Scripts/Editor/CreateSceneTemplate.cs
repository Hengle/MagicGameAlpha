using UnityEngine;
using UnityEditor;

public class CreateSceneTemplate : ScriptableWizard
{
    public Texture2D HeightMap;
    public Texture2D GroundMap;
    public Texture2D DocMap;
    public string GroundType;
    public string TemplateName;

    [MenuItem("MGA/Create/Scene/Create Template")]
    static void CreateWizard()
    {
        DisplayWizard<CreateSceneTemplate>("Create Scene Template", "Create");
    }

    void OnWizardCreate()
    {
        #region Step1: Check files

        if (string.IsNullOrEmpty(TemplateName))
        {
            CRuntimeLogger.LogError("必须有个模板名称");
            return;
        }

        Texture2D heighttx = (Texture2D)EditorCommonFunctions.GetReadWritable(HeightMap);
        if (null == heighttx || SceneConstant.m_iHightmapSize != heighttx.width || SceneConstant.m_iHightmapSize != heighttx.height)
        {
            CRuntimeLogger.LogError(string.Format("必须是{0}x{0}的高度贴图，如果你的图片正确，可以检查下导入设置是否是Editor/GUI", SceneConstant.m_iHightmapSize));
            return;
        }
        Texture2D groundtx = (Texture2D)EditorCommonFunctions.GetReadWritable(GroundMap);
        if (null == groundtx || SceneConstant.m_iHightmapSize != groundtx.width || SceneConstant.m_iHightmapSize != groundtx.height)
        {
            CRuntimeLogger.LogError(string.Format("必须是{0}x{0}的地皮贴图，如果你的图片正确，可以检查下导入设置是否是Editor/GUI", SceneConstant.m_iHightmapSize));
            return;
        }
        Texture2D doctx = (Texture2D)EditorCommonFunctions.GetReadWritable(DocMap);
        if (null == doctx || SceneConstant.m_iDecoratemapSize != doctx.width || SceneConstant.m_iDecoratemapSize != doctx.height)
        {
            CRuntimeLogger.LogError(string.Format("必须是{0}x{0}的装饰贴图", SceneConstant.m_iDecoratemapSize));
            return;
        }

        #endregion

        #region Step2: Create

        string sGroundFileName = SceneConstant.m_sArtworkPath + "Templates/"
            + string.Format("T{0}X{0}", SceneConstant.m_iHightmapSize)
            + "/GenerateMesh/" + TemplateName + ".asset";

        Texture2D wmap, nmap;
        SceneEditorUtility.CreateGround(heighttx, groundtx, doctx, sGroundFileName, GroundType, false, out wmap, out nmap);


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

        #region Step3: Make Record

        CSceneTemplate templates = new CSceneTemplate();
        templates.Load();
        CSceneTemplateElement element = templates[TemplateName];
        if (null == element)
        {
            element = templates.CreateElement(TemplateName);
        }

        element.m_sDecoratePath = AssetDatabase.GetAssetPath(DocMap);
        element.m_sGroundPath = AssetDatabase.GetAssetPath(GroundMap);
        element.m_sHeightPath = AssetDatabase.GetAssetPath(HeightMap);

        templates.Save();

        #endregion

    }
}
