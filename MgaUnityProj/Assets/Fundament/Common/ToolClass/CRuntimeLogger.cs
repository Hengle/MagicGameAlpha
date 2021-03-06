using UnityEngine;

public enum ELogType
{
    Alexis   = 0,
    Java     = 1,

    ELT_Max,
}

public delegate void logFunc(object obj, Object obj2 = null);
public delegate void logFormatFunc(string format, params object[] args);

public class CRuntimeLogger
{
    public static logFunc Log = Debug.Log;
    public static logFormatFunc LogFormat = Debug.LogFormat;
    public static logFunc LogWarning = Debug.LogWarning;
    public static logFunc LogError = Debug.LogError;

    #region Typed logs

    public static logFunc[] TLog = { Debug.Log, Debug.Log };
    public static logFormatFunc[] TLogFormat = { Debug.LogFormat, Debug.LogFormat };
    public static logFunc[] TLogWarning = { Debug.LogWarning, Debug.LogWarning };
    public static logFunc[] TLogError = { Debug.LogError, Debug.LogError };

    #endregion

    static public void DisableLog()
    {
        Log = EmptyLog;
        LogFormat = EmptyLogFormat;
        LogWarning = EmptyLogWarning;
        LogError = EmptyLogError;
    }

    static public void DisableLog(ELogType eType)
    {
        TLog[(int)eType] = EmptyLog;
        TLogFormat[(int)eType] = EmptyLogFormat;
        TLogWarning[(int)eType] = EmptyLog;
        TLogError[(int)eType] = EmptyLog;
    }

    static public void EmptyLog(object sString, Object obj2 = null)
    {

    }

    public static void EmptyLogFormat(string format, params object[] args)
    {

    }

    public static void EmptyLogWarning(object obj1, Object obj2 = null)
    {
        
    }

    public static void EmptyLogError(object obj1, Object obj2 = null)
    {

    }
}
