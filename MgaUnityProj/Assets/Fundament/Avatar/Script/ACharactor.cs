﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("MGA/Avatar/ACharactor")]
public class ACharactor : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {

    }

    //has to be a creature first! buildings cannot move!
    #region Move 

    public sfloat m_fMoveSpeed = 0.0f; //Current Max Speed
    public Vector2 m_vLastDelta = Vector2.zero;

    #endregion

    #region Model

    public ACharactorModel m_pModel;
    public Transform m_pModelShell;
    public sfloat m_fModelSize = 1.0f;

    #endregion

    #region Animation

    public ACharactorAnimation m_pAnim;
    //for some buff that can forzen the animation
    public float GetAnimRate()
    {
        return 1.0f;
    }

    #endregion
}
