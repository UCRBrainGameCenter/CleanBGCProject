using System;
using System.Collections;
using UnityEngine;
using BGC.Users;
using BGC.IO;
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

        if (!DataManagement.DataDirectoryExists("HRTF"))
        {
            Debug.LogError("Hey, Dummy!  You gotta run the Splashscreen Scene at least once to extract the HRTF Files!");
        }

        IEnumerator initializationProcess = RunInitialization();

        while (initializationProcess.MoveNext())
        {
            //Do not wait
            //We need to do all initialization immediately
        }
    }

    private static IEnumerator RunInitialization()
    {
#if !UNITY_EDITOR
        if (Screen.fullScreen)
        {
            Debug.Log("Changing application to Windowed mode");
            Screen.SetResolution(1920, 1080, false);
        }
#endif

        //Unlock account for first-time users
        if (!PlayerData.EverUnlocked)
        {
            Debug.LogWarning("First Run Detected - Unlocking for demo purposes");
            PlayerData.IsLocked = false;
        }

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
