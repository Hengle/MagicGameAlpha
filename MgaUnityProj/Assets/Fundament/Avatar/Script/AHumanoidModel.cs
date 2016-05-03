using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[AddComponentMenu("MGA/Avatar/AHumanoidModel")]
public class AHumanoidModel : ACharactorModel
{

    public const int m_iMaxCount = 1000;
    public bool TestRandom = false;
    public bool OpenAll = false;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if (OpenAll)
	    {
	        OpenAll = false;
            RecoverAll();
	    }

        if (TestRandom)
        {
            TestRandom = false;
            Randomize();
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

    public int[] m_pHats;
    public int[] m_pGlass;

    public int[] m_pWings; // left * m_iMaxCount + right

    //Weapons
    public int[] m_pAllPosibleSHandCombineFight; //left * m_iMaxCount + right, or shield * m_iMaxCount + right, or 0 * m_iMaxCount + right
    public int[] m_pAllPosibleSHandCombineWizard; //shield * m_iMaxCount + right, or 0 * m_iMaxCount + right
    public int[] m_pAllPosibleDHandCombineFight;
    public int[] m_pAllPosibleDHandCombineWizard;
    public int[] m_pAllPosibleShoot;

    //Other things
    public GameObject[] m_pBoltPos;
    public GameObject m_pCurrentBoltPos;
    public GameObject m_pShadow;
    public GameObject m_pBloodLineBack;
    public GameObject m_pBloodLineFront;

    #endregion

    public void Randomize(ECharactorAttackType eCharactorAttack, ECharactorMoveType eMove, EHumanoidType eGendle, bool bFix = false)
    {
        ShowWithIndex(m_pHands[Random.Range(0, m_pHands.Length)]);
        if (ECharactorMoveType.ECMT_Sky != eMove && Random.Range(0.0f, 1.0f) > 0.5f)
        {
            ShowWithIndex(m_pHands[Random.Range(0, m_pHands.Length)]);    
        }
        if (ECharactorMoveType.ECMT_Sky == eMove)
        {
            ShowWithIndex(m_pWings[Random.Range(0, m_pWings.Length)]);
        }

        if (eGendle == EHumanoidType.EHT_Female)
        {
            ShowWithIndex(m_pFemaleBody[Random.Range(0, m_pFemaleBody.Length)]);
            ShowWithIndex(m_pFemaleFeet[Random.Range(0, m_pFemaleFeet.Length)]);
            ShowWithIndex(m_pFemaleHead[Random.Range(0, m_pFemaleHead.Length)]);

            if (Random.Range(0.0f, 1.0f) > 0.7f)
            {
                ShowWithIndex(m_pHats[Random.Range(0, m_pHats.Length)]);
            }

            if (Random.Range(0.0f, 1.0f) > 0.9f)
            {
                ShowWithIndex(m_pGlass[Random.Range(0, m_pGlass.Length)]);
            }
        }
        else
        {
            ShowWithIndex(m_pMaleBody[Random.Range(0, m_pMaleBody.Length)]);
            ShowWithIndex(m_pMaleFeet[Random.Range(0, m_pMaleFeet.Length)]);
            ShowWithIndex(m_pMaleHead[Random.Range(0, m_pMaleHead.Length)]);
        }

        //===================================
        //Decide weapon
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
                break;
        }
        EHumanoidWeapon choose = weapons[Random.Range(0, weapons.Count)];

        switch (choose)
        {
            case EHumanoidWeapon.EHT_Shoot:
                ShowWithIndex(m_pAllPosibleShoot[Random.Range(0, m_pAllPosibleShoot.Length)]);
                break;
            case EHumanoidWeapon.EHT_SingleHand_Wizard:
                ShowWithIndex(m_pAllPosibleSHandCombineWizard[Random.Range(0, m_pAllPosibleSHandCombineWizard.Length)]);
                break;
            case EHumanoidWeapon.EHT_SingleHand_Fight:
                ShowWithIndex(m_pAllPosibleSHandCombineFight[Random.Range(0, m_pAllPosibleSHandCombineFight.Length)]);
                break;
            case EHumanoidWeapon.EHT_DoubleHand_Fight:
                ShowWithIndex(m_pAllPosibleDHandCombineFight[Random.Range(0, m_pAllPosibleDHandCombineFight.Length)]);
                break;
            case EHumanoidWeapon.EHT_DoubleHand_Wizard:
                ShowWithIndex(m_pAllPosibleDHandCombineWizard[Random.Range(0, m_pAllPosibleDHandCombineWizard.Length)]);
                break;
        }

        m_pCurrentBoltPos = m_pBoltPos[(int) choose];
        m_pCurrentBoltPos.SetActive(true);
        m_pBloodLineBack.SetActive(true);
        m_pBloodLineFront.SetActive(true);
        m_pShadow.SetActive(true);
    }

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

        CHumanoidDesc theDesc = new CHumanoidDesc();
        theDesc.Load();

        #region Find Heads feet and wings

        CHumanoidDescElement[] allLeftHands = theDesc[new[] { "Hand", "Left"}];
        m_pHands = new int[allLeftHands.Length];
        for (int i = 0; i < allLeftHands.Length; ++i)
        {
            int lHand = FindIndex(allLeftHands[i].m_sObjectPath);
            int rHand = FindIndex(allLeftHands[i].m_sObjectPath.Replace("Left", "Right").Replace("L", "R"));
            m_pHands[i] = lHand + 1 + m_iMaxCount*(rHand + 1);
        }
        CHumanoidDescElement[] allLeftFFeets = theDesc[new[] { "Foot", "Female", "Left" }];
        m_pFemaleFeet = new int[allLeftFFeets.Length];
        for (int i = 0; i < allLeftFFeets.Length; ++i)
        {
            int lFoot = FindIndex(allLeftFFeets[i].m_sObjectPath);
            int rFoot = FindIndex(allLeftFFeets[i].m_sObjectPath.Replace("Left", "Right").Replace("L", "R"));
            m_pFemaleFeet[i] = lFoot + 1 + m_iMaxCount * (rFoot + 1);
        }
        CHumanoidDescElement[] allLeftMFeets = theDesc[new[] { "Foot", "Male", "Left" }];
        m_pMaleFeet = new int[allLeftMFeets.Length];
        for (int i = 0; i < allLeftMFeets.Length; ++i)
        {
            int lFoot = FindIndex(allLeftMFeets[i].m_sObjectPath);
            int rFoot = FindIndex(allLeftMFeets[i].m_sObjectPath.Replace("Left", "Right").Replace("L", "R"));
            m_pMaleFeet[i] = lFoot + 1 + m_iMaxCount * (rFoot + 1);
        }
        CHumanoidDescElement[] allLeftWings = theDesc[new[] { "Wing", "Left" }];
        m_pWings = new int[allLeftWings.Length];
        for (int i = 0; i < allLeftWings.Length; ++i)
        {
            int lWing = FindIndex(allLeftMFeets[i].m_sObjectPath);
            int rWing = FindIndex(allLeftMFeets[i].m_sObjectPath.Replace("Left", "Right").Replace("L", "R"));
            m_pWings[i] = lWing + 1 + m_iMaxCount * (rWing + 1);
        }

        #endregion

        #region Find Body and backs

        CHumanoidDescElement[] allBodyF = theDesc[new[] { "Body", "Female" }];
        m_pFemaleBody = new int[allBodyF.Length];
        for (int i = 0; i < allBodyF.Length; ++i)
        {
            m_pFemaleBody[i] = FindIndex(allBodyF[i].m_sObjectPath) + 1;
        }
        CHumanoidDescElement[] allBodyM = theDesc[new[] { "Body", "Male" }];
        m_pMaleBody = new int[allBodyM.Length];
        for (int i = 0; i < allBodyM.Length; ++i)
        {
            m_pMaleBody[i] = FindIndex(allBodyM[i].m_sObjectPath) + 1;
        }
        CHumanoidDescElement[] allBack = theDesc[new[] { "Back"}];
        m_pBacks = new int[allBack.Length];
        for (int i = 0; i < allBack.Length; ++i)
        {
            m_pBacks[i] = FindIndex(allBack[i].m_sObjectPath) + 1;
        }

        #endregion

        #region Find Hair and heads

        //Find Male Heads

        //step 1: find male hair, and dependent head. add them, and record the head we add.
        List<string> added = new List<string>();
        CHumanoidDescElement[] allHairM = theDesc[new[] { "Hair", "Male" }];
        List<int> toAdd = new List<int>();
        foreach (CHumanoidDescElement element in allHairM)
        {
            if (null != element.m_sDependency && 1 == element.m_sDependency.Length)
            {
                CHumanoidDescElement dependentHead = theDesc[element.m_sDependency[0]];
                if (null != dependentHead)
                {
                    if (!added.Contains(dependentHead.m_sElementName))
                    {
                        added.Add(dependentHead.m_sElementName);
                    }
                    toAdd.Add(FindIndex(dependentHead.m_sObjectPath) + 1 + m_iMaxCount * (FindIndex(element.m_sObjectPath) + 1));
                }                
            }
        }

        //step 2: find male head with dependency, and dependent head. add them, and record the head we add.
        CHumanoidDescElement[] allHeadM = theDesc[new[] { "Head", "Male" }];
        foreach (CHumanoidDescElement element in allHeadM)
        {
            if (null != element.m_sDependency && 1 == element.m_sDependency.Length)
            {
                CHumanoidDescElement dependentHead = theDesc[element.m_sDependency[0]];
                if (null != dependentHead)
                {
                    if (!added.Contains(dependentHead.m_sElementName))
                    {
                        added.Add(dependentHead.m_sElementName);
                    }
                    if (!added.Contains(element.m_sElementName))
                    {
                        added.Add(element.m_sElementName);
                    }
                    toAdd.Add(FindIndex(dependentHead.m_sObjectPath) + 1 + m_iMaxCount * (FindIndex(element.m_sObjectPath) + 1));
                }
            }
        }

        //step 3: find male head without dependency.
        foreach (CHumanoidDescElement element in allHeadM)
        {
            if (!added.Contains(element.m_sElementName))
            {
                toAdd.Add(FindIndex(element.m_sObjectPath) + 1);
            }
        }

        m_pMaleHead = toAdd.ToArray();

        //Find Female Head and Hairs
        toAdd.Clear();
        //find male hair, and dependent head. add them, and record the head we add.
        CHumanoidDescElement[] allHairF = theDesc[new[] { "Hair", "Female" }];
        foreach (CHumanoidDescElement element in allHairF)
        {
            if (null != element.m_sDependency && 1 == element.m_sDependency.Length)
            {
                CHumanoidDescElement dependentHead = theDesc[element.m_sDependency[0]];
                if (null != dependentHead)
                {
                    toAdd.Add(FindIndex(dependentHead.m_sObjectPath) + 1 + m_iMaxCount * (FindIndex(element.m_sObjectPath) + 1));
                }
            }
        }
        m_pFemaleHead = toAdd.ToArray();

        //find glasses and hats
        CHumanoidDescElement[] allGlass = theDesc[new[] { "Glass"}];
        m_pGlass = new int[allGlass.Length];
        for (int i = 0; i < allGlass.Length; ++i)
        {
            m_pGlass[i] = FindIndex(allGlass[i].m_sObjectPath) + 1;
        }
        CHumanoidDescElement[] allHats = theDesc[new[] { "Hat" }];
        m_pHats = new int[allHats.Length];
        for (int i = 0; i < allHats.Length; ++i)
        {
            m_pHats[i] = FindIndex(allHats[i].m_sObjectPath) + 1;
        }

        #endregion

        #region Find Weapons

        CHumanoidDescElement[] sHandFightRight = theDesc[new[] { "SHand", "Fight", "Right" }];
        CHumanoidDescElement[] sHandWizardRight = theDesc[new[] { "SHand", "Wizard", "Right" }];
        CHumanoidDescElement[] sHandFightLeft = theDesc[new[] { "SHand", "Fight", "Left" }];
        CHumanoidDescElement[] sHandWizardLeft = theDesc[new[] { "SHand", "Wizard", "Left" }];

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
        CHumanoidDescElement[] sShoot = theDesc[new[] { "Shoot" }];
        m_pAllPosibleShoot = new int[sShoot.Length];
        for (int i = 0; i < sShoot.Length; ++i)
        {
            m_pAllPosibleShoot[i] = FindIndex(sShoot[i].m_sObjectPath) + 1;
        }

        //Single DHand Fight
        CHumanoidDescElement[] sDHandFight = theDesc[new[] { "DHand", "Fight" }];
        m_pAllPosibleDHandCombineFight = new int[sDHandFight.Length];
        for (int i = 0; i < sDHandFight.Length; ++i)
        {
            m_pAllPosibleDHandCombineFight[i] = FindIndex(sDHandFight[i].m_sObjectPath) + 1;
        }

        //Single DHand Wizard
        CHumanoidDescElement[] sDHandWizard = theDesc[new[] { "DHand", "Wizard" }];
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

        m_pShadow = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/shadow");
        m_pBloodLineBack = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineBack");
        m_pBloodLineFront = CommonFunctions.FindChildrenByName(gameObject, "Humanoid/BloodLineBack/BloodLineFront");

        m_pOwner = gameObject.transform.parent.gameObject.GetComponent<ACharactor>();
        m_pOwner.m_pModel = this;
        m_pAnim = GetComponent<ACharactorAnimation>();
        m_pOwner.m_pAnim = m_pAnim;
        m_pAnim.m_pOwner = m_pOwner;
        m_pAnim.m_pModel = this;
        m_pAnim.m_pAnim = GetComponent<Animation>();

        #endregion
    }

    private int FindIndex(string sObjPath)
    {
        for (int i = 0; i < m_pAllComponents.Length; ++i)
        {
            if ((gameObject.name + "/" + sObjPath).Equals(CommonFunctions.FindFullName(gameObject, m_pAllComponents[i])))
            {
                return i;
            }
        }
        return -1;
    }

    private void ShowWithIndex(int iIndex)
    {
        int iI1 = iIndex % m_iMaxCount - 1;
        int iI2 = iIndex / m_iMaxCount - 1;
        if (iI1 >= 0)
        {
            m_pAllComponents[iI1].SetActive(true);
        }
        if (iI2 >= 0)
        {
            m_pAllComponents[iI2].SetActive(true);
        }
    }

    private void FixBodyAndHead()
    {
        
    }

    private void FixHand()
    {

    }

    private void FixBack()
    {

    }

    private void FixFoot()
    {

    }

    private void FixHair()
    {
        
    }

    private void FixHat()
    {

    }

    private void FixGlass()
    {

    }

    private void FixWeapon()
    {

    }

    private void FixAnimations()
    {
        
    }

    #region Need to be override

    override public void Assemble()
    {

    }

    override public void AssembleAndFix()
    {

    }

    override public void Randomize()
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
            eType[Random.Range(0, eType.Count)]
            );
    }

    override public void RandomizeFix()
    {

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

    #endregion
}
