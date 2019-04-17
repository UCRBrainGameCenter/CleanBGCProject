using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using LightJson;
using BGC.IO;
using BGC.Audio;

public class SoundTest : MonoBehaviour
{
    [SerializeField]
    private InputField titleInput = null;
    [SerializeField]
    private StreamSelector streamSelector = null;
    [SerializeField]
    private FilterSelector[] filterSelectors = null;

    [SerializeField]
    private Button saveButton = null;
    [SerializeField]
    private Button loadButton = null;
    [SerializeField]
    private Button playButton = null;
    [SerializeField]
    private Button stopButton = null;
    [SerializeField]
    private InputField volumeField = null;
    [SerializeField]
    private BGCClipPlayer clipPlayer = null;

    private void Awake()
    {
        Calibration.Initialize();

        playButton.onClick.AddListener(Play);
        stopButton.onClick.AddListener(Stop);
        saveButton.onClick.AddListener(Save);
        loadButton.onClick.AddListener(Load);
    }

    private void Play()
    {
        Stop();

        IBGCStream stream = streamSelector.GetStream();

        if (stream == null)
        {
            return;
        }

        foreach (FilterSelector filterer in filterSelectors)
        {
            stream = filterer.ApplyFilter(stream);
        }

        stream = stream.Normalize(float.Parse(volumeField.text));

        clipPlayer.PlayStream(stream);
    }

    private void Stop()
    {
        clipPlayer.Stop();
    }

    private void Save()
    {
        string defaultFilePath = DataManagement.PathForDataFile("TestStimuli", $"{titleInput.text}.json");
        string filePath = DataManagement.NextAvailableFilePath(defaultFilePath);

        FileWriter.WriteJson(
            path: filePath,
            createJson: Serialize,
            pretty: true);
    }

    private void Load()
    {
        string filePath = DataManagement.PathForDataFile("TestStimuli", $"{titleInput.text}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File doesn't exist: {filePath}");
            return;
        }

        FileReader.ReadJsonFile(
            path: filePath,
            successCallback: Deserialize);
    }

    private JsonObject Serialize()
    {
        JsonArray filters = new JsonArray();

        foreach (FilterSelector filterer in filterSelectors)
        {
            filters.Add(filterer.Serialize());
        }

        return new JsonObject
        {
            { "Name", titleInput.text },
            { "BaseStream", streamSelector.Serialize() },
            { "Filters", filters },
            { "Level", float.Parse(volumeField.text) }
        };
    }

    private void Deserialize(JsonObject serializedSound)
    {
        titleInput.text = serializedSound["Name"].AsString;
        volumeField.text = serializedSound["Level"].AsNumber.ToString();

        streamSelector.Deserialize(serializedSound["BaseStream"]);


        JsonArray filters = serializedSound["Filters"];
        for (int i = 0; i < filterSelectors.Length; i++)
        {
            if (i < filters.Count)
            {
                filterSelectors[i].Deserialize(filters[i]);
            }
            else
            {
                filterSelectors[i].ResetFilter();
            }
        }
    }
}