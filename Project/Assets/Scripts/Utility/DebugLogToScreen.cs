﻿using UnityEngine;
using System.Collections;

public class DebugLogToScreen : MonoBehaviour
{
    string myLog;
    Queue myLogQueue = new Queue();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog( string logString, string stackTrace, LogType type )
    {
        myLog = logString;
        string newString = "\n [" + type + "] : " + myLog;
        myLogQueue.Enqueue( newString );
        if( type == LogType.Exception )
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue( newString );
        }
        myLog = string.Empty;
        foreach( string mylog in myLogQueue )
        {
            myLog += mylog;
        }
    }

    void OnGUI()
    {
        GUI.color = Color.black;
        GUILayout.Label( myLog );
    }
}