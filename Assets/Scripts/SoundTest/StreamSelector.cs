using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using LightJson;
using BGC.IO;
using BGC.Audio;
using BGC.Audio.Synthesis;

public class StreamSelector : MonoBehaviour
{
    [SerializeField]
    private Dropdown streamDropdown = null;

    [SerializeField]
    private InputField frequencyInput = null;
    [SerializeField]
    private InputField dutyCycleInput = null;
    [SerializeField]
    private InputField fileNameInput = null;

    private float Frequency
    {
        get => float.Parse(frequencyInput.text);
        set => frequencyInput.text = value.ToString();
    }

    private float DutyCycle
    {
        get => float.Parse(dutyCycleInput.text);
        set => dutyCycleInput.text = value.ToString();
    }

    private string FileName => Path.Combine(DataManagement.RootDirectory, fileNameInput.text);

    private const float DEFAULT_FREQUENCY = 440f;
    private const float DEFAULT_DUTYCYCLE = 0.5f;
    private const string DEFAULT_FILENAME = "Test/000000.wav";

    private StreamType CurrentStream => (StreamType)streamDropdown.value;

    private enum StreamType
    {
        SineWave = 0,
        SawtoothWave,
        TriangleWave,
        SquareWave,
        WAVFile,
        MAX
    }

    private void Awake()
    {
        streamDropdown.ClearOptions();

        List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>();

        for (StreamType opt = 0; opt < StreamType.MAX; opt++)
        {
            optionData.Add(new Dropdown.OptionData(opt.ToString()));
        }

        streamDropdown.AddOptions(optionData);

        streamDropdown.value = 0;

        streamDropdown.onValueChanged.AddListener(StreamChanged);

        frequencyInput.text = DEFAULT_FREQUENCY.ToString();
        dutyCycleInput.text = DEFAULT_DUTYCYCLE.ToString();
        fileNameInput.text = DEFAULT_FILENAME;

        StreamChanged(0);
    }

    private void StreamChanged(int index)
    {
        frequencyInput.transform.parent.gameObject.SetActive(false);
        dutyCycleInput.transform.parent.gameObject.SetActive(false);
        fileNameInput.transform.parent.gameObject.SetActive(false);

        switch ((StreamType)index)
        {
            case StreamType.SineWave:
            case StreamType.SawtoothWave:
                frequencyInput.transform.parent.gameObject.SetActive(true);
                break;

            case StreamType.TriangleWave:
            case StreamType.SquareWave:
                frequencyInput.transform.parent.gameObject.SetActive(true);
                dutyCycleInput.transform.parent.gameObject.SetActive(true);
                break;

            case StreamType.WAVFile:
                fileNameInput.transform.parent.gameObject.SetActive(true);
                break;

            default:
                throw new ArgumentException($"Unexpected StreamType: {(StreamType)index}");
        }
    }

    public IBGCStream GetStream()
    {
        switch (CurrentStream)
        {
            case StreamType.SineWave: return new SineWave(1.0f, Frequency);
            case StreamType.SawtoothWave: return new SawtoothWave(1.0f, Frequency);
            case StreamType.TriangleWave: return new TriangleWave(1.0f, Frequency, DutyCycle);
            case StreamType.SquareWave: return new SquareWave(1.0f, Frequency, DutyCycle);
            case StreamType.WAVFile:
                if (WaveEncoding.LoadBGCStream(FileName, out IBGCStream stream))
                {
                    return stream;
                }
                throw new ArgumentException($"Failed to load file: {FileName}");

            default:
                throw new Exception($"Unexpected StreamType: {CurrentStream}");
        }
    }

    public JsonObject Serialize()
    {
        JsonObject streamData = new JsonObject()
        {
            { "StreamType", CurrentStream.ToString() }
        };

        switch (CurrentStream)
        {
            case StreamType.SineWave:
            case StreamType.SawtoothWave:
                streamData.Add("Frequency", Frequency);
                break;

            case StreamType.TriangleWave:
            case StreamType.SquareWave:
                streamData.Add("Frequency", Frequency);
                streamData.Add("DutyCycle", DutyCycle);
                break;

            case StreamType.WAVFile:
                streamData.Add("FileName", fileNameInput.text);
                break;

            default:
                break;
        }

        return streamData;
    }

    public void Deserialize(JsonObject streamData)
    {
        Enum.TryParse(streamData["StreamType"], out StreamType streamType);

        switch (streamType)
        {
            case StreamType.SineWave:
            case StreamType.SawtoothWave:
            case StreamType.TriangleWave:
            case StreamType.SquareWave:
            case StreamType.WAVFile:
                //it's good, keep going
                break;

            default:
                //Reinit
                StreamChanged((int)StreamType.SineWave);
                return;
        }

        streamDropdown.value = (int)streamType;
        streamDropdown.RefreshShownValue();

        StreamChanged((int)streamType);

        switch (streamType)
        {
            case StreamType.SineWave:
            case StreamType.SawtoothWave:
                Frequency = (float)streamData["Frequency"].AsNumber;
                break;

            case StreamType.TriangleWave:
            case StreamType.SquareWave:
                Frequency = (float)streamData["Frequency"].AsNumber;
                DutyCycle = (float)streamData["DutyCycle"].AsNumber;
                break;

            case StreamType.WAVFile:
                fileNameInput.text = streamData["FileName"].AsString;
                break;

            default:
                throw new ArgumentException($"Unexpected StreamType: {streamType}");
        }

    }
}
