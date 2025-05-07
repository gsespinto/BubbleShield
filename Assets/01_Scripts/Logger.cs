using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Logger
{
    public static void Info(string message, MonoBehaviour context = null)
    {
#if UNITY_EDITOR || DEBUG
        Debug.Log(message, context);
#endif
    }

    public static void Warning(string message, MonoBehaviour context = null)
    {
#if UNITY_EDITOR || DEBUG
        Debug.LogWarning(message, context);
#endif
    }

    public static void Error(string message, MonoBehaviour context = null)
    {
#if UNITY_EDITOR || DEBUG
        Debug.LogError(message, context);
#endif
    }
    
    public static void Exception(Exception exception, MonoBehaviour context = null)
    {
#if UNITY_EDITOR || DEBUG
        Debug.LogException(exception, context);
#endif
    }
    
    public static void Assert(bool condition, string message = "", MonoBehaviour context = null)
    {
#if UNITY_EDITOR || DEBUG
        Debug.Assert(condition, message, context);
#endif
    }

}
