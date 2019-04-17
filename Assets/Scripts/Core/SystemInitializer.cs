using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BGC.Users;
using BGC.Study;
using BGC.Audio;

public class SystemInitializer : MonoBehaviour
{
    private static bool initialized = false;

    void Awake()
    {
        InitializeInline();
    }

    public static IEnumerator Initialize()
    {
        if (initialized)
        {
            yield break;
        }

        initialized = true;

        Application.logMessageReceived += LogManager.HandleLog;

        IEnumerator initializationProcess = RunInitialization();

        while (initializationProcess.MoveNext())
        {
            yield return initializationProcess.Current;
        }
    }

    private void InitializeInline()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        Application.logMessageReceived += LogManager.HandleLog;

        IEnumerator initializationProcess = RunInitialization();

        while (initializationProcess.MoveNext())
        {
            //Do not wait
            //We need to do all initialization immediately
        }
    }

    private static IEnumerator RunInitialization()
    {
        try
        {
            Serialization.Initialize();
        }
        catch (Exception e)
        {
            LogManager.LogException(
                excpMessage: e.Message,
                excpSource: e.Source,
                excpStackTrace: e.StackTrace,
                excpDescription: "Serialization.Initialize() Failed");
        }

        yield return null;

        try
        {
            Calibration.Initialize();
        }
        catch (Exception e)
        {
            LogManager.LogException(
                excpMessage: e.Message,
                excpSource: e.Source,
                excpStackTrace: e.StackTrace,
                excpDescription: "Calibration.Initialize() Failed");
        }

        yield return null;

        try
        {
            PlayerData.DeserializeUsers();
        }
        catch (Exception e)
        {
            LogManager.LogException(
                excpMessage: e.Message,
                excpSource: e.Source,
                excpStackTrace: e.StackTrace,
                excpDescription: "PlayerData.DeserializeUsers() Failed");
        }
    }
}
