using System;
using UnityEngine;
using UnityEngine.UI;
using BGC.Utility;
using GaborFunctions;

public abstract class VisualStimulus : MonoBehaviour
{
    public abstract ObjectType ObjectType { get; }

    public int Id { get; private set; }

    private RectTransform _rectTransform = null;
    public virtual RectTransform RectTransform => _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());

    public abstract Image Image { get; }

    public abstract float Contrast { get; set; }
    public abstract float Orientation { get; set; }
    public abstract float Offset { get; set; }
    public abstract float SpatialFrequency { get; set; }

    public Action TapFeedback { get; set; }
    public Action MissFeedback { get; set; }

    public abstract void SetFeatures(
        float contrast,
        float orientation,
        float offset,
        float spatialFrequency);

    public abstract void LogSpawn(int block, int trial);

    private VisualStimulusClickEvent _clickHandler = null;
    /// <summary> Caches attached ClickHandler.  Creates logging click handler if none. </summary>
    public VisualStimulusClickEvent ClickHandler => _clickHandler ??
        (_clickHandler = GetComponent<VisualStimulusClickEvent>()) ??
        (_clickHandler = gameObject.AddComponent<LoggingStimulusClickEvent>());

    private VisualStimulusLerpedActionChannel _lerpHandler = null;
    public VisualStimulusLerpedActionChannel LerpHandler => _lerpHandler ??
        (_lerpHandler = GetComponent<VisualStimulusLerpedActionChannel>()) ??
        (_lerpHandler = gameObject.AddComponent<VisualStimulusLerpedActionChannel>());


    private float _spawnTime;
    public float LifeSpan => Time.time - _spawnTime;

    private void Awake()
    {
        Id = StaticIdManager.GetId();
        _spawnTime = Time.time;
    }
}
