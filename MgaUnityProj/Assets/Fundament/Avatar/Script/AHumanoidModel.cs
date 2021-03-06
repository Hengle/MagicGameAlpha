using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[AddComponentMenu("MGA/Avatar/AHumanoidModel")]
public class AHumanoidModel : ACharactorModel
{
    public const int m_iMaxCount = 1000;

    // Use this for initialization
    public void OnEnable()
    {
        m_fFixBloodLine = Random.Range(-1.0f, -0.5f);
        m_iFixBloodLineTick = Random.Range(0, 4);
        m_fCheckLod = -1.0f;

        m_bHideLod = false;
        foreach (MeshRenderer r in m_pAllLODRenderer)
        {
            if (null != r)
            {
                r.enabled = true;    
            }
        }
    }

    // Update is called once per frame
    private const float m_fFixBloodLineSep = 0.5f;
    private const int m_iFixBloodLineSep = 4;
    private int m_iFixBloodLineTick = 4;
    private float m_fFixBloodLine = 0.5f;

    private bool m_bHideLod = false;
    private const float m_fLod = 1600.0f;

    private const float m_fLod2 = 2000.0f;
    private const float m_fLodSep = 0.5f;
    private float m_fCheckLod = -1.0f;

    private void Update()
    {
        if (!m_bFirstFrameInitial)
        {
            FirstFrameInitial();
        }

        m_fFixBloodLine -= Time.deltaTime;
        --m_iFixBloodLineTick;
        if (m_fFixBloodLine <= 0.0f && m_iFixBloodLineTick < 0)
        {
            m_fFixBloodLine += m_fFixBloodLineSep;
            m_iFixBloodLineTick += m_iFixBloodLineSep;
            if ((m_pBloodLineShell.eulerAngles - Camera.main.transform.eulerAngles).sqrMagnitude > 1.0f)
            {
                m_pBloodLineShell.eulerAngles = Camera.main.transform.eulerAngles;
            }
            if (Mathf.Abs(m_pShadow.transform.position.y - 0.05f) > 0.0f)
            {
                m_pShadow.transform.position = new Vector3(m_pShadow.transform.position.x, 0.05f, m_pShadow.transform.position.z);
            }
        }

        m_fCheckLod -= Time.deltaTime;
        if (m_fCheckLod < 0.0f)
        {
            float fSq = (transform.position - Camera.main.transform.position).sqrMagnitude;
            if (fSq > m_fLod2/m_fModelSize)
            {
                if (!m_bHideLod)
                {
                    m_bHideLod = true;
                    foreach (MeshRenderer r in m_pAllLODRenderer)
                    {
                        r.enabled = false;
                    }
                }
                m_fCheckLod = m_fLodSep;
            }
            else if (fSq > m_fLod / m_fModelSize)
            {
                if (!m_bHideLod)
                {
                    m_bHideLod = true;
                    foreach (MeshRenderer r in m_pAllLODRenderer)
                    {
                        r.enabled = false;
                    }
                }
            }
            else
            {
                if (m_bHideLod)
                {
                    m_bHideLod = false;
                    foreach (MeshRenderer r in m_pAllLODRenderer)
                    {
                        r.enabled = true;
                    }
                }
            }            
        }
    }

    #region Assemble

    public GameObject[] m_pAllComponents;

    //The max component linked together is 2.
    //store as (index + 1) * m_iMaxCount + (index + 1)
    public int[] m_pMaleFeet; // left * m_iMaxCount + right
    public int[] m_pFemaleFeet; // left * m_iMaxCount + right
    public int[] m_pHands; // left * m_iMaxCount + right
    public int[] m_pMaleHead; //head and face, or head and hair
    public int[] m_pFemaleHead;

    public int[] m_pFemaleBody; //0 x m_iMaxCount + (index + 1)
    public int[] m_pMaleBody; //0 x m_iMaxCount + (index + 1)
    public int[] m_pBacks; //0 x m_iMaxCount + (index + 1)

    public int[] m_pWings; // left * m_iMaxCount + right

    //Weapons
    public int[] m_pAllPosibleSHandCombineFight; //left * m_iMaxCount + right, or shield * m_iMaxCount + right, or 0 * m_iMaxCount + right
    public int[] m_pAllPosibleSHandCombineWizard; //shield * m_iMaxCount + right, or 0 * m_iMaxCount + right
    public int[] m_pAllPosibleDHandCombineFight;
    public int[] m_pAllPosibleDHandCombineWizard;
    public int[] m_pAllPosibleShoot;

    protected static readonly Vector3[] m_vModelSizes =
    {
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.8f, 0.8f, 0.8f),
        new Vector3(1.2f, 1.2f, 1.2f),
        new Vector3(0.9f, 1.15f, 0.9f),
        new Vector3(1.1f, 0.9f, 1.1f),
    };

    //Other things
    public GameObject[] m_pBoltPos;
    public GameObject m_pCurrentBoltPos;
    public GameObject m_pShadow;
    public Transform m_pBloodLineShell;
    public GameObject[] m_pBloodLineFront;

    public List<MeshRenderer> m_pAllShowModelRenderer;
    public List<MeshRenderer> m_pAllLODRenderer;

    public EHumanoidWeapon m_eHumanWeapon = EHumanoidWeapon.EHW_BareHand;
    public EHumanoidType m_eHumanType = EHumanoidType.EHT_Both;

    #endregion

    protected MeshFilter[] m_pAllVisibleFilters = null;
    protected Mesh[,] m_pAllVisibleMeshes = null;

    public void Randomize(ECharactorAttackType eCharactorAttack, ECharactorMoveType eMove, EHumanoidType eGendle, ECharactorSubColor subC, bool bFix = false)
    {
        m_eAttackType = eCharactorAttack;
        m_eMoveType = eMove;
        m_eCharactorType = ECharactorType.ECT_Humanoid;
        m_eHumanType = eGendle;

        m_eVisible = ECharactorVisible.ECV_None;
        m_eCamp = ECharactorCamp.ECC_Max;

        m_pAllShowModelRenderer = new List<MeshRenderer>();
        m_pAllLODRenderer = new List<MeshRenderer>();

        #region Decide components

        if (ECharactorMoveType.ECMT_Sky != eMove && Random.Range(0.0f, 1.0f) > 0.7f)
        {
            ShowWithIndex(m_pBacks[Random.Range(0, m_pBacks.Length)]);    
        }
        if (ECharactorMoveType.ECMT_Sky == eMove)
        {
            ShowWithIndex(m_pWings[Random.Range(0, m_pWings.Length)], false);
        }

        if (eGendle == EHumanoidType.EHT_Female)
        {
            ShowWithIndex(m_pFemaleBody[Random.Range(0, m_pFemaleBody.Length)], false);
            ShowWithIndex(m_pFemaleFeet[Random.Range(0, m_pFemaleFeet.Length)]);
            ShowWithIndex(m_pFemaleHead[Random.Range(0, m_pFemaleHead.Length)], false);
        }
        else
        {
            ShowWithIndex(m_pMaleBody[Random.Range(0, m_pMaleBody.Length)], false);
            ShowWithIndex(m_pMaleFeet[Random.Range(0, m_pMaleFeet.Length)]);
            ShowWithIndex(m_pMaleHead[Random.Range(0, m_pMaleHead.Length)], false);
        }

        #region Decide weapon

        List<EHumanoidWeapon> weapons = new List<EHumanoidWeapon>();
        switch (eCharactorAttack)
        {
            case ECharactorAttackType.ECAT_Mele:
                weapons.Add(EHumanoidWeapon.EHT_SingleHand_Fight);
                weapons.Add(EHumanoidWeapon.EHT_DoubleHand_Fight);
                break;
            case ECharactorAttackType.ECAT_Range:
                weapons.Add(EHumanoidWeapon.EHT_Shoot);
                break;
            case ECharactorAttackType.ECAT_Wizard:
                weapons.Add(EHumanoidWeapon.EHT_SingleHand_Wizard);
                weapons.Add(EHumanoidWeapon.EHT_DoubleHand_Wizard);
                weapons.Add(EHumanoidWeapon.EHW_BareHand);
                break;
        }
        m_eHumanWeapon = weapons[Random.Range(0, weapons.Count)];

        bool bDoubleHandSWeapon = false;
        switch (m_eHumanWeapon)
        {
            case EHumanoidWeapon.EHW_BareHand:
                ShowWithIndex(m_pHands[Random.Range(0, m_pHands.Length)]);
                break;
            case EHumanoidWeapon.EHT_Shoot:
                ShowWithIndex(m_pAllPosibleShoot[Random.Range(0, m_pAllPosibleShoot.Length)]);
                break;
            case EHumanoidWeapon.EHT_SingleHand_Wizard:
                if (2 == ShowWithIndex(m_pAllPosibleSHandCombineWizard[Random.Range(0, m_pAllPosibleSHandCombineWizard.Length)]))
                {
                    bDoubleHandSWeapon = true;
                }
                break;
            case EHumanoidWeapon.EHT_SingleHand_Fight:
                if (2 == ShowWithIndex(m_pAllPosibleSHandCombineFight[Random.Range(0, m_pAllPosibleSHandCombineFight.Length)]))
                {
                    bDoubleHandSWeapon = true;
                }
                break;
            case EHumanoidWeapon.EHT_DoubleHand_Fight:
                ShowWithIndex(m_pAllPosibleDHandCombineFight[Random.Range(0, m_pAllPosibleDHandCombineFight.Length)]);
                break;
            case EHumanoidWeapon.EHT_DoubleHand_Wizard:
                ShowWithIndex(m_pAllPosibleDHandCombineWizard[Random.Range(0, m_pAllPosibleDHandCombineWizard.Length)]);
                break;
        }

        m_pCurrentBoltPos = m_pBoltPos[(int)m_eHumanWeapon];
        m_pCurrentBoltPos.SetActive(true);

        #endregion

        for (int i = 0; i < m_pBloodLineFront.Length; ++i)
        {
            m_pBloodLineFront[i].SetActive(true);
            if (7 != i)
            {
                m_pBloodLineFront[i].GetComponent<MeshRenderer>().enabled = false;
            }
            m_pAllShowModelRenderer.Add(m_pBloodLineFront[i].GetComponent<MeshRenderer>());
        }
        m_pShadow.SetActive(true);
        m_pAllShowModelRenderer.Add(m_pShadow.GetComponent<MeshRenderer>());

        #endregion

        #region Size

        List <EHumanoidSize> sizes = new List<EHumanoidSize>();
        sizes.Add(EHumanoidSize.EHS_Normal);
        sizes.Add(EHumanoidSize.EHS_Normal);
        sizes.Add(EHumanoidSize.EHS_Normal);
        sizes.Add(EHumanoidSize.EHS_Big);
        sizes.Add(EHumanoidSize.EHS_Small);
        sizes.Add(EHumanoidSize.EHS_Fat);
        sizes.Add(EHumanoidSize.EHS_ThinTall);

        SetHumanSize(sizes[Random.Range(0, sizes.Count)]);

        #endregion

        #region Animations

        m_pAnim.m_sAnimList = new string[(int)EAnimationType.EAT_Max];
        if (ECharactorMoveType.ECMT_Sky == m_eMoveType)
        {
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Born] = "FBorn";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Die] = "FDie";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_KnockDown] = "FDie";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Dash] = "FRun";
            m_pAnim.m_sAnimList[(int) EAnimationType.EAT_Idle] = "FIdle";
            m_pAnim.m_sAnimList[(int) EAnimationType.EAT_Run] = "FRun";

            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Attack] = EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon ? "FAttack_Shoot" : "FAttack";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Skill] = EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon ? "FAttack_Shoot" : "FAttack";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_SkillHold] = EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon ? "FSkill_Shoot" : "FSkill_Hold";

        }
        else
        {
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Born] = "GBorn";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Die] = "GDie";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_KnockDown] = "GKnock";
            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Dash] = "GDash";

            m_pAnim.m_sAnimList[(int) EAnimationType.EAT_Idle] = (EHumanoidWeapon.EHT_DoubleHand_Fight == m_eHumanWeapon ||
                    EHumanoidWeapon.EHT_DoubleHand_Wizard == m_eHumanWeapon) ? "GIdle_DHand": "GIdle";

            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Run] = (EHumanoidWeapon.EHT_DoubleHand_Fight == m_eHumanWeapon ||
                    EHumanoidWeapon.EHT_DoubleHand_Wizard == m_eHumanWeapon) ? "GRun_DHand" :
                    ((EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon || bDoubleHandSWeapon) ? "GRun_SHand" : "GRun");

            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Attack] = (EHumanoidWeapon.EHT_DoubleHand_Fight == m_eHumanWeapon ||
                    EHumanoidWeapon.EHT_DoubleHand_Wizard == m_eHumanWeapon) ? "GAttack_DHand" :
                    (EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon ? "GAttack_Shoot" : "GAttack");

            if (EHumanoidWeapon.EHT_DoubleHand_Wizard == m_eHumanWeapon)
            {
                m_pAnim.m_sAnimList[(int) EAnimationType.EAT_Skill] = "GSkill_DHand1";
            }
            else if (EHumanoidWeapon.EHT_DoubleHand_Fight == m_eHumanWeapon)
            {
                m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Skill] = "GSkill_DHand2";
            }
            else if (EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon)
            {
                m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Skill] = "GAttack_Shoot";
            }
            else if (bDoubleHandSWeapon)
            {
                m_pAnim.m_sAnimList[(int) EAnimationType.EAT_Skill] = "GSkill_SHand";
            }
            else
            {
                m_pAnim.m_sAnimList[(int)EAnimationType.EAT_Skill] = "GAttack";
            }

            m_pAnim.m_sAnimList[(int)EAnimationType.EAT_SkillHold] = EHumanoidWeapon.EHT_Shoot == m_eHumanWeapon ? "GSkill_Shoot" : "GSkill_Hold";
        }

        #endregion

        #region Load MeshFilters

        List<MeshFilter> filters = new List<MeshFilter>();
        foreach (MeshRenderer renderers in m_pAllShowModelRenderer)
        {
            filters.Add(renderers.gameObject.GetComponent<MeshFilter>());
        }
        m_pAllVisibleFilters = filters.ToArray();
        m_pAllVisibleMeshes = new Mesh[m_pAllVisibleFilters.Length, (int)ECharactorMainColor.ECMC_Max * 2];
        for (int i = 0; i < m_pAllVisibleFilters.Length; ++i)
        {
            for (int j = 0; j < (int) ECharactorMainColor.ECMC_Max * 2; ++j)
            {
                string sName = string.Format("CharMesh/M{0}S{1}/{2}",
                    j % (int) (ECharactorMainColor.ECMC_Max),
                    j < (int) (ECharactorMainColor.ECMC_Max) ? (int) subC : (int)ECharactorSubColor.ECSC_Max,
                    m_pAllVisibleFilters[i].sharedMesh.name);

                m_pAllVisibleMeshes[i, j] = ResourcesManager.Load<Mesh>(sName);
            }
        }

        #endregion

        m_bFixed = bFix;
    }

    #region Editor Use
#if UNITY_EDITOR

    protected CHumanoidDesc m_pDesc = null;
    public void EditorFix()
    {
        #region Find All

        List<GameObject> allGos = new List<GameObject>();
        foreach (MeshRenderer renderers in GetComponentsInChildren<MeshRenderer>(true))
        {
            allGos.Add(renderers.gameObject);
        }
        m_pAllComponents = allGos.ToArray();
        if (m_pAllComponents.Length >= m_iMaxCount)
        {
            CRuntimeLogger.LogError("零件数量超出上限1000个！");
            return;
        }

        #endregion

        m_pDesc = new CHumanoidDesc();
        m_pDesc.Load();

        #region Find hand, feet, wings, body, back, head

        m_pHands = FindPartWithTags(true, new[] { "Hand", "Left" });
        m_pFemaleFeet = FindPartWithTags(true, new[] { "Foot", "Female", "Left" });
        m_pMaleFeet = FindPartWithTags(true, new[] { "Foot", "Male", "Left" });
        m_pWings = FindPartWithTags(true, new[] { "Wing", "Left" });

        m_pFemaleBody = FindPartWithTags(false, new[] { "Body", "Female" });
        m_pMaleBody = FindPartWithTags(false, new[] { "Body", "Male" });
        m_pBacks = FindPartWithTags(false, new[] { "Back" });
        m_pFemaleHead = FindPartWithTags(false, new[] { "Head", "Female" });
        m_pMaleHead = FindPartWithTags(false, new[] { "Head", "Male" });

        #endregion

        #region Find Weapons

        CHumanoidDescElement[] sHandFightRight = m_pDesc[new[] { "SHand", "Fight", "Right" }];
        CHumanoidDescElement[] sHandWizardRight = m_pDesc[new[] { "SHand", "Wizard", "Right" }];
        CHumanoidDescElement[] sHandFightLeft = m_pDesc[new[] { "SHand", "Fight", "Left" }];
        CHumanoidDescElement[] sHandWizardLeft = m_pDesc[new[] { "SHand", "Wizard", "Left" }];

        m_pAllPosibleSHandCombineFight = new int[sHandFightRight.Length + sHandFightRight.Length * sHandFightLeft.Length];
        //Single Hand Fight
        for (int i = 0; i < sHandFightRight.Length; ++i)
        {
            m_pAllPosibleSHandCombineFight[i] = FindIndex(sHandFightRight[i].m_sObjectPath) + 1;
        }

        //Single Hand Fight 2 hands
        for (int i = 0; i < sHandFightRight.Length; ++i)
        {
            for (int j = 0; j < sHandFightLeft.Length; ++j)
            {
                m_pAllPosibleSHandCombineFight[sHandFightRight.Length + i * sHandFightLeft.Length + j]
                    = FindIndex(sHandFightRight[i].m_sObjectPath) + 1
                    + m_iMaxCount * (FindIndex(sHandFightLeft[j].m_sObjectPath) + 1);    
            }
        }

        m_pAllPosibleSHandCombineWizard = new int[sHandWizardRight.Length + sHandWizardRight.Length * sHandWizardLeft.Length];
        //Single Hand Wizard
        for (int i = 0; i < sHandWizardRight.Length; ++i)
        {
            m_pAllPosibleSHandCombineWizard[i] = FindIndex(sHandWizardRight[i].m_sObjectPath) + 1;
        }

        //Single Hand Wizard 2 hands
        for (int i = 0; i < sHandWizardRight.Length; ++i)
        {
            for (int j = 0; j < sHandWizardLeft.Length; ++j)
            {
                m_pAllPosibleSHandCombineWizard[sHandWizardRight.Length + i * sHandWizardLeft.Length + j]
                    = FindIndex(sHandWizardRight[i].m_sObjectPath) + 1
                    + m_iMaxCount * (FindIndex(sHandWizardLeft[j].m_sObjectPath) + 1);
            }
        }

        //Single Shoot
        CHumanoidDescElement[] sShoot = m_pDesc[new[] { "Shoot" }];
        m_pAllPosibleShoot = new int[sShoot.Length];
        for (int i = 0; i < sShoot.Length; ++i)
        {
            m_pAllPosibleShoot[i] = FindIndex(sShoot[i].m_sObjectPath) + 1;
        }

        //Single DHand Fight
        CHumanoidDescElement[] sDHandFight = m_pDesc[new[] { "DHand", "Fight" }];
        m_pAllPosibleDHandCombineFight = new int[sDHandFight.Length];
        for (int i = 0; i < sDHandFight.Length; ++i)
        {
            m_pAllPosibleDHandCombineFight[i] = FindIndex(sDHandFight[i].m_sObjectPath) + 1;
        }

        //Single DHand Wizard
        CHumanoidDescElement[] sDHandWizard = m_pDesc[new[] { "DHand", "Wizard" }];
        m_pAllPosibleDHandCombineWizard = new int[sDHandWizard.Length];
        for (int i = 0; i < sDHandWizard.Length; ++i)
        {
            m_pAllPosibleDHandCombineWizard[i] = FindIndex(sDHandWizard[i].m_sObjectPath) + 1;
        }

        #endregion

        #region Others

        m_pBoltPos = new GameObject[(int)EHumanoidWeapon.EHT_Max];
        m_pBoltPos[(int)EHumanoidWeapon.EHW_BareHand] = CommonFunctions.FindChildrenByName(gameObject, "BHandBoltPos", true);
        m_pBoltPos[(int)EHumanoidWeapon.EHT_SingleHand_Fight] = CommonFunctions.FindChildrenByName(gameObject, "SHandBoltPos");
        m_pBoltPos[(int)EHumanoidWeapon.EHT_SingleHand_Wizard] = CommonFunctions.FindChildrenByName(gameObject, "SHandBoltPos");
        m_pBoltPos[(int)EHumanoidWeapon.EHT_Shoot] = CommonFunctions.FindChildrenByName(gameObject, "ShootBoltPos");
        m_pBoltPos[(int)EHumanoidWeapon.EHT_DoubleHand_Fight] = CommonFunctions.FindChildrenByName(gameObject, "DHandBoltPos");
        m_pBoltPos[(int)EHumanoidWeapon.EHT_DoubleHand_Wizard] = CommonFunctions.FindChildrenByName(gameObject, "DHandBoltPos");

        m_pShadow = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/Shadow");
        m_pBloodLineShell = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell").transform;
        m_pBloodLineFront = new GameObject[8];
        m_pBloodLineFront[0] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront1");
        m_pBloodLineFront[1] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront2");
        m_pBloodLineFront[2] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront3");
        m_pBloodLineFront[3] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront4");
        m_pBloodLineFront[4] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront5");
        m_pBloodLineFront[5] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront6");
        m_pBloodLineFront[6] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront7");
        m_pBloodLineFront[7] = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineShell/BloodLineFront8");

        m_pOwner = gameObject.transform.parent.gameObject.GetComponent<ACharactor>();
        m_pOwner.m_pModel = this;
        m_pAnim = GetComponent<ACharactorAnimation>();
        m_pOwner.m_pAnim = m_pAnim;
        m_pAnim.m_pOwner = m_pOwner;
        m_pAnim.m_pModel = this;
        m_pAnim.m_pAnim = GetComponent<Animation>();

        #endregion

    }

    protected int[] FindPartWithTags(bool bHasLeft, string[] sTags)
    {
        List<int> ret = new List<int>();
        CHumanoidDescElement[] allDesc = m_pDesc[sTags];
        foreach (CHumanoidDescElement t in allDesc)
        {
            int iFound = 0;
            if (bHasLeft)
            {
                iFound = FindIndex(
                    t.m_sObjectPath,
                    t.m_sObjectPath.Replace("Left", "Right").Replace("L", "R")
                    );
            }
            else
            {
                iFound = FindIndex(t.m_sObjectPath);
            }
            if (0 != iFound)
            {
                for (int i = 0; i < t.m_iWeight; ++i)
                {
                    ret.Add(iFound);
                }                
            }
        }

        return ret.ToArray();
    }

    protected int FindIndex(params string[] sObjPath)
    {
        int[] iParts = {-1, -1};
        for (int i = 0; i < sObjPath.Length && i < 2; ++i)
        {
            for (int j = 0; j < m_pAllComponents.Length; ++j)
            {
                if ((gameObject.name + "/" + sObjPath[i]).Equals(CommonFunctions.FindFullName(gameObject, m_pAllComponents[j])))
                {
                    iParts[i] = j;
                    break;
                }
            }
        }
        return (iParts[1] + 1) * m_iMaxCount + iParts[0] + 1;
    }

#endif
    #endregion

    protected int ShowWithIndex(int iIndex, bool bLod = true)
    {
        int iI1 = iIndex % m_iMaxCount - 1;
        int iI2 = iIndex / m_iMaxCount - 1;
        int iRet = 0;
        if (iI1 >= 0)
        {
            m_pAllComponents[iI1].SetActive(true);
            m_pAllShowModelRenderer.Add(m_pAllComponents[iI1].GetComponent<MeshRenderer>());
            if (bLod)
            {
                m_pAllLODRenderer.Add(m_pAllComponents[iI1].GetComponent<MeshRenderer>());
            }
            ++iRet;
        }
        if (iI2 >= 0)
        {
            m_pAllComponents[iI2].SetActive(true);
            m_pAllShowModelRenderer.Add(m_pAllComponents[iI2].GetComponent<MeshRenderer>());
            if (bLod)
            {
                m_pAllLODRenderer.Add(m_pAllComponents[iI1].GetComponent<MeshRenderer>());
            }
            ++iRet;
        }
        return iRet;
    }

    protected void SetHumanSize(EHumanoidSize eSize)
    {
        transform.localScale = m_vModelSizes[(int) eSize];
        m_fModelSize = transform.localScale.x;
        m_pOwner.m_fModelSize = transform.localScale.x;
    }

    #region Need to be override

    override public void Assemble()
    {

    }

    override public void AssembleAndFix()
    {

    }

    override public void Fix()
    {
        foreach (GameObject candelete in m_pAllComponents)
        {
            if (!candelete.activeSelf)
            {
                Destroy(candelete);
            }
        }        
    }

    override public void Randomize(bool bFix = false)
    {
        HideAll();

        List<ECharactorAttackType> eAttackType = new List<ECharactorAttackType>();
        eAttackType.Add(ECharactorAttackType.ECAT_Mele);
        eAttackType.Add(ECharactorAttackType.ECAT_Mele);
        eAttackType.Add(ECharactorAttackType.ECAT_Range);
        eAttackType.Add(ECharactorAttackType.ECAT_Wizard);

        List<EHumanoidType> eType = new List<EHumanoidType>();
        eType.Add(EHumanoidType.EHT_Female);
        eType.Add(EHumanoidType.EHT_Female);
        eType.Add(EHumanoidType.EHT_Male);

        List<ECharactorMoveType> moveType = new List<ECharactorMoveType>();
        moveType.Add(ECharactorMoveType.ECMT_Ground);
        moveType.Add(ECharactorMoveType.ECMT_Ground);
        moveType.Add(ECharactorMoveType.ECMT_Sky);

        Randomize(
            eAttackType[Random.Range(0, eAttackType.Count)],
            moveType[Random.Range(0, moveType.Count)],
            eType[Random.Range(0, eType.Count)],
            (ECharactorSubColor)Random.Range(0, (int)ECharactorSubColor.ECSC_Max),
            bFix
            );
    }

    override public void HideAll()
    {
        foreach (GameObject gos in m_pAllComponents)
        {
            gos.SetActive(false);
        }

        foreach (GameObject gos in m_pBoltPos)
        {
            gos.SetActive(false);
        }
    }

    override public void RecoverAll()
    {
        foreach (GameObject gos in m_pAllComponents)
        {
            gos.SetActive(true);
        }

        foreach (GameObject gos in m_pBoltPos)
        {
            gos.SetActive(true);
        }
    }

    override public void SetVisible(ECharactorVisible eVisible)
    {
        if (eVisible == ECharactorVisible.ECV_None || eVisible == m_eVisible)
        {
            return;
        }
        m_eVisible = eVisible;
        SetMat(m_eCamp, m_eVisible);
    }

    override public void SetCamp(ECharactorCamp eCamp)
    {
        if (eCamp == ECharactorCamp.ECC_Max || eCamp == m_eCamp)
        {
            return;
        }
        m_eCamp = eCamp;
        SetMat(m_eCamp, m_eVisible);
    }

    protected bool m_bFirstFrameInitial = false;
    protected void FirstFrameInitial()
    {
        m_bFirstFrameInitial = true;
        if (m_bFixed)
        {
            foreach (Transform candelete in GetComponentsInChildren<Transform>(true))
            {
                if ( (0 == candelete.gameObject.GetComponentsInChildren<MeshRenderer>().Length || !candelete.gameObject.activeInHierarchy)
                    && candelete.gameObject != m_pCurrentBoltPos)
                {
                    Destroy(candelete.gameObject);
                }
            } 
        }
    }

    #endregion

    protected void SetMat(ECharactorCamp eCamp, ECharactorVisible eVisible)
    {
        int iIndex = eVisible == ECharactorVisible.ECV_InVisible
            ? (int) eCamp + (int) ECharactorMainColor.ECMC_Max
            : (int) eCamp;

        for (int i = 0; i < m_pAllVisibleFilters.Length; ++i)
        {
            m_pAllVisibleFilters[i].sharedMesh = m_pAllVisibleMeshes[i, iIndex];
        }
    }
}
