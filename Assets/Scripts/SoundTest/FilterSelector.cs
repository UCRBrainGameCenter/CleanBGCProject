using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LightJson;
using BGC.Mathematics;
using BGC.Audio;
using BGC.Audio.Filters;

/// <summary>
/// Note!  This implementation was quick and dirty because it was an internal test/demo scene.
/// See PART for procedural UI generation based on Reflection and Custom Attributes
/// </summary>
public class FilterSelector : MonoBehaviour
{
    [SerializeField]
    private Dropdown filterDropdown = null;

    [SerializeField]
    private Text unsupportedText = null;

    [Header("Input Fields")]
    [SerializeField]
    private InputField floatInputA = null;
    [SerializeField]
    private InputField floatInputB = null;
    [SerializeField]
    private InputField floatInputC = null;
    [SerializeField]
    private InputField floatInputD = null;

    [SerializeField]
    private InputField intInputA = null;
    [SerializeField]
    private InputField intInputB = null;

    [Header("Input Field Labels")]
    [SerializeField]
    private Text floatInputALabel = null;
    [SerializeField]
    private Text floatInputBLabel = null;
    [SerializeField]
    private Text floatInputCLabel = null;
    [SerializeField]
    private Text floatInputDLabel = null;

    [SerializeField]
    private Text intInputALabel = null;
    [SerializeField]
    private Text intInputBLabel = null;

    private float FloatA
    {
        get => float.Parse(floatInputA.text);
        set => floatInputA.text = value.ToString();
    }

    private float FloatB
    {
        get => float.Parse(floatInputB.text);
        set => floatInputB.text = value.ToString();
    }

    private float FloatC
    {
        get => float.Parse(floatInputC.text);
        set => floatInputC.text = value.ToString();
    }

    private float FloatD
    {
        get => float.Parse(floatInputD.text);
        set => floatInputD.text = value.ToString();
    }

    private int IntA
    {
        get => int.Parse(intInputA.text);
        set => intInputA.text = value.ToString();
    }

    private int IntB
    {
        get => int.Parse(intInputB.text);
        set => intInputB.text = value.ToString();
    }

    private string FloatALabel
    {
        get => floatInputALabel.text;
        set => floatInputALabel.text = value;
    }

    private string FloatBLabel
    {
        get => floatInputBLabel.text;
        set => floatInputBLabel.text = value;
    }

    private string FloatCLabel
    {
        get => floatInputCLabel.text;
        set => floatInputCLabel.text = value;
    }

    private string FloatDLabel
    {
        get => floatInputDLabel.text;
        set => floatInputDLabel.text = value;
    }

    private string IntALabel
    {
        get => intInputALabel.text;
        set => intInputALabel.text = value;
    }

    private string IntBLabel
    {
        get => intInputBLabel.text;
        set => intInputBLabel.text = value;
    }

    private FilterType CurrentFilter => (FilterType)filterDropdown.value;

    private enum FilterType
    {
        None = 0,
        AllPass,
        HighPass,
        LowPass,
        BandPass,
        Notch,
        LowShelf,
        HighShelf,
        CarlileShuffler,
        ChannelIsolater,
        StreamCenterer_Total,
        StreamCenterer_Explicit,
        Convolution,
        FrequencyModulation,
        MultiConvolution,
        PhaseVocoder,
        StreamAdder,
        StreamConcatenator,
        StreakFork,
        StreamMerge,
        StreamWindower_Relative,
        StreamWindower_Explicit,
        UpChannelMono,
        Spatializer,
        ADSR,
        MAX
    }

    private void Awake()
    {
        filterDropdown.ClearOptions();

        List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>();

        for (FilterType opt = 0; opt < FilterType.MAX; opt++)
        {
            optionData.Add(new Dropdown.OptionData(GetTitle(opt)));
        }

        filterDropdown.AddOptions(optionData);
        filterDropdown.value = 0;
        filterDropdown.onValueChanged.AddListener(FilterChanged);

        FilterChanged(0);
    }

    public IBGCStream ApplyFilter(IBGCStream input)
    {
        switch (CurrentFilter)
        {
            case FilterType.None:
            case FilterType.Convolution:
            case FilterType.MultiConvolution:
            case FilterType.StreamAdder:
            case FilterType.StreamConcatenator:
            case FilterType.StreakFork:
            case FilterType.StreamMerge:
            case FilterType.MAX:
                return input;

            case FilterType.AllPass:
                return new AllPassFilter(
                    stream: input,
                    coeff: new Complex32(FloatA, FloatB),
                    delay: IntA);

            case FilterType.HighPass:
                return BiQuadFilter.HighpassFilter(
                    stream: input,
                    criticalFrequency: FloatA,
                    qFactor: FloatB);

            case FilterType.LowPass:
                return BiQuadFilter.LowpassFilter(
                    stream: input,
                    criticalFrequency: FloatA,
                    qFactor: FloatB);

            case FilterType.BandPass:
                return BiQuadFilter.BandpassFilter(
                    stream: input,
                    centralFrequency: FloatA,
                    qFactor: FloatB);

            case FilterType.Notch:
                return BiQuadFilter.NotchFilter(
                    stream: input,
                    criticalFrequency: FloatA,
                    qFactor: FloatB);

            case FilterType.LowShelf:
                return BiQuadFilter.LowShelfFilter(
                    stream: input,
                    criticalFrequency: FloatA,
                    dbGain: FloatB);

            case FilterType.HighShelf:
                return BiQuadFilter.HighShelfFilter(
                    stream: input,
                    criticalFrequency: FloatA,
                    dbGain: FloatB);

            case FilterType.CarlileShuffler:
                return new CarlileShuffler(
                    stream: input,
                    freqLowerBound: FloatA,
                    freqUpperBound: FloatB,
                    bandCount: IntA);

            case FilterType.ChannelIsolater:
                return new ChannelIsolaterFilter(
                    stream: input,
                    channelIndex: IntA);

            case FilterType.StreamCenterer_Total:
                return new StreamCenterer(
                    stream: input,
                    totalDuration: FloatA);

            case FilterType.StreamCenterer_Explicit:
                return new StreamCenterer(
                    stream: input,
                    preDelaySamples: IntA,
                    postDelaySamples: IntB);

            case FilterType.FrequencyModulation:
                return new FrequencyModulationFilter(
                    stream: input,
                    modRate: FloatA,
                    modDepth: FloatB);

            case FilterType.PhaseVocoder:
                return new PhaseVocoder(
                    stream: input,
                    speed: FloatA);

            case FilterType.StreamWindower_Relative:
                return new StreamWindower(
                    stream: input,
                    function: Windowing.Function.Hamming,
                    smoothingSamples: IntA,
                    sampleOffset: IntB);

            case FilterType.StreamWindower_Explicit:
                return new StreamWindower(
                    stream: input,
                    function: Windowing.Function.Hamming,
                    totalDuration: FloatA,
                    smoothingSamples: IntA,
                    sampleOffset: IntB);

            case FilterType.UpChannelMono:
                return new UpChannelMonoFilter(
                    stream: input,
                    channelCount: 2);

            case FilterType.Spatializer:
                return input.Spatialize(
                    angle: FloatA);

            case FilterType.ADSR:
                return input.ADSR(
                    timeToPeak: FloatA,
                    timeToSustain: FloatB,
                    sustainAmplitude: FloatC,
                    sustainDecayTime: FloatD);

            default:
                Debug.LogError($"Unexpected FilterType: {CurrentFilter}");
                return input;
        }
    }

    public void ResetFilter()
    {
        filterDropdown.value = (int)FilterType.None;
        filterDropdown.RefreshShownValue();
        FilterChanged((int)FilterType.None);
    }
    
    private string GetTitle(FilterType type)
    {
        switch (type)
        {
            case FilterType.CarlileShuffler: return "Carlile Shuffler";
            case FilterType.StreamCenterer_Total: return "Stream Centerer (Total)";
            case FilterType.StreamCenterer_Explicit: return "Stream Centerer (Explicit)";
            case FilterType.StreamWindower_Relative: return "Stream Windower (Relative)";
            case FilterType.StreamWindower_Explicit: return "Stream Windower (Explicit)";
            case FilterType.ADSR: return "ADSR (Synth Env)";
            default: return type.ToString();
        }
    }

    private void FilterChanged(int value)
    {
        //Disable All
        floatInputA.transform.parent.gameObject.SetActive(false);
        floatInputB.transform.parent.gameObject.SetActive(false);
        floatInputC.transform.parent.gameObject.SetActive(false);
        floatInputD.transform.parent.gameObject.SetActive(false);
        intInputA.transform.parent.gameObject.SetActive(false);
        intInputB.transform.parent.gameObject.SetActive(false);
        unsupportedText.transform.parent.gameObject.SetActive(false);


        //Enable and reset used values
        switch ((FilterType)value)
        {
            case FilterType.None:
                //Do Nothing
                break;

            case FilterType.AllPass:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Real Coefficient";
                FloatA = 1.0f;

                floatInputB.transform.parent.gameObject.SetActive(true);
                FloatBLabel = "Imag Coefficient";
                FloatB = 1.0f;

                intInputA.transform.parent.gameObject.SetActive(true);
                IntALabel = "Sample Delay";
                IntA = 100;
                break;

            case FilterType.HighPass:
            case FilterType.LowPass:
            case FilterType.BandPass:
            case FilterType.Notch:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Critical Frequency";
                FloatA = 1000f;

                floatInputB.transform.parent.gameObject.SetActive(true);
                FloatBLabel = "Q Factor";
                FloatB = 1f / Mathf.Sqrt(2f);
                break;

            case FilterType.LowShelf:
            case FilterType.HighShelf:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Critical Frequency";
                FloatA = 1000f;

                floatInputB.transform.parent.gameObject.SetActive(true);
                FloatBLabel = "dbGain";
                FloatB = 20f;
                break;

            case FilterType.CarlileShuffler:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Frequency LB";
                FloatA = 20f;

                floatInputB.transform.parent.gameObject.SetActive(true);
                FloatBLabel = "Frequency UB";
                FloatB = 16000f;

                intInputA.transform.parent.gameObject.SetActive(true);
                IntALabel = "Band Count";
                IntA = 22;
                break;

            case FilterType.ChannelIsolater:
                intInputA.transform.parent.gameObject.SetActive(true);
                IntALabel = "Channel Index";
                IntA = 0;
                break;

            case FilterType.StreamCenterer_Total:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Total Duration";
                FloatA = 20f;
                break;

            case FilterType.StreamCenterer_Explicit:
                intInputA.transform.parent.gameObject.SetActive(true);
                IntALabel = "Pre-Delay Samples";
                IntA = 1000;

                intInputB.transform.parent.gameObject.SetActive(true);
                IntBLabel = "Post-Delay Samples";
                IntB = 1000;
                break;

            case FilterType.Convolution:
            case FilterType.MultiConvolution:
            case FilterType.StreamAdder:
            case FilterType.StreamConcatenator:
            case FilterType.StreakFork:
            case FilterType.StreamMerge:
                unsupportedText.transform.parent.gameObject.SetActive(true);
                break;

            case FilterType.FrequencyModulation:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Modulation Rate";
                FloatA = 20f;

                floatInputB.transform.parent.gameObject.SetActive(true);
                FloatBLabel = "Modulation Depth";
                FloatB = 20f;
                break;

            case FilterType.PhaseVocoder:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Speed";
                FloatA = 0.5f;
                break;

            case FilterType.StreamWindower_Relative:
                intInputA.transform.parent.gameObject.SetActive(true);
                IntALabel = "Smoothing Samples";
                IntA = 1000;

                intInputB.transform.parent.gameObject.SetActive(true);
                IntBLabel = "Sample Offset";
                IntB = 0;
                break;

            case FilterType.StreamWindower_Explicit:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Total Duration";
                FloatA = 20f;

                intInputA.transform.parent.gameObject.SetActive(true);
                IntALabel = "Smoothing Samples";
                IntA = 1000;

                intInputB.transform.parent.gameObject.SetActive(true);
                IntBLabel = "Sample Offset";
                IntB = 0;
                break;

            case FilterType.UpChannelMono:
                break;

            case FilterType.Spatializer:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Angle";
                FloatA = 45f;
                break;

            case FilterType.ADSR:
                floatInputA.transform.parent.gameObject.SetActive(true);
                FloatALabel = "Time To Peak";
                FloatA = 0.01f;

                floatInputB.transform.parent.gameObject.SetActive(true);
                FloatBLabel = "Time To Sustain";
                FloatB = 1f;

                floatInputC.transform.parent.gameObject.SetActive(true);
                FloatCLabel = "Sustain Amplitude";
                FloatC = 0.9f;

                floatInputD.transform.parent.gameObject.SetActive(true);
                FloatDLabel = "Sustain Decay Time";
                FloatD = 0.5f;
                break;

            default:
                throw new Exception($"Unexpected FilterType: {(FilterType)value}");
        }
    }



    public JsonObject Serialize()
    {
        JsonObject filterData = new JsonObject()
        {
            { "FilterType", CurrentFilter.ToString() }
        };

        switch (CurrentFilter)
        {
            case FilterType.None:
            case FilterType.UpChannelMono:
                //No Parameters
                break;

            case FilterType.AllPass:
                filterData.Add(FloatALabel, FloatA);
                filterData.Add(FloatBLabel, FloatB);
                filterData.Add(IntALabel, IntA);
                break;

            case FilterType.HighPass:
            case FilterType.LowPass:
            case FilterType.BandPass:
            case FilterType.Notch:
            case FilterType.LowShelf:
            case FilterType.HighShelf:
                filterData.Add(FloatALabel, FloatA);
                filterData.Add(FloatBLabel, FloatB);
                break;

            case FilterType.CarlileShuffler:
                filterData.Add(FloatALabel, FloatA);
                filterData.Add(FloatBLabel, FloatB);
                filterData.Add(IntALabel, IntA);
                break;

            case FilterType.ChannelIsolater:
                filterData.Add(IntALabel, IntA);
                break;

            case FilterType.StreamCenterer_Total:
                filterData.Add(FloatALabel, FloatA);
                break;

            case FilterType.StreamCenterer_Explicit:
                filterData.Add(IntALabel, IntA);
                filterData.Add(IntBLabel, IntB);
                break;

            case FilterType.Convolution:
            case FilterType.MultiConvolution:
            case FilterType.StreamAdder:
            case FilterType.StreamConcatenator:
            case FilterType.StreakFork:
            case FilterType.StreamMerge:
                //Unsupported
                break;

            case FilterType.FrequencyModulation:
                filterData.Add(FloatALabel, FloatA);
                filterData.Add(FloatBLabel, FloatB);
                break;

            case FilterType.PhaseVocoder:
                filterData.Add(FloatALabel, FloatA);
                break;

            case FilterType.StreamWindower_Relative:
                filterData.Add(IntALabel, IntA);
                filterData.Add(IntBLabel, IntB);
                break;

            case FilterType.StreamWindower_Explicit:
                filterData.Add(FloatALabel, FloatA);
                filterData.Add(IntALabel, IntA);
                filterData.Add(IntBLabel, IntB);
                break;

            case FilterType.Spatializer:
                filterData.Add(FloatALabel, FloatA);
                break;

            case FilterType.ADSR:
                filterData.Add(FloatALabel, FloatA);
                filterData.Add(FloatBLabel, FloatB);
                filterData.Add(FloatCLabel, FloatC);
                filterData.Add(FloatDLabel, FloatD);
                break;

            default:
                throw new Exception($"Unexpected FilterType: {CurrentFilter}");
        }

        return filterData;
    }

    public void Deserialize(JsonObject streamData)
    {
        Enum.TryParse(streamData["FilterType"], out FilterType filterType);

        switch (filterType)
        {
            case FilterType.None:
            case FilterType.UpChannelMono:
            case FilterType.AllPass:
            case FilterType.HighPass:
            case FilterType.LowPass:
            case FilterType.BandPass:
            case FilterType.Notch:
            case FilterType.LowShelf:
            case FilterType.HighShelf:
            case FilterType.CarlileShuffler:
            case FilterType.ChannelIsolater:
            case FilterType.StreamCenterer_Total:
            case FilterType.StreamCenterer_Explicit:
            case FilterType.Convolution:
            case FilterType.FrequencyModulation:
            case FilterType.MultiConvolution:
            case FilterType.PhaseVocoder:
            case FilterType.StreamAdder:
            case FilterType.StreamConcatenator:
            case FilterType.StreakFork:
            case FilterType.StreamMerge:
            case FilterType.StreamWindower_Relative:
            case FilterType.StreamWindower_Explicit:
            case FilterType.Spatializer:
            case FilterType.ADSR:
                //We're good, keep going
                break;
            default:
                Debug.LogError($"Unexpected FilterType: {filterType}");
                filterType = FilterType.None;
                break;
        }


        filterDropdown.value = (int)filterType;
        filterDropdown.RefreshShownValue();

        FilterChanged((int)filterType);

        switch (filterType)
        {
            case FilterType.None:
            case FilterType.UpChannelMono:
                //None
                break;

            case FilterType.AllPass:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                FloatB = (float)streamData[FloatBLabel].AsNumber;
                IntA = streamData[IntALabel];
                break;

            case FilterType.HighPass:
            case FilterType.LowPass:
            case FilterType.BandPass:
            case FilterType.Notch:
            case FilterType.LowShelf:
            case FilterType.HighShelf:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                FloatB = (float)streamData[FloatBLabel].AsNumber;
                break;

            case FilterType.CarlileShuffler:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                FloatB = (float)streamData[FloatBLabel].AsNumber;
                IntA = streamData[IntALabel];
                break;

            case FilterType.ChannelIsolater:
                IntA = streamData[IntALabel];
                break;

            case FilterType.StreamCenterer_Total:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                break;

            case FilterType.StreamCenterer_Explicit:
                IntA = streamData[IntALabel];
                IntB = streamData[IntBLabel];
                break;

            case FilterType.FrequencyModulation:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                FloatB = (float)streamData[FloatBLabel].AsNumber;
                break;

            case FilterType.Convolution:
            case FilterType.MultiConvolution:
            case FilterType.StreamAdder:
            case FilterType.StreamConcatenator:
            case FilterType.StreakFork:
            case FilterType.StreamMerge:
                //Unsupported
                break;

            case FilterType.PhaseVocoder:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                break;

            case FilterType.StreamWindower_Relative:
                IntA = streamData[IntALabel];
                IntB = streamData[IntBLabel];
                break;

            case FilterType.StreamWindower_Explicit:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                IntA = streamData[IntALabel];
                IntB = streamData[IntBLabel];
                break;

            case FilterType.Spatializer:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                break;

            case FilterType.ADSR:
                FloatA = (float)streamData[FloatALabel].AsNumber;
                FloatB = (float)streamData[FloatBLabel].AsNumber;
                FloatC = (float)streamData[FloatCLabel].AsNumber;
                FloatD = (float)streamData[FloatDLabel].AsNumber;
                break;

            default:
                throw new Exception($"Unexpected FilterType: {filterType}");
        }

    }
}
