using UnityEngine;
using System.Collections;

public class Util
{
    public static void Log(string log)
    {
#if UNITY_EDITOR
        Debug.Log(log);
#endif
    }

    public static void Log(int log)
    {
#if UNITY_EDITOR
        Debug.Log(log);
#endif
    }

    public static void LogError(string log)
    {
#if UNITY_EDITOR
        Debug.LogError(log);
#endif
    }

    public static void LogError(int log)
    {
#if UNITY_EDITOR
        Debug.LogError(log);
#endif
    }
}
