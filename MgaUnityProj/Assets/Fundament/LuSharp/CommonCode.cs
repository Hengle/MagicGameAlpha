//#define UNITY

using System.Linq;
using System;
//using System.Collections;


#if UNITY
using UnityEngine;
using Random = UnityEngine.Random;
#endif

#if UNITY_EDITOR
using System.Diagnostics;
#endif

public enum ELogLevel
{
    Editor,
    Log,
    Warnning,
    Error,
    BigBug,
}

public delegate void LuaLogger(string data);

public static class CommonCode
{
	public static LuaLogger Logger;

    public static void Log(string sInfo, ELogLevel iLevel = ELogLevel.Log)
    {
        if (Logger != null)
		{
			Logger(sInfo);
		}
    }

    public static void LogWarning(string sInfo)
    {
#if UNITY
        UnityEngine.Debug.LogWarning(sInfo);
#else
#endif
    }

    public static void LogTraceBack(Exception e)
    {
#if UNITY_EDITOR
        //CRuntimeLogger.NetworkLogWarning(e.StackTrace);
        var trace = new StackTrace();
        StackFrame[] stackFrames = trace.GetFrames();
        if (stackFrames == null)
        {
            return;
        }
        foreach (var method in stackFrames.Select(frame => frame.GetMethod()).Where(method => !method.Name.Equals("LogTraceBack")))
        {
            //if (method.ReflectedType != null)
                //CRuntimeLogger.NetworkLogWarning(string.Format("<color=#FF8888>neterror</color>{0}::{1}",
            //        method.ReflectedType.Name,
            //        method.Name));
        }

#endif
    }


    #region Random

    private static Random random = null;

    public static int Range(int iLower, int iUpper)
    {
#if UNITY
        return Random.Range(iLower, iUpper);
#else
        if (null == random)
        {
            random = new System.Random();
        }

        return random.Next(iLower, iUpper);
#endif
    }

    public static float Range(float fLower, float fUpper)
    {
#if UNITY
        return Random.Range(fLower, fUpper);
#else
        if (null == random)
        {
            random = new Random();
        }

        return (float) random.NextDouble()*(fUpper - fLower) + fLower;
#endif
    }

    #endregion

    #region Math Functions

    //Math Functions
    public static int RoundToInt(float fIn)
    {
#if UNITY
        return Mathf.RoundToInt(fIn);
#else
        return (int)Math.Round((float)fIn);
#endif
    }

    public static int FloorToInt(float fIn)
    {
#if UNITY
        return Mathf.FloorToInt(fIn);
#else
        return (int)Math.Floor((float)fIn);
#endif
    }

    public static float Floor(float fIn)
    {
#if UNITY
        return Mathf.FloorToInt(fIn);
#else
        return (float)Math.Floor((float)fIn);
#endif
    }

    public static int CeilToInt(float fIn)
    {
#if UNITY
        return Mathf.CeilToInt(fIn);
#else
        return (int)Math.Ceiling((float)fIn);
#endif
    }

    public static float Ceil(float fIn)
    {
#if UNITY
        return Mathf.Ceil(fIn);
#else
        return (float)Math.Ceiling((float)fIn);
#endif
    }

    public static float Sin(float fIn)
    {
#if UNITY
        return Mathf.Sin(fIn);
#else
        return (float)Math.Sin((float)fIn);
#endif
    }

    public static float Cos(float fIn)
    {
#if UNITY
        return Mathf.Cos(fIn);
#else
        return (float)Math.Cos((float)fIn);
#endif
    }

    public static float Tan(float fIn)
    {
#if UNITY
        return Mathf.Tan(fIn);
#else
        return (float)Math.Tan((float)fIn);
#endif
    }

    public static float Acos(float fIn)
    {
#if UNITY
        return Mathf.Acos(fIn);
#else
        return (float)Math.Acos((float)fIn);
#endif
    }

    public static float Asin(float fIn)
    {
#if UNITY
        return Mathf.Asin(fIn);
#else
        return (float)Math.Asin((float)fIn);
#endif
    }

    public static float Atan(float f1)
    {
#if UNITY
        return Mathf.Atan(f1);
#else
        return (float)Math.Atan((float)f1);
#endif
    }

    public static float Atan2(float f1, float f2)
    {
#if UNITY
        return Mathf.Atan2(f1, f2);
#else
        return (float)Math.Atan2((float)f1, (float)f2);
#endif
    }

    public static float Pow(float f1, float f2)
    {
#if UNITY
        return Mathf.Pow(f1, f2);
#else
        return (float)Math.Pow((float)f1, (float)f2);
#endif
    }

    public static float Abs(float fIn)
    {
#if UNITY
        return Mathf.Abs(fIn);
#else
        return Math.Abs((float)fIn);
#endif
    }

    public static int Abs(int iIn)
    {
#if UNITY
        return Mathf.Abs(iIn);
#else
        return Math.Abs(iIn);
#endif
    }

    public static float Log(sfloat fIn)
    {
#if UNITY
        return Mathf.Log(fIn);
#else
        return (float)Math.Log((float)fIn);
#endif
    }

    public static float Log10(sfloat fIn)
    {
#if UNITY
        return Mathf.Log10(fIn);
#else
        return (float)Math.Log10((float)fIn);
#endif
    }

    public static float Exp(sfloat fIn)
    {
#if UNITY
        return Mathf.Exp(fIn);
#else
        return (float)Math.Exp((float)fIn);
#endif
    }

    public static float Sqrt(sfloat fIn)
    {
#if UNITY
        return Mathf.Sqrt(fIn);
#else
        return (float)Math.Sqrt((float)fIn);
#endif
    }

    public static CPseduLuaNumber MinPLua(CPseduLuaNumber ps1, CPseduLuaNumber ps2)
    {
        if (ps1.GetExpectedType() == ELuaExpectedType.ELET_Int && ps2.GetExpectedType() == ELuaExpectedType.ELET_Int)
        {
            return new CPseduLuaNumber(MinInt((int)ps1, (int)ps2));
        }

        return new CPseduLuaNumber(Min((float)ps1, (float)ps2));
    }

    public static int MinInt(params int[] fIns)
    {
#if UNITY
        return Mathf.Min(fIns);
#else
        int fRet = fIns[0];
        for (int i = 1; i < fIns.Length; ++i)
        {
            fRet = Math.Min(fRet, fIns[i]);
        }
        return fRet;
#endif
    }

    public static float Min(params float[] fIns)
    {
#if UNITY
        return Mathf.Min(fIns);
#else
        float fRet = fIns[0];
        for (int i = 1; i < fIns.Length; ++i)
        {
            fRet = Math.Min(fRet, fIns[i]);
        }
        return fRet;
#endif
    }

#if Discard
    public static int Min(params int[] iIns)
    {
#if UNITY
        return Mathf.Min(iIns);
#else
        int fRet = iIns[0];
        for (int i = 1; i < iIns.Length; ++i)
        {
            fRet = Math.Min(fRet, iIns[0]);
        }
        return fRet;
#endif
    }
#endif

    public static CPseduLuaNumber MaxPLua(CPseduLuaNumber ps1, CPseduLuaNumber ps2)
    {
        if (ps1.GetExpected() is int && ps2.GetExpected() is int)
        {
            return new CPseduLuaNumber(MaxInt((int)ps1, (int)ps2));
        }

        return new CPseduLuaNumber(Max((float)ps1, (float)ps2));
    }

    public static int MaxInt(params int[] iIns)
    {
#if UNITY
        return Mathf.Max(iIns);
#else
        int fRet = iIns[0];
        for (int i = 1; i < iIns.Length; ++i)
        {
            fRet = Math.Max(fRet, iIns[i]);
        }
        return fRet;
#endif
    }

    public static float Max(params float[] iIns)
    {
#if UNITY
        return Mathf.Max(iIns);
#else
        float fRet = iIns[0];
        for (int i = 1; i < iIns.Length; ++i)
        {
            fRet = Math.Max(fRet, iIns[i]);
        }
        return fRet;
#endif
    }

    public static float Max(params int[] iIns)
    {
#if UNITY
        return Mathf.Max(iIns);
#else
        int fRet = iIns[0];
        for (int i = 1; i < iIns.Length; ++i)
        {
            fRet = Math.Max(fRet, iIns[i]);
        }
        return fRet;
#endif
    }

#if Discard
    public static int Max(params int[] iIns)
    {
#if UNITY
        return Mathf.Max(iIns);
#else
        int fRet = iIns[0];
        for (int i = 1; i < iIns.Length; ++i)
        {
            fRet = Math.Max(fRet, iIns[0]);
        }
        return fRet;
#endif
    }
#endif

#if UNITY
    public const float Rad2Deg = Mathf.Rad2Deg;
    public const float Deg2Rad = Mathf.Deg2Rad;
#else
    public const float Rad2Deg = (float) (180.0f/Math.PI);
    public const float Deg2Rad = (float)(Math.PI / 180.0f);
#endif

    public static float PingPong(float fT, float fLength)
    {
#if UNITY
        return Mathf.PingPong(fT, fLength);
#else
        if (fLength <= float.Epsilon && fLength >= float.Epsilon)
        {
            return 0.0f;
        }

        fT -= 2.0f * fLength*(float)Math.Floor(fT * 0.5f / fLength);
        return fT > fLength ? (fLength - fT) : fT;
#endif
    }

    public static float Clamp(float fValue, float fMin, float fMax)
    {
#if UNITY
        return Mathf.Clamp(fValue, fMin, fMax);
#else
        if (fValue > fMax)
        {
            return fMax;
        }
        else if (fValue < fMin)
        {
            return fMin;
        }

        return fValue;
#endif
    }

    public static int Clamp(int fValue, int fMin, int fMax)
    {
#if UNITY        
        return Mathf.Clamp(fValue, fMin, fMax);
#else
        if (fValue > fMax)
        {
            return fMax;
        }
        else if (fValue < fMin)
        {
            return fMin;
        }

        return fValue;
#endif
    }

    public static float Clamp01(float fValue)
    {
#if UNITY
        return Mathf.Clamp01(fValue);
#else
        return Clamp(fValue, 0.0f, 1.0f);
#endif
    }

    public static float Round(float fIn)
    {
#if UNITY
        return Mathf.Round(fIn);
#else
        return (float)Math.Round(fIn);
#endif
    }

    public static float Repeat(float fT, float fLength)
    {
#if UNITY
        return Mathf.Repeat(fT, fLength);
#else
        if (fLength <= float.Epsilon && fLength >= -float.Epsilon)
        {
            return 0.0f;
        }

        fT -= fLength * (float)Math.Floor(fT / fLength);
        return fT;
#endif
    }

    public static float Sign(float fValue)
    {
#if UNITY
        return Mathf.Sign(fValue);
#else
        return (fValue > float.Epsilon) ? 1.0f : (fValue < -float.Epsilon ? -1.0f : 0.0f);
#endif
    }

    #endregion

    //Other Functions
    public static string GetAppDataRoot()
    {
#if UNITY
		return Application.dataPath;
#else
        return "./";
#endif
    }


    /// <summary>
    /// 移除名字中的#及后面的id
    /// </summary>
    /// <param name="sText"></param>
    /// <returns></returns>
    public static string RemoveSharp(this String sText)
    {
        return sText.Contains("#") ? sText.Substring(0, sText.IndexOf('#')) : sText;   
    }

    /// <summary>
    /// 从字符串实例出一个Vector3对象
    /// </summary>
    public static sVector3 ToVector3(this String sV3String)
    {
        bool bFailed = false;

        sV3String = sV3String.ToUpper();
        sV3String = sV3String.Replace("(", "");
        sV3String = sV3String.Replace(")", "");
        sV3String = sV3String.Replace(" ", "");
        sV3String = sV3String.Replace("F", "");

        var pSplitValue = sV3String.Split(',');

        float f0 = 0.0f;
        float f1 = 0.0f;
        float f2 = 0.0f;

        if (pSplitValue.Length != 3)
        {
            bFailed = true;
        }
        else
        {
            if (!float.TryParse(pSplitValue[0], out f0))
            {
                bFailed = true;
            }
            if (!float.TryParse(pSplitValue[1], out f1))
            {
                bFailed = true;
            }
            if (!float.TryParse(pSplitValue[2], out f2))
            {
                bFailed = true;
            }   
        }

        if (bFailed)
        {
            return new sVector3(-999.7F, -999.8F, -999.9F);
        }

        return new sVector3(f0, f1, f2);
    }
	
}

#if NEEDSAVESTRUCTURE
#region Native Save struct

public struct sfloat
{
#if DISCARD_TOO_COMPLECATED

    private float m_fRadnomAngle;
    private float m_fRandomDirX;
    private float m_fRandomDirY;

    private float m_fCut1;
    private float m_fCut2;

    public sfloat(sfloat sfValue) : this()
    {
        SetValue(sfValue.GetValue());
    }

    public sfloat(float fValue) : this()
    {
        SetValue(fValue);
    }

    public float GetValue()
    {
        float fC1 = Mathf.Cos(-m_fRadnomAngle) * m_fCut1 - Mathf.Sin(-m_fRadnomAngle) * m_fCut2;
        float fC2 = Mathf.Sin(-m_fRadnomAngle) * m_fCut1 + Mathf.Cos(-m_fRadnomAngle) * m_fCut2;
        return fC1 + fC2;
    }

    public void SetValue(float fValue)
    {
        m_fRadnomAngle = Random.Range(-7.0f, 7.0f);
        float fC1 = Random.Range(-1.0f, 2.0f)*fValue;
        float fC2 = fValue - m_fCut1;
        m_fCut1 = Mathf.Cos(m_fRadnomAngle) * fC1 - Mathf.Sin(m_fRadnomAngle) * fC2;
        m_fCut2 = Mathf.Sin(m_fRadnomAngle) * fC1 + Mathf.Cos(m_fRadnomAngle) * fC2;
    }

    public static implicit operator sfloat(float fValue)
    {
        return new sfloat(fValue);
    }

    public static implicit operator float(sfloat sfValue)
    {
        return sfValue.GetValue();
    }

#endif

    private float m_fCut1;
    private float m_fCut2;

    public sfloat(sfloat sfValue)
        : this()
    {
        SetValue(sfValue.GetValue());
    }

    public sfloat(float fValue)
        : this()
    {
        SetValue(fValue);
    }

    private float GetValue()
    {
        return m_fCut1 + m_fCut2;
    }

    private void SetValue(float fValue)
    {
        m_fCut1 = CRandom.Range(-1.0f, 2.0f) * fValue;
        m_fCut2 = fValue - m_fCut1;
    }

	public override string ToString()
	{
		return ((float)this).ToString();
	}

    public static implicit operator sfloat(float fValue)
    {
        return new sfloat(fValue);
    }

    public static implicit operator float(sfloat sfValue)
    {
        return sfValue.GetValue();
    }

}

public struct sint
{
    private int m_iCut1;
    private int m_iCut2;

    public sint(sint sfValue) : this()
    {
        SetValue(sfValue.GetValue());
    }

    public sint(int fValue) : this()
    {
        SetValue(fValue);
    }

    private int GetValue()
    {
        return m_iCut1 + m_iCut2;
    }

    private void SetValue(int iValue)
    {
        m_iCut1 = CRandom.Range(iValue > 0 ? -10 * iValue - 10 : 10 * iValue - 10, iValue > 0 ? 10 * iValue + 10 : -10 * iValue + 10);
        m_iCut2 = iValue - m_iCut1;
    }

	public override string ToString()
	{
		return ((int)this).ToString();
	}

    public static implicit operator sint(int fValue)
    {
        return new sint(fValue);
    }

    public static implicit operator int(sint sfValue)
    {
        return sfValue.GetValue();
    }

    public static implicit operator sint(uint fValue)
    {
        sint sret = new sint((int)fValue);
        return sret;
    }

    public static implicit operator uint(sint sfValue)
    {
        return (uint)sfValue.GetValue();
    }

    public static implicit operator sint(float fValue)
    {
        return new sint(CommonCode.RoundToInt(fValue));
    }

    public static implicit operator float(sint sfValue)
    {
        return sfValue.GetValue();
    }

    public static implicit operator sint(sfloat fValue)
    {
        return new sint(CommonCode.RoundToInt(fValue));
    }

    public static implicit operator sfloat(sint sfValue)
    {
        return sfValue.GetValue();
    }
}

public struct sbool
{
    private int m_fValue;

    public sbool(sbool sfValue)
        : this()
    {
        SetValue(sfValue.GetValue());
    }

    public sbool(bool fValue)
        : this()
    {
        SetValue(fValue);
    }

    private bool GetValue()
    {
        return m_fValue >= 0;
    }

    private void SetValue(bool iValue)
    {
        m_fValue = iValue ? CRandom.Range(0, 100) : CRandom.Range(-100, -1);
    }

    public static implicit operator sbool(bool fValue)
    {
        return new sbool(fValue);
    }

    public static implicit operator bool(sbool sfValue)
    {
        return sfValue.GetValue();
    }
}

#endregion
#endif

#region Vectors replacement

public struct sVector2
{
    public float x;
    public float y;

    public sVector2(float x, float y) : this()
    {
        this.x = x;
        this.y = y;
    }

    public static sVector2 operator +(sVector2 c1, sVector2 c2)
    {
        return new sVector2 { x = c1.x + c2.x, y = c1.y + c2.y};
    }

    public static sVector2 operator -(sVector2 c1, sVector2 c2)
    {
        return new sVector2 { x = c1.x - c2.x, y = c1.y - c2.y};
    }

    public static sVector2 operator *(sVector2 c1, sVector2 c2)
    {
        return new sVector2 { x = c1.x * c2.x, y = c1.y * c2.y};
    }

    private float _magnitude;
    private float _sqrmagnitude;

    public float magnitude
    {
        get
        {
            if (_sqrmagnitude < float.Epsilon)
            {
                _sqrmagnitude = x * x + y * y;
            }
            if (_magnitude < float.Epsilon)
            {
                _magnitude = (float)Math.Sqrt(_sqrmagnitude);
            }
            return _magnitude;
        }
    }

    public float sqrMagnitude
    {
        get
        {
            if (_sqrmagnitude < float.Epsilon)
            {
                _sqrmagnitude = x * x + y * y;
            }
            return _sqrmagnitude;
        }
    }

    static public sVector2 Scale(sVector2 v1, sVector2 v2)
    {
        return new sVector2(v1.x * v2.x, v1.y * v2.y);
    }
}

public struct sVector3
{
    public float x;
    public float y;
    public float z;

    public sVector3(float x, float y) : this()
    {
        this.x = x;
        this.y = y;
    }

    public sVector3(float x, float y, float z) : this()
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static sVector3 operator +(sVector3 c1, sVector3 c2)
    {
        return new sVector3 { x = c1.x + c2.x, y = c1.y + c2.y, z = c1.z + c2.z };
    }

    public static sVector3 operator -(sVector3 c1, sVector3 c2)
    {
        return new sVector3 { x = c1.x - c2.x, y = c1.y - c2.y, z = c1.z - c2.z };
    }

    public static sVector3 operator *(sVector3 c1, sVector3 c2)
    {
        return new sVector3 { x = c1.x * c2.x, y = c1.y * c2.y, z = c1.z * c2.z };
    }

    private float _magnitude;
    private float _sqrmagnitude;

    public float magnitude
    {
        get 
        { 
            if (_sqrmagnitude < float.Epsilon)
            {
                _sqrmagnitude = x*x + y*y + z*z;
            }
            if (_magnitude < float.Epsilon)
            {
                _magnitude = (float)Math.Sqrt(_sqrmagnitude);
            }
            return _magnitude; 
        }
    }

    public float sqrMagnitude
    {
        get
        {
            if (_sqrmagnitude < float.Epsilon)
            {
                _sqrmagnitude = x * x + y * y + z * z;
            }
            return _sqrmagnitude;
        }
    }
}

public struct sVector4
{
    public float x;
    public float y;
    public float z;
    public float w;

    public sVector4(float x, float y) : this()
    {
        this.x = x;
        this.y = y;
    }

    public sVector4(float x, float y, float z)
        : this()
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public sVector4(float x, float y, float z, float w)
        : this()
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static sVector4 operator +(sVector4 c1, sVector4 c2)
    {
        return new sVector4 { x = c1.x + c2.x, y = c1.y + c2.y, z = c1.z + c2.z, w = c1.w + c2.w };
    }

    public static sVector4 operator -(sVector4 c1, sVector4 c2)
    {
        return new sVector4 { x = c1.x - c2.x, y = c1.y - c2.y, z = c1.z - c2.z, w = c1.w + c2.w };
    }

    public static sVector4 operator *(sVector4 c1, sVector4 c2)
    {
        return new sVector4 { x = c1.x * c2.x, y = c1.y * c2.y, z = c1.z * c2.z, w = c1.w + c2.w };
    }
}

#endregion
