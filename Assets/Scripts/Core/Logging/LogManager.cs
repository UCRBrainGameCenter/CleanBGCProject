using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightJson;
using BGC.Utility;
using BGC.Users;

public class LogManager
{
    public enum LogLevel
    {
        None = 0,
        Exceptions,
        Asserts,
        Errors,
        Warnings,
        Logs,
        All,
        Invalid
    }

    //Exception Log
    private static LocalLogger _exceptionLog = null;

    #region Helper Properties


    //Exception Log
    private static LocalLogger ExceptionLog => _exceptionLog ?? (_exceptionLog = new LocalLogger("Exceptions", "Exception"));

    #endregion Helper Properties

    public static void LogClick(
        int id,
        ObjectType type,
        float x,
        float y,
        bool validClick)
    {
        //Dummy function to demonstrate functionality
    }

    public static void ClearElementLogs()
    {
    }

    public static void ClearAllLogs()
    {
        ClearElementLogs();
    }

    #region Error Logging

    public static void HandleLog(
        string excpCondition,
        string excpStackTrace,
        LogType type)
    {
        if (GetShouldLog(type))
        {
            ExceptionLog.PushLines(
                $"Handled {type.ToString()} logged on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToLongTimeString()}",
                "Message:",
                excpCondition,
                "",
                excpStackTrace,
                "",
                "------------------------------------------",
                "");
        }
    }

    public static void LogException(
        string excpMessage,
        string excpSource = "",
        string excpStackTrace = "",
        string excpDescription = "")
    {
        ExceptionLog.PushLines(
            $"Exception logged on {DateTime.Now.ToLongDateString()} at {DateTime.Now.ToLongTimeString()}",
            "Exception Message:",
            excpMessage,
            "");

        if (excpSource != "")
        {
            ExceptionLog.PushLine(excpSource);
        }

        if (excpStackTrace != "")
        {
            ExceptionLog.PushLine(excpStackTrace);
        }

        if (excpDescription != "")
        {
            ExceptionLog.PushLines(
                "",
                "User Description:",
                excpDescription);
        }

        ExceptionLog.PushLines(
            "",
            "------------------------------------------",
            "");
    }
    
    private static bool GetShouldLog(LogType type)
    {
        LogLevel currentLogLevel = (LogLevel)SettingsMenu.GetSettingInt(SettingsMenu.Keys.LogLevel);

        if (currentLogLevel >= ConvertLogType(type))
        {
            return true;
        }

        return false;
    }

    public static string GetLogLevelName(int level)
    {
        switch ((LogLevel)level)
        {
            case LogLevel.None: return "None";
            case LogLevel.Exceptions: return "Exceptions";
            case LogLevel.Asserts: return "Asserts";
            case LogLevel.Errors: return "Errors";
            case LogLevel.Warnings: return "Warnings";
            case LogLevel.Logs: return "Logs";
            case LogLevel.All: return "All";
            default:
                Debug.LogError($"Unexpected LogLevel: {(LogLevel)level}");
                return "";
        }
    }

    //Converts logtype to the appropriate heirarcy for simple comparison
    private static LogLevel ConvertLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Error: return LogLevel.Errors;
            case LogType.Assert: return LogLevel.Asserts;
            case LogType.Warning: return LogLevel.Warnings;
            case LogType.Log: return LogLevel.Logs;
            case LogType.Exception: return LogLevel.Exceptions;
            default:
                Debug.LogError($"Unexpected LogType: {type}");
                return LogLevel.Invalid;
        }
    }

    #endregion Error Logging
}