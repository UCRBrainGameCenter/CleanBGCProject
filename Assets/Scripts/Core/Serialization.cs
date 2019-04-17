using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BGC.IO;
using LightJson;

/// <summary>
/// Example serialized enum
/// </summary>
public enum SerializedEnum
{
    First = 0,
    Second,
    Third,
    Etc,
    OtherValues,
    MAX
}

/// <summary>
/// Example serialized enum
/// </summary>
public enum AnotherSerializedEnum
{
    A = 0,
    B,
    C,
    D,
    E,
    F,
    G,
    MAX
}



public static class Serialization
{
    private static bool initialized = false;
    private static readonly int serializationVersion = 1;
    private static readonly string systemSettingsDir = "System";
    private static readonly string systemSettingsFile = "Config.json";

    private static readonly Dictionary<string, SerializedEnum> serializedEnums =
        new Dictionary<string, SerializedEnum>();
    private static readonly Dictionary<string, AnotherSerializedEnum> anotherSerializedEnums =
        new Dictionary<string, AnotherSerializedEnum>();

    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        for (SerializedEnum i = 0; i < SerializedEnum.MAX; i++)
        {
            serializedEnums[i.ToSerializationString()] = i;
        }

        for (AnotherSerializedEnum i = 0; i < AnotherSerializedEnum.MAX; i++)
        {
            anotherSerializedEnums[i.ToSerializationString()] = i;
        }

        LoadSystemConfig();
    }

    public static void LoadSystemConfig()
    {
        string path = DataManagement.PathForDataDirectory(systemSettingsDir);
        string filePath = Path.Combine(path, systemSettingsFile);

        bool updateConfigFile = false;

        int prevSerializationVersion = -1;

        FileReader.ReadJsonFile(
            path: filePath,
            successCallback: (JsonObject jsonConfigs) =>
            {
                prevSerializationVersion = jsonConfigs["SerializationVersion"];
            });

        //Update Serialization and system options
        if (prevSerializationVersion < serializationVersion)
        {
            updateConfigFile = true;
        }

        if (updateConfigFile)
        {
            SaveConfiguration();
        }
    }

    private static void SaveConfiguration()
    {
        FileWriter.WriteJson(
            path: Path.Combine(DataManagement.PathForDataDirectory(systemSettingsDir), systemSettingsFile),
            createJson: () => new JsonObject()
            {
                { "SerializationVersion", serializationVersion }
            },
            pretty: true);
    }

    public static string ToSerializationString(this SerializedEnum element)
    {
        //Example of explicit translation immune to many simple enum refactor issues
        switch (element)
        {
            case SerializedEnum.First: return "First";
            case SerializedEnum.Second: return "OldSerializedValue_1";
            case SerializedEnum.Third: return "OldSerializedValue_2";
            case SerializedEnum.Etc: return "ETC";
            case SerializedEnum.OtherValues: return "OtherValues";

            default:
                Debug.LogError($"Unknown SerializedEnum: {element}");
                return "";
        }
    }

    public static string ToSerializationString(this AnotherSerializedEnum element)
    {
        //Example of implicit translation that would break on enum value name refactor
        switch (element)
        {
            case AnotherSerializedEnum.A: 
            case AnotherSerializedEnum.B: 
            case AnotherSerializedEnum.C: 
            case AnotherSerializedEnum.D: 
            case AnotherSerializedEnum.E: 
            case AnotherSerializedEnum.F: 
            case AnotherSerializedEnum.G: return element.ToString();

            default:
                Debug.LogError($"Unknown AnotherSerializedEnum: {element}");
                return "";
        }
    }

    /// <exception cref="ParsingException">The serialization string is not in the dictionary</exception>
    private static T ReadValue<T>(Dictionary<string, T> dict, string value)
    {
        if (dict.ContainsKey(value))
        {
            return dict[value];
        }
        else
        {
            string validValues = "";
            foreach (string key in dict.Keys)
            {
                validValues += $"\n\t{key}";
            }

            throw new ParsingException(
                $"Failed to parse {typeof(T).Name} string: \"{value}\"",
                $"Valid Values: {validValues}");
        }
    }

    /// <exception cref="ParsingException">The serialization string is not in the dictionary</exception>
    public static SerializedEnum ParseSerializedEnum(string value) =>
        ReadValue(serializedEnums, value);

    /// <exception cref="ParsingException">The serialization string is not in the dictionary</exception>
    public static AnotherSerializedEnum ParseAnotherSerializedEnum(string value) =>
        ReadValue(anotherSerializedEnums, value);
}
