using UnityEngine;
using System;

public class CrashLogger : MonoBehaviour
{
    void Awake()
    {
        // Capturar excepciones no manejadas
        AppDomain.CurrentDomain.UnhandledException += HandleException;
        
        // Capturar logs de error de Unity
        Application.logMessageReceived += HandleLog;
    }

    private void HandleException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception ex = (Exception)e.ExceptionObject;
        Debug.LogError($"[CrashLogger] EXCEPCIÓN NO CAPTURADA:\n{ex.Message}\n{ex.StackTrace}");
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            Debug.LogError($"[CrashLogger] {type}: {logString}\n{stackTrace}");
        }
    }
}
